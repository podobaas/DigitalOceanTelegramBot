using System.Threading.Tasks;
using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands
{
    public interface IBotCallback
    {
        Task Execute(CallbackQuery callback, Message message, SessionState sessionState);
    }
}
