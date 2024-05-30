using AspirePlayground.EventFeed;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDaprClient();

builder.AddAzureCosmosClient("cosmosConnectionName");

builder.Services.AddHostedService<CustomerEventWorker>();

builder.AddServiceDefaults();

var host = builder.Build();

host.Run();