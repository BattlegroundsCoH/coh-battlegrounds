using System;
using System.Diagnostics;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;
using Battlegrounds.Networking.Requests;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Class that listens for incoming events and dispatches them to targetted destination.
    /// </summary>
    public sealed class NetworkObjectListener : IIncomingRequestHandler {
        
        public IConnection Connection { get; }
        
        public IObjectPool SharedObjects { get; }

        public IStaticInterface StaticInterface { get; }

        public NetworkObjectListener(IConnection connection, IObjectPool objectPool, IStaticInterface staticInterface) {
            this.Connection = connection;
            this.SharedObjects = objectPool;
            this.StaticInterface = staticInterface;
        }

        public void CloseIncomingHandler() { }

        public IMessage HandleRequest(IMessage message) => message switch {
            RemoteCallMessage rcm => this.HandleRemoteProcedureCall(rcm),
            _ => new NullMessage()
        };

        private IMessage HandleRemoteProcedureCall(RemoteCallMessage rcm) {

            // Update args
            rcm.UpdateArguments(this.SharedObjects);

            // Demangle
            (string typename, string name, string op) = Mangler.Demangle(rcm.Mangled);
            Type type = Type.GetType(typename);

            // Log call dispatching
            Trace.WriteLineIf(NetworkInterface.LogDispatchCalls, $"Dispatching call {name} on type {typename} with {rcm.Arguments.Length} arguments");

            // Get ID
            IObjectID id = new ObjectKey();
            id.FromString(rcm.ObjectdID);

            // Get object
            object obj = this.SharedObjects.Get(id);
            if (obj is null) {
                obj = this.SharedObjects.Get(this.StaticInterface.FromID(rcm.ObjectdID));
            }

            // Check pool type
            if (obj is CachedObject cobj) {

                // Dummy set-property
                object setprpt() {
                    cobj.SetProperty(name, rcm.Arguments[0]);
                    return VoidValue.Void;
                }

                // Marshal result
                return RemoteMarshal.MarshalToMessage(op switch {
                    Mangler.PROPERTY_GET => cobj.GetProperty(name), // Get property
                    Mangler.PROPERTY_SET => setprpt(), // Set property
                    _ => cobj.InvokeAsFunction(name, rcm.Arguments) // Normal call
                }, this.SharedObjects);

            } else if (obj is not null) {

                // Marshal result
                return RemoteMarshal.MarshalToMessage(op switch {
                    Mangler.PROPERTY_GET => type.GetProperty(name).GetMethod.Invoke(obj, Array.Empty<object>()), // Get property
                    Mangler.PROPERTY_SET => type.GetProperty(name).SetMethod.Invoke(obj, rcm.Arguments), // Set property
                    _ => type.GetMethod(name).Invoke(obj, rcm.Arguments) // Normal call
                }, this.SharedObjects);

            } else {

                return new NullMessage();

            }

        }

    }

}
