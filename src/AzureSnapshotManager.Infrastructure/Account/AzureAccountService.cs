using AzureSnapshotManager.Infrastructure.Security;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace AzureSnapshotManager.Infrastructure.Account;

public record AccessibleSubscription(string SubscriptionId, string DisplayName);

/// <summary>
/// Lets the dashboard auto-detect which subscription(s) the active
/// credential can see, instead of requiring the user to paste a
/// subscription ID by hand. In offline desktop mode that credential is
/// whatever account the user last ran `az login` with — same mechanism as
/// <see cref="ManagedIdentityAuthProvider"/> already uses everywhere else.
/// </summary>
public class AzureAccountService
{
    private readonly ArmClient _armClient;
    private readonly ILogger<AzureAccountService> _logger;

    public AzureAccountService(ManagedIdentityAuthProvider authProvider, ILogger<AzureAccountService> logger)
    {
        _armClient = new ArmClient(authProvider.GetCredential());
        _logger = logger;
    }

    public async Task<List<AccessibleSubscription>> ListAccessibleSubscriptionsAsync(CancellationToken ct = default)
    {
        var results = new List<AccessibleSubscription>();

        await foreach (var subscription in _armClient.GetSubscriptions().GetAllAsync(ct))
        {
            results.Add(new AccessibleSubscription(
                subscription.Data.SubscriptionId,
                subscription.Data.DisplayName ?? subscription.Data.SubscriptionId));
        }

        _logger.LogInformation("Detected {Count} accessible subscription(s) for the active credential", results.Count);
        return results;
    }
}
