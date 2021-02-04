using System.Linq;
using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    internal static class DropletMessage
    {
        public static string GetDropletInfoMessage(Droplet droplet)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine($"\U0001F4A7 *{droplet.Name}*");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"Id: *{droplet.Id.ToString()}*");
            stringBuilder.AppendLine($"Created at: *{droplet.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}*");
            stringBuilder.AppendLine($"Status: {GetStatusDroplet(droplet.Status)} *({droplet.Status})*");
            stringBuilder.AppendLine($"Image: *{droplet.Image.Distribution} {droplet.Image.Name}*");
            stringBuilder.AppendLine($"vCPUs: *{droplet.Vcpus.ToString()}*");
            stringBuilder.AppendLine($"Memory: *{droplet.Memory.ToString()}MB*");
            stringBuilder.AppendLine($"Disk: *{droplet.Disk.ToString()}GB*");
            stringBuilder.AppendLine($"Region: *{droplet.Region.Name}*");
            stringBuilder.AppendLine($"IPv4: *{string.Join(',', droplet.Networks.V4.Select(ip => ip.IpAddress))}*");
            stringBuilder.AppendLine($"IPv6: *{string.Join(',', droplet.Networks.V6.Select(ip => ip.IpAddress))}*");
            stringBuilder.AppendLine($"Tags: *{string.Join(',', droplet.Tags)}*");

            return stringBuilder.ToString();
        }

        public static string GetSelectedDropletMessage(Droplet droplet)
        {
            return $"Selected droplet: \U0001F4A7 *{droplet.Name}*";
        }
        
        public static string GetDropletsNotFoundMessage()
        {
            return "You don't have a droplets \U0001F914";
        }
        
        public static string GetEnterNameMessage()
        {
            return "Enter a name for the droplet";
        }
        
        public static string GetEnterSnapshotNameMessage()
        {
            return "Enter a name for the snapshot";
        }
        
        private static string GetStatusDroplet(string status) => status switch
        {
            "off" => "\U0001F534",
            "active" => "\U0001F7E2",
            "archive" => "\U0001F7E4",
            "new" => "\U000026AA",
            _ => "\U000026AB"
        };
    }
}