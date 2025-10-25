using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SabidosAPI_Core.DTOs;
using Xunit;

namespace Api.Tests.Integration;

public class FlashcardControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    // O token de teste deve simular a presença de um 'user_id' ou 'sub' para o Controller
    private readonly string TestToken = "valid-test-token-for-user-1";
    private readonly string Endpoint = "/api/flashcard";

    public FlashcardControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // --- Helpers ---

    private void SetAuthorizationHeader()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);
    }

    // ---------------------------------------------------------
    // Testes de Integração para GET /api/flashcard/count
    // ---------------------------------------------------------

    [Fact]
    public async Task GetFlashcardsCountCountByUser_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync($"{Endpoint}/count");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFlashcardsCountCountByUser_ComAutorizacao_DeveRetornar200Ok()
    {
        // Arrange
        SetAuthorizationHeader();

        // Act
        var response = await _client.GetAsync($"{Endpoint}/count");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Opcional: Assert.Equal("0", await response.Content.ReadAsStringAsync());
    }

    // ---------------------------------------------------------
    // Testes de Integração para POST /api/flashcard
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateFlashcard_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        SetAuthorizationHeader();
        var createDto = new FlashcardCreateUpdateDto { Frente = "Qual é o DTO?", Verso = "FlashcardResponseDto" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verifica o Location Header ou o corpo da resposta
        var responseContent = await response.Content.ReadAsStringAsync();
        var flashcard = JsonConvert.DeserializeObject<FlashcardResponseDto>(responseContent);
        Assert.True(flashcard.Id > 0);
    }

    [Fact]
    public async Task CreateFlashcard_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var createDto = new FlashcardCreateUpdateDto { Frente = "Qual é o DTO?", Verso = "FlashcardResponseDto" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------------------------------------------------
    // Testes de Integração para DELETE /api/flashcard/{id}
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteFlashcard_ComIdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        SetAuthorizationHeader();
        int nonexistentId = 9999;

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/{nonexistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
