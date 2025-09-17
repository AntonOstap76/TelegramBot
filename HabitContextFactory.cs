using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace TelegramBot;

public class HabitContextFactory: IDesignTimeDbContextFactory<HabitContext>
{
public HabitContext CreateDbContext(string[] args)
{
    // var configuration = new ConfigurationBuilder()
    //     .SetBasePath(Directory.GetCurrentDirectory())
    //     .AddJsonFile("appsettings.json")
    //     .Build();
    
    var builder = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json", true, true);
    var config = builder.Build();

    var connectionString = config["ConnectionStrings:DefaultConnection"];

    var optionsBuilder = new DbContextOptionsBuilder<HabitContext>();
    optionsBuilder.UseSqlServer(connectionString);

    return new HabitContext(optionsBuilder.Options);
}
}




