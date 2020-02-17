namespace DigitalOceanBot.MongoDb.Models
{
    public class UserInfo
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public Info info { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }

    public class Info
    {
        public string name { get; set; }
        public string email { get; set; }
        public string uuid { get; set; }
    }

}
