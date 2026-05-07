using BlazorAdminPanel;
using BlazorAdminPanel.Services;
using BlogApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthenticationStateProvider, AuthServices>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<NavManagerHistory>();
builder.Services.AddScoped<Blackboard>();
builder.Services.AddSingleton(sp => new JsonSerializerOptions(JsonSerializerDefaults.Web));

var host = builder.Build();

await host.RunAsync();
