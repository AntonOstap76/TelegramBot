using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;
using Microsoft.EntityFrameworkCore;
    
// var token = Environment.GetEnvironmentVariable("TOKEN");

var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json",true,true);
var config = builder.Build();
var token = config["TOKEN"];
    
using var cts = new CancellationTokenSource(); // help to stop bot wait until it is forcefully stoped by Enter
var bot = new TelegramBotClient(token!, cancellationToken:cts.Token);
var me = await bot.GetMe();

//for storing habbits
Dictionary<long,List<string>> userHabbit =  new Dictionary<long,List<string>>();
Dictionary<long,string?> userState = new Dictionary<long,string?>();
Dictionary<long, List<string>> doneHabits =new Dictionary<long,List<string>>();

bot.OnMessage += OnMessage;// when bot.OnMessage invoke it is called InMessage created by me
bot.OnError += OnError;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running. If You click Enter i finish my job ");
Console.ReadLine();
cts.Cancel();//stop the bot

// method to handle errors in polling or in  OnMessage/OnUpdate code
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception.Message);
}


// method that handle messages received by the bot:
async Task OnMessage(Message message, UpdateType type  )
{
    if (message.Text is null) return; // handle only text for now
    
    var chatId = message.Chat.Id;

    if (userState.TryGetValue(chatId, out var state) && state == "adding")
    {
        if (!userHabbit.ContainsKey(chatId))
        {
            userHabbit[chatId] = new List<string>();
        }

        userHabbit[chatId].Add(message.Text);
           userState[chatId] = null;

           await bot.SendMessage(chatId, $"{message.Text} was successfully added to the list of habbits ", replyMarkup: GetMainKeyboard());
           return;
        
    }


    switch (message.Text)
    {
        case "/start":
            await bot.SendMessage(chatId, "Hi there, <b>What you want to do?</b>", ParseMode.Html,
                replyMarkup: GetMainKeyboard());
            break;
        case "Add" or "/add":
            userState[chatId] = "adding";
            await bot.SendMessage(chatId, "<em>Send me what to add to list...</em>", ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove());

            break;

        case "List" or "/list":
            if (userHabbit.TryGetValue(chatId, out var tasks) && tasks.Count > 0)
            {
                var list = string.Join("\n", tasks.Select((t, i) => $"{i + 1}. {t}"));
                await bot.SendMessage(chatId, "Your habits:\n" + list);
            }
            else
            {
                await bot.SendMessage(chatId, " Your task list is empty....", replyMarkup: GetMainKeyboard());
            }

            break;

        case "Delete" or "/delete":
            if (userHabbit.TryGetValue(chatId, out var task) && task.Count > 0)
            {
                var buttons = task
                    .Select((t, i) => InlineKeyboardButton.WithCallbackData(
                        text: (i + 1).ToString(), // what user sees
                        callbackData: (i + 1).ToString() // what is sent back to bot
                    ))
                    .Chunk(5) // optional: put max 5 buttons in a row
                    .Select(row => row.ToArray())
                    .ToArray(); // what is sent back to bot))
                
                await bot.SendMessage(chatId, "<em>Choose what habit you want to remove from list</em>", ParseMode.Html,
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                );
            }
            else
            {
                await bot.SendMessage(chatId, " Your task list is empty....", replyMarkup: GetMainKeyboard());
            }
            break;
        
        case "Done" or "/done" :
            if (userHabbit.TryGetValue(chatId, out var habit) && habit.Count > 0)
            {
                var buttons = habit.Select((t, i)=>InlineKeyboardButton.WithCallbackData(
                    text: $"{i + 1}.{t.ToString()}",
                    callbackData: $"done:{i + 1}"))
                    .Chunk(5)
                    .Select(row => row.ToArray())
                    .ToArray();
                
                await bot.SendMessage(chatId, "<em>Choose what habit you want to mark as DONE </em>", ParseMode.Html,
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                );
            }
            else
            {
                await bot.SendMessage(chatId, " Your task list is empty....", replyMarkup: GetMainKeyboard());
            }
            break;
        case "Help" or  "/help":
                break;
        case "History" or "/history":
            if (doneHabits.TryGetValue(chatId, out var done) && done.Count > 0)
            {
                var list = string.Join("\n", done.Select((t, i) => $"{i + 1}. {t}"));
                await bot.SendMessage(chatId, "Your done habits:\n" + list);
            }
            else
            {
                await bot.SendMessage(chatId, " Your task list is empty....", replyMarkup: GetMainKeyboard());
            }

            break;
        default:
            await bot.SendMessage(chatId, $"User {message.From?.Username} said : {message.Text}");
            break;
            
    }
    
    
    Console.WriteLine($"Recieved {type} and '{message.Text}' in '{message.Chat}'");
    // await bot.SendMessage(message.Chat.Id, $"{message.From} said : {message.Text}");
}

// method that handle other types of updates received by the bot:
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        var chatId = query.Message!.Chat.Id;

        if (int.TryParse(query.Data, out var i) && userHabbit.TryGetValue(chatId,out var habits) 
                                                && i>0 && i<=habits.Count)
        {
            string removed = habits[i - 1];
            habits.RemoveAt(i - 1);
            await bot.SendMessage(chatId, $"Removed habit: <b>{removed}</b>",
                ParseMode.Html,
                replyMarkup: GetMainKeyboard());
        }

       else if (query.Data!.StartsWith("done:") &&
            int.TryParse(query.Data.Split(":")[1], out var k) &&
            userHabbit.TryGetValue(chatId, out var habit) &&
            k > 0 && k <= habit.Count)
        {
            string finished = habit[k - 1];
            habit.RemoveAt(k - 1);

            if (!doneHabits.ContainsKey(chatId))
                doneHabits[chatId] = new List<string>();
            doneHabits[chatId].Add(finished);

            await bot.SendMessage(chatId, $"The habit <b>{finished}</b> is now marked as DONE!",
                ParseMode.Html,
                replyMarkup: GetMainKeyboard());
        }
        else
        {
            await bot.SendMessage(chatId, "Invalid selected item", replyMarkup: GetMainKeyboard());
        }
        
    
        await bot.AnswerCallbackQuery(query.Id); // just dismiss loading animation
    }
}

// Helper: reply keyboard
    ReplyKeyboardMarkup GetMainKeyboard()
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


