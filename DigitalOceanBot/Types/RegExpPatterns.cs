namespace DigitalOceanBot.Types
{
    public static class RegExpPatterns
    {
        public const string NetworkAddress = @"(tcp|udp|icmp)[:](\d{1,5}|\d{1,5}[-]\d{1,5})[:](\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})[\/](\d{1,3})";
    }
}