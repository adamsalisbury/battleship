using BattleshipGame.Components;
using BattleshipGame.Hubs;
using BattleshipGame.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Razor / Blazor Server
// Keep circuits alive for 30 s after connection drop — quick enough for
// immediate disconnect detection (OnConnectionDownAsync), while still
// allowing Blazor's built-in auto-reconnect to work for brief drops.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(opts =>
    {
        opts.DisconnectedCircuitMaxRetained = 20;
        opts.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(30);
    });

// SignalR
builder.Services.AddSignalR();

// Game services
builder.Services.AddSingleton<GameSessionService>();
builder.Services.AddHostedService<SessionCleanupService>();

// Disconnect detection — scoped so each circuit gets its own instance
builder.Services.AddScoped<PlayerCircuitTracker>();
builder.Services.AddScoped<CircuitHandler, GameCircuitHandler>();

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// SignalR hub
app.MapHub<BattleshipHub>("/hubs/battleship");

// Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
