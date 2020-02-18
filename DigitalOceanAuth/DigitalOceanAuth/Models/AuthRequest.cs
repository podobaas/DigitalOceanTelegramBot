using Microsoft.AspNetCore.Mvc;

namespace DigitalOceanAuth.Models
{
    public class AuthRequest
    {
        [FromQuery(Name = "state")]
        public string State { get; set; }

        [FromQuery(Name = "code")]
        public string Code { get; set; }

        [FromQuery(Name = "error")]
        public string Error { get; set; }

        [FromQuery(Name = "error_description")]
        public string ErrorDescription { get; set; }
    }
}
