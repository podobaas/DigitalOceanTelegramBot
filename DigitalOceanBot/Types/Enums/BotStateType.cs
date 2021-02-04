namespace DigitalOceanBot.Types.Enums
{
    public enum BotStateType
    {
        None,

        #region Droplet

        DropletCreateWaitingEnterName,
        DropletCreateWaitingEnterImage,
        DropletCreateWaitingEnterRegion,
        DropletCreateWaitingEnterSize,
        DropletUpdateWaitingEnterNewName,
        DropletUpdateWaitingEnterSnapshotName,

        #endregion

        #region Firewall

        FirewallUpdateWaitingEnterInboundRule,
        FirewallUpdateWaitingEnterOutboundRule,
        FirewallUpdateWaitingEnterAddDroplet,
        FirewallUpdateWaitingEnterRemoveDroplet,
        FirewallCreateWaitingEnterName,
        FirewallCreateWaitingEnterInboundRule,
        FirewallCreateWaitingEnterAddDroplet,

        #endregion

        #region Project

        ProjectUpdateWaitingEnterNewName,
        ProjectUpdateWaitingEnterNewDescription,
        ProjectUpdateWaitingEnterNewPurpose,
        ProjectUpdateWaitingEnterNewEnvironment,
        ProjectCreateWaitingEnterName,
        ProjectCreateWaitingEnterPurpose,
        ProjectCreateWaitingEnterDescription,

        #endregion
    }
}