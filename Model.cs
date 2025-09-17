namespace TelegramBot;


public class Model
{
    public int Id { get; set; }           // Primary Key
    public long UserId { get; set; }      // Telegram chat ID
    public string Habit { get; set; } = "";
    public bool IsDone { get; set; } = false;
}