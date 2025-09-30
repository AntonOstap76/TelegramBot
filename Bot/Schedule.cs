using Bot.Services;
using Telegram.Bot;
using TelegramBot;

namespace Bot.Bot;

public class Schedule
{
    
    private readonly TimeSpan _dailyTime = new TimeSpan(9, 0, 0);
    private static Timer _dailyTimer;
    private readonly HabitService _habitService;
    private readonly TelegramBotClient _telegramBotClient;

    public Schedule(HabitService habitService, TelegramBotClient telegramBotClient)
    {
        _habitService = habitService;
        _telegramBotClient = telegramBotClient;
    }

    public void ScheduleNextMessage()
    {
        DateTime now = DateTime.Now;
        DateTime nextSend = new DateTime(now.Year, now.Month, now.Day,
            _dailyTime.Hours, _dailyTime.Minutes, 0);

        if (now > nextSend)
            nextSend = nextSend.AddDays(1); // schedule for tomorrow

        TimeSpan delay = nextSend - now;
        Console.WriteLine($"Next message will be sent in {delay.TotalMinutes:F2} minutes.");

        _dailyTimer?.Dispose();
        _dailyTimer = new Timer(async _ =>
        {
            await SendDailyMessage();

            // Reschedule for the next day
            ScheduleNextMessage();

        }, null, delay, Timeout.InfiniteTimeSpan); // run once, then reschedule
    }

    private async Task SendDailyMessage()
    {
        var users = await  _habitService.GetAllUsers();
        
        foreach (var chatId in users)
        {
            try
            {
                var habits = await _habitService.GetAllHabits(chatId);
                string habitsText = habits.Any()
                    ? string.Join("\n", habits.Select(h => h.Habit))
                    : "You don`t have any habits to track 🎉";
                
                await _telegramBotClient.SendMessage(chatId, $"Hello! This is your daily message  with all habits you need to do today!\n" +
                                                             $"{habitsText} ");
                Console.WriteLine($"Message sent to {chatId} at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message to {chatId}: {ex.Message}");
            }
        }
    }
}