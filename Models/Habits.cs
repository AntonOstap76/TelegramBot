namespace TelegramBot;


public class Habits
{
    public int Id { get; set; }           // Primary Key
    public long UserId { get; set; }      // Telegram chat ID
    public string Habit { get; set; } = "";
   
    public DateTime? LastCompletedDate { get; set; } = null;
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
}