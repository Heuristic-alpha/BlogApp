global using BlazorAdminPanel;
global using BlazorAdminPanel.Services;
global using BlogApp.Infrastructures.Localization;
global using BlogApp.Services;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthenticationStateProvider, AuthServices>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<NavManagerHistory>();
builder.Services.AddScoped<Blackboard>();
builder.Services.AddSingleton(sp => new JsonSerializerOptions(JsonSerializerDefaults.Web));
builder.Services.AddSingleton<LocalManager>();

var host = builder.Build();

LocalManager localManager = host.Services.GetRequiredService<LocalManager>();
HttpClient http = host.Services.GetRequiredService<HttpClient>();
string? localizationPayload = await http.GetStringAsync(LocalManager.LocalizationFileURL);
await localManager.LoadFromJsonAsync(localizationPayload!);

await host.RunAsync();