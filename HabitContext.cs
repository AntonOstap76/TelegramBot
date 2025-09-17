using Microsoft.EntityFrameworkCore;

namespace TelegramBot;

public class HabitContext:DbContext
{
    public HabitContext(DbContextOptions<HabitContext> options) : base(options) { }
    
    public DbSet<Model> Habits { get; set; }

    
    
}