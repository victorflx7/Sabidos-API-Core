using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json; // Use System.Text.Json para consistência com ASP.NET Core
using Xunit;
using SabidosAPI_Core.DTOs;

namespace Api.Tests.Integration;

public class ResumoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string TestToken = "valid-test-token-for-resumo-user-1";
    private readonly string Endpoint = "/api/resumos";
    private readonly CustomWebApplicationFactory<Program> _factory; // Adicionado para gerenciar o client

    public ResumoControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        // Cria um cliente limpo para cada teste
        _client = _factory.CreateClient(); 
    }

    // --- Helpers ---

    private void SetAuthorizationHeader()
    {
        // Define o header de autorização para o cliente atual
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);
    }

    // ---------------------------------------------------------
    // Testes de Integração para GET /api/resumos
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        // Garante que o cabeçalho de autorização está nulo (cenário não autorizado)
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        // Esperado 401 Unauthorized (Com a correção no FakeJwtHandler, isso deve funcionar)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ComAutorizacao_DeveRetornar200Ok()
    {
        // Arrange
        // CORREÇÃO 1: Garante que o cabeçalho de autorização é DEFINIDO (cenário autorizado)
        SetAuthorizationHeader();

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        // CORREÇÃO 2: A asserção deve esperar OK (200), conforme o nome do teste
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ... (restante dos testes Create, Update, Delete)
    
    // ---------------------------------------------------------
    // Testes de Integração para POST /api/resumos
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        SetAuthorizationHeader();
        var createDto = new ResumoCreateUpdateDto { Titulo = "Resumo Teste", Conteudo = "Conteúdo teste" };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(createDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), 
            Encoding.UTF8, 
            "application/json"
        );

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resumo = JsonSerializer.Deserialize<ResumoResponseDto>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.True(resumo.Id > 0);
    }
    
    // ... (Os outros testes já estavam passando após a correção do DTO)
    // ---------------------------------------------------------
    // Testes de Integração para PUT /api/resumos/{id}
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ComIdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        SetAuthorizationHeader();
        int nonexistentId = 9999;
        var updateDto = new ResumoCreateUpdateDto { Titulo = "Atualizado", 
            Conteudo = "Conteúdo Válido do Resumo com mais de 8 caracteres"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updateDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), 
            Encoding.UTF8, 
            "application/json"
        );
        

        // Act
        var response = await _client.PutAsync($"{Endpoint}/{nonexistentId}", jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------
    // Testes de Integração para DELETE /api/resumos/{id}
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ComIdInexistente_DeveRetornar404NotFound()
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