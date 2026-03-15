using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Common;
namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTECFunctionKernel(this IServiceCollection services)
    {
        services.AddSingleton<TECFunctionKernel>();

        services.AddActionLabelHandlers(typeof(IAction).Assembly);
        services.AddBackendSoapClients(typeof(ITECBackendSoapClient).Assembly);

        return services;
    }

    public static IServiceCollection AddBackendSoapClients(
        this IServiceCollection services,
        Assembly assembly)
    {
        var clients = assembly
            .GetTypes()
            .Where(t =>
                typeof(ITECBackendSoapClient).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract);

        foreach (var clientType in clients)
        {
            var instance = (ITECBackendSoapClient)Activator.CreateInstance(clientType)!;

            var key = instance.BackendName;

            services.AddKeyedSingleton(typeof(ITECBackendSoapClient), key, clientType);
        }

        return services;
    }

    public static IServiceCollection AddActionLabelHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        var handlers = assembly
            .GetTypes()
            .Where(t => typeof(IAction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in handlers)
        {
            var attr = type.GetCustomAttribute<ActionAttribute>();
            if (attr == null)
                continue;
            services.AddKeyedSingleton(typeof(IAction), attr.Action, type);
        }
        return services;
    }
}