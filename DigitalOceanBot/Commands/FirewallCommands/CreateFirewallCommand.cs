using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API.Exceptions;
using DigitalOceanBot.CheckLists;
using DigitalOceanBot.Factory;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Requests = DigitalOcean.API.Models.Requests;
using Responses = DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Commands.FirewallCommands
{
    public class CreateFirewallCommand : IBotCommand, IBotCallback
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IRepository<Session> _sessionRepo;
        private readonly ILogger<DigitalOceanWorker> _logger;
        private readonly ICheckListPageFactory _checkListFactory;
        private readonly IRepository<HandlerCallback> _handlerCallbackRepo;
        private readonly IDigitalOceanClientFactory _digitalOceanClientFactory;

        public CreateFirewallCommand(
            ILogger<DigitalOceanWorker> logger,
            ITelegramBotClient telegramBotClient,
            IRepository<Session> sessionRepo,
            IRepository<HandlerCallback> handlerCallbackRepo,
            ICheckListPageFactory checkListFactory,
            IDigitalOceanClientFactory digitalOceanClientFactory)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _sessionRepo = sessionRepo;
            _handlerCallbackRepo = handlerCallbackRepo;
            _checkListFactory = checkListFactory;
            _digitalOceanClientFactory = digitalOceanClientFactory;
        }

        #region Commands

        public async Task Execute(Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                
                switch (sessionState)
                {
                    case SessionState.FirewallsMenu:
                        await InputNameFirewall(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputNameFirewall:
                        await InputInboundRule(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputInboundFirewallRule:
                        await InputOutboundRule(message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitInputOutboundFirewallRule:
                        await AddDropletsToFirewall(message).ConfigureAwait(false);
                        break;
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task InputNameFirewall(Message message)
        {
            _sessionRepo.Update(message.From.Id, session =>
            {
                session.State = SessionState.WaitInputNameFirewall;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input name for new firewall:");
        }

        private async Task InputInboundRule(Message message)
        {
            var firewall = new Requests.Firewall
            {
                Name = message.Text
            };

            _sessionRepo.Update(message.From.Id, session =>
            {
                session.Data = firewall;
                session.State = SessionState.WaitInputInboundFirewallRule;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input an inbound rule in format:\n*tcp or udp or icmp:port or range:addresses*\n\nFor example:\n*tcp:80:0.0.0.0/0;icmp:8000-9000:0.0.0.0/0;udp:421:1.1.1.1,0.0.0.0/0*", ParseMode.Markdown);
        }

        private async Task InputOutboundRule(Message message)
        {
            var inboundRules = new List<Requests.InboundRule>();
            var userRules = message.Text.Split(';');
            foreach (var rule in userRules)
            {
                var ruleData = rule.Split(':');
                if (ruleData.Length == 3)
                {
                    var inboundRule = new Requests.InboundRule
                    {
                        Protocol = ruleData[0],
                        Ports = ruleData[1],
                        Sources = new Requests.SourceLocation
                        {
                            Addresses = ruleData[2].Split(',').ToList()
                        }
                    };

                    inboundRules.Add(inboundRule);
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Invalid rule: {rule}\nPlease, try again");
                    return;
                }
            }

            _sessionRepo.Update(message.From.Id, session =>
            {
                var firewall = session.Data.CastObject<Requests.Firewall>();
                firewall.InboundRules = inboundRules;

                session.Data = firewall;
                session.State = SessionState.WaitInputOutboundFirewallRule;
            });

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Input an outbound rule in format:\n*tcp or udp or icmp:port or range:address*\n\nFor example:\n*tcp:80:0.0.0.0/0;udp:433:0.0.0.0/0*", ParseMode.Markdown);
        }

        private async Task AddDropletsToFirewall(Message message)
        {
            var outboundRules = new List<Requests.OutboundRule>();
            var userRules = message.Text.Split(';');
            foreach (var rule in userRules)
            {
                var ruleData = rule.Split(':');
                if (ruleData.Length == 3)
                {
                    var outboundRule = new Requests.OutboundRule
                    {
                        Protocol = ruleData[0],
                        Ports = ruleData[1],
                        Destinations = new Requests.SourceLocation
                        {
                            Addresses = ruleData[2].Split(',').ToList()
                        }
                    };

                    outboundRules.Add(outboundRule);
                }
                else
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Invalid rule: {rule}\nPlease, try again");
                    return;
                }
            }

            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(message.From.Id);
            var droplets = await digitalOceanApi.Droplets.GetAll();
            var checkList = _checkListFactory.GetInstance<Responses.Droplet, DropletCheckList>();
            var pageModel = checkList.GetCheckListPage(droplets, true);
            
            _sessionRepo.Update(message.From.Id, session =>
            {
                var firewall = session.Data.CastObject<Requests.Firewall>();
                firewall.OutboundRules = outboundRules;
                session.Data = firewall;
                session.State = SessionState.WaitChooseDropletsForFirewall;
            });

            var sendMessage = await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, pageModel.Message, ParseMode.Markdown, replyMarkup: pageModel.Keyboard);
                
            _handlerCallbackRepo.Update(message.From.Id, calllback =>
            {
                calllback.MessageId = sendMessage.MessageId;
                calllback.UserId = message.From.Id;
                calllback.HandlerType = GetType().FullName;
            });
        }

        #endregion


        #region Callbacks

        public async Task Execute(CallbackQuery callback, Message message, SessionState sessionState)
        {
            try
            {
                await _telegramBotClient.SendChatActionAsync(callback.From.Id, ChatAction.Typing);
                var callBackData = callback.Data.Split(';');

                switch (sessionState)
                {
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "SelectDroplet":
                        await SelectDropletForFirewall(callback, message).ConfigureAwait(false);
                        break;
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "Skip":
                        await CreateFirewall(callback, message, false).ConfigureAwait(false);
                        break;
                    case SessionState.WaitChooseDropletsForFirewall when callBackData[0] == "Ok":
                        await CreateFirewall(callback, message, true).ConfigureAwait(false);
                        break;
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"DigitalOcean API Error: {ex.Message.Replace(".", "\\.")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UserId={message.From.Id.ToString()}, Error={ex.Message}, StackTrace={ex.StackTrace}");
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Sorry, аn error has occurred \U0001F628");
            }
        }

        private async Task SelectDropletForFirewall(CallbackQuery callback, Message message)
        {
            var dropletId = callback.Data.Split(';')[1];
            var button = message.ReplyMarkup.InlineKeyboard.FirstOrDefault(k => k.Any(a => a.CallbackData.Split(';')[1] == dropletId)).FirstOrDefault();
            
            if (button != null)
            {
                if (!button.Text.Contains("\U00002705"))
                {
                    button.Text = $"{button.Text} \U00002705";
                }
                else
                {
                    button.Text = button.Text.Replace("\U00002705", "");
                }
            }

            await _telegramBotClient.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, message.ReplyMarkup);
        }

        private async Task CreateFirewall(CallbackQuery callback, Message message, bool hasDroplets)
        {
            var digitalOceanApi = _digitalOceanClientFactory.GetInstance(callback.From.Id);
            var session = _sessionRepo.Get(callback.From.Id);
            var firewall = session.Data.CastObject<Requests.Firewall>();

            if (hasDroplets)
            {
                firewall.DropletIds = new List<int>();
                var selectedDroplets = message.ReplyMarkup.InlineKeyboard.Where(k => k.Any(a => a.Text.Contains("\U00002705")));

                foreach (var button in selectedDroplets)
                {
                    var id = button.FirstOrDefault()?.CallbackData?.Split(';')[1];
                    firewall.DropletIds.Add(int.Parse(id));
                }
            }

            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"\U0001F4C0 Creating firewall...");
            await digitalOceanApi.Firewalls.Create(firewall);

            _sessionRepo.Update(callback.From.Id, session =>
            {
                session.State = SessionState.FirewallsMenu;
            });
            
            await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Done \U00002705", ParseMode.MarkdownV2);
        }

        #endregion
    }
}
