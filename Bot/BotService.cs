using System.Reflection.Metadata;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Bot;

public class BotService
{
    private readonly TelegramBotClient _telegramBotClient;
    private readonly MessageHandler _messageHandler;
    private readonly CallbackHandler _callbackHandler; 
    private CancellationTokenSource? _cts;
    private readonly Schedule _schedule;

    public BotService(TelegramBotClient telegramBotClient ,MessageHandler messageHandler ,
        CallbackHandler callbackHandler, Schedule schedule)
    {
        
        _messageHandler = messageHandler;
        _callbackHandler = callbackHandler;
        _telegramBotClient = telegramBotClient;
        _schedule = schedule;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        
        // Start receiving updates in the background
        _telegramBotClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            cancellationToken: _cts.Token
            );
        
        // Console.WriteLine($"@{me.Username} is running. If You click Enter i finish my job ");
        // Console.ReadLine(); 
        // await _cts.CancelAsync();
        
        var me = await _telegramBotClient.GetMe();
        Console.WriteLine($"Bot {me.Username} started.");
        
        _schedule.ScheduleNextMessage();
        
        // keep running forever
        await Task.Delay(-1, _cts.Token);
        
    }

    // Handles incoming updates
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        
        if (update.Message != null)
        {
            await _messageHandler.HandleAsync(update.Message);
        }
        else if (update.CallbackQuery != null)
        {
            await _callbackHandler.HandleAsync(update.CallbackQuery);
        }
        
    }
    
    // Handles polling errors
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}