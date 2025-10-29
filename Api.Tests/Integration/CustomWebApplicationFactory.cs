using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System;
using System.Reflection; // Necessário para AutoMapper
using AutoMapper; // Necessário para AutoMapper
using SabidosAPI_Core.Models; // Adicione para ter acesso ao modelo Evento

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // 🔑 CORREÇÃO CRÍTICA: Gera um nome de DB único por instância de IClassFixture.
    // Isso garante que EnsureDeleted e EnsureCreated funcionem de forma isolada e resolve o erro "Key: 1 already added".
    private static string UniqueDbName { get; } = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1. Limpa DbContexts existentes
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // ⚙️ Reconfigura o contexto explicitamente como InMemory com NOME ÚNICO POR FIXTURE
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(UniqueDbName));

            // 2. Configuração do AutoMapper (Resolve o 500 Internal Server Error)
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AppDbContext)));

            // 3. Configuração do Mock de Autenticação (Mantido, para o FakeJwtHandler)
            var authServices = services
                .Where(s => s.ServiceType.FullName?.Contains("Microsoft.AspNetCore.Authentication") == true)
                .ToList();

            foreach (var descriptor in authServices)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "FakeScheme";
                options.DefaultChallengeScheme = "FakeScheme";
                options.DefaultForbidScheme = "FakeScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("FakeScheme", options => { });


            // 4. Seeding do banco de dados (Com todas as correções de nome de propriedade/DbSet)
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Garante que o banco está limpo e criado de forma isolada
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // Seeding: Adiciona os dados necessários
                if (!db.Eventos.Any(e => e.Id == 1))
                {
                    db.Eventos.Add(new Evento
                    {
                        Id = 1,
                        TitleEvent = "Evento para Teste de Autorização",
                        AuthorUid = "firebase-uid-outro-usuario",
                        DataEvento = DateTime.Now
                    });

                    // CORREÇÃO: Usando db.Users e propriedade Name
                    db.Users.Add(new SabidosAPI_Core.Models.User
                    {
                        FirebaseUid = "firebase-uid-outro-usuario",
                        Name = "Outro Usuário",
                        CreatedAt = DateTime.UtcNow
                    });

                    db.SaveChanges();
                }
            }
        });
    }
}