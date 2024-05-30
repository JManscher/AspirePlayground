using AspirePlayground.Web.Frontend.Components;
using AspirePlayground.Web.Frontend.Customer;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDaprClient();

builder.Services.AddOutputCache();

builder.Services.AddSingleton<CustomerApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", true);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();