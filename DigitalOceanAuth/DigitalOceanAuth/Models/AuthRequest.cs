using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
