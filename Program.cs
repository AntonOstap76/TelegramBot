
using Bot.Bot;
using Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBot;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json",true,true);
        var config = builder.Build();
        
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(config);

        services.AddDbContext<HabitContext>(options =>
            options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));

        services.AddSingleton<TelegramBotClient>(t =>
            new TelegramBotClient(config["TOKEN"]!));
        
        services.AddSingleton<KeyboardService>();
        services.AddScoped<HabitService>();
        
         //for user state
        services.AddSingleton(new Dictionary<long, string?>());
        
        services.AddTransient<MessageHandler>();
        services.AddTransient<CallbackHandler>();
        services.AddScoped<Schedule>();
        services.AddTransient<BotService>();

        var provider = services.BuildServiceProvider();

        var botService = provider.GetRequiredService<BotService>();
        await botService.StartAsync();
    }
}