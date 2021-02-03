using System.Threading.Tasks;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.StateHandlers
{
    public interface IBotStateHandler
    {
        Task ExecuteHandlerAsync(Message message);
    }
}