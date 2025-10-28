using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using Xunit;

namespace SabidosAPI_Core.Tests.Services
{
    // CLASSE AUXILIAR: Simula o DbSet para Moq
    // É necessário para que métodos como .Where, .OrderBy e .ToListAsync funcionem no contexto do Mock.
    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> AsDbSetMock<T>(this IQueryable<T> queryable) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Mock para Find (assumindo que T tem propriedade Id)
            mockSet.Setup(x => x.Find(It.IsAny<object[]>()))
                   .Returns<object[]>(ids => queryable.FirstOrDefault(x => (int)typeof(T).GetProperty("Id").GetValue(x) == (int)ids[0]));

            // Mock para operações de alteração de estado
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(entity => { /* Simula adição */ });
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(entity => { /* Simula remoção */ });

            return mockSet;
        }
    }

    public class EventoServiceTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly EventoService _service;
        private readonly string _testUserUid = "test-uid-evento-service-1";
        private readonly List<Evento> _eventos;

        public EventoServiceTests()
        {
            // Inicializa Mocks
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());


            _mockMapper = new Mock<IMapper>();
            // Dados de teste (incluindo o relacionamento com User para evitar NullReferenceException no .Include)
            _eventos = new List<Evento>
            {
                new Evento { Id = 1, TitleEvent = "Reunião 1", AuthorUid = _testUserUid, DataEvento = DateTime.Now.AddDays(-1), User = new User { FirebaseUid = _testUserUid } },
                new Evento { Id = 2, TitleEvent = "Compromisso 2", AuthorUid = "other-uid", DataEvento = DateTime.Now.AddDays(-2), User = new User { FirebaseUid = "other-uid" } },
                new Evento { Id = 3, TitleEvent = "Reunião 3", AuthorUid = _testUserUid, DataEvento = DateTime.Now.AddDays(-3), User = new User { FirebaseUid = _testUserUid } },
            };

            // Configura o DbSet simulado (IQueryable)
            var mockSet = _eventos.AsQueryable().AsDbSetMock();

            // Configura o AppDbContext para retornar o DbSet simulado
            _mockContext.Setup(c => c.Eventos).Returns(mockSet.Object);
            // Configura SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);


            // Cria a instância do serviço com os Mocks
            _service = new EventoService(_mockContext.Object, _mockMapper.Object);
        }

        // --- Testes para GetAllEventosAsync ---

        [Fact]
        public async Task GetAllEventosAsync_DeveRetornarTodosEventosSeNenhumUidForFornecido()
        {
            // Arrange
            // Simula o mapeamento de 3 entidades para 3 DTOs
            _mockMapper.Setup(m => m.Map<List<EventoResponseDto>>(It.IsAny<List<Evento>>()))
                       .Returns(new List<EventoResponseDto> { new EventoResponseDto(), new EventoResponseDto(), new EventoResponseDto() });

            // Act
            var result = await _service.GetAllEventosAsync(null);

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllEventosAsync_DeveRetornarApenasEventosDoUsuarioSeUidForFornecido()
        {
            // Arrange
            // O serviço deve filtrar 2 eventos com o UID especificado
            _mockMapper.Setup(m => m.Map<List<EventoResponseDto>>(It.IsAny<List<Evento>>()))
                       .Returns(new List<EventoResponseDto> { new EventoResponseDto(), new EventoResponseDto() });

            // Act
            var result = await _service.GetAllEventosAsync(_testUserUid);

            // Assert
            Assert.Equal(2, result.Count);
        }

        // --- Testes para GetEventosByIdAsync ---

        [Fact]
        public async Task GetEventosByIdAsync_DeveRetornarEvento_QuandoExiste()
        {
            // Arrange
            var evento = _eventos.First();
            var expectedDto = new EventoResponseDto { Id = 1, TitleEvent = evento.TitleEvent };

            // O mock para FirstOrDefaultAsync deve ser configurado com o IQueryable
            // Por simplicidade, vamos usar o FindAsync ou Mockar o retorno do Include/FirstOrDefault
            _mockContext.Setup(c => c.Eventos.FindAsync(It.IsAny<object[]>())).ReturnsAsync(evento);
            _mockContext.Setup(c => c.Eventos.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, User>>>()).FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>(), default))
                        .ReturnsAsync(evento);
            _mockMapper.Setup(m => m.Map<EventoResponseDto>(evento)).Returns(expectedDto);

            // Act
            var result = await _service.GetEventosByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetEventosByIdAsync_DeveRetornarNull_QuandoNaoExiste()
        {
            // Arrange
            _mockContext.Setup(c => c.Eventos.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, User>>>()).FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>(), default))
                        .ReturnsAsync((Evento)null);

            // Act
            var result = await _service.GetEventosByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        // --- Testes para GetEventosCountByUserAsync ---

        [Fact]
        public async Task GetEventosCountByUserAsync_DeveRetornarContagemCorreta()
        {
            // Arrange
            const int expectedCount = 2; // Contagem esperada com base nos dados de _eventos

            // Simular o comportamento de CountAsync do Entity Framework Core
            // Nota: Em um ambiente de testes unitários puro, esta é uma simulação da chamada
            _mockContext.Setup(c => c.Eventos).Returns(_eventos.AsQueryable().AsDbSetMock().Object);

            // Act
            // O serviço usará o Linq to Objects na coleção mockada para o filtro
            var actualCount = await _service.GetEventosCountByUserAsync(_testUserUid);

            // Assert
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public async Task GetEventosCountByUserAsync_DeveRetornarZero_ParaUsuarioDesconhecido()
        {
            // Arrange
            var unknownUid = "unknown-user";

            // Act
            var actualCount = await _service.GetEventosCountByUserAsync(unknownUid);

            // Assert
            Assert.Equal(0, actualCount);
        }

        // --- Testes para CreateEventoAsync ---

        [Fact]
        public async Task CreateEventoAsync_DeveAdicionarEventoERetornarResponseDto()
        {
            // Arrange
            var createDto = new EventoResponseDto { TitleEvent = "Novo", DataEvento = DateTime.Now };
            var returnedEvento = new Evento { Id = 10, AuthorUid = _testUserUid, TitleEvent = "Novo" };
            var responseDto = new EventoResponseDto { Id = 10 };

            _mockMapper.Setup(m => m.Map<Evento>(createDto)).Returns(returnedEvento);
            _mockMapper.Setup(m => m.Map<EventoResponseDto>(returnedEvento)).Returns(responseDto);

            // Act
            var result = await _service.CreateEventoAsync(createDto, _testUserUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            _mockContext.Verify(c => c.Eventos.Add(It.Is<Evento>(e => e.AuthorUid == _testUserUid)), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        // --- Testes para UpdateEventoAsync ---

        [Fact]
        public async Task UpdateEventoAsync_DeveAtualizarERetornarDto_QuandoExiste()
        {
            // Arrange
            const int eventId = 1;
            var updateDto = new EventoResponseDto { TitleEvent = "Título Novo" };
            var existingEvento = _eventos.First(e => e.Id == eventId);
            var updatedDto = new EventoResponseDto { Id = eventId, TitleEvent = "Título Novo" };

            _mockContext.Setup(c => c.Eventos.FindAsync(It.IsAny<object[]>())).ReturnsAsync(existingEvento);
            _mockMapper.Setup(m => m.Map(updateDto, existingEvento)).Verifiable(); // Simula o mapeamento
            _mockMapper.Setup(m => m.Map<EventoResponseDto>(existingEvento)).Returns(updatedDto);

            // Act
            var result = await _service.UpdateEventoAsync(eventId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Título Novo", result.TitleEvent);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateEventoAsync_DeveRetornarNull_QuandoNaoExiste()
        {
            // Arrange
            const int eventId = 99;
            var updateDto = new EventoResponseDto { TitleEvent = "Título Novo" };

            _mockContext.Setup(c => c.Eventos.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Evento)null);

            // Act
            var result = await _service.UpdateEventoAsync(eventId, updateDto);

            // Assert
            Assert.Null(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        // --- Testes para DeleteEventoAsync ---

        [Fact]
        public async Task DeleteEventoAsync_DeveRetornarTrue_QuandoExisteEDeletado()
        {
            // Arrange
            const int eventId = 1;
            var existingEvento = _eventos.First(e => e.Id == eventId);

            _mockContext.Setup(c => c.Eventos.FindAsync(It.IsAny<object[]>())).ReturnsAsync(existingEvento);
            _mockContext.Setup(c => c.Eventos.Remove(existingEvento)).Verifiable();

            // Act
            var result = await _service.DeleteEventoAsync(eventId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(c => c.Eventos.Remove(existingEvento), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteEventoAsync_DeveRetornarFalse_QuandoNaoExiste()
        {
            // Arrange
            const int eventId = 99;

            _mockContext.Setup(c => c.Eventos.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Evento)null);

            // Act
            var result = await _service.DeleteEventoAsync(eventId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.Eventos.Remove(It.IsAny<Evento>()), Times.Never);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }
    }
}