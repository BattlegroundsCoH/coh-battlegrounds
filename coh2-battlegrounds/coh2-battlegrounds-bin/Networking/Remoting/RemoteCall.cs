using System;
using System.Diagnostics;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;

namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// Class responsible for handling remote calls over a <see cref="IConnection"/>.
/// </summary>
internal class RemoteCall {

    protected uint m_cid;

    /// <summary>
    /// Get the connection in use by the <see cref="RemoteCall"/> instance.
    /// </summary>
    public IConnection Connection { get; }

    /// <summary>
    /// Initialise a new <see cref="RemoteCall"/> instance for <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection">The connection to send and receive remote call information over.</param>
    public RemoteCall(IConnection connection) {
        this.m_cid = 10000;
        this.Connection = connection;
    }

    /// <summary>
    /// Invoke <paramref name="method"/> using the remote call procedure.
    /// </summary>
    /// <remarks>
    /// This should never be invoked for 'void' methods.
    /// </remarks>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="method">The name of the method to invoke.</param>
    /// <param name="args">The arguments to marshal and send.</param>
    /// <returns>The result of the call.</returns>
    /// <exception cref="RemoteCallException"></exception>
    public T? Call<T>(string method, params object[] args)
        => this.CallWithTime<T>(method, args);

    /// <summary>
    /// Invoke <paramref name="method"/> using the remote call procedure and wait for the specified amount of time for a reply.
    /// </summary>
    /// <remarks>
    /// This should never be invoked for 'void' methods.
    /// </remarks>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="method">The name of the method to invoke.</param>
    /// <param name="args">The arguments to marshal and send.</param>
    /// <param name="waitTime">The amount of time to wait for the result.</param>
    /// <returns>The result of the call.</returns>
    /// <exception cref="RemoteCallException"></exception>
    public T? CallWithTime<T>(string method, object[] args, TimeSpan? waitTime = null) {

        // Create message
        Message msg = new Message() {
            CID = this.m_cid++,
            Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = args.Map(x => x.ToString()).NotNull() }),
            Mode = MessageMode.BrokerCall,
            Sender = this.Connection.SelfId,
            Target = 0
        };

        // Send and await response
        if (this.Connection.SendAndAwaitReply(msg, waitTime) is ContentMessage response) {
            if (response.MessageType is ContentMessgeType.Error) {
                throw new RemoteCallException(method, args, response.StrMsg is null ? "Received error message from server with no description" : response.StrMsg);
            }
            switch (response.StrMsg) {
                case "Primitive":
                    // It feels nasty using 'dynamic'
                    return (T)(dynamic)(response.DotNetType switch {
                        nameof(Boolean) => response.Raw[0] == 1,
                        nameof(UInt32) => response.Raw.ConvertBigEndian(BitConverter.ToUInt32),
                        _ => throw new RemoteCallException(method, args, $"Server returned primitive type '{response.DotNetType}' which is not implemented.")
                    });
                case "IteratorResult":
                    var elemtype = typeof(T).GenericTypeArguments[0];
                    var itter = OnlineIteratorFactory.CreateIterator(elemtype, response.Who, this);
                    if (itter is T itt) {
                        return itt;
                    } else {
                        Trace.WriteLine($"Error in call '{method}({string.Join(',', args)})':\n\tFailed to unmarshal iterator. The type '{typeof(T).Name}' is not a valid iterator type.", nameof(RemoteCall));
                        break;
                    }
                default:
                    if (GoMarshal.JsonUnmarshal<T>(response.Raw) is T content) {
                        return content;
                    }
                    Trace.WriteLine($"Error in call '{method}({string.Join(',', args)})':\n\tFailed to unmarshal return raw response bytes of length '{response.Raw.Length}'", nameof(RemoteCall));
                    break;
            }
        }

        // Log default and return it
        Trace.WriteLine($"Returning 'default' in call '{method}' with parameters '{string.Join(',', args)}'", nameof(RemoteCall));
        return default;

    }

    /// <summary>
    /// Invoke a remote void method.
    /// </summary>
    /// <param name="method">The name of the method to invoke.</param>
    /// <param name="args">The arguments to marshal and send.</param>
    public void Call(string method, params object[] args) {

        // Create message
        Message msg = new Message() {
            CID = this.m_cid++,
            Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = args.Map(x => x.ToString()).NotNull() }),
            Mode = MessageMode.BrokerCall,
            Sender = this.Connection.SelfId,
            Target = 0
        };

        // Send but ignore response
        this.Connection.SendMessage(msg);

    }

}

/// <summary>
/// Subclass of <see cref="RemoteCall"/> that are associated with a handle and automatically assigns return values the handle if they implement <see cref="IHandleObject{T}"/>.
/// </summary>
/// <typeparam name="THandle"></typeparam>
internal sealed class RemoteCall<THandle> : RemoteCall {

    /// <summary>
    /// Get the handle to assign to return values.
    /// </summary>
    public THandle Handle { get; }

    /// <summary>
    /// Initialise a new <see cref="RemoteCall{THandle}"/> instance associated with the <paramref name="source"/> handle.
    /// </summary>
    /// <param name="source">The handle to assign to return values.</param>
    /// <param name="connection">The connection to use when performing remote calls.</param>
    public RemoteCall(THandle source, IConnection connection) : base(connection) {
        this.Handle = source;
    }

    /// <summary>
    /// Invoke <paramref name="method"/> using the remote call procedure.
    /// </summary>
    /// <remarks>
    /// This should never be invoked for 'void' methods.
    /// </remarks>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="method">The name of the method to invoke.</param>
    /// <param name="args">The arguments to marshal and send.</param>
    /// <returns>The result of the call.</returns>
    /// <exception cref="RemoteCallException"></exception>
    public new T? Call<T>(string method, params object[] args) {
        return this.CallWithTime<T>(method, args);
    }

    /// <summary>
    /// Invoke <paramref name="method"/> using the remote call procedure and wait for the specified amount of time for a reply.
    /// </summary>
    /// <remarks>
    /// This should never be invoked for 'void' methods.
    /// </remarks>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="method">The name of the method to invoke.</param>
    /// <param name="args">The arguments to marshal and send.</param>
    /// <param name="waitTime">The amount of time to wait for the result.</param>
    /// <returns>The result of the call.</returns>
    /// <exception cref="RemoteCallException"></exception>
    public new T? CallWithTime<T>(string method, object[] args, TimeSpan? waitTime = null) {

        // Get result from base
        var res = base.CallWithTime<T>(method, args, waitTime);
        if (res is IHandleObject<THandle> handleObj) {
            handleObj.SetHandle(this.Handle);
        }

        // Return result
        return res;

    }

}
