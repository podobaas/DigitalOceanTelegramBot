using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    internal static class FirewallMessage
    {
        public static string GetFirewallInfoMessage(Firewall firewall, IEnumerable<Droplet> droplets)
        {
                var stringBuilder = new StringBuilder(string.Empty);
                
                stringBuilder.Append($"\U0001F3F0 *{firewall!.Name}*\n\n");
                stringBuilder.AppendLine($"Id: *{firewall.Id}*");
                stringBuilder.AppendLine($"Created at: *{firewall.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}*");
                stringBuilder.Append($"Status: *{GetFirewallStatus(firewall.Status)} ({firewall.Status})*\n");

                if (firewall is {DropletIds: not null and {Count: > 0}})
                {
                    stringBuilder.Append($"Associated droplets: *{string.Join(',', droplets.Where(d => firewall.DropletIds.Contains(d.Id)).Select(d => d.Name))}*\n");
                }

                if (firewall is {Tags: not null and {Count: > 0}})
                {
                    stringBuilder.Append($"Tags: *{string.Join(',', firewall.Tags)}*\n");
                }

                stringBuilder.Append($"\n\U00002B07 *Inbound rules:* \n");

                foreach (var rule in firewall.InboundRules)
                {
                    stringBuilder.Append($"---------------------------\n");
                    stringBuilder.Append($"Protocol: *{rule.Protocol}*\n");
                    stringBuilder.Append($"Ports: *{rule.Ports}*\n");

                    if (rule.Sources?.Addresses?.Count > 0)
                    {
                        stringBuilder.Append($"Addresses: *{string.Join(',', rule.Sources.Addresses)}*\n");
                    }
                }

                stringBuilder.Append($"---------------------------\n");
                stringBuilder.Append($"\n\U00002B06	*Outbound rules:* \n");

                foreach (var rule in firewall.OutboundRules)
                {
                    stringBuilder.Append($"---------------------------\n");
                    stringBuilder.Append($"Protocol: *{rule.Protocol}*\n");
                    stringBuilder.Append($"Ports: *{rule.Ports}*\n");

                    if (rule?.Destinations?.Addresses?.Count > 0)
                    {
                        stringBuilder.Append($"Destinations: *{string.Join(',', rule.Destinations.Addresses)}*\n");
                    }
                }

                stringBuilder.Append($"---------------------------");

            return stringBuilder.ToString();
        }

        public static string GetSelectedFirewallMessage(Firewall firewall)
        {
            return $"\U0001F3F0 Selected firewall: *{firewall.Name}*";
        }
        
        public static string GetEnterCreationDataFirewallMessage()
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine("Enter the data to create a firewall in the format: <b>Name;Inbound rule</b>");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("Example");
            stringBuilder.AppendLine("<code>TestFirewall;tcp:22:0.0.0.0/0</code>");
            return stringBuilder.ToString();
        }
        
        public static string GetInvalidCreationDataFirewallMessage()
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine("Invalid data");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("Example");
            stringBuilder.AppendLine("<code>TestFirewall;tcp:80:0.0.0.0/0</code>");
            return stringBuilder.ToString();
        }
        
        public static string GetEnterNameMessage()
        {
            return "Enter a name for the firewall";
        }
        
        public static string GetFirewallsNotFoundMessage()
        {
            return "You don't have a firewalls \U0001F914";
        }

        public static string GetEnterBoundRuleMessage(string ruleType = null)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine(ruleType is not null ? $"Enter an {ruleType} rule(s)" : "Enter a rule(s)");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("Example");
            stringBuilder.AppendLine("<code>tcp:80:0.0.0.0/0;icmp:8000-9000:0.0.0.0/0;udp:421:1.1.1.1/0</code>");
            return stringBuilder.ToString();
        }
        
        public static string GetCreatedBoundRulesMessage(int count)
        {
            return $"Created *{count}* rule(s) for the firewall";
        }
        
        public static string GetInvalidBoundRulesMessage(IEnumerable<string> invalidRules)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine($"<b>{invalidRules.Count()}</b> rule(s) are not valid");
            stringBuilder.AppendLine("");

            foreach (var invalidRule in invalidRules)
            {
                stringBuilder.AppendLine($"<code>{invalidRule}</code>");
            }

            return stringBuilder.ToString();
        }

        public static string GetDropletsListMessage(IEnumerable<Droplet> droplets)
        {
            var count = 1;
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine("Enter droplet number separated by commas or range");
            stringBuilder.AppendLine("Example: 2 or 2,3,4,5 or 2-5");
            stringBuilder.AppendLine("");

            foreach (var droplet in droplets)
            {
                stringBuilder.AppendLine($"*{count}* - {droplet.Name}");
                count++;
            }

            return stringBuilder.ToString();
        }

        public static string GetNoAssociatedDropletsMessage()
        {
            return "Firewall has no associated droplets";
        }
        
        private static string GetFirewallStatus(string status) => status switch
        {
            "failed" => "\U0001F534",
            "succeeded" => "\U0001F7E2",
            "waiting" => "\U0001F7E4",
            _ => "\U000026AB"
        };
    }
}