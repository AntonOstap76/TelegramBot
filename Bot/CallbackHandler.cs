using Bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Bot;

public class CallbackHandler
{
    public readonly HabitService _habitService;
    public readonly TelegramBotClient _botClient;
    public readonly KeyboardService _keyboardService;
    
    
    public CallbackHandler(HabitService habitService, TelegramBotClient telegramBotClient,
                            KeyboardService keyBoardService)
    {
        _habitService= habitService;
        _botClient = telegramBotClient;
        _keyboardService = keyBoardService;
       
    }
    public async Task HandleAsync(CallbackQuery callback)
    {
        
        // delete habit
        if (callback.Message is null) return ;

        var chatId = callback.Message.Chat.Id;
        
        if (int.TryParse(callback.Data, out var i))
        {
            var removed = await _habitService.RemoveHabit(chatId, i-1);
            if (removed != null)
            {
                await _botClient.SendMessage(chatId, $"Removed habit: <b>{removed.Habit}</b>",
                    ParseMode.Html,
                    replyMarkup: _keyboardService.GetMainKeyboard());
                return;
            }
            
        }
        
        // done a habit
        if (callback.Data!.StartsWith("done:") &&
                 int.TryParse(callback.Data.Split(":")[1], out var k))
        {
            var finished = await _habitService.MarkAsDone(chatId,k-1);
            

            if (finished != null)
            {
                await _botClient.SendMessage(chatId, $"The habit <b>{finished.Habit}</b> is now marked as DONE!",
                    ParseMode.Html,
                    replyMarkup: _keyboardService.GetMainKeyboard());
                return;
            };

            
        }
        
        await _botClient.SendMessage(chatId, 
            "Invalid selected item", 
            replyMarkup: _keyboardService.GetMainKeyboard());
        
        

    }
}