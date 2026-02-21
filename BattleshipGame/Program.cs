using BattleshipGame.Components;
using BattleshipGame.Hubs;
using BattleshipGame.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor / Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SignalR
builder.Services.AddSignalR();

// Game services
builder.Services.AddSingleton<GameSessionService>();
builder.Services.AddHostedService<SessionCleanupService>();

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
