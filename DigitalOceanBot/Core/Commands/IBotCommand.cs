using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands
{
    public interface IBotCommand
    {
        Task ExecuteCommandAsync(Message message);
    }
}