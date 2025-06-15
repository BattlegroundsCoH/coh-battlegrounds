using Serilog.Core;
using Serilog.Events;

namespace Battlegrounds.Logging;

public sealed class ClassSourceEnricher : ILogEventEnricher {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
        var sourceContext = logEvent.Properties.GetValueOrDefault("SourceContext")?.ToString();
        if (sourceContext != null) {
            var className = sourceContext.Split('.').LastOrDefault();
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ClassName", className?.TrimEnd('"') ?? "???"));
        } else {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ClassName", "???"));
        }
    }
}
