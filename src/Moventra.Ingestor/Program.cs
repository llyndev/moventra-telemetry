using MassTransit;
using Moventra.Ingestor.Consumers;
using Microsoft.EntityFrameworkCore;
using Moventra.Ingestor.Data;

var builder = Host.CreateApplicationBuilder(args);

var rabbitHost = builder.Configuration["RabbitMq:host"]
                 ?? throw new InvalidOperationException("RabbitMq:Host não configurado.");

var rabbitUser = builder.Configuration["RabbitMq:Username"]
                 ?? throw new InvalidOperationException("RabbitMq:Username não configurado");

var rabbitPassword = builder.Configuration["RabbitMq:Password"]
                     ?? throw new InvalidOperationException("RabbitMq:Password não configurado");

var connectionString = builder.Configuration.GetConnectionString("Telemetry")
                       ?? throw new InvalidOperationException("ConnectionStrings:Telemetry não configurada.");

builder.Services.AddDbContext<TelemetryDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddMassTransit(x =>
{
    // Nomes das filas no formato "telemetry-recorded"
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<TelemetryRecordedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPassword);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();