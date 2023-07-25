using System;
using System.IO;
using System.Net.Http;

namespace Battlegrounds.Networking;

/// <summary>
/// Struct representing an endpoint that can be connected to.
/// </summary>
public readonly struct NetworkEndpoint {

    /// <summary>
    /// The IP address of the remote network endpoint
    /// </summary>
    public readonly string RemoteIPAddress;

    /// <summary>
    /// The TCP port to connect to
    /// </summary>
    public readonly int Tcp;

    /// <summary>
    /// The HTTP port to connect to
    /// </summary>
    public readonly int Http;

    /// <summary>
    /// Get the IP and TCP string
    /// </summary>
    public string TcpStr => $"{this.RemoteIPAddress}:{this.Tcp}";

    /// <summary>
    /// Get the IP and HTTP port string
    /// </summary>
    public string HttpStr => $"{this.RemoteIPAddress}:{this.Http}";

    /// <summary>
    /// Creates a network endpoint that can be connected to.
    /// </summary>
    /// <param name="remoteIP">The remote IP address or hostname of the endpoint</param>
    /// <param name="http">The HTTP port to use</param>
    /// <param name="tcp">The TCP port to use</param>
    public NetworkEndpoint(string remoteIP, int http, int tcp) {
        this.RemoteIPAddress = remoteIP;
        this.Http = http;
        this.Tcp = tcp;
    }

    /// <summary>
    /// Checks if the network endpoint is connectable.
    /// </summary>
    /// <param name="timeout">The amount of milliseconds to wait before failing the connectability attempt.</param>
    /// <returns>If the network endpoint can be connected to.</returns>
    public bool IsConnectable(int timeout = 600) {
        try {
            HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var request = new HttpRequestMessage {
                RequestUri = new Uri($"http://{this.RemoteIPAddress}:{this.Http}/ping"),
                Method = HttpMethod.Get,
            };
            var response = client.Send(request);
            return response.IsSuccessStatusCode;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Asks the network endpoint what version it currently is.
    /// </summary>
    /// <param name="timeout">The amount of milliseconds to wait before failing to retrieve version.</param>
    /// <returns>The network endpoint version.</returns>
    public string GetVersion(int timeout = 600) {
        try {
            HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var request = new HttpRequestMessage {
                RequestUri = new Uri($"http://{this.RemoteIPAddress}:{this.Http}/version"),
                Method = HttpMethod.Get,
            };
            var response = client.Send(request);
            using var data = response.Content.ReadAsStream();
            using var dataReader = new StreamReader(data);
            return dataReader.ReadToEnd();
        } catch {
            return "0.0.0";
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"Endpoint [Http: {HttpStr}; Tcp: {TcpStr}]";

}
