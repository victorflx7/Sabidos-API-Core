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
    public class EventoControllerTests : IClassFixture<EventoControllerTests.TestAppFactory>
    {
        private readonly TestAppFactory _factory;
        private const string EventosEndpoint = "/api/eventos";

        public EventoControllerTests(TestAppFactory factory) => _factory = factory;

        [Fact]
        public async Task GetEventos_ReturnsOkAndContainsSeededItems()
        {
            using var client = _factory.CreateClient();
            var response = await client.GetAsync(EventosEndpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);
            Assert.True(doc.RootElement.GetArrayLength() >= 3); // seed adiciona 3 eventos
        }

        [Fact]
        public async Task PostEvento_AddsNewEvento_ThenCanBeRetrieved()
        {
            using var client = _factory.CreateClient();

            var novo = new Evento
            {
                TitleEvent = "Evento Teste",
                DataEvento = DateTime.UtcNow.AddDays(1),
                AuthorUid = "test-user"
            };

            var postResponse = await client.PostAsJsonAsync(EventosEndpoint, novo);
            Assert.True(postResponse.IsSuccessStatusCode, "POST deve retornar sucesso (201/200)");

            // Recupera lista e valida incremento
            var getResponse = await client.GetAsync(EventosEndpoint);
            getResponse.EnsureSuccessStatusCode();

            var stream = await getResponse.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);

            // Verifica se existe ao menos um item com TitleEvent == "Evento Teste"
            bool found = false;
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("titleEvent", out var titleProp) &&
                    titleProp.GetString() == "Evento Teste")
                {
                    found = true;
                    break;
                }
            }

            Assert.True(found, "Evento criado não foi encontrado na lista.");
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
                        options.UseInMemoryDatabase("TestDb_Evento");
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
