using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using web;
using web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? "https://bc-integration-po-cqhxfrcdakbmgfdd.northeurope-01.azurewebsites.net/api/";
var functionKey = builder.Configuration["FunctionKey"] ?? "";

builder.Services.AddScoped<BcApiService>();
builder.Services.AddScoped(sp => new HttpClient(new FunctionKeyHandler(functionKey))
{
    BaseAddress = new Uri(apiBaseUrl)
});

await builder.Build().RunAsync();
