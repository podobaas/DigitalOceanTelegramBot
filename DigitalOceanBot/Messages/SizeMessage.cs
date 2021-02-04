using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    public static class SizeMessage
    {
        public static string GetSizeInfoMessage(Size size)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine($"*\U0001F4BB {size.Slug}*");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"Memory: *{size.Memory.ToString()}MB*");
            stringBuilder.AppendLine($"vCPUs: *{size.Vcpus.ToString()}*");
            stringBuilder.AppendLine($"Disk: *{size.Disk.ToString()}MB*");
            stringBuilder.AppendLine($"Price per hour: *${size.PriceHourly.ToString("0.000")}*");
            stringBuilder.AppendLine($"Price per month: *${size.PriceMonthly.ToString()}*");

            return stringBuilder.ToString();
        }
        
        public static string GetSelectedSizeMessage(Size size)
        {
            return $"Selected size: \U0001F4BB *{size.Slug}*";
        }
    }
}