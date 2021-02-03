namespace DigitalOceanBot.Types.Enums
{
    public enum BotStateType
    {
        None,
        DropletCreateWaitingEnterName,
        DropletCreateWaitingEnterImage,
        DropletCreateWaitingEnterRegion,
        DropletUpdateWaitingEnterNewName,
        DropletUpdateWaitingEnterSnapshotName,
        FirewallUpdateWaitingEnterInboundRule,
        FirewallUpdateWaitingEnterOutboundRule,
        FirewallUpdateWaitingEnterAddDroplet,
        FirewallUpdateWaitingEnterRemoveDroplet,
        FirewallCreateWaitingEnterName,
        FirewallCreateWaitingEnterInboundRule,
        FirewallCreateWaitingEnterAddDroplet,
        ProjectUpdateWaitingEnterNewName,
        ProjectUpdateWaitingEnterNewDescription,
        ProjectUpdateWaitingEnterNewPurpose,
        ProjectUpdateWaitingEnterNewEnvironment,
        ProjectCreateWaitingEnterName,
        ProjectCreateWaitingEnterPurpose,
        ProjectCreateWaitingEnterDescription,
    }
}