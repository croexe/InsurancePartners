using Microsoft.AspNetCore.SignalR;
using Partners.Api.Hubs;
using Partners.Core.Contracts;

namespace Partners.Api.Notifications;

public class SignalRPartnerNotifier : IPartnerNotifier
{
    private readonly IHubContext<PartnerHub> _hubContext;

    public SignalRPartnerNotifier(IHubContext<PartnerHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyPartnerFlagChangedAsync(int partnerId, bool isFlagged)
    {
        return _hubContext.Clients.All.SendAsync("PartnerFlagChanged", new
        {
            partnerId,
            isFlagged
        });
    }
}
