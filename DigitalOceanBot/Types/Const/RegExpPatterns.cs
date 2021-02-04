namespace DigitalOceanBot.Types
{
    internal static class RegExpPatterns
    {
        public const string NetworkAddress = @"(tcp|udp|icmp)[:](\d{1,5}|\d{1,5}[-]\d{1,5})[:](\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})[\/](\d{1,3})";
        public const string NumbersSeparatedByCommas = @"^\d+(?:,\d+)*$";
        public const string NumbersSeparatedByDash = @"^\d+(?:-\d+)*$";
    }
}