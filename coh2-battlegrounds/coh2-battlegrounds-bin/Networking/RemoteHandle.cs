using System;
using System.Runtime.CompilerServices;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.LobbySystem.Roles;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Query;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Basic remote handle implementation for getting and invokeing remote properties and methods.
    /// </summary>
    public class RemoteHandle : IRemoteHandle {

        private readonly IConnection m_connection;

        public RemoteHandle(IConnection connection)
            => this.m_connection = connection;

        public TProperty GetRemoteProperty<TProperty, TCaller>(IObjectID objId, [CallerMemberName] string propertyName = "") {

            // Ensure we actually have a property
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            // Mangle
            string mangle = Mangler.Mangle(typeof(TCaller).Name, propertyName, Mangler.PROPERTY_GET);

            // Send the message
            if (this.m_connection.SendMessage(new RemoteCallMessage(objId.ToString(), mangle, Array.Empty<object>()), true) is PrimitiveMessage msg) {
                if (msg.Value is TProperty propertyValue) {
                    return propertyValue;
                }

            }

            return default;

        }

        public TReturnValue RemoteCall<TReturnValue, TCaller>(IObjectID objId, string methodName, params object[] remoteCallArgs) {

            // Mangle
            string mangle = Mangler.Mangle(typeof(TCaller).Name, methodName);

            // Update args
            for (int i = 0; i < remoteCallArgs.Length; i++) {
                if (remoteCallArgs[i] is IRemoteReference remoteRef) {
                    remoteCallArgs[i] = remoteRef.ObjectID;
                }
            }

            // Send the broadcast message
            if (this.m_connection.SendMessage(new RemoteCallMessage(objId.ToString(), mangle, remoteCallArgs), true) is PrimitiveMessage msg) {
                if (msg.Value is TReturnValue returnValue) {
                    return returnValue;
                }
            }

            return default;

        }

        public TRemoteObject GetRemotableObject<TRemoteObject>(string getObjectString, Func<IObjectID, IRemoteHandle, TRemoteObject> objectCtor) {

            // Create get request message
            GetObjectMessage gobj = new(getObjectString, false);

            // Send and await
            if (this.m_connection.SendMessage(gobj, true) is IDMessage id) {
                return objectCtor(id.ID, this);
            }

            // Return a default
            return default;

        }

        public CommandQueryResult Query(CommandQuery query) {

            // Create query message
            QueryMessage qmsg = new() { Query = query };

            // Send and await
            return this.m_connection.SendMessage(qmsg, true) is ByteObjectMessage response && response.Object is CommandQueryResult result ? result : (new(false));
        }

    }

}
