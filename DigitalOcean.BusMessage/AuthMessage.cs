using System;

namespace DigitalOceanBot.BusMessage
{
    public class AuthMessage
    {
        public int UserId { get; set; }

        public long ChatId { get; set; }

        public bool IsSuccess { get; set; }
    }
}
