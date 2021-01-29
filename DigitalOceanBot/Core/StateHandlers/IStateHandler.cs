using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.StateHandlers
{
    public interface IStateHandler
    {
        Task ExecuteHandlerAsync(Message message);
    }
}