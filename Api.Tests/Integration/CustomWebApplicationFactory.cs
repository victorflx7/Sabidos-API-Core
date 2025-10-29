using AutoMapper; // Necessário para AutoMapper
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Models; // Adicione para ter acesso ao modelo Evento
using System;
using System.Linq;
using System.Reflection; // Necessário para AutoMapper
using Microsoft.Extensions.Logging;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // --- 1. Limpa DbContexts existentes ---
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove também o próprio AppDbContext se estiver registrado como Scoped/Transient
            var dbContextService = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
            if (dbContextService != null)
            {
                services.Remove(dbContextService);
            }

            // --- 2. Reconfigura o context como InMemory com NOME ÚNICO POR INSTÂNCIA DA FÁBRICA ---
            // Isso garante que cada classe de teste (IClassFixture) obtenha um DB isolado.
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");
            });

            // --- 3. Configuração do AutoMapper ---
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AppDbContext)));

            // --- 4. Configuração do Mock de Autenticação ---
            var authServices = services
                .Where(s => s.ServiceType.FullName?.Contains("Microsoft.AspNetCore.Authentication") == true)
                .ToList();

            foreach (var descriptor in authServices)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication("FakeScheme") // Define o esquema padrão aqui
                   .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("FakeScheme", options => { });


            // --- 5. Seeding do banco de dados ---
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                try
                {
                    // Garante que o banco está limpo e criado
                    // O EnsureDeleted pode falhar se o DB não existir, mas o EnsureCreated recria.
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Adiciona o Evento ID 1 para o teste de Delete
                    db.Eventos.Add(new Evento
                    {
                        Id = 1,
                        TitleEvent = "Evento para Teste de Autorização",
                        AuthorUid = "firebase-uid-outro-usuario",
                        DataEvento = DateTime.Now
                    });

                    // Adiciona o User associado ao Evento ID 1
                    db.Users.Add(new SabidosAPI_Core.Models.User
                    {
                        // Se o User usa ID numérico como PK, ele deve ser != 1 para evitar conflito com Evento ID 1.
                        // Se usa FirebaseUid como PK, não há problema. Assumindo FirebaseUid como PK:
                        FirebaseUid = "firebase-uid-outro-usuario",
                        Name = "Outro Usuário",
                        CreatedAt = DateTime.UtcNow
                    });

                    db.SaveChanges(); // Salva os dados de seeding
                }
                catch (Exception ex)
                {
                    // Logar o erro se o seeding falhar ajuda a depurar
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();
                    logger.LogError(ex, "Erro durante o seeding do banco de dados de teste.");
                    throw; // Re-lança a exceção para que o teste falhe claramente
                }
            }
        });
    }
}