using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NamesFunctionApp.Services;
using Common;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddHttpClient<INamesBackendClient, NamesBackendClient>();

builder.Services.AddActionLabelHandlers(typeof(IActionLabelHandler).Assembly);

builder.Build().Run();
