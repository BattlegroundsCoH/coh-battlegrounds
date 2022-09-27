using System;
using System.Diagnostics;

using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;

namespace Battlegrounds.Networking.Remoting;

internal class RemoteCall {

    protected uint m_cid;

    public IConnection Connection { get; }

    public RemoteCall(IConnection connection) {
        this.m_cid = 10000;
        this.Connection = connection;
    }

    public virtual T? Call<T>(string method, params object[] args)
        => this.CallWithTime<T>(method, args);

    public virtual T? CallWithTime<T>(string method, object[] args, TimeSpan? waitTime = null) {

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
            if (response.MessageType == ContentMessgeType.Error) {
                string e = response.StrMsg is null ? "Received error message from server with no description" : response.StrMsg;
                throw new Exception(e);
            }
            switch (response.StrMsg) {
                case "Primitive":
                    // It feels nasty using 'dynamic'
                    return (T)(dynamic)(response.DotNetType switch {
                        nameof(Boolean) => response.Raw[0] == 1,
                        nameof(UInt32) => response.Raw.ConvertBigEndian(BitConverter.ToUInt32),
                        _ => throw new NotImplementedException($"Support for primitive response of type '{response.DotNetType}' not implemented.")
                    });
                case "IteratorResult":
                    if (Activator.CreateInstance(typeof(T), response.Who, this) is T itt) {
                        return itt;
                    } else {
                        Trace.WriteLine($"Failed to unmarshal iterator. The type '{typeof(T).Name}' is not a valid iterator type.", nameof(RemoteCall));
                        break;
                    }
                default:
                    if (GoMarshal.JsonUnmarshal<T>(response.Raw) is T content) {
                        return content;
                    }
                    Trace.WriteLine($"Failed to unmarshal return raw response bytes of length '{response.Raw.Length}'", nameof(RemoteCall));
                    break;
            }
        }

        // TODO: Warning
        return default;

    }

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

internal class RemoteCall<THandle> : RemoteCall {

    private readonly THandle m_source;

    public RemoteCall(THandle source, IConnection connection) : base(connection) {
        this.m_source = source;
    }

    public new T? Call<T>(string method, params object[] args) {
        return this.CallWithTime<T>(method, args);
    }

    public new T? CallWithTime<T>(string method, object[] args, TimeSpan? waitTime = null) {

        // Get result from base
        var res = base.CallWithTime<T>(method, args, waitTime);
        if (res is IHandleObject<THandle> handleObj) {
            handleObj.SetHandle(this.m_source);
        }

        // Return result
        return res;

    }

}
