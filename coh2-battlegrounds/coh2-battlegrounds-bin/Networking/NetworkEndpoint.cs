using Battlegrounds.Logging;

using System;
using System.Diagnostics;
using System.Net.Http;

namespace Battlegrounds.Networking;

/// <summary>
/// Struct representing an endpoint that can be connected to.
/// </summary>
public readonly struct NetworkEndpoint {

    private static readonly Logger logger = Logger.CreateLogger();

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
    /// Create 
    /// </summary>
    /// <param name="remoteIP"></param>
    /// <param name="http"></param>
    /// <param name="tcp"></param>
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
                RequestUri = new Uri($"http://{this.RemoteIPAddress}:{this.Http}/api/ping"),
                Method = HttpMethod.Get,
            };
            var response = client.Send(request);
            return response.IsSuccessStatusCode;
        } catch (Exception e) {
            return false;
        }
    }

}
