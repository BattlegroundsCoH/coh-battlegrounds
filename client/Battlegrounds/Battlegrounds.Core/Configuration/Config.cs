using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Battlegrounds.Core.Configuration;

public class Config {

    public GrpcConfig GrpcConfig { get; set; } = new GrpcConfig("localhost", "8443", false);

    public void Inject(IServiceCollection services) {
        services.AddSingleton(this);
        services.AddSingleton(this.GrpcConfig);
    }

    public static Config FromStream(Stream fs) {

        var deserialiser = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

        try {
            using var sr = new StreamReader(fs);

            return deserialiser.Deserialize<Config>(sr);
        } catch {
            return new Config();
        }

    }

}

public static class ConfigDI {
    public static IServiceCollection AddConfig(this IServiceCollection services, Stream source) {
        Config config = Config.FromStream(source);
        config.Inject(services);
        return services;
    }
}
