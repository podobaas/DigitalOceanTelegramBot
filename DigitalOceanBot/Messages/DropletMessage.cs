using System.Linq;
using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    public static class DropletMessage
    {
        public static string GetDropletInfoMessage(Droplet droplet)
        {
            var stringBuilder = new StringBuilder(string.Empty);

            stringBuilder.Append($"\U0001F4A7 *{droplet.Name} (Created at UTC {droplet.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")})*\n\n");
            stringBuilder.Append($"Status: {GetStatusDroplet(droplet.Status)} *({droplet.Status})*\n");
            stringBuilder.Append($"Image: *{droplet.Image.Distribution} {droplet.Image.Name}*\n");
            stringBuilder.Append($"vCPUs: *{droplet.Vcpus.ToString()}*\n");
            stringBuilder.Append($"Memory: *{droplet.Memory.ToString()}MB*\n");
            stringBuilder.Append($"Disk: *{droplet.Disk.ToString()}GB*\n");
            stringBuilder.Append($"Region: *{droplet.Region.Name}*\n");
            stringBuilder.Append($"IPv4: *{string.Join(',', droplet.Networks.V4.Select(ip => ip.IpAddress))}*\n");
            stringBuilder.Append($"IPv6: *{string.Join(',', droplet.Networks.V6.Select(ip => ip.IpAddress))}*\n");
            stringBuilder.Append($"Tags: *{string.Join(',', droplet.Tags)}*\n");

            return stringBuilder.ToString();
        }

        public static string GetSelectedDropletMessage(Droplet droplet)
        {
            return $"\U0001F4A7 Selected droplet: *{droplet.Name}*";
        }

        public static string GetLoadingDropletsMessage()
        {
            return "\U0001F4C0 Loading your droplets...";
        }

        public static string GetDropletsNotFoundMessage()
        {
            return "You don't have a droplets \U0001F914";
        }
        
        public static string GetEnterNewNameMessage()
        {
            return "Enter a new name for the droplet";
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