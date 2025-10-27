using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Linq;
using System; // Adicionado para Guid

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Define o ambiente para "Testing"
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ⚠️ 🔑 CORREÇÃO CRÍTICA: REMOVE TODOS os serviços relacionados ao AppDbContext e suas opções.
            // Esta remoção mais abrangente impede o erro de múltiplos provedores.
            var dbContextServices = services
                .Where(d => d.ServiceType == typeof(AppDbContext) ||
                            d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            d.ServiceType.FullName.Contains("IHostedService") // Remove possíveis serviços relacionados a migrações
                            )
                .ToList();

            foreach (var descriptor in dbContextServices)
            {
                services.Remove(descriptor);
            }

            // ⚙️ Reconfigura o contexto explicitamente como InMemory (isolado)
            // Usa um nome único para o banco de dados para isolar os testes
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


            // Cria banco limpo e garante que as operações de EnsureDeleted e EnsureCreated
            // ocorram DENTRO DO ESCOPO de serviço correto.
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope()) // Usa o bloco 'using' para garantir o descarte
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // A linha 64 (db.Database.EnsureDeleted()) agora deve funcionar sem o erro
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // PONTO IMPORTANTE: Aqui é onde você popularia dados iniciais (se necessário)
            }
            ;
        });
    }
}