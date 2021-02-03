using System.Threading.Tasks;

namespace DigitalOceanBot.Core.CallbackQueries
{
    public interface IBotCallbackQuery
    {
         Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload);
    }
}