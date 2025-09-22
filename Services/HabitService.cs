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
            IsDone = false
        };
        _habitContext.Habits.Add(habit);
        await _habitContext.SaveChangesAsync();
        return habit;
    }

    public async Task<List<Habits>> GetAllHabits(long chatId, bool done = false)
    {
        return await _habitContext.Habits
            .Where(h => h.UserId == chatId && h.IsDone == done)
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
        var habits = await GetAllHabits(chatId, done: false);

        if (index < 0 || index > habits.Count) return null;

        var habit = habits[index];

        habit.IsDone = true;
        await _habitContext.SaveChangesAsync();
        return habit;

    }

    public async Task<List<Habits>> GetDoneHabits(long chatId)
    {
        return await GetAllHabits(chatId, done:true);
    }
    
}