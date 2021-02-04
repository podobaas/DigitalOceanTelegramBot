using System.Text;
using DigitalOcean.API.Models.Responses;

namespace DigitalOceanBot.Messages
{
    public static class ImageMessage
    {
        public static string GetImageInfoMessage(Image image)
        {
            var stringBuilder = new StringBuilder(string.Empty);
            stringBuilder.AppendLine($"\U0001F4BF *{image.Distribution}*");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"Description: *{image.Description}*");
            stringBuilder.AppendLine($"Slug: *{image.Slug}*");
            stringBuilder.AppendLine($"Minimum disk size: *{image.MinDiskSize}GB*");
            stringBuilder.AppendLine($"Size image: *{image.SizeGigabytes}GB*");
            stringBuilder.AppendLine($"Public: *{image.Public}*");

            return stringBuilder.ToString();
        }
        
        public static string GetSelectedImageMessage(Image image)
        {
            return $"Selected image: \U0001F4BF *{image.Name} ({image.Slug})*";
        }
    }
}