using System;
using System.Threading.Tasks;
using DigitalOceanBot.MongoDb.Models;
using Newtonsoft.Json;
using RestSharp;

namespace DigitalOceanBot.Helpers
{
    public class TokenManager
    {
        public async Task<UserInfo> RefreshToken(string refreshToken)
        {
            var url = $"{Environment.GetEnvironmentVariable("REFRESH_TOKEN_URL")}&refresh_toke={refreshToken}";
            var client = new RestClient(url)
            {
                Timeout = -1
            };

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var response = await client.ExecuteAsync(request);
            return JsonConvert.DeserializeObject<UserInfo>(response.Content);
        }

        public async Task<bool> RevokeToken(string accessToken)
        {
            var url = $"{Environment.GetEnvironmentVariable("REVOKE_TOKEN_URL")}?token={accessToken}";
            var client = new RestClient(url)
            {
                Timeout = -1
            };

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var response = await client.ExecuteAsync(request);
            
            if(response.Content == "{}")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
