using AzureSnapshotManager.Infrastructure.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureSnapshotManager.Api.Controllers;

/// <summary>
/// Lets the dashboard ask "which subscriptions can I actually see?" instead
/// of requiring the user to paste an ID — used for the offline desktop
/// exe's auto-detect-and-run flow, but works the same way in any mode since
/// it just reflects whatever the active credential can access.
/// </summary>
[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly AzureAccountService _accountService;

    public AccountController(AzureAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptions(CancellationToken ct)
    {
        try
        {
            var subscriptions = await _accountService.ListAccessibleSubscriptionsAsync(ct);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            // Most common cause: no `az login` session (or it expired).
            return StatusCode(502, new
            {
                error = "Could not list Azure subscriptions. Make sure you've run 'az login' and try again.",
                detail = ex.Message
            });
        }
    }
}
