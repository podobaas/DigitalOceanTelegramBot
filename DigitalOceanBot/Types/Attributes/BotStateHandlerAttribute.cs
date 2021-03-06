﻿using System;
using DigitalOceanBot.Types.Enums;

namespace DigitalOceanBot.Types.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class BotStateHandlerAttribute : Attribute
    {
        public BotStateType BotStateType { get; }

        public BotStateHandlerAttribute(BotStateType botStateType)
        {
            BotStateType = botStateType;
        }
    }
}