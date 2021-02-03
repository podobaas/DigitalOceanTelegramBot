using System;

namespace DigitalOceanBot.Core
{
    internal static class EnvironmentVars
    {
        public static int GetUserId()
        {
            var envVar = Environment.GetEnvironmentVariable("USER_ID");

            if (string.IsNullOrEmpty(envVar))
            {
                throw new ArgumentNullException(nameof(envVar), "The environment variable USER_ID can't be null");
            }

            var result = int.TryParse(envVar, out var userId);

            if (!result)
            {
                throw new ArgumentException("The environment variable USER_ID must be an integer", nameof(userId));
            }

            return userId;
        }
        
        public static string GetTelegramToken()
        {
            var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "The environment variable TELEGRAM_TOKEN can't be null");
            }
            
            return token;
        }
        
        public static string GetDigitalOceanToken()
        {
            var token = Environment.GetEnvironmentVariable("DIGITALOCEAN_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "The environment variable DIGITALOCEAN_TOKEN can't be null");
            }
            
            return token;
        }
    }
}