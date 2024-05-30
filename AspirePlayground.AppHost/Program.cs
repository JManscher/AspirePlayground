using Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDapr((options) =>
{
    options.EnableTelemetry = true;
});

builder.AddProject<AspirePlayground_Web_Frontend>("webfrontend")
    .WithDaprSidecar("web");

builder.AddProject<AspirePlayground_Web_Backend>("webbackend")
    .WithDaprSidecar("bff");

builder.AddProject<AspirePlayground_CustomerService>("customerservice")
    .WithDaprSidecar("customerservice");

builder.AddProject<AspirePlayground_EventFeed>("eventfeed")
    .WithDaprSidecar("eventfeed");

// Workaround for https://github.com/dotnet/aspire/issues/2219
if (builder.Configuration.GetValue<string>("DAPR_CLI_PATH") is { } daprCliPath)
    builder.Services.Configure<DaprOptions>(options => { options.DaprPath = daprCliPath; });

builder.Build().Run();