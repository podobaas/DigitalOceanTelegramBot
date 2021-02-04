using System.ComponentModel;
using DigitalOceanBot.Types.Const;

namespace DigitalOceanBot.Types.Enums
{
    internal enum BotCommandType
    {
        #region Main

        [Description(BotCommands.Start)]
        Start,
        
        [Description(BotCommands.Back)]
        Back,

        #endregion

        #region Account

        [Description(BotCommands.Account)]
        Account,

        #endregion

        #region Projects

        [Description(BotCommands.Projects)]
        Projects,

        [Description(BotCommands.SetAsDefaultProject)]
        SetAsDefaultProject,
        
        [Description(BotCommands.RenameProject)]
        RenameProject,
        
        [Description(BotCommands.ChangeDescriptionProject)]
        ChangeDescriptionProject,
        
        [Description(BotCommands.ChangePurposeProject)]
        ChangePurposeProject,
        
        [Description(BotCommands.ChangeEnvironmentProject)]
        ChangeEnvironmentProject,

        #endregion

        #region Droplet

        [Description(BotCommands.Droplets)]
        Droplets,
        
        [Description(BotCommands.DropletRename)]
        DropletRename,
        
        [Description(BotCommands.DropletReboot)]
        DropletReboot,
        
        [Description(BotCommands.DropletPowerCycle)]
        DropletPowerCycle,
        
        [Description(BotCommands.DropletShutdown)]
        DropletShutdown,
        
        [Description(BotCommands.DropletPowerOn)]
        DropletPowerOn,
        
        [Description(BotCommands.DropletResetPassword)]
        DropletResetPassword,
        
        [Description(BotCommands.DropletCreateSnapshot)]
        DropletCreateSnapshot,

        #endregion

        #region Firewall
        
        [Description(BotCommands.Firewalls)]
        Firewalls,
        
        [Description(BotCommands.FirewallAddInboundRule)]
        FirewallAddInboundRule,
        
        [Description(BotCommands.FirewallAddOutboundRule)]
        FirewallAddOutboundRule,
        
        [Description(BotCommands.FirewallAddDroplets)]
        FirewallAddDroplets,
        
        [Description(BotCommands.FirewallRemoveDroplets)]
        FirewallRemoveDroplets
        
        #endregion
    }
}