using DotnetEventsViewer;
using DotnetEventsViewer.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<StateContainer>();
builder.Services.AddFluentUIComponents(config =>
{
    config.DefaultStyles.FluentStackHorizontalGap = "10px";
    config.DefaultStyles.FluentStackVerticalGap = "10px";
});

await builder.Build().RunAsync();
