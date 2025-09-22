using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Bot;

public class KeyboardService
{
    public ReplyKeyboardMarkup GetMainKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { " Add", " List", "History" },
            new KeyboardButton[] { " Delete", "Done" }
        })
        {
            ResizeKeyboard = true
        };
    }
    
    public InlineKeyboardMarkup GetDeleteKeyboard(List<string> tasks)
    {
        var buttons = tasks
            .Select((t, i) => InlineKeyboardButton.WithCallbackData(
                text: (i + 1).ToString(),
                callbackData: (i + 1).ToString()))
            .Chunk(5)
            .Select(row => row.ToArray())
            .ToArray();

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetDoneKeyboard(List<string> habits)
    {
        var buttons = habits
            .Select((t, i) => InlineKeyboardButton.WithCallbackData(
                text: $"{i + 1}.{t}",
                callbackData: $"done:{i + 1}"))
            .Chunk(5)
            .Select(row => row.ToArray())
            .ToArray();

        return new InlineKeyboardMarkup(buttons);
    }
}