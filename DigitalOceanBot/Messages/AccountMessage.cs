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

            stringBuilder.Append($"\U0001F4C4 Account info: \n");
            stringBuilder.Append($"Email: *{account.Email}* \n");
            var emailIconState = account.EmailVerified ? "\U00002705" : "\U0000274C";
            stringBuilder.Append($"Email verified: *{emailIconState}* \n");
            stringBuilder.Append($"Account status: *{account.Status}* \n");
            stringBuilder.Append($"Droplet limit: *{account.DropletLimit.ToString()}* \n");
            stringBuilder.Append($"Floating IP limit: *{account.FloatingIpLimit.ToString()}* \n\n");

            stringBuilder.Append($"\U0001F4B0 Balance info: \n");
            stringBuilder.Append($"Balance as of the generated at time: *{balance.MonthToDateBalance}* \n");
            stringBuilder.Append($"Billing activity balance: *{balance.AccountBalance}* \n");
            stringBuilder.Append($"Amount used in the current billing period: *{balance.MonthToDateUsage}* \n");

            return stringBuilder.ToString();
        }
    }
}