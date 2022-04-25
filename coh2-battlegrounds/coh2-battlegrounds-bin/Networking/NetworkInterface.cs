using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking;

/// <summary>
/// Static class for setting global network interface values
/// </summary>
public static class NetworkInterface {

    private static readonly string __localServerAddr = File.Exists("network.test.txt") ? File.ReadAllText("network.test.txt") : "192.168.1.107";

    /// <summary>
    /// Get or set the amount of milliseconds to wait for a response (before resending a request)
    /// </summary>
    public static int TimeoutMilliseconds { get; set; } = 2000;

    /// <summary>
    /// Get or set the amount of resends to attempt for a response.
    /// </summary>
    public static int ResendAttempts { get; set; } = 5;

    /// <summary>
    /// Get or set the size of the receive buffer.
    /// </summary>
    public static int ReceiveBufferSize { get; set; } = 4096;

    /// <summary>
    /// Get or set the size of the send buffer.
    /// </summary>
    public static int SendBufferSize { get; set; } = 4096;

    /// <summary>
    /// Get or set if dispatching class should be logged.
    /// </summary>
    public static bool LogDispatchCalls { get; set; } = false;

    /// <summary>
    /// Get or set if API class should be logged.
    /// </summary>
    public static bool LogAPICalls { get; set; } = true;

    /// <summary>
    /// Get or set if the interface will accept local server instances.
    /// </summary>
    public static bool AllowLocalServerInstance { get; set; } = true;

    /// <summary>
    /// Get or set the local address
    /// </summary>
    public static string LocalAddress { get; set; } = __localServerAddr;

    /// <summary>
    /// Get or set the self identifier
    /// </summary>
    public static ulong SelfIdentifier { get; set; }

    /// <summary>
    /// Get the <see cref="ServerAPI"/>.
    /// </summary>
    public static ServerAPI? APIObject => __api;

    private static ServerAPI? __api;
    private static List<IConnection> __connections = new();
    private static string? __bestAddress;

    private static readonly object m_connlock = new();

    /// <summary>
    /// Setup the network API.
    /// </summary>
    public static void Setup(bool debug = false) {
        __connections = new();
        Task.Run(() => {
            string addr = GetBestAddress();
            __api = new ServerAPI(debug ? "__localServerAddr" : addr);
        });
    }

    /// <summary>
    /// Determines if the local machine is connected to the internet (can connect to http://www.google.com ) at the time of the call.
    /// </summary>
    /// <param name="timeout">The amount of milliseconds to wait before declaring connection timed out.</param>
    /// <returns>If internet connection was established <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
    public static bool HasInternetConnection(int timeout = 1000) {
        try {
            return (new Ping().Send("google.com", timeout)).Status == IPStatus.Success;
        } catch (Exception) {
            return false;
        }
    }

    /// <summary>
    /// Get the best server address available.
    /// </summary>
    /// <returns>The IP address of the best available address that can be connected to.</returns>
    public static string GetBestAddress(int timeout = 600) {
        if (__bestAddress is null) {
            Trace.WriteLine("No best address specified - will attempt to find it.", nameof(NetworkInterface));
            __bestAddress = "194.37.80.249";
            if (AllowLocalServerInstance && HasLocalServer()) {
                __bestAddress = "localhost";
            } else {
                try {
                    __bestAddress = PingServer(LocalAddress, 80, timeout: timeout) ? LocalAddress : "194.37.80.249";
                } catch (Exception e) {
                    Trace.WriteLine(e, nameof(NetworkInterface));
                }
            }
            Trace.WriteLine($"Using {__bestAddress} as connection address", nameof(NetworkInterface));
        }
        return __bestAddress;
    }

    /// <summary>
    /// Ping specified server at <paramref name="address"/> using <paramref name="port"/>.
    /// </summary>
    /// <param name="address">The addres to ping.</param>
    /// <param name="port">The port to ping.</param>
    /// <param name="timeout">The amount of milliseconds to wait before timing out the ping request.</param>
    /// <returns>If a ping response was given, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public static bool PingServer(string address, int port, int timeout = 200) {
        try {
            HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var request = new HttpRequestMessage {
                RequestUri = new Uri($"http://{address}:{port}/api/ping"),
                Method = HttpMethod.Get,
            };
            var response = client.Send(request);
            return response.IsSuccessStatusCode;
        } catch {
            return false;
        }

    }

    /// <summary>
    /// Get if there's a local server instance running.
    /// </summary>
    /// <returns><see langword="true"/> if there's a local server instance running; Otherwise <see langword="false"/>.</returns>
    public static bool HasLocalServer()
        => Process.GetProcessesByName("bg-server").Length > 0;

    /// <summary>
    /// Register a connection with the interface.
    /// </summary>
    /// <param name="connection">The connection instance to register.</param>
    public static void RegisterConnection(IConnection? connection) {
        if (__connections is null) {
            Trace.WriteLine("Please setup the NetworkInterface before registering connections.", nameof(NetworkInterface));
            return;
        }
        if (connection is not null) {
            __connections.Add(connection);
        }
    }

    /// <summary>
    /// Unregister a connection from the interface.
    /// </summary>
    /// <param name="connection">The connection instance to unregister.</param>
    public static void UnregisterConnection(IConnection? connection) {
        if (__connections is null) {
            Trace.WriteLine("Please setup the NetworkInterface before unregistering connections.", nameof(NetworkInterface));
            return;
        }
        lock (m_connlock) {
            if (connection is not null) {
                __connections.Remove(connection);
            }
        }
    }

    /// <summary>
    /// Shutdown all active connections.
    /// </summary>
    public static void Shutdown() {
        if (__connections is null) {
            Trace.WriteLine("Please setup the NetworkInterface before shutting it down.", nameof(NetworkInterface));
            return;
        }
        if (__connections.Count > 0) {
            var ls = new List<IConnection>(__connections);
            try {
                foreach (IConnection connection in ls) {
                    try {
                        connection.Shutdown();
                    } catch { }
                }
            } catch (Exception e) { Trace.WriteLine($"Error on shutdown! {e}", nameof(NetworkInterface)); }
        }
    }

}

