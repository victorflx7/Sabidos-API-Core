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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1. Limpa DbContexts existentes e configura o In-Memory com NOME FIXO
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Usar um nome fixo (ex: "IntegrationTestDb") garante que EnsureDeleted funcione consistentemente.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));

            // 2. CORREÇÃO CRÍTICA: Configuração do AutoMapper (Resolve o 500 Internal Server Error)
            // Isso força o TestServer a carregar seus perfis de mapeamento.
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


            // 4. Seeding do banco de dados (Feito de forma idempotente)
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Garante que o banco está limpo e criado
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // 🔑 Adiciona o Evento com ID 1 APENAS se não existir (Idempotência)
                // O erro "Key: 1" desaparece com o nome fixo e o EnsureDeleted.
                if (!db.Eventos.Any(e => e.Id == 1))
                {
                    db.Eventos.Add(new Evento
                    {
                        Id = 1,
                        TitleEvent = "Evento para Teste de Autorização",
                        // UID diferente do TestToken para testar a autorização de exclusão.
                        AuthorUid = "firebase-uid-outro-usuario",
                        DataEvento = DateTime.Now
                    });

                    // Adiciona um User Profile para evitar que outros testes de User falhem no Upsert
                    db.UserProfiles.Add(new SabidosAPI_Core.Models.User
                    {
                        FirebaseUid = "firebase-uid-outro-usuario",
                        DisplayName = "Outro Usuário",
                        CreatedAt = DateTime.UtcNow
                    });

                    db.SaveChanges();
                }
            }
        });
    }
}