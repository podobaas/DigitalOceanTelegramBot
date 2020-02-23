using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Helpers
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("\U0001F30A Account"),
                    new KeyboardButton("\U0001F4DD Projects"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("\U0001F4A7 Droplets"),
                    new KeyboardButton("\U0001F3F0 Firewalls"),
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetDropletsMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Create new droplet"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetSelectedDropletsMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Rename droplet"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Reboot droplet"),
                    new KeyboardButton("Power cycle droplet"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Shutdown droplet"),
                    new KeyboardButton("Power on droplet"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Create snapshot"),
                    new KeyboardButton("Reset password"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetConfirmKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Yes"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("No")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }

        public static ReplyKeyboardMarkup GetFirewallMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Create new firewall"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetSelectedFirewallMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Add inbound rule"),
                    new KeyboardButton("Add outbound rule"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Add droplets to firewall"),
                    new KeyboardButton("Remove droplets from firewall")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetSelectedProjectMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Rename project"),
                    new KeyboardButton("Change description"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Change purpose"),
                    new KeyboardButton("Change environment")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Set as default"),
                    new KeyboardButton("Delete project"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetProjectsMenuKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Create new project"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Back")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
        
        public static ReplyKeyboardMarkup GetPurposeKeyboard()
        {
            var inlineKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Just trying out DigitalOcean"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Class project / Educational purposes")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Website or blog")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Web Application")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Service or API")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Mobile Application")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Machine learning / AI / Data processing")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("IoT")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Operational / Developer tooling")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Other")
                }
            };

            return new ReplyKeyboardMarkup(inlineKeyboardButtons, true, true);
        }
    }
}
