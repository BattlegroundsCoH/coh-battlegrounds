using System.Net;
using System.Net.Http;

using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

/// <summary>
/// Provides an asynchronous HTTP client for sending HTTP requests with configurable timeouts and integrated logging
/// support.
/// </summary>
/// <remarks>AsyncHttpClient is designed for scenarios where HTTP requests need to be sent asynchronously with
/// consistent timeout handling and error logging. The client wraps an existing HttpClient instance and applies the
/// configured timeout to each request. It is thread-safe and intended for use in applications that require robust HTTP
/// communication with detailed diagnostics.</remarks>
/// <param name="httpClient">The underlying HTTP client used to send requests.</param>
/// <param name="configuration">The configuration settings that control request behavior, such as timeouts.</param>
/// <param name="logger">The logger used to record request and error information.</param>
public sealed class AsyncHttpClient(HttpClient httpClient, Configuration configuration, ILogger<AsyncHttpClient> logger) : IAsyncHttpClient {

    /// <summary>
    /// Sends an HTTP request asynchronously and returns the response message.
    /// </summary>
    /// <remarks>The request will automatically time out after the configured request timeout period. The
    /// returned response may have a status code of RequestTimeout or Conflict to indicate timeout or error conditions,
    /// respectively. Callers should check the response's StatusCode property to determine the outcome of the
    /// request.</remarks>
    /// <param name="request">The HTTP request message to send. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message received
    /// from the server. If the request times out or is canceled, the response will have a status code of
    /// RequestTimeout. If an error occurs, the response will have a status code of Conflict and the reason phrase will
    /// contain the error message.</returns>
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
