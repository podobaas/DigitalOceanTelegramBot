using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Types.Enums;

namespace DigitalOceanBot.Extensions
{
    internal static class ActionExtension
    {
        public static ActionStatus GetStatus(this Action self) => self.Status switch
        {
            "completed" => ActionStatus.Success,
            "errored" => ActionStatus.Error,
            _ => ActionStatus.Waiting
        };
    }
}