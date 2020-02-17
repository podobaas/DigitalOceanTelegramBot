using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands
{
    public interface IBotCommand
    {
        void Execute(Message message, SessionState sessionState);
    }
}
