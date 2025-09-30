using Microsoft.EntityFrameworkCore;
using TelegramBot;

namespace Bot.Services;

public class HabitService
{
    private readonly HabitContext _habitContext;

    public HabitService(HabitContext habitContext)
    {
        _habitContext = habitContext;
    }

    public async Task<Habits> AddHabit(long chatId, string title)
    {
        var habit = new Habits()
        {
            UserId = chatId,
            Habit = title,
            
        };
        _habitContext.Habits.Add(habit);
        await _habitContext.SaveChangesAsync();
        return habit;
    }

    public async Task<List<Habits>> GetAllHabits(long chatId, bool done = false)
    {
        return await _habitContext.Habits
            .Where(h => h.UserId == chatId )
            .OrderBy(h => h.Id)
            .ToListAsync();
    }

    public async Task<Habits?> RemoveHabit(long chatId, int index)
    {
        var habits = await GetAllHabits(chatId, done: false);
        if (index < 0 || index > habits.Count) return null;
        var habit = habits[index];
        _habitContext.Habits.Remove(habit);
        await _habitContext.SaveChangesAsync();
        return habit;
    }

    public async Task<Habits?> MarkAsDone(long chatId, int index)
    {
        var habits = await GetAllHabits(chatId); // get all habits 

        if (index < 0 || index >= habits.Count) return null;

        var habit = habits[index];
        var today = DateTime.UtcNow.Date;

        if (habit.LastCompletedDate.HasValue && habit.LastCompletedDate.Value.Date == today)
        {
            // Already done today
            return habit;
        }

        // Update streak
        if (habit.LastCompletedDate.HasValue && habit.LastCompletedDate.Value.Date == today.AddDays(-1))
            habit.CurrentStreak++;
        else
            habit.CurrentStreak = 1;

        habit.LastCompletedDate = today;
        habit.LongestStreak = Math.Max(habit.LongestStreak, habit.CurrentStreak);

        await _habitContext.SaveChangesAsync();
        return habit;
    }


    public async Task<List<Habits>> GetDoneHabits(long chatId)
    {
        return await _habitContext.Habits
            .Where(h => h.UserId == chatId && h.LastCompletedDate != null)
            .OrderByDescending(h => h.LastCompletedDate)
            .ToListAsync();
    }

    public async Task<List<long>> GetAllUsers()
    {
        return await _habitContext.Habits
            .Select(h => h.UserId).Distinct().ToListAsync();
    }
}