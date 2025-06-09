using System.Net;
using System.Net.Http;

using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

public sealed class AsyncHttpClient(HttpClient httpClient, Configuration configuration, ILogger<AsyncHttpClient> logger) : IAsyncHttpClient {

    public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request) {
        using var context = new CancellationTokenSource(configuration.API.RequestTimeout);
        try {
            return await httpClient.SendAsync(request, context.Token);
        } catch (TaskCanceledException) when (!context.Token.IsCancellationRequested) {
            logger.LogError("Request to {RequestUri} was canceled.", request.RequestUri);
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout) {
                RequestMessage = request,
                ReasonPhrase = "Request timed out or was canceled."
            };
        } catch (TaskCanceledException) {
            logger.LogError("Request to {RequestUri} timed out after {Timeout} seconds.", request.RequestUri, configuration.API.RequestTimeout.TotalSeconds);
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout) {
                RequestMessage = request,
                ReasonPhrase = "Request timed out or was canceled."
            };
        } catch (Exception ex) {
            logger.LogError(ex, "An error occurred while sending request to {RequestUri}.", request.RequestUri);
            return new HttpResponseMessage(HttpStatusCode.Conflict) {
                RequestMessage = request,
                ReasonPhrase = ex.Message
            };
        }
    }

}
