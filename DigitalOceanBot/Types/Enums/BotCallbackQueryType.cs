namespace DigitalOceanBot.Types.Enums
{
    internal enum BotCallbackQueryType
    {
        None,

        #region Droplet

        DropletCreateNew,
        DropletPrevious,
        DropletNext,
        DropletSelect,

        #endregion

        #region Firewall

        FirewallPrevious,
        FirewallNext,
        FirewallSelect,
        FirewallCreateNew,

        #endregion

        #region Project

        ProjectPrevious,
        ProjectNext,
        ProjectSelect,
        ProjectCreateNew,

        #endregion

        #region Image

        ImagePrevious,
        ImageNext,
        ImageSelect,

        #endregion

        #region Region

        RegionPrevious,
        RegionNext,
        RegionSelect,

        #endregion

        #region Size

        SizePrevious,
        SizeNext,
        SizeSelect

        #endregion
    }
}