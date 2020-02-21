using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Helpers;
using DigitalOceanBot.Models;
using DigitalOceanBot.MongoDb;
using DigitalOceanBot.MongoDb.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Pages
{
    public class ProjectPage : IPage
    {
        private readonly IRepository<Session> _sessionRepo;

        public ProjectPage(IRepository<Session> sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public PageModel GetPage(int userId, int page = 0)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var projects = session.Data.CastObject<IReadOnlyList<Project>>();

            if (projects.Count > 0)
            {
                var project = projects.Skip(page).Take(1).FirstOrDefault();

                stringBuilder.Append($"\U0001F4DD *{project.Name} (Created at UTC {project.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")})*\n\n");
                stringBuilder.Append($"Description: *{project.Description.GetDefaultIfStringEmpty()} *\n");
                stringBuilder.Append($"Purpose: *{project.Purpose}*\n");
                stringBuilder.Append($"Environment: *{project.Environment.GetDefaultIfStringEmpty()}*\n");
                stringBuilder.Append($"Is default: *{project.IsDefault.ToString()}*\n");
                
                return new PageModel
                {
                    Message = stringBuilder.ToString(),
                    Keyboard = GetInlineKeyboard(project.Id, projects.Count(), page)
                };
            }
            else
            {
                return new PageModel();
            }
        }

        public PageModel SelectPage(int userId, object id)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            var session = _sessionRepo.Get(userId);
            var project = session.Data.CastObject<IEnumerable<Project>>().Where(d => d.Id == (string)id)?.FirstOrDefault();

            if (project != null)
            {
                stringBuilder.Append($"Selected project: \U0001F4DD *{project.Name}*\n");
                
                return new PageModel
                {
                    Message = stringBuilder.ToString()
                };
            }
            else
            {
                return new PageModel();
            }
        }

        private InlineKeyboardMarkup GetInlineKeyboard(string projectId, int projectCount, int page)
        {
            var back = new InlineKeyboardButton()
            {
                Text = "\U0001F448 Back",
                CallbackData = page > 0 ? $"BackProject;{(page - 1).ToString()}" : "Empty"
            };

            var next = new InlineKeyboardButton
            {
                Text = "Next \U0001F449",
                CallbackData = page < projectCount - 1 ? $"NextProject;{(page + 1).ToString()}" : "Empty"
            };

            var count = new InlineKeyboardButton
            {
                Text = $"{(page + 1).ToString()}/{projectCount.ToString()}",
                CallbackData = "Empty"
            };

            var choose = new InlineKeyboardButton
            {
                Text = $"Select this project",
                CallbackData = $"SelectProject;{projectId}"
            };

            var buttons = new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>
                {
                    back,
                    count,
                    next
                },
                new List<InlineKeyboardButton>
                {
                    choose
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
