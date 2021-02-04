using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    public static class RegionMessage
    {
        public static string GetRegionInfoMessage(Region region)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine($"*{region.Name}* {GetFlag(region.Slug)}");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"Slug: *{region.Slug}*");
            stringBuilder.AppendLine($"Available: *{region.Available}*");

            return stringBuilder.ToString();
        }
        
        public static string GetSelectedRegionMessage(Region region)
        {
            return $"Selected image: *{GetFlag(region.Slug)} {region.Name} ({region.Slug})*";
        }
        
        private static string GetFlag(string slug)
        {
            switch (slug)
            {
                case "nyc1":
                case "nyc2":
                case "nyc3":
                case "sfo1":
                case "sfo2":
                case "sfo3":
                    return "\U0001F1FA\U0001F1F8";
                case "ams1":
                case "ams2":
                case "ams3":
                    return "\U0001F1F3\U0001F1F1";
                case "sgp1":
                case "sgp2":
                case "sgp3":
                    return "\U0001F1F8\U0001F1EC";
                case "lon1":
                case "lon2":
                case "lon3":
                    return "\U0001F1EC\U0001F1E7";
                case "fra1":
                case "fra2":
                case "fra3":
                    return "\U0001F1E9\U0001F1EA";
                case "tor1":
                case "tor2":
                case "tor3":
                    return "\U0001F1E8\U0001F1E6";
                case "blr1":
                case "blr2":
                case "blr3":
                    return "\U0001F1EE\U0001F1F3";
                default:
                    return "\U0001F6A9";
            }
        }
    }
}