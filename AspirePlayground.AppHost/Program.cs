using Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);


// Disabled this part, as the emulator suffers from SSL issues
// var cosmos = builder.AddAzureCosmosDB("cosmosConnectionName");

// cosmos.RunAsEmulator();

builder.AddDapr((options) =>
{
    options.EnableTelemetry = true;
});

builder.AddProject<AspirePlayground_Web_Frontend>("webfrontend")
    .WithDaprSidecar("web");

builder.AddProject<AspirePlayground_Web_Backend>("webbackend")
    // .WithReference(cosmos)
    .WithDaprSidecar("bff");

builder.AddProject<AspirePlayground_CustomerService>("customer-service")
    // .WithReference(cosmos)
    .WithDaprSidecar("customerservice");

builder.AddProject<AspirePlayground_EventFeed>("eventfeed")
    // .WithReference(cosmos)
    .WithDaprSidecar("eventfeed");

// Workaround for https://github.com/dotnet/aspire/issues/2219
if (builder.Configuration.GetValue<string>("DAPR_CLI_PATH") is { } daprCliPath)
    builder.Services.Configure<DaprOptions>(options => { options.DaprPath = daprCliPath; });

builder.Build().Run();