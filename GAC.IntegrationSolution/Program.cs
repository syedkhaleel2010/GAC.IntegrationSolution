using GAC.Integration.Infrastructure.ApiClients;
using GAC.Integration.Scheduler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddHttpClient<WmsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WmsApi:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register Background Service
builder.Services.AddHostedService<FilePollingService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
