using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using Microsoft.EntityFrameworkCore;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 🔄 Garante que estamos sempre em ambiente "Testing"
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            // ⚠️ 🔑 REMOVE TODOS os serviços relacionados ao AppDbContext e suas opções.
            // Isso garante que o registro do SQL Server (feito no Program.cs) seja removido.
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

            // Cria banco limpo
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // A linha 31 do rastreamento de pilha (db.Database.EnsureDeleted())
            // agora deve funcionar corretamente após a remoção dos provedores conflitantes.
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }
}
