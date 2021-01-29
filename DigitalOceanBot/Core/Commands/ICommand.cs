using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DigitalOceanBot.Core.Commands
{
    public interface ICommand
    {
        Task ExecuteCommandAsync(Message message);
    }
}