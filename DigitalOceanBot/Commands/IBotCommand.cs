using System.Threading.Tasks;
using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Commands
{
    public interface IBotCommand
    {
        Task Execute(Message message, SessionState sessionState);
    }
}
