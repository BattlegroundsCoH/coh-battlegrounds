using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    private static readonly NetworkEndpoint LocalEndpoint = new(__localServerAddr, 80, 11000);
    private static readonly NetworkEndpoint RemoteDebugEndpoint = new("194.37.80.249", 81, 5000);
    public static readonly NetworkEndpoint RemoteReleaseEndpoint = new("194.37.80.249", 80, 11000);

    /// <summary>
    /// Get or set the amount of milliseconds to wait for a response (before resending a request)
    /// </summary>
    public static int TimeoutMilliseconds { get; set; } = 2000;

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
    /// Get the endpoint information to use when making remote connections
    /// </summary>
    public static NetworkEndpoint Endpoint => __bestEndpoint;

    /// <summary>
    /// Get if a connection was established to <see cref="Endpoint"/>.
    /// </summary>
    public static bool HasServerConnection => __hasServerConnection;

    /// <summary>
    /// Get the <see cref="ServerAPI"/>.
    /// </summary>
    public static ServerAPI? APIObject => __api;

    private static ServerAPI? __api;
    private static NetworkEndpoint __bestEndpoint;
    private static List<IConnection> __connections = new();
    private static bool __hasServerConnection;

    private static readonly object m_connlock = new();

    /// <summary>
    /// Setup the network API.
    /// </summary>
    public static void Setup(bool debug = false) {
        __connections = new();
        Task.Run(() => {

            // Decide best address
            DecideBestAddress();

            // Create API
            __api = new ServerAPI(__bestEndpoint.RemoteIPAddress, __bestEndpoint.Http);

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

    private static void DecideBestAddress(int timeout = 600) {
#if DEBUG
        // Try and connect to local server (if possible)
        if (AllowLocalServerInstance && HasLocalServer() && LocalEndpoint.IsConnectable()) {
            Trace.WriteLine("Picked locally running server as endpoint", nameof(NetworkInterface));
            __bestEndpoint = LocalEndpoint;
            __hasServerConnection = true;
            return;
        }
        // Then try connect to debug server
        if (RemoteDebugEndpoint.IsConnectable()) {
            Trace.WriteLine("Picked remote debug server as endpoint", nameof(NetworkInterface));
            __bestEndpoint = RemoteDebugEndpoint;
            __hasServerConnection = true;
            return;
        }
#endif
        // Set as release
        __hasServerConnection = RemoteReleaseEndpoint.IsConnectable();
        Trace.WriteLine($"Picked remote release server as endpoint (Connection = {__hasServerConnection}", nameof(NetworkInterface));
        __bestEndpoint = RemoteReleaseEndpoint;
    }

    /// <summary>
    /// Get if there's a local server instance running.
    /// </summary>
    /// <returns><see langword="true"/> if there's a local server instance running; Otherwise <see langword="false"/>.</returns>
    public static bool HasLocalServer()
        => Process.GetProcessesByName("bgserver").Length > 0;

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

