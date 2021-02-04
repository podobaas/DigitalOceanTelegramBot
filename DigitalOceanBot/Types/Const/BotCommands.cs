namespace DigitalOceanBot.Types.Const
{
    internal static class BotCommands
    {
        public const string Start = "/start";
        public const string Back = "Back";
        
        public const string Account = "🌊 Account";
        
        public const string Projects = "📝 Projects";
        public const string SetAsDefaultProject = "Set as default";
        public const string RenameProject = "Rename project";
        public const string ChangeDescriptionProject = "Change description";
        public const string ChangePurposeProject = "Change purpose";
        public const string ChangeEnvironmentProject = "Change environment";

        public const string Droplets = "💧 Droplets";
        public const string DropletRename = "Rename droplet";
        public const string DropletReboot = "Reboot droplet";
        public const string DropletPowerCycle = "Power cycle droplet";
        public const string DropletShutdown = "Shutdown droplet";
        public const string DropletPowerOn = "Power on droplet";
        public const string DropletResetPassword = "Reset password";
        public const string DropletCreateSnapshot = "Create snapshot";
        
        public const string Firewalls = "🏰 Firewalls";
        public const string FirewallAddInboundRule = "Add inbound rule";
        public const string FirewallAddOutboundRule = "Add outbound rule";
        public const string FirewallAddDroplets = "Add droplets to firewall";
        public const string FirewallRemoveDroplets = "Remove droplets from firewall";
    }
}