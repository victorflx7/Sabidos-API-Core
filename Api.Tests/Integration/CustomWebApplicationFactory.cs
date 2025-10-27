using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication; // Novo using necessário
using Microsoft.Extensions.Options; // Novo using necessário
using System.Linq;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 🔄 Garante que estamos sempre em ambiente "Testing"
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            // ⚠️ 🔑 REMOVE TODOS os serviços relacionados ao AppDbContext e suas opções.
            var dbContextServices = services
                .Where(d => d.ServiceType == typeof(AppDbContext) ||
                            d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var descriptor in dbContextServices)
            {
                services.Remove(descriptor);
            }

            // ⚙️ Reconfigura o contexto explicitamente como InMemory (isolado)
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));


            // 🔑 CORREÇÃO CRÍTICA: Configuração da Autenticação para Testes
            // 1. Remove toda a configuração de autenticação existente (Firebase JWT)
            var authServices = services
                .Where(s => s.ServiceType.FullName?.Contains("Microsoft.AspNetCore.Authentication") == true)
                .ToList();

            foreach (var descriptor in authServices)
            {
                services.Remove(descriptor);
            }

            // 2. Adiciona o esquema Fake como o esquema padrão (Default)
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "FakeScheme";
                options.DefaultChallengeScheme = "FakeScheme";
                options.DefaultForbidScheme = "FakeScheme";
            })
            // Registra o FakeJwtHandler
            .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("FakeScheme", options => { });


            // Cria banco limpo
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }
}