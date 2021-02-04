using System;
using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    internal static class AccountMessage
    {
        public static string GetAccountInfoMessage(Account account, Balance balance)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine("📄 Account info:");
            stringBuilder.AppendLine($"Email: *{account.Email}*");
            var emailIconState = account.EmailVerified ? "\U00002705" : "\U0000274C";
            stringBuilder.AppendLine($"Email verified: *{emailIconState}*");
            stringBuilder.AppendLine($"Account status: *{account.Status}*");
            stringBuilder.AppendLine($"Droplet limit: *{account.DropletLimit.ToString()}*");
            stringBuilder.AppendLine($"Floating IP limit: *{account.FloatingIpLimit.ToString()}*");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("💰 Balance info:");
            stringBuilder.AppendLine($"Balance as of the generated at time: *{balance.MonthToDateBalance}*");
            stringBuilder.AppendLine($"Billing activity balance: *{balance.AccountBalance}*");
            stringBuilder.AppendLine($"Amount used in the current billing period: *{balance.MonthToDateUsage}*");

            return stringBuilder.ToString();
        }
    }
}