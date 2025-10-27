using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using SabidosAPI_Core.DTOs;
using Xunit;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UserControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    //---------------------------------------------------------
    // Testes de Integração para GET /api/user/me
    //---------------------------------------------------------

    [Fact]
    public async Task GetMe_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/user/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ComAutorizacao_DeveRetornar200Ok()
    {
        // Simulação de um token JWT com as claims necessárias
        var token = "seu-token-de-teste-aqui";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/user/me");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    //---------------------------------------------------------
    // Testes de Integração para POST /api/user/profile
    //---------------------------------------------------------

    [Fact]
    public async Task UpsertProfile_SemAutorizacao_DeveRetornar401Unauthorized()
    {
        var dto = new UserUpdateDto { Name = "Test" };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsync("/api/user/profile", jsonContent);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpsertProfile_ComDtoInvalido_DeveRetornar400BadRequest()
    {
        // Nome com mais de 160 caracteres
        var invalidName = new string('a', 161);
        var dto = new UserUpdateDto { Name = invalidName };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var token = "seu-token-de-teste-aqui";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/user/profile", jsonContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpsertProfile_ComUsuarioValido_DeveRetornar200Ok()
    {
        var newName = "New Test Name";
        var token = "seu-token-de-teste-aqui";
        var updateDto = new UserUpdateDto { Name = newName };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/user/profile", jsonContent);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}