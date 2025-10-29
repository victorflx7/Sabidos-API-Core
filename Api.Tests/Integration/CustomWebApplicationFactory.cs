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
using SabidosAPI_Core.Models; // Adicione para ter acesso ao modelo Evento e User
using Microsoft.Extensions.Logging; // Necessário para ILogger

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
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            var dbContextService = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
            if (dbContextService != null) services.Remove(dbContextService);

            // --- 2. Reconfigura o context como InMemory com NOME ÚNICO POR INSTÂNCIA ---
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");
            });

            // --- 3. Configuração do AutoMapper ---
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AppDbContext)));

            // --- 4. Configuração do Mock de Autenticação ---
            services.AddAuthentication("FakeScheme")
                   .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("FakeScheme", options => { });

            // --- 5. Seeding do banco de dados ---
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

                try
                {
                    logger.LogInformation("Iniciando seeding do banco de dados de teste...");
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // --- Seeding ---

                    // Usuário para teste de acesso não autorizado/outros cenários
                    var otherUserUid = "firebase-uid-outro-usuario";
                    if (!db.Users.Any(u => u.FirebaseUid == otherUserUid))
                    {
                        db.Users.Add(new SabidosAPI_Core.Models.User { FirebaseUid = otherUserUid, Name = "Outro Usuário", CreatedAt = DateTime.UtcNow });
                    }

                    // Evento ID 1 para teste de Delete sem Autorização
                    if (!db.Eventos.Any(e => e.Id == 1))
                    {
                        db.Eventos.Add(new Evento { Id = 1, TitleEvent = "Evento para Teste Delete", AuthorUid = otherUserUid, DataEvento = DateTime.Now });
                    }

                    // 🔑 CORREÇÃO CRÍTICA: Usuário autenticado (UID do FakeJwtHandler)
                    // O FakeJwtHandler usa "test-user-resumo-1"
                    var authenticatedUserUid = "test-user-resumo-1";
                    if (!db.Users.Any(u => u.FirebaseUid == authenticatedUserUid))
                    {
                        db.Users.Add(new SabidosAPI_Core.Models.User { FirebaseUid = authenticatedUserUid, Name = "Usuário Teste Auth", CreatedAt = DateTime.UtcNow });
                        logger.LogInformation($"Usuário de teste autenticado '{authenticatedUserUid}' adicionado.");
                    }
                    else
                    {
                        logger.LogInformation($"Usuário de teste autenticado '{authenticatedUserUid}' já existe.");
                    }

                    int changes = db.SaveChanges();
                    logger.LogInformation($"Seeding concluído. {changes} entidades salvas.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro durante o seeding do banco de dados de teste.");
                    throw;
                }
            }
        });
    }
}