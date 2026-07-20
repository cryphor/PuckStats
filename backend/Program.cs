using Microsoft.EntityFrameworkCore;
using Serilog;
using PuckStats.Api.Data;
using PuckStats.Api.Services;
using PuckStats.Api.Hubs;
using PuckStats.Analytics;

// Railway/cloud deployment: use PORT env var if present
var port = Environment.GetEnvironmentVariable("PORT");
var url = !string.IsNullOrEmpty(port) ? $"http://0.0.0.0:{port}" : null;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = "Production",
    ApplicationName = "PuckStats.Api"
});

// Clear default config sources (which use file watchers causing inotify issues)
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

if (url != null)
    builder.WebHost.UseUrls(url);

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Database — only configure if we have a valid connection string
var connStr = builder.Configuration.GetConnectionString("DefaultConnection") 
              ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
              ?? Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? "";
              
if (!string.IsNullOrEmpty(connStr))
{
    Log.Information("Configuring PostgreSQL: {ConnStrPrefix}", connStr.Length > 30 ? connStr[..30] + "..." : connStr);
    try
    {
        builder.Services.AddDbContext<PuckStatsDbContext>(options =>
            options.UseNpgsql(connStr));
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to configure PostgreSQL - running without database");
        builder.Services.AddDbContext<PuckStatsDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=inmemory;Username=postgres;Password=postgres"));
    }
}
else
{
    Log.Warning("No database connection string found — API will start without persistence");
    builder.Services.AddDbContext<PuckStatsDbContext>(options =>
        options.UseNpgsql("Host=localhost;Database=puckstats;Username=postgres;Password=postgres"));
}

// Services
builder.Services.AddSingleton<RatingEngine>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<ReplayService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddHostedService<TelemetryProcessorService>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 256 * 1024;
});

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PuckStats API", Version = "v1" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply migrations (try, but don't crash if DB not available yet)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PuckStatsDbContext>();
        db.Database.EnsureCreated();
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "Database migration failed — will retry on next request");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PuckStats API v1"));
}

app.UseSerilogRequestLogging();
app.UseCors("WebClient");
app.UseAuthorization();
app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");
app.MapHub<ReplayHub>("/hubs/replay");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Direct player endpoint (bypasses controller DI issues)
app.MapGet("/api/player/{steamId}", async (string steamId, PlayerService playerService) =>
{
    var profile = await playerService.GetPlayerProfile(steamId);
    return Results.Ok(profile);
});

// Debug: echo route
app.MapGet("/api/echo/{**rest}", (string rest) => Results.Ok(new { path = rest, time = DateTime.UtcNow }));

app.Run();
