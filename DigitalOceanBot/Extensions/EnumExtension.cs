using System;
using System.ComponentModel;

namespace DigitalOceanBot.Extensions
{
    public static class EnumExtension
    {
        public static string GetStringCommand(this Enum enumVal)
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : string.Empty;
        }
    }
}