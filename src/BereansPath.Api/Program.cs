using BereansPath.Api.Data;
using BereansPath.Api.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var logStore = new AppLogStore(builder.Environment);
builder.Services.AddSingleton(logStore);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new AppLoggerProvider(logStore));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=bereans.db";
var provider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";

if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueDev", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

logStore.Write("INFO", "Startup", $"Bereans Path API starting ({app.Environment.EnvironmentName})");
logStore.Write(
    "INFO",
    "Startup",
    string.IsNullOrWhiteSpace(app.Configuration["ESV_API_KEY"])
        ? "ESV_API_KEY is NOT configured"
        : "ESV_API_KEY is configured");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    logStore.Write("INFO", "Startup", "Database EnsureCreated completed");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueDev");
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
    logStore.Write("INFO", "Startup", "Application started — open /logs in the Vue app or GET /api/diagnostics/logs"));

app.Run();
