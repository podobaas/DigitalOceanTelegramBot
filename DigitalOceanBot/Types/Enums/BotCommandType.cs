using System.ComponentModel;

namespace DigitalOceanBot.Types.Enums
{
    internal enum BotCommandType
    {
        #region Main

        [Description(CommandConst.Start)]
        Start,
        
        [Description(CommandConst.Back)]
        Back,

        #endregion

        #region Account

        [Description(CommandConst.Account)]
        Account,

        #endregion

        #region Projects

        [Description(CommandConst.Projects)]
        Projects,
        
        [Description(CommandConst.CreateNewProject)]
        CreateNewProject,
        
        [Description(CommandConst.SetAsDefaultProject)]
        SetAsDefaultProject,
        
        [Description(CommandConst.RenameProject)]
        RenameProject,
        
        [Description(CommandConst.ChangeDescriptionProject)]
        ChangeDescriptionProject,
        
        [Description(CommandConst.ChangePurposeProject)]
        ChangePurposeProject,
        
        [Description(CommandConst.ChangeEnvironmentProject)]
        ChangeEnvironmentProject,

        #endregion

        #region Droplet

        [Description(CommandConst.Droplets)]
        Droplets,
        
        [Description(CommandConst.DropletCreateNew)]
        DropletCreateNew,
        
        [Description(CommandConst.DropletRename)]
        DropletRename,
        
        [Description(CommandConst.DropletReboot)]
        DropletReboot,
        
        [Description(CommandConst.DropletPowerCycle)]
        DropletPowerCycle,
        
        [Description(CommandConst.DropletShutdown)]
        DropletShutdown,
        
        [Description(CommandConst.DropletPowerOn)]
        DropletPowerOn,
        
        [Description(CommandConst.DropletResetPassword)]
        DropletResetPassword,
        
        [Description(CommandConst.DropletCreateSnapshot)]
        DropletCreateSnapshot,

        #endregion

        #region Firewall
        
        [Description(CommandConst.Firewalls)]
        Firewalls,
        
        [Description(CommandConst.FirewallCreateNew)]
        FirewallCreateNew,
        
        [Description(CommandConst.FirewallAddInboundRule)]
        FirewallAddInboundRule,
        
        [Description(CommandConst.FirewallAddOutboundRule)]
        FirewallAddOutboundRule,
        
        [Description(CommandConst.FirewallAddDroplets)]
        FirewallAddDroplets,
        
        [Description(CommandConst.FirewallRemoveDroplets)]
        FirewallRemoveDroplets
        
        #endregion
    }
}