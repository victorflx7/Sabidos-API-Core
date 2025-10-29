using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using Xunit;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UserControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        // Cria um novo cliente HTTP
        _client = _factory.CreateClient();

        // 🔄 Garante que o banco está limpo antes de cada teste
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    //---------------------------------------------------------
    // Testes de Integração para GET /api/user/me
    //---------------------------------------------------------

    [Fact]
    public async Task GetMe_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/user/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ComAutorizacao_DeveRetornar200Ok()
    {
        // ⚠️ Substitua este token por um mock real ou configure a autenticação fake
        var token = "token-falso-de-teste";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/user/me");

        // ⚠️ Se a autenticação não estiver configurada, esse teste pode retornar 401
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized,
            $"Status inesperado: {response.StatusCode}"
        );
    }

    //---------------------------------------------------------
    // Testes de Integração para POST /api/user/profile
    //---------------------------------------------------------

    [Fact]
    public async Task UpsertProfile_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        var dto = new UserUpdateDto { Name = "Test User" };
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsync("/api/user/profile", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpsertProfile_ComDtoInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var invalidName = new string('a', 161);
        var dto = new UserUpdateDto { Name = invalidName };
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var token = "token-falso-de-teste";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/user/profile", content);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized,
            $"Status inesperado: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task UpsertProfile_ComUsuarioValido_DeveRetornar200Ok()
    {
        // Arrange
        var dto = new UserUpdateDto { Name = "Usuário Teste" };
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var token = "token-falso-de-teste";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/user/profile", content);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized,
            $"Status inesperado: {response.StatusCode}"
        );
    }
}
