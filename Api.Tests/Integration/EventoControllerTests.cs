using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SabidosAPI_Core.DTOs;
using Xunit;

namespace Api.Tests.Integration;

// IClassFixture<CustomWebApplicationFactory<Program>> deve ser o seu padrão de integração
public class EventoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    // Token de teste que simula um usuário autenticado (UID será extraído pelo Controller)
    private readonly string TestToken = "valid-test-token-for-evento-user-1";
    private readonly string Endpoint = "/api/eventos";

    public EventoControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // --- Helpers ---

    private void SetAuthorizationHeader()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);
    }

    // ---------------------------------------------------------
    // Testes de Integração para GET /api/eventos/count
    // ---------------------------------------------------------

    [Fact]
    public async Task GetEventosCountCountByUser_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync($"{Endpoint}/count");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEventosCountCountByUser_ComAutorizacao_DeveRetornar200Ok()
    {
        // Arrange
        SetAuthorizationHeader();

        // Act
        var response = await _client.GetAsync($"{Endpoint}/count");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // O corpo deve ser "0" (int) se o banco estiver limpo
    }

    // ---------------------------------------------------------
    // Testes de Integração para POST /api/eventos
    // ---------------------------------------------------------
    [Fact]
    public async Task CreateEvento_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        SetAuthorizationHeader();

        // 🔑 CORREÇÃO: Usar EventoCreateDto (o DTO de entrada correto)
        var createDto = new EventoCreateDto
        {
            TitleEvent = "Novo Evento via Teste",
            DataEvento = DateTime.Now,
            AuthorUid = "placeholder-uid" // Necessário para satisfazer a validação [Required] do DTO
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert (Linha 88, que estava falhando)
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verifica o Location Header ou o corpo da resposta
        var responseContent = await response.Content.ReadAsStringAsync();
        var evento = JsonConvert.DeserializeObject<EventoResponseDto>(responseContent);
        Assert.True(evento.Id > 0);
    }

    [Fact]
    public async Task CreateEvento_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var createDto = new EventoResponseDto { TitleEvent = "Evento Sem Auth" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------------------------------------------------
    // Testes de Integração para DELETE /api/eventos/{id}
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteEvento_ComIdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        SetAuthorizationHeader();
        int nonexistentId = 9999;

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/{nonexistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvento_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
