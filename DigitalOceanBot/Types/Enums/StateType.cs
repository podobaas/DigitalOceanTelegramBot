namespace DigitalOceanBot.Types.Enums
{
    public enum StateType
    {
        None,
        DropletWaitEnterNewName,
        DropletWaitEnterSnapshotName,
        FirewallWaitEnterInboundRule,
        FirewallWaitEnterOutboundRule,
        FirewallWaitEnterCreationData
    }
}