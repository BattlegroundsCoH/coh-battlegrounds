using System.Text;
using System.Text.Json;

using Battlegrounds.Grpc;

using Google.Protobuf;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobbyResources(LobbyService.LobbyServiceClient client, ILogger logger) {

    private readonly LobbyService.LobbyServiceClient _client = client;
    private readonly ILogger _logger = logger;

    public const int CHUNK_SIZE = 2048;

    public async Task<bool> UploadResourceAsync(LobbyUserContext userContext, string resourceId, LobbyResourceKind resourceKind, Stream inputStream) {

        int chunks = (int)((inputStream.Length + CHUNK_SIZE - 1) / CHUNK_SIZE);

        try {

            var call = _client.UploadLobbyResource();
            byte[] buffer = new byte[CHUNK_SIZE];
            for (int i = 0; i < chunks; i++) {
                int read = await inputStream.ReadAsync(buffer.AsMemory(0, CHUNK_SIZE));
                await call.RequestStream.WriteAsync(new UploadLobbyResourceRequest {
                    ChunkThis = i,
                    ChunkTotal = chunks,
                    Contents = ByteString.CopyFrom(buffer, 0, read),
                    Kind = resourceKind,
                    Resource = resourceId,
                    User = userContext
                });
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;
            if (response is null) {
                return false;
            }

            return response.Received == inputStream.Length;

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed uploading lobby resource {name} : {kind}", resourceId, resourceKind);
            return false;
        }

    }

    public async Task<bool> DownloadResourceAsync(LobbyUserContext userContext, string resourceId, Stream outputStream) {

        using BinaryWriter writer = new BinaryWriter(outputStream, Encoding.UTF8, true);

        try {
            var call = _client.DownloadLobbyResource(new DownloadLobbyResourceRequest { Resource = resourceId, User = userContext });

            await foreach (var response in call.ResponseStream.ReadAllAsync()) {
                writer.Write(response.Contents.Memory.Span);
                if (response.ChunkThis + 1 == response.ChunkTotal) {
                    break;
                }
            }

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed downloading lobby resource {name}", resourceId);
            return false;
        }

        return true;

    }

    public async Task<ICollection<Stream>> DownloadResourceBundleAsync(LobbyUserContext userContext, LobbyResourceKind resourceKind) {

        // Download a manifest file (a json string array)
        using var manifestStream = new MemoryStream();
        if (!await DownloadResourceAsync(userContext, resourceKind.ToString(), manifestStream)) {
            return [];
        }

        manifestStream.Seek(0, SeekOrigin.Begin);

        var manifest = await JsonSerializer.DeserializeAsync<string[]>(manifestStream);
        if (manifest is null) {
            return [];
        }

        LinkedList<Stream> list = new LinkedList<Stream>();
        for (int i = 0; i < manifest.Length; i++) {
            var memoryStream = new MemoryStream();
            if (await DownloadResourceAsync(userContext, manifest[i], memoryStream)) {
                list.AddLast(memoryStream);
            }
        }

        return list;

    }

    public async IAsyncEnumerable<Stream> DownloadIterableResourceBundleAsync(LobbyUserContext userContext, LobbyResourceKind resourceKind) {

        // Download a manifest file (a json string array)
        using var manifestStream = new MemoryStream();
        if (!await DownloadResourceAsync(userContext, resourceKind.ToString(), manifestStream)) {
            yield break;
        }

        manifestStream.Seek(0, SeekOrigin.Begin);

        var manifest = await JsonSerializer.DeserializeAsync<string[]>(manifestStream);
        if (manifest == null || manifest.Length == 0) {
            yield break;
        }

        // Download each resource specified in the manifest
        foreach (var resource in manifest) {
            var memoryStream = new MemoryStream();

            if (await DownloadResourceAsync(userContext, resource, memoryStream)) {
                memoryStream.Seek(0, SeekOrigin.Begin);
                yield return memoryStream; // Yield each stream as it's downloaded
            }

        }

    }

}
