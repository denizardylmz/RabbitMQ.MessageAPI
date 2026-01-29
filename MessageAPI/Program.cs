using MessageAPI.Abstractions;
using MessageAPI.API.Infrastructure;
using MessageAPI.Infrastructure.Interceptors;
using MessageAPI.Infrastructure.Settings;
using MessageService.Contracts;
using MessageService.Settings;
using TelegramBotService.Contracts;
using TelegramBotService.Settings;
using MessageAPI.Application.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MessageBusSettings>(
    builder.Configuration.GetSection("MessageBusSettings"));

builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection("Telegram"));

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("DatabaseOptions"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "DatabaseOptions:ConnectionString is required.")
    .ValidateOnStart();

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("DatabaseOptions"));

builder.Services.AddAPIServices();
builder.Services.AddMessagingServices();
builder.Services.AddTelegramServices();
builder.Services.AddDBServices();
builder.Services.AddApplicaitonServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public static class ServiceExtensions
{
    public static IServiceCollection AddAPIServices(
        this IServiceCollection services)
    {

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddControllers();
        //servicesout configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}



