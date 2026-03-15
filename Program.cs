using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using NamesFunctionApp.Services;
using Common;
using A;
using B;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddHttpClient<INamesBackendClient, NamesBackendClient>();

builder.Services.AddSingleton<HandlerResolver>();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<IActionLabelHandler>()
    .AddClasses(c => c.AssignableTo<IActionLabelHandler>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());

builder.Build().Run();
