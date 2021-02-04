using System.Threading.Tasks;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Attributes;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;

namespace DigitalOceanBot.Core.CallbackQueries.Droplet
{
    [BotCallbackQuery(BotCallbackQueryType.DropletCreateNew)]
    public sealed class CreateNewDropletCallbackQuery: IBotCallbackQuery
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;
        
        public CreateNewDropletCallbackQuery(
            ITelegramBotClient telegramBotClient,
            StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }
        
        public async Task ExecuteCallbackQueryAsync(long chatId, int messageId, string callbackQueryId, string payload)
        {
            _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.DropletCreateWaitingEnterName);

            await _telegramBotClient.DeleteMessageAsync(
                chatId: chatId, 
                messageId: messageId);
            
            await _telegramBotClient.SendTextMessageAsync(
                chatId:chatId, 
                text:DropletMessage.GetEnterNameMessage());
        }
    }
}