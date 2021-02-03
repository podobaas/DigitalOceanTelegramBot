using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    internal static class ProjectMessage
    {
        public static string GetProjectInfoMessage(Project project)
        {
            var stringBuilder = new StringBuilder(string.Empty);

            stringBuilder.AppendLine($"\U0001F4DD *{project.Name}*");
            stringBuilder.AppendLine($"Id: *{project.Id}*");
            stringBuilder.AppendLine($"Created at: *{project.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}*");
            stringBuilder.AppendLine($"Description: *{project.Description} *");
            stringBuilder.AppendLine($"Purpose: *{project.Purpose}*\n");
            stringBuilder.AppendLine($"Environment: *{project.Environment}*");
            stringBuilder.AppendLine($"Is default: *{project.IsDefault.ToString()}*\n");

            return stringBuilder.ToString();
        }

        public static string GetSelectedProjectMessage(Project project)
        {
            return $"Selected project: \U0001F4DD *{project.Name}*";
        }
        
        public static string GetProjectsNotFoundMessage()
        {
            return "You don't have a projects \U0001F4DD";
        }
        
        public static string GetEnterNameMessage()
        {
            return "Enter a name for the project";
        }
        
        public static string GetEnterDescriptionMessage()
        {
            return "Enter a description for the project";
        }

        public static string GetChoosePurposeMessage()
        {
            return "Choose a purpose";
        }
        
        public static string GetChooseEnvironmentMessage()
        {
            return "Choose an environment";
        }
    }
}