using System;
using System.Diagnostics;

using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;

namespace Battlegrounds.Networking.Remoting;

internal class RemoteCall<THandle> {

    private readonly THandle m_source;
    private uint m_cid;

    public IConnection Connection { get; }

    public RemoteCall(THandle source, IConnection connection) {
        this.m_cid = 10000;
        this.m_source = source;
        this.Connection = connection;
    }

    public T? Call<T>(string method, params object[] args)
        => this.CallWithTime<T>(method, args);

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
            if (response.MessageType == ContentMessgeType.Error) {
                string e = response.StrMsg is null ? "Received error message from server with no description" : response.StrMsg;
                throw new Exception(e);
            }
            if (response.StrMsg == "Primitive") {
                // It feels nasty using 'dynamic'
                return (T)(dynamic)(response.DotNetType switch {
                    nameof(Boolean) => response.Raw[0] == 1,
                    nameof(UInt32) => response.Raw.ConvertBigEndian(BitConverter.ToUInt32),
                    _ => throw new NotImplementedException($"Support for primitive response of type '{response.DotNetType}' not implemented.")
                });
            } else {
                if (GoMarshal.JsonUnmarshal<T>(response.Raw) is T content) {
                    if (content is IHandleObject<THandle> handleObject) {
                        handleObject.SetHandle(this.m_source);
                    }
                    return content;
                }
                Trace.WriteLine($"Failed to unmarshal return raw response bytes of length '{response.Raw.Length}'", nameof(RemoteCall<T>));
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
