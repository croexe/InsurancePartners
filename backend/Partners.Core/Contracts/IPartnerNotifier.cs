namespace Partners.Core.Contracts;

public interface IPartnerNotifier
{
    Task NotifyPartnerFlagChangedAsync(int partnerId, bool isFlagged);
}
