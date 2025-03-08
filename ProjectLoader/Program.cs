using ProjectLoader.Services;
using Serilog;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddSerilog(lc => lc
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(Path.Join(builder.Environment.ContentRootPath, "logs", "ProjectLoader.log")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>().GetValue<string>("Redis:Queue")));

builder.Services.AddSingleton<Func<string, IConnectionMultiplexer>>(sp => name =>
{
    var connections = sp.GetServices<IConnectionMultiplexer>().ToList();
    return name switch
    {
        "queue" => connections[0],
        _ => throw new ArgumentException("Unknown Redis connection name")
    };
});

builder.Services.AddHostedService<ProjectLoaderService>();
builder.Services.AddHttpClient();

var host = builder.Build();
host.Run();