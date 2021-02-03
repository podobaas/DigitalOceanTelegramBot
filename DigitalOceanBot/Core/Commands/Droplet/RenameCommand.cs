using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Services;
using DigitalOceanBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigitalOceanBot.Core.Commands.Droplet
{
    [BotCommand(BotCommandType.DropletRename)]
    public sealed class RenameCommand : IBotCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly StorageService _storageService;

        public RenameCommand(ITelegramBotClient telegramBotClient, StorageService storageService)
        {
            _telegramBotClient = telegramBotClient;
            _storageService = storageService;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            var dropletId = _storageService.Get<long>(StorageKeys.DropletId);
            
            if (dropletId > 0)
            {
                _storageService.AddOrUpdate(StorageKeys.BotCurrentState, BotStateType.DropletUpdateWaitingEnterNewName);
                
                await _telegramBotClient.SendTextMessageAsync(
                    chatId:message.Chat.Id, 
                    text:DropletMessage.GetEnterNameMessage(), 
                    parseMode:ParseMode.Markdown);
            }
        }
    }
}
