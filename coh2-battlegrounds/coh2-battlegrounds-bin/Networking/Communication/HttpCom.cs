using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Battlegrounds.Networking.Communication;

/// <summary>
/// Represents HTTP response data to a POST request.
/// </summary>
public readonly struct HttpPOSTResponseData {

    /// <summary>
    /// Flag showing if the request yielded a successful response.
    /// </summary>
    public readonly bool Success;

    /// <summary>
    /// The status string of the response.
    /// </summary>
    public readonly string Status;

    /// <summary>
    /// The received data content of the response.
    /// </summary>
    public readonly MemoryStream Content;

    /// <summary>
    /// Initialize a new <see cref="HttpPOSTResponseData"/> instance with fields defined.
    /// </summary>
    /// <param name="b">Is the request responded to with a success code.</param>
    /// <param name="s">The status code of the response.</param>
    /// <param name="p">The content sent with the response.</param>
    /// <param name="encoding">The encoding of the response data.</param>
    public HttpPOSTResponseData(bool b, string s, Stream p, Encoding encoding) {
        this.Success = b;
        this.Status = s;
        this.Content = new MemoryStream();
        using (BinaryReader reader = new BinaryReader(p)) {
            using (BinaryWriter writer = new BinaryWriter(this.Content, encoding, true)) {
                byte[] buffer = reader.ReadBytes((int)p.Length);
                writer.Write(buffer);
            }
        }
        this.Content.Position = 0;
    }

}

/// <summary>
/// Represents HTTP response data to a GET request.
/// </summary>
public readonly struct HttpGETResponseData {

    /// <summary>
    /// Flag showing if the request yielded a successful response.
    /// </summary>
    public readonly bool Success;

    /// <summary>
    /// The received data content of the response.
    /// </summary>
    public readonly MemoryStream Content;

    /// <summary>
    /// Get the <see cref="Content"/> parsed as a string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The received status code.
    /// </summary>
    public readonly HttpStatusCode StatusCode;

    /// <summary>
    /// Initialize a new <see cref="HttpGETResponseData"/> instance with fields defined.
    /// </summary>
    /// <param name="b">Is the request responded to with a success code.</param>
    /// <param name="s">The returned body stream content of the response.</param>
    /// <param name="encoding">The encoding of the received stream content.</param>
    /// <param name="httpStatus">The status code of the response.</param>
    public HttpGETResponseData(bool b, Stream s, Encoding encoding, HttpStatusCode httpStatus) {
        this.StatusCode = httpStatus;
        this.Success = b;
        this.Content = new MemoryStream();
        using (BinaryReader reader = new BinaryReader(s)) {
            using (BinaryWriter writer = new BinaryWriter(this.Content, encoding, true)) {
                byte[] buffer = reader.ReadBytes((int)s.Length);
                writer.Write(buffer);
            }
        }
        this.Content.Position = 0;
        this.Value = new StreamReader(this.Content).ReadToEnd();
        this.Content.Position = 0;
    }
}

/// <summary>
/// Static extension class for send HTTP requests (GET and POST)
/// </summary>
public static class HttpCom {

    /// <summary>
    /// Send a POST request to <paramref name="address"/> with <paramref name="data"/>.
    /// </summary>
    /// <param name="client">The client to use for sending the request.</param>
    /// <param name="address">The address of the server to send request to.</param>
    /// <param name="api">The api to invoke with request.</param>
    /// <param name="data">The data to post.</param>
    /// <param name="port">The specific port to use when sending the request.</param>
    /// <returns>A <see cref="HttpPOSTResponseData"/> instance containing the response to the request.</returns>
    public static HttpPOSTResponseData POST(this HttpClient client, string address, string api, HttpContent data, int port = 80) {

        // Create request
        var request = new HttpRequestMessage {
            RequestUri = new Uri($"http://{address}:{port}/api/{api}"),
            Method = HttpMethod.Post,
            Content = data,
        };

        // Send 
        var response = client.Send(request);

        // Log API call
        if (NetworkInterface.LogAPICalls) {
            Trace.WriteLine($"[HTTP] POST -> {api} returned : {response.StatusCode}", nameof(HttpCom));
        }

        // Ensure response is OK
        return new HttpPOSTResponseData(response.IsSuccessStatusCode, response.ReasonPhrase ?? string.Empty, response.Content.ReadAsStream(), Encoding.ASCII);

    }

