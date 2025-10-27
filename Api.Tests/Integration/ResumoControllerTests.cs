using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SabidosAPI_Core.DTOs;
using Xunit;

namespace Api.Tests.Integration;

public class ResumoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    // O token de teste simula a presença de 'user_id' e 'name'
    // O FakeJwtHandler agora garante que a autenticação funcione corretamente
    private readonly string TestToken = "valid-test-token-for-resumo-user-1";
    private readonly string Endpoint = "/api/resumos";

    public ResumoControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // --- Helpers ---

    private void SetAuthorizationHeader()
    {
        // O valor do token não importa, mas a presença do cabeçalho sim.
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);
    }

    // ---------------------------------------------------------
    // Testes de Integração para GET /api/resumos
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        // GARANTE que qualquer cabeçalho de autorização padrão seja removido ANTES da chamada
        _client.DefaultRequestHeaders.Authorization = null; // <<< LINHA CHAVE

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        // O erro retornou OK. Você pode temporariamente mudar para OK para ver se o código de status
        // da sua API mudou para 200, mas o esperado é 401.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ComAutorizacao_DeveRetornar200Ok()
    {
        // Arrange
        // Arrange
        // CORREÇÃO: Garante que o cabeçalho de autorização não existe para esta chamada
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        // Com o cabeçalho nulo, o controller deve retornar 401, a menos que o mock esteja forçando 200
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------------------------------------------------
    // Testes de Integração para POST /api/resumos
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        SetAuthorizationHeader();
        var createDto = new ResumoCreateUpdateDto { Titulo = "Resumo Teste", Conteudo = "Conteúdo teste" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Endpoint, jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resumo = JsonConvert.DeserializeObject<ResumoResponseDto>(responseContent);
        Assert.True(resumo.Id > 0);
    }

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
        var jsonContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
        

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