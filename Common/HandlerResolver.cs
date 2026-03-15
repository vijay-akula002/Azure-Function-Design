using System.Reflection;

namespace Common;

public class HandlerResolver
{
    private readonly Dictionary<string, IActionLabelHandler> _handlers;

    public HandlerResolver(IEnumerable<IActionLabelHandler> handlers)
    {
        _handlers = handlers
            .Select(h =>
            {
                var attr = h.GetType()
                    .GetCustomAttribute<ActionAttribute>();

                if (attr == null)
                    throw new InvalidOperationException(
                        $"{h.GetType().Name} missing ActionAttribute");

                return new { attr.Action, Handler = h };
            })
            .ToDictionary(
                x => x.Action,
                x => x.Handler,
                StringComparer.OrdinalIgnoreCase);
    }

    public IActionLabelHandler Resolve(string action)
    {
        if (!_handlers.TryGetValue(action, out var handler))
            throw new InvalidOperationException(
                $"No handler registered for SOAP action '{action}'");

        return handler;
    }
}