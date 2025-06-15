using System.Net.Http;

namespace Battlegrounds.Facades.API;

/// <summary>
/// Represents an asynchronous HTTP client capable of sending HTTP requests and receiving responses.
/// </summary>
/// <remarks>This interface provides a method for sending HTTP requests asynchronously. Implementations of this
/// interface should handle the underlying HTTP communication and return the response to the caller.</remarks>
public interface IAsyncHttpClient {
    
    /// <summary>
    /// Sends an HTTP request asynchronously and returns the response.
    /// </summary>
    /// <remarks>This method performs an asynchronous HTTP request using the provided <see
    /// cref="HttpRequestMessage"/>. Ensure that the request is properly configured, including the URI, HTTP method,
    /// headers, and content. The caller is responsible for disposing of the returned <see
    /// cref="HttpResponseMessage"/>.</remarks>
    /// <param name="request">The <see cref="HttpRequestMessage"/> representing the HTTP request to send. Must not be <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the  <see
    /// cref="HttpResponseMessage"/> received from the server.</returns>
    Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request);

}
