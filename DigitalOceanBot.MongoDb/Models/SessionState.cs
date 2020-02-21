namespace DigitalOceanBot.MongoDb.Models
{
    public enum SessionState
    {
        Unknow = 0,
        WaitAuth = 1,
        MainMenu = 2,
        DropletsMenu = 3,
        SelectedDroplet = 4,
        WaitInputNewNameDroplet = 5,
        WaitAction = 6,
        WaitConfirmReboot = 7,
        WaitConfirmPowerCycle = 8,
        WaitConfirmShutdown = 9,
        WaitInputSnapshotName = 10,
        WaitInputNameDroplet = 11,
        WaitChooseImageDroplet = 12,
        WaitChooseRegionDroplet = 13,
        WaitChooseSizeDroplet = 14,
        WaitConfirmResetPassword = 15,
        WaitConfirmDeleteDroplet = 16,
        FirewallsMenu = 17,
        SelectedFirewall = 18,
        WaitInputNameFirewall = 19,
        WaitInputInboundFirewallRule = 20,
        WaitInputOutboundFirewallRule = 21,
        WaitChooseDropletsForFirewall = 22,
        WaitInputAddInboundRuleFirewall = 23,
        WaitInputAddOutboundRuleFirewall = 24,
        ProjectsMenu = 25,
        SelectedProject = 26
    }
}
