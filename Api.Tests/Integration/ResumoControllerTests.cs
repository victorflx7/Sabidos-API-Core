using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Models;
using Xunit;

namespace Api.Tests.Integration
{
    
    public class ResumoControllerTests : IClassFixture<ResumoControllerTests.TestAppFactory>
    {
        private readonly TestAppFactory _factory;
        private const string ResumoEndpoint = "/api/resumo"; 

        public ResumoControllerTests(TestAppFactory factory) => _factory = factory;

        [Fact]
        public async Task GetResumo_ReturnsOk()
        {
            using var client = _factory.CreateClient();
            var response = await client.GetAsync(ResumoEndpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetResumo_RespostaContemDadosEsperados_QuandoDadosExistem()
        {
            using var client = _factory.CreateClient();
            var response = await client.GetAsync(ResumoEndpoint);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            Assert.True(doc.RootElement.ValueKind == JsonValueKind.Object || doc.RootElement.ValueKind == JsonValueKind.Array);
        }

        public class TestAppFactory : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
            {
                builder.ConfigureServices(services =>
                {
                    
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_Resumo");
                    });

                   
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();

                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    SeedTestData(db);
                });
            }

            private static void SeedTestData(AppDbContext db)
            {
                
                db.Eventos.AddRange(
                    new Evento { TitleEvent = "Evento A", DataEvento = DateTime.UtcNow.AddDays(-1), AuthorUid = "user1" },
                    new Evento { TitleEvent = "Evento B", DataEvento = DateTime.UtcNow, AuthorUid = "user2" },
                    new Evento { TitleEvent = "Evento C", DataEvento = DateTime.UtcNow, AuthorUid = "user1" }
                );
                db.SaveChanges();
            }
        }
    }
}
