using System;
using System.Collections.Generic;
using System.Linq;
using DigitalOcean.API.Models.Responses;
using DigitalOceanBot.Keyboards;
using DigitalOceanBot.Messages;
using DigitalOceanBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Services.Paginators
{
    public sealed class ProjectPaginatorService : IPaginator
    {
        private readonly StorageService _storageService;

        public ProjectPaginatorService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public Paginator<InlineKeyboardMarkup> GetPage(int pageIndex)
        {
            var projects = _storageService.Get<IReadOnlyList<Project>>(StorageKeys.Projects);

            if (projects is not (not null and {Count: > 0}))
            {
                throw new ArgumentNullException(nameof(projects));
            }

            var project = projects.Skip(pageIndex).Take(1).FirstOrDefault();
            
            return new Paginator<InlineKeyboardMarkup>
            {
                MessageText = ProjectMessage.GetProjectInfoMessage(project),
                Keyboard = ProjectKeyboard.GetProjectPaginatorKeyboard(pageIndex, projects.Count, project!.Id)
            };

        }

        public Paginator<ReplyKeyboardMarkup> Select(string id)
        {
            var project = _storageService.Get<IReadOnlyList<Project>>(StorageKeys.Projects).FirstOrDefault(x => x.Id == id);

            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }
            
            return new Paginator<ReplyKeyboardMarkup>
            {
                MessageText = ProjectMessage.GetSelectedProjectMessage(project),
                Keyboard = ProjectKeyboard.GetProjectOperationsKeyboard()
            };

        }
    }
}
