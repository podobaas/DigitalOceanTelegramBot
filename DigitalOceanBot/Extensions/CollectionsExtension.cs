using System;
using System.Collections.Generic;
using System.Linq;
using DigitalOceanBot.Core.Attributes;
using DigitalOceanBot.Core.CallbackQueries;
using DigitalOceanBot.Core.Commands;
using DigitalOceanBot.Core.StateHandlers;
using DigitalOceanBot.Types.Enums;

namespace DigitalOceanBot.Extensions
{
    internal static class CollectionsExtension
    {
        public static bool TryGetCommand(this IEnumerable<IBotCommand> commands, string name, out IBotCommand botCommand)
        {
            if (commands is null)
            {
                throw new ArgumentNullException(nameof(commands));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            foreach (var item in commands)
            {
                if (Attribute.IsDefined(item.GetType(), typeof(BotCommandAttribute)))
                {
                    var commandType = (BotCommandAttribute) Attribute.GetCustomAttribute(item.GetType(), typeof(BotCommandAttribute));
                    var commandName = commandType.BotCommandType.GetStringCommand();
                    if (commandName == name)
                    {
                        botCommand = item;
                        return true;
                    }
                }
            }

            botCommand = null;
            return false;
        }
        
        public static bool TryGetStateHandler(this IEnumerable<IBotStateHandler> stateHandlers, BotStateType botStateType, out IBotStateHandler botStateHandler)
        {
            if (stateHandlers is null)
            {
                throw new ArgumentNullException(nameof(stateHandlers));
            }

            if (botStateType is BotStateType.None)
            {
                botStateHandler = null;
                return false;
            }
            
            foreach (var item in stateHandlers)
            {
                if (Attribute.IsDefined(item.GetType(), typeof(BotStateHandlerAttribute)))
                {
                    var stateHandlerAttributes = (BotStateHandlerAttribute[]) Attribute.GetCustomAttributes(item.GetType(), typeof(BotStateHandlerAttribute));
                    var attribute = stateHandlerAttributes.FirstOrDefault(x => x.BotStateType == botStateType);
                    if (attribute is not null)
                    {
                        botStateHandler = item;
                        return true;
                    }
                }
            }

            botStateHandler = null;
            return false;
        }
        
        public static bool TryGetCallbackQuery(this IEnumerable<IBotCallbackQuery> callbackQueries, BotCallbackQueryType botCallbackQueryType, out IBotCallbackQuery botCallbackQuery)
        {
            if (callbackQueries is null)
            {
                throw new ArgumentNullException(nameof(callbackQueries));
            }
            
            if (botCallbackQueryType is BotCallbackQueryType.None)
            {
                botCallbackQuery = null;
                return false;
            }
            
            foreach (var item in callbackQueries)
            {
                if (Attribute.IsDefined(item.GetType(), typeof(BotCallbackQueryAttribute)))
                {
                    var stateHandlerAttributes = (BotCallbackQueryAttribute[]) Attribute.GetCustomAttributes(item.GetType(), typeof(BotCallbackQueryAttribute));
                    var attribute = stateHandlerAttributes.FirstOrDefault(x => x.BotCallbackQueryType == botCallbackQueryType);
                    if (attribute is not null)
                    {
                        botCallbackQuery = item;
                        return true;
                    }
                }
            }

            botCallbackQuery = null;
            return false;
        }
    }
}