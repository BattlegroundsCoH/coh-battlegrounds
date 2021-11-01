using System;
using System.Runtime.CompilerServices;

using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.LobbySystem.Roles;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Query;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Basic remote handle implementation for getting and invokeing remote properties and methods.
    /// </summary>
    public class RemoteHandle : IRemoteHandle {

        private readonly BrokerHandler m_handler;

        public RemoteHandle(BrokerHandler connection)
            => this.m_handler = connection;

        public TProperty GetRemoteProperty<TProperty, TCaller>(IObjectID objId, [CallerMemberName] string propertyName = "") {

            // Ensure we actually have a property
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            // Do remote call
            if (this.m_handler.RemoteCall(objId, propertyName, Array.Empty<object>(), 1) is TProperty prop) {
                return prop;
            } else {
                return default;
            }

        }

        public TReturnValue RemoteCall<TReturnValue, TCaller>(IObjectID objId, string methodName, params object[] remoteCallArgs) {

            // Update args
            for (int i = 0; i < remoteCallArgs.Length; i++) {
                if (remoteCallArgs[i] is IRemoteReference remoteRef) {
                    remoteCallArgs[i] = remoteRef.ObjectID;
                }
            }

            // Do remote call
            if (this.m_handler.RemoteCall(objId, methodName, Array.Empty<object>(), 1) is TReturnValue prop) {
                return prop;
            } else {
                return default;
            }

        }

        public TRemoteObject GetRemotableObject<TRemoteObject>(string getObjectString, Func<IObjectID, IRemoteHandle, TRemoteObject> objectCtor) {

            // Send and await
            if (this.m_handler.GetObjectIDByName(getObjectString) is IObjectID id) {
                return objectCtor(id, this);
            }

            // Return a default
            return default;

        }

        public CommandQueryResult Query(CommandQuery query)
            => this.m_handler.Query(query);

    }

}
