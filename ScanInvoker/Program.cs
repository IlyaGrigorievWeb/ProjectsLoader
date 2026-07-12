using Microsoft.EntityFrameworkCore;
using Npgsql;
using ScanInvoker.Analyzers;
using ScanInvoker.Interfaces;
using ScanInvoker.Jobs;
using ScanInvoker.Services;
using Serilog;
using StackExchange.Redis;
using Storages.EntitiesStorage;

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
    .WriteTo.File(Path.Join(builder.Environment.ContentRootPath, "logs", "ScanInvoker.log")));

var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection"));

dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();

builder.Services.AddSingleton(dataSource);

builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(dataSource));

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

builder.Services.AddSingleton<IProjectAnalyzer, ClusteringProjectAnalyzer>();

builder.Services.AddSingleton<IArchiveService, ArchiveService>();

builder.Services.AddSingleton<IProjectClusteringService, ProjectClusteringService>();

builder.Services.AddHostedService<InvokeProjectScanner>();

var host = builder.Build();
host.Run();