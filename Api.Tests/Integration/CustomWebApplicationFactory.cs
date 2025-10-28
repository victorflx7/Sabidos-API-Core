using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using AutoMapper; // ADICIONAR ESTE USING
using System.Reflection; // ADICIONAR ESTE USING
using SabidosAPI_Core.Models;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ⚠️ REMOÇÃO DE DbContext EXISTENTE (Mais seguro)
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // ⚙️ Reconfigura o contexto explicitamente como InMemory com NOME FIXO
            // Usar um nome fixo (ex: "IntegrationTestDb") garante que todos os testes 
            // no mesmo IClassFixture usem a mesma instância.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));

            // 🔑 CORREÇÃO CRÍTICA 2: Configuração do AutoMapper
            // Adicione a configuração do AutoMapper forçando o carregamento do seu perfil
            // Assumo que o seu perfil de mapeamento está na mesma assembly da AppDbContext ou do Program.
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AppDbContext)));


            // 🔑 Mock de Autenticação (Mantido, já funciona)
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


            // 🌟 Seeding do banco de dados
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // 🔑 Adiciona o Evento com ID 1
                if (!db.Eventos.Any(e => e.Id == 1))
                {
                    db.Eventos.Add(new Evento
                    {
                        Id = 1,
                        TitleEvent = "Evento para Teste de Autorização",
                        AuthorUid = "firebase-uid-outro-usuario",
                        DataEvento = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
        });
    }
}