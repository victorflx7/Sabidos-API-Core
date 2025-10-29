using System; // ADICIONADO: Para Type, Activator, etc.
using System.Collections; // ADICIONADO: Necessário para System.Collections.IEnumerable em TestAsyncEnumerable
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using SabidosAPI_Core.Models; // Ajuste este using se a localização da sua classe base de Modelos for diferente

namespace Api.Tests
{
    // --- CLASSES AUXILIARES PARA MOCK ASYNC DO EF CORE ---

    // 1. IAsyncEnumerator
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }

    // 2. IQueryable e IAsyncEnumerable
    public class TestAsyncEnumerable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _query;
        public IQueryProvider Provider { get; }
        public Expression Expression => _query.Expression;
        public Type ElementType => _query.ElementType;

        public TestAsyncEnumerable(IQueryable<T> query, IQueryProvider provider)
        {
            _query = query;
            Provider = provider;
        }

        public IEnumerator<T> GetEnumerator() => _query.GetEnumerator();
        // O método GetEnumerator() não-genérico agora resolve devido ao using System.Collections
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _query.GetEnumerator();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_query.GetEnumerator());
        }
    }

    // 3. IAsyncQueryProvider
    public class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            // Tratamento padrão para consultas que retornam IQueryable não-genérico
            return new TestAsyncEnumerable<object>(_inner.CreateQuery<object>(expression), new TestAsyncQueryProvider<object>(_inner));
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(_inner.CreateQuery<TElement>(expression), new TestAsyncQueryProvider<TElement>(_inner));
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        // CORREÇÃO CRÍTICA: Lógica para converter o resultado síncrono em Task<TResult> ou ValueTask<TResult>
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            // Executa a query de forma síncrona
            var result = Execute<TResult>(expression);

            var type = typeof(TResult);
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                // Tratamento para ValueTask<T> (e.g., FindAsync)
                if (genericTypeDefinition == typeof(ValueTask<>))
                {
                    var innerType = type.GetGenericArguments()[0];
                    var valueTaskType = typeof(ValueTask<>).MakeGenericType(innerType);
                    return (TResult)Activator.CreateInstance(valueTaskType, result)!;
                }
                // Tratamento para Task<T> (e.g., ToListAsync, CountAsync)
                else if (genericTypeDefinition == typeof(Task<>))
                {
                    var innerType = type.GetGenericArguments()[0];
                    // Usa Task.FromResult<T> para criar o Task<T>
                    var taskFromMethod = typeof(Task).GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(innerType);
                    return (TResult)taskFromMethod.Invoke(null, new object[] { result! })!;
                }
                // Tratamento para ValueTask (não genérica, como para SaveChangesAsync se implementado aqui)
                else if (genericTypeDefinition == typeof(ValueTask))
                {
                    return (TResult)(object)ValueTask.CompletedTask;
                }
            }

            // Se for um tipo não genérico (por exemplo, Task), retorna o Task.CompletedTask
            if (type == typeof(Task))
            {
                return (TResult)(object)Task.CompletedTask;
            }

            // Retorna o resultado síncrono como fallback
            return result;
        }
    }

    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> AsDbSetMock<T>(this IQueryable<T> queryable) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            // Cria o provider customizado
            var asyncProvider = new TestAsyncQueryProvider<T>(queryable.Provider);

            // Configura IQueryable e IAsyncEnumerable
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(asyncProvider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                   .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            // Simulação de FindAsync
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                      .Returns<object[]>(ids =>
                      {
                          var idValue = ids.FirstOrDefault();

                          var entity = queryable.FirstOrDefault(x =>
                              x.GetType().GetProperty("Id")!.GetValue(x)!.Equals(idValue));

                          return new ValueTask<T?>(entity);
                      });
            }

            // Mock para operações de alteração de estado
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(entity => { /* Simula adição */ });
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(entity => { /* Simula remoção */ });

            return mockSet;
        }
    }
}