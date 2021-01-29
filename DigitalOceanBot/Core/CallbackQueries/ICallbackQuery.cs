using System.Threading.Tasks;

namespace DigitalOceanBot.Core.CallbackQueries
{
    public interface ICallbackQuery
    {
         Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload);
    }
}