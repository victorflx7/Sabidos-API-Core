using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using System.Linq;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove o DbContext configurado no ambiente de produ��o (SQL Server)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));


            if (descriptor != null)
                services.Remove(descriptor);

            // Adiciona um novo contexto InMemory, com nome aleat�rio para evitar conflitos entre testes
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"InMemoryDbForTesting_{Guid.NewGuid()}");
            });

            // Adiciona autentica��o fake
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("TestScheme", options => { });


            // Constr�i o provedor e inicializa o banco em mem�ria
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
