using System.Threading.Channels;
using AspirePlayground.CustomerService.Delegates;
using AspirePlayground.CustomerService.Model;
using AspirePlayground.CustomerService.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddAzureCosmosClient("cosmosConnectionName");

// Add services to the container.
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDaprClient();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddKeyedSingleton(Channel.CreateUnbounded<RepublishCustomerEvents>(), "customer-events");
builder.Services.AddHostedService<RepublishCustomerFeed>();

var app = builder.Build();

app.UseCloudEvents();
app.UseRouting();
app.MapSubscribeHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapGet("/customer/{id:guid}", CustomerDelegates.GetCustomerById)
    .WithName("GetCustomerById")
    .WithOpenApi();

app.MapPost("/customer/events", CustomerEventDelegates.ProcessCustomerEvents)
    .WithTopic("pubsub", "customer-events")
    .WithOpenApi();

app.MapPost("/customer/events/republish", CustomerDelegates.RepublishCustomerEvents)
    .WithName("RepublishCustomerEvents")
    .WithOpenApi();

app.MapDefaultEndpoints();

app.Run();