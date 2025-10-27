using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

public class FakeJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FakeJwtHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    // Seu arquivo: FakeJwtHandler.cs

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // CORREÇÃO CRÍTICA: Se o header 'Authorization' estiver ausente, FALHA!
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            // Se a autenticação falhar, o pipeline do ASP.NET Core retorna 401 Unauthorized
            return Task.FromResult(AuthenticateResult.Fail("Header de Autorização ausente."));
        }

        // Se o header estiver presente, injeta o usuário de teste
        var claims = new[]
        {
        // Certifique-se de incluir todos os claims que seus controllers usam
        new Claim("user_id", "test-user-resumo-1"),
        new Claim(ClaimTypes.NameIdentifier, "test-user-resumo-1"),
        new Claim(ClaimTypes.Email, "teste@sabidos.com"),
        new Claim(ClaimTypes.Name, "Usuário Teste"),
        new Claim("sub", "test-user-resumo-1")
    };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Retorna sucesso (o teste autorizado passará)
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
