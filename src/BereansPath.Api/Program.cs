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
    // EnsureCreated won't add new tables to an existing SQLite file — create memoirs if missing.
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "BookmarkNoteMemoirs" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_BookmarkNoteMemoirs" PRIMARY KEY AUTOINCREMENT,
            "BookmarkId" INTEGER NOT NULL,
            "NoteText" TEXT NOT NULL,
            "ArchivedAt" TEXT NOT NULL,
            CONSTRAINT "FK_BookmarkNoteMemoirs_Bookmarks_BookmarkId"
                FOREIGN KEY ("BookmarkId") REFERENCES "Bookmarks" ("Id") ON DELETE CASCADE
        );
        """);
    db.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_BookmarkNoteMemoirs_BookmarkId"
        ON "BookmarkNoteMemoirs" ("BookmarkId");
        """);
    logStore.Write("INFO", "Startup", "Database ready (including note memoirs)");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueDev");
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// SPA fallback for product/single-host mode (Vue built into wwwroot)
app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStarted.Register(() =>
    logStore.Write("INFO", "Startup", "Application started — open /logs in the Vue app or GET /api/diagnostics/logs"));

app.Run();
