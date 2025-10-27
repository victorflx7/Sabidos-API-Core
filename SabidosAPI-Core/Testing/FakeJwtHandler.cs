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

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. CORREÇÃO CRÍTICA: Verifica a presença do cabeçalho de autorização.
        // Se o teste é 'SemAutorizacao', ele terá Authorization=null.
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            // Se o header estiver ausente, a autenticação falha, resultando em 401 Unauthorized.
            return Task.FromResult(AuthenticateResult.Fail("Header de Autorização ausente."));
        }

        // 2. Cria um usuário de teste (se o header existir, simulando um token válido)
        var claims = new[]
        {
        // O `user_id` é o que seu controller espera para o service
        new Claim("user_id", "test-user-resumo-1"),

        new Claim(ClaimTypes.NameIdentifier, "test-user-resumo-1"),
        new Claim(ClaimTypes.Email, "teste@sabidos.com"),
        new Claim(ClaimTypes.Name, "Usuário Teste"),
        new Claim("sub", "test-user-resumo-1") // Garante que 'sub' também está presente
    };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Retorna sucesso (200 OK para endpoints autorizados)
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
