using MessageService.Contracts;
using MessageService.Services;
using MessageService.Settings;
using Microsoft.AspNetCore.Builder;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MessageBusSettings>(
    builder.Configuration.GetSection("MessageBusSettings"));

builder.Services.AddMessagingServices();


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
    public static IServiceCollection AddApplicaitonServices(
        this IServiceCollection services)
    {

        services.AddControllers();
        //servicesout configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}



