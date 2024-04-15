using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Core.Configuration;

public record GrpcConfig(string Host, string Port, bool EnableTls) {
    public GrpcConfig() : this(string.Empty, string.Empty, false) {}

    public string Scheme => EnableTls ? "https" : "http";
    public string Address => $"{Scheme}://{Host}:{Port}";

    private static GrpcChannelOptions BuildOptions(GrpcConfig grpcConfig) {
        return new GrpcChannelOptions() {
            Credentials = grpcConfig.EnableTls ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure
        };
    }

    public static Grpc.LobbyService.LobbyServiceClient LobbyServiceClientFactory(IServiceProvider serviceProvider) {

        GrpcConfig grpcConfig = serviceProvider.GetService<GrpcConfig>() ?? new GrpcConfig();
        GrpcChannelOptions grpcChannelOptions = BuildOptions(grpcConfig);
        GrpcChannel grpcChannel = GrpcChannel.ForAddress(grpcConfig.Address, grpcChannelOptions);

        return new Grpc.LobbyService.LobbyServiceClient(grpcChannel);

    }

}
