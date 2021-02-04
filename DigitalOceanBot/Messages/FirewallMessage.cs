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
                stringBuilder.AppendLine($"\U0001F3F0 *{firewall!.Name}*");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine($"Id: *{firewall.Id}*");
                stringBuilder.AppendLine($"Created at: *{firewall.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}*");
                stringBuilder.AppendLine($"Status: *{GetFirewallStatus(firewall.Status)} ({firewall.Status})*");

                if (firewall is {DropletIds: not null and {Count: > 0}})
                {
                    stringBuilder.AppendLine($"Associated droplets: *{string.Join(',', droplets.Where(d => firewall.DropletIds.Contains(d.Id)).Select(d => d.Name))}*");
                }

                if (firewall is {Tags: not null and {Count: > 0}})
                {
                    stringBuilder.AppendLine($"Tags: *{string.Join(',', firewall.Tags)}*");
                }

                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("⬇ *Inbound rules:*");
                stringBuilder.AppendLine("");

                foreach (var rule in firewall.InboundRules)
                {
                    stringBuilder.AppendLine("---------------------------");
                    stringBuilder.AppendLine($"Protocol: *{rule.Protocol}*");
                    stringBuilder.AppendLine($"Ports: *{rule.Ports}*");

                    if (rule.Sources?.Addresses?.Count > 0)
                    {
                        stringBuilder.AppendLine($"Addresses: *{string.Join(',', rule.Sources.Addresses)}*");
                    }
                }

                stringBuilder.AppendLine("---------------------------");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("⬆\t*Outbound rules:*");
                stringBuilder.AppendLine("");

                foreach (var rule in firewall.OutboundRules)
                {
                    stringBuilder.AppendLine("---------------------------");
                    stringBuilder.AppendLine($"Protocol: *{rule.Protocol}*");
                    stringBuilder.AppendLine($"Ports: *{rule.Ports}*");

                    if (rule?.Destinations?.Addresses?.Count > 0)
                    {
                        stringBuilder.AppendLine($"Destinations: *{string.Join(',', rule.Destinations.Addresses)}*");
                    }
                }

                stringBuilder.AppendLine("---------------------------");

            return stringBuilder.ToString();
        }

        public static string GetSelectedFirewallMessage(Firewall firewall)
        {
            return $"Selected firewall: \U0001F3F0 *{firewall.Name}*";
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