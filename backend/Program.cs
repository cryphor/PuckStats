using Microsoft.EntityFrameworkCore;
using Serilog;
using PuckStats.Api.Data;
using PuckStats.Api.Services;
using PuckStats.Api.Hubs;
using PuckStats.Analytics;

var builder = WebApplication.CreateBuilder(args);

// Railway/cloud deployment: use PORT env var if present
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<PuckStatsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        db.Database.Migrate();
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
app.MapHealthChecks("/health");

app.Run();
