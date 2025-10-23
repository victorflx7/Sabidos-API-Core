using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SabidosAPI_Core.Dtos;
using Xunit;

namespace Api.Tests.Integration;

public class PomodoroControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string TestToken = "valid-test-token-uid-123";
    private readonly string Endpoint = "/api/pomodoro";

    public PomodoroControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        // O Factory cria o cliente HTTP, injetando as dependências de teste (Db em Memória)
        _client = factory.CreateClient();
    }
    
    // --- Helpers ---

    private void SetAuthorizationHeader()
    {
        // Simula o cabeçalho de autorização que contém o UID (para ser capturado pelo Controller)
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);
    }
    
    // ---------------------------------------------------------
    // Testes de Integração para GET /api/pomodoro
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ComAutorizacao_DeveRetornar200Ok()
    {
        // Arrange
        SetAuthorizationHeader();

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.EnsureSuccessStatusCode(); 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Não verificamos o conteúdo, apenas a resposta bem-sucedida do endpoint.
    }
    
    // ---------------------------------------------------------
    // Testes de Integração para POST /api/pomodoro
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        SetAuthorizationHeader();
        var createDto = new PomoCreateDto { Duration = 25, Description = "Tarefa com foco" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        response.EnsureSuccessStatusCode(); 
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Opcional: Verificar se o item foi realmente adicionado (não faremos aqui para simplicidade)
    }

    [Fact]
    public async Task Create_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var createDto = new PomoCreateDto { Duration = 25, Description = "Tarefa com foco" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}