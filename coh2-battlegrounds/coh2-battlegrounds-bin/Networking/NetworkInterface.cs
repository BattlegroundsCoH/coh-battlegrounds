using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Battlegrounds.Logging;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking;

/// <summary>
/// Static class for setting global network interface values
/// </summary>
public static class NetworkInterface {

    private static readonly Logger logger = Logger.CreateLogger();

    private static readonly NetworkEndpoint LocalEndpoint = new("127.0.0.1", 80, 11000);
    private static readonly NetworkEndpoint RemoteDebugEndpoint = new("194.37.80.249", 5001, 5000);

    /// <summary>
    /// Endpoint of the remote-release server
    /// </summary>
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
    /// Get or set the local address
    /// </summary>
    public static string LocalAddress { get; set; } = "127.0.0.1";

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
        if (TrySetBestAddress(timeout, LocalEndpoint, "Local Server")) {
            return;
        }
        if (TrySetBestAddress(timeout, RemoteDebugEndpoint, "Debug Server")) {
            return;
        }
#endif
        if (TrySetBestAddress(timeout, RemoteReleaseEndpoint, "Release Server")) {
            return;
        }
        logger.Warning("Failed to connect to any remote endpoint - no internet or Battlegrounds server is down!");
    }

    private static bool TrySetBestAddress(int timeout, NetworkEndpoint endpoint, string endpointName) {
        var connected = endpoint.IsConnectable(timeout);
        if (connected) {
            string version = endpoint.GetVersion(timeout);
            logger.Info("Picked endpoint {0} ({1}) of version {2}", endpoint, endpointName, version);
            __bestEndpoint = endpoint;
            __hasServerConnection = true;
        }
        return connected;
    }

    /// <summary>
    /// Register a connection with the interface.
    /// </summary>
    /// <param name="connection">The connection instance to register.</param>
    public static void RegisterConnection(IConnection? connection) {
        if (__connections is null) {
            logger.Warning("Please setup the NetworkInterface before registering connections.");
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
            logger.Warning("Please setup the NetworkInterface before unregistering connections.");
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
            logger.Warning("Please setup the NetworkInterface before shutting it down.");
            return;
        }

        if (__connections.Count <= 0) {
            return;
        }

        var ls = new List<IConnection>(__connections);
        try {
            foreach (IConnection connection in ls) {
                try {
                    connection.Shutdown();
                } catch { }
            }
        } catch (Exception e) {
            logger.Exception(e);
        }

    }

}
