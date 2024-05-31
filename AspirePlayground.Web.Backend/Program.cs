using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.Web.Backend.Customers;
using Dapr.Client;
using static AspirePlayground.Web.Backend.Customers.Delegates.Customers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddDaprClient();
builder.AddAzureCosmosClient("cosmosConnectionName");

// Add services to the container.
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

//app.UseCloudEvents();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapGet("/customer/{id:guid}", GetCustomerById)
    .WithName("GetCustomerById")
    .WithOpenApi();

app.MapPost("/customer", CreateCustomer)
    .WithName("CreateCustomer")
    .WithOpenApi();

app.MapPut("/customer/{id:guid}", UpdateCustomer)
    .WithName("UpdateCustomer")
    .WithOpenApi();

app.MapGet("/customer", GetCustomers)
    .WithName("GetCustomers")
    .WithOpenApi();

app.MapPost("/customer/events", ProcessCustomerEvents)
    .WithTopic("pubsub", "customer-changed")
    .WithOpenApi();

app.MapDefaultEndpoints();

var daprClient = app.Services.GetRequiredService<DaprClient>();
// For test purposes we will request a resync of all customers
await daprClient.InvokeMethodAsync("customerservice", "customer/events/republish", new { RequestId = Guid.NewGuid() });

app.Run();