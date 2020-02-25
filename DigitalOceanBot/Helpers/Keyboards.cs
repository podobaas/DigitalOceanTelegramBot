using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Helpers
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("\U0001F30A Account"),
                    new KeyboardButton("\U0001F4DD Projects"),
                },
                new[]
                {
                    new KeyboardButton("\U0001F4A7 Droplets"),
                    new KeyboardButton("\U0001F3F0 Firewalls"),
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetDropletsMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Create new droplet"),
                },
                new[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetSelectedDropletsMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Rename droplet"),
                },
                new[]
                {
                    new KeyboardButton("Reboot droplet"),
                    new KeyboardButton("Power cycle droplet"),
                },
                new[]
                {
                    new KeyboardButton("Shutdown droplet"),
                    new KeyboardButton("Power on droplet"),
                },
                new[]
                {
                    new KeyboardButton("Create snapshot"),
                    new KeyboardButton("Reset password"),
                },
                new[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetConfirmKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Yes"),
                },
                new[]
                {
                    new KeyboardButton("No")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetFirewallMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Create new firewall"),
                },
                new[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetSelectedFirewallMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Add inbound rule"),
                    new KeyboardButton("Add outbound rule"),
                },
                new[]
                {
                    new KeyboardButton("Add droplets to firewall"),
                    new KeyboardButton("Remove droplets from firewall")
                },
                new[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetSelectedProjectMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Rename project"),
                    new KeyboardButton("Change description"),
                },
                new[]
                {
                    new KeyboardButton("Change purpose"),
                    new KeyboardButton("Change environment")
                },
                new[]
                {
                    new KeyboardButton("Set as default"),
                    new KeyboardButton("Delete project"),
                },
                new[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetProjectsMenuKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Create new project"),
                },
                new[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetPurposeKeyboard()
        {
            var inlineKeyboardButtons = new[]
            {
                new[]
                {
                    new KeyboardButton("Just trying out DigitalOcean"),
                },
                new[]
                {
                    new KeyboardButton("Class project / Educational purposes")
                },
                new[]
                {
                    new KeyboardButton("Website or blog")
                },
                new[]
                {
                    new KeyboardButton("Web Application")
                },
                new[]
                {
                    new KeyboardButton("Service or API")
                },
                new[]
                {
                    new KeyboardButton("Mobile Application")
                },
                new[]
                {
                    new KeyboardButton("Machine learning / AI / Data processing")
                },
                new[]
                {
                    new KeyboardButton("IoT")
                },
                new[]
                {
                    new KeyboardButton("Operational / Developer tooling")
                },
                new[]
                {
                    new KeyboardButton("Other")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
    }
}
