using System.Security.Cryptography;
using System.Text;

namespace AzureSnapshotManager.Api.Development;

/// <summary>
/// DEMO MODE ONLY. When the app runs publicly with DemoMode=true (dev auth
/// handler + in-memory storage), this puts an HTTP Basic username/password
/// prompt in front of EVERYTHING (dashboard, API, Swagger) so the open dev
/// auth handler is never reachable by anonymous visitors.
///
/// Credentials come from the DEMO_USER / DEMO_PASSWORD settings. The
/// browser shows its native login dialog once and then re-sends the
/// credentials on every same-origin request (fetch, CSV download, etc.),
/// so the dashboard needs no code changes.
///
/// /healthz is exempt so the hosting platform's health checks still work.
/// </summary>
public class DemoBasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly byte[] _expectedUser;
    private readonly byte[] _expectedPassword;

    public DemoBasicAuthMiddleware(RequestDelegate next, string user, string password)
    {
        _next = next;
        _expectedUser = Encoding.UTF8.GetBytes(user);
        _expectedPassword = Encoding.UTF8.GetBytes(password);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/healthz"))
        {
            await _next(context);
            return;
        }

        if (IsAuthorized(context.Request.Headers.Authorization.ToString()))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Azure Snapshot Manager demo\"";
    }

    private bool IsAuthorized(string header)
    {
        if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..]));
            var separator = decoded.IndexOf(':');
            if (separator < 0) return false;

            var user = Encoding.UTF8.GetBytes(decoded[..separator]);
            var password = Encoding.UTF8.GetBytes(decoded[(separator + 1)..]);

            // Constant-time comparison so response timing can't leak the password.
            var userOk = CryptographicOperations.FixedTimeEquals(user, _expectedUser);
            var passwordOk = CryptographicOperations.FixedTimeEquals(password, _expectedPassword);
            return userOk & passwordOk;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