    /// <summary>
    /// Send a POST request to <paramref name="address"/> with <paramref name="data"/>.
    /// </summary>
    /// <param name="client">The client to use for sending the request.</param>
    /// <param name="address">The address of the server to send request to.</param>
    /// <param name="api">The api to invoke with request.</param>
    /// <param name="data">The data to post.</param>
    /// <param name="port">The specific port to use when sending the request.</param>
    /// <returns>A <see cref="HttpPOSTResponseData"/> instance containing the response to the request.</returns>
    public static async Task<HttpPOSTResponseData> AsyncPOST(this HttpClient client, string address, string api, HttpContent data, int port = 80)
        => await Task.Run(() => POST(client, address, api, data, port));

    /// <summary>
    /// Send a GET request to <paramref name="address"/> using specified <paramref name="api"/> query string.
    /// </summary>
    /// <param name="client">The client to use for sending the request.</param>
    /// <param name="address">The address of the server to send request to.</param>
    /// <param name="api">The api to invoke with request.</param>
    /// <param name="port">The specific port to use when sending the request.</param>
    /// <param name="log">Should the runtime log this GET request.</param>
    /// <returns>A <see cref="HttpGETResponseData"/> instance containing the response to the request.</returns>
    public static HttpGETResponseData GET(this HttpClient client, string address, string api, int port = 80, bool log = true) {

        // Create request
        var request = new HttpRequestMessage {
            RequestUri = new Uri($"http://{address}:{port}/api/{api}"),
            Method = HttpMethod.Get,
        };

        // Send and get response
        using var response = client.Send(request);

        // Log API call
        if (log && NetworkInterface.LogAPICalls) {
            Trace.WriteLine($"[HTTP] GET -> {api} returned : {response.StatusCode}", nameof(HttpCom));
        }

        // If success
        if (response.IsSuccessStatusCode) {

            // Read stream and return
            return new HttpGETResponseData(true, response.Content.ReadAsStream(), Encoding.ASCII, response.StatusCode);

        } else {

            // Return a null response
            return new HttpGETResponseData(false, response.Content.ReadAsStream(), Encoding.ASCII, response.StatusCode);

        }

    }

    /// <summary>
    /// Send a GET request to <paramref name="address"/> using specified <paramref name="api"/> query string.
    /// </summary>
    /// <param name="client">The client to use for sending the request.</param>
    /// <param name="address">The address of the server to send request to.</param>
    /// <param name="api">The api to invoke with request.</param>
    /// <param name="port">The specific port to use when sending the request.</param>
    /// <param name="log">Should the runtime log this GET request.</param>
    /// <returns>A <see cref="HttpGETResponseData"/> instance containing the response to the request.</returns>
    public static async Task<HttpGETResponseData> AsyncGET(this HttpClient client, string address, string api, int port = 80, bool log = true)
        => await Task.Run(() => GET(client, address, api, port, log));

    /// <summary>
    /// Send a DELETE request to <paramref name="address"/> using specified <paramref name="api"/> query string.
    /// </summary>
    /// <param name="client">The client to use for sending the request.</param>
    /// <param name="address">The address of the server to send request to.</param>
    /// <param name="api">The api to invoke with request.</param>
    /// <param name="port">The specific port to use when sending the request.</param>
    /// <param name="log">Should the runtime log this GET request.</param>
    /// <returns>If server returns a success code, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public static bool DELETE(this HttpClient client, string address, string api, int port, bool log = true) {
        
        // Create request
        var request = new HttpRequestMessage {
            RequestUri = new Uri($"http://{address}:{port}/api/{api}"),
            Method = HttpMethod.Delete,
        };

        // Send and get response
        using var response = client.Send(request);

        // Log API call
        if (log && NetworkInterface.LogAPICalls) {
            Trace.WriteLine($"[HTTP] DELETE -> {api} returned : {response.StatusCode}", nameof(HttpCom));
        }

        // If success
        return response.IsSuccessStatusCode;

    }

}
