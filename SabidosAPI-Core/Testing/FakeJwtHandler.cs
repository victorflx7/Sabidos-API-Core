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
        // Cria um usuário de teste sempre autenticado
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test_user_id"),
            new Claim(ClaimTypes.Email, "teste@sabidos.com"),
            new Claim(ClaimTypes.Name, "Usuário Teste")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
