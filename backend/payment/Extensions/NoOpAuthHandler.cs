using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace payment.Extensions;

/// <summary>
/// No-op authentication handler used when Auth:Enabled=false.
/// Passes all requests through with an anonymous identity so that
/// [Authorize] attributes on controllers don't crash with "No authenticationScheme" errors.
/// Authorization policies are registered as pass-through (RequireAssertion(_ => true))
/// so all endpoints are accessible.
/// </summary>
public class NoOpAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "NoOp";

    public NoOpAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "anonymous"),
            new Claim("sub", Guid.Empty.ToString()),
        }, SchemeName);

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
