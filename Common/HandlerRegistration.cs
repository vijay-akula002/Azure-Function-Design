using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace Common;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddActionLabelHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t =>
                typeof(IActionLabelHandler).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .ToList();

        // Scrutor scanning (for constructor DI support)
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo<IActionLabelHandler>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        foreach (var type in handlerTypes)
        {
            var attr = type.GetCustomAttribute<ActionAttribute>();

            if (attr == null)
                continue;

            services.AddKeyedSingleton(
                typeof(IActionLabelHandler),
                attr.Action,
                type);
        }

        return services;
    }
}