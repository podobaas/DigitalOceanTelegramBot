using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands
{
    public interface IBotCallback
    {
        void Execute(CallbackQuery callback, Message message, SessionState sessionState);
    }
}
