using Telegram.Bot.Types.ReplyMarkups;

namespace DigitalOceanBot.Types
{
    public sealed record Paginator<T> where T : IReplyMarkup
    {
        public string MessageText { get; init; }

        public T Keyboard { get; init; }
    }
}
