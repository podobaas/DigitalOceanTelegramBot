using System.Collections.Generic;
using System.Linq;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.CheckLists
{
    public class DropletCheckList: ICheckListPage<Droplet>
    {
        public PageModel GetCheckListPage(IEnumerable<Droplet> collection, bool hasSkip = false)
        {
            var buttonList = new List<InlineKeyboardButton>();

            var buttons = collection.Select(droplet => 
            new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton
                {
                    Text = droplet.Name, 
                    CallbackData = $"SelectDroplet;{droplet.Id.ToString()}"
                }
            }).ToList();

            var skipButton = new InlineKeyboardButton
            {
                Text = "Skip",
                CallbackData = $"Skip"
            };
            
            var okButton = new InlineKeyboardButton
            {
                Text = "Ok",
                CallbackData = $"Ok"
            };

            var actionButtons = new List<InlineKeyboardButton>();
            if (hasSkip)
            {
                actionButtons.Add(skipButton);
            }
            
            actionButtons.Add(okButton);
            buttons.Add(buttonList);
            buttons.Add(actionButtons);
            
            return new PageModel
            {
                Message = "Select droplets",
                Keyboard = new InlineKeyboardMarkup(buttons)
            };
        }

        public InlineKeyboardMarkup SelectItem(InlineKeyboardMarkup inlineKeyboardMarkup, string id)
        {
            var button = inlineKeyboardMarkup.InlineKeyboard.FirstOrDefault(k => k.Any(a => a.CallbackData.Split(';')[1] == id)).FirstOrDefault();
            
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

            return inlineKeyboardMarkup;
        }

        public IEnumerable<string> GetSelectedItems(InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            var selectedItems = inlineKeyboardMarkup.InlineKeyboard.Where(k => k.Any(a => a.Text.Contains("\U00002705")));
            
            if(selectedItems.Any())
            {
                return selectedItems.Select(button => button.FirstOrDefault()?.CallbackData?.Split(';')[1]).ToList();
            }

            return new List<string>();
        }
    }
}