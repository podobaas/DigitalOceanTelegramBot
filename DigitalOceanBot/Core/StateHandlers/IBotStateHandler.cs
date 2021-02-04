using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.StateHandlers
{
    public interface IBotStateHandler
    {
        Task ExecuteHandlerAsync(Message message);
    }
}