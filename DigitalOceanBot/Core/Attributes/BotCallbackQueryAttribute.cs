using System;
using DigitalOceanBot.Types.Enums;

namespace DigitalOceanBot.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class BotCallbackQueryAttribute : Attribute
    {
        public BotCallbackQueryType BotCallbackQueryType { get; }

        public BotCallbackQueryAttribute(BotCallbackQueryType botCallbackQueryType)
        {
            BotCallbackQueryType = botCallbackQueryType;
        }
    }
}