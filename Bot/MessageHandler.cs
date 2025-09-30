using Bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Bot;

public class MessageHandler
{
    
    private readonly HabitService _habitService;
    private readonly TelegramBotClient _botClient;
    private readonly KeyboardService _keyboardService;
    
    private readonly Dictionary<long, string?> _userState;

    public MessageHandler(HabitService habitService, TelegramBotClient botClient,
                KeyboardService keyboardService,Dictionary<long, string?> userState)
    {
        _habitService = habitService;
        _botClient = botClient;
        _keyboardService = keyboardService;
        _userState = userState;
        
    }
    
    
    public async Task HandleAsync(Message message)
    {
        if (message.Text is null) return; // handle only text for now
    
    var chatId = message.Chat.Id;

    switch (message.Text)
    {
        case "/start":
            
            string welcomeMessage = 
                "<b>Welcome to HabbitTrakerBot!</b>\n\n" +
                "I'm here to help you build better habits and stay consistent every day. ✨\n\n" +
                "Here’s what you can do:\n" +
                "📌 Add new habits\n" +
                "📖 View your current habits\n" +
                "✅ Track your progress\n\n" +
                "<i>So... what would you like to do first?</i>";
            
            await _botClient.SendMessage(chatId, welcomeMessage, ParseMode.Html,
                replyMarkup: _keyboardService.GetMainKeyboard());
            break;
        
        case "Add" or "/add":
            _userState[chatId] = "adding";
            await _botClient.SendMessage(chatId, "<em>Send me what to add to list...</em>", ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove());
            break;

        case "List" or "/list":

            var habits = await _habitService.GetAllHabits(chatId);
            if (habits.Any())
            {
                var list = string.Join("\n", habits.Select((t, i) => $"{i + 1}. {t.Habit}"));
                await _botClient.SendMessage(chatId, "Your habits:\n" + list);
            }
            else
            {
                await _botClient.SendMessage(chatId, " Your task list is empty....", replyMarkup: _keyboardService.GetMainKeyboard());
            }

            break;

        case "Delete" or "/delete":
            
            var habitsD = await _habitService.GetAllHabits(chatId);
            if (habitsD.Any())
            {
                var markup = _keyboardService.GetDeleteKeyboard(habitsD.Select(t=>t.Habit).ToList());
                
                await _botClient.SendMessage(chatId, "<em>Choose what habit you want to remove from list</em>",
                    ParseMode.Html,
                    replyMarkup: markup);
            }
            else
            {
                await _botClient.SendMessage(chatId, 
                    " Your task list is empty....", replyMarkup: _keyboardService.GetMainKeyboard());
            }
            break;
        
        case "Done" or "/done" :
            
            var habitsDone = await _habitService.GetAllHabits(chatId);
            if (habitsDone.Any())
            {
                var markup = _keyboardService.GetDoneKeyboard(habitsDone.Select(d=>d.Habit).ToList());

                await _botClient.SendMessage(chatId, "<em>Choose what habit you want to mark as DONE </em>",
                    ParseMode.Html,
                    replyMarkup: markup);
            }
            else
            {
                await _botClient.SendMessage(chatId, 
                    " Your task list is empty....", replyMarkup: _keyboardService.GetMainKeyboard());
            }
            break;
        
        
        case "Help" or  "/help":
            await _botClient.SendMessage(chatId, "This Bot has a few commands to use: \n"+
                                                 "/add - his command add a habit to your list of habits\n"+
                                                 "/list - his command list all your habits\n"+ 
                                                     "/delete - allow you to delete habit from your list\n " +
                                                     "/done - by this command you mark your habit as done and put it into another done list \n"+
                                                     "/history - this command show you what habits you mark as done",
                replyMarkup: _keyboardService.GetMainKeyboard());   
                break;
        case "History" or "/history":

            var doneHabits = await _habitService.GetDoneHabits(chatId);
            
            if (doneHabits.Any())
            {
                var list = string.Join("\n", doneHabits.Select((t, i) => $"{i + 1}. {t.Habit}"));
                await _botClient.SendMessage(chatId, "Your done habits:\n" + list);
            }
            else
            {
                await _botClient.SendMessage(chatId, " Your task list is empty....", replyMarkup: _keyboardService.GetMainKeyboard());
            }

            break;
        default:
            // check if user is in "adding" mode
            if (_userState.TryGetValue(chatId, out var state) && state == "adding")
            {
                await _habitService.AddHabit(chatId, message.Text);
                _userState[chatId] = null;

                await _botClient.SendMessage(chatId,
                    $"Added habit: <b>{message.Text}</b>",
                    ParseMode.Html,
                    replyMarkup: _keyboardService.GetMainKeyboard());
            }
            else
            {
                await _botClient.SendMessage(chatId,
                    $"User {message.From?.Username} said: {message.Text}");
            }
            break;
        
    }
    
    
    Console.WriteLine($"Recieved {message.Type} and '{message.Text}' in '{message.Chat}'");
    }
}