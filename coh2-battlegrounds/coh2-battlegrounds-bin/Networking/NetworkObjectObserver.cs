using System;
using System.Collections.Generic;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Requests;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Class that observes network objects and triggers remote events.
    /// </summary>
    public sealed class NetworkObjectObserver {

        private readonly HashSet<object> m_observedObjects;
        private readonly IConnection m_connection;
        private readonly IObjectPool m_pool;

        public NetworkObjectObserver(IConnection connection, IObjectPool objectPool) {
            this.m_connection = connection;
            this.m_pool = objectPool;
            this.m_observedObjects = new();
        }

        public void AddInstance<T>(INetworkObjectObservable<T> observable) {
            if (this.m_observedObjects.Add(observable)) {
                observable.ValueChanged += this.OnNetworkObjectValueEvent;
                observable.MethodInvoked += this.OnNetworkObjectMethodEvent;
            }
        }

        private void OnNetworkObjectValueEvent<T>(T sender, ObservableValueChangedEventArgs args) {

            // Get ID of sender
            var senderID = this.m_pool.RegisterIfNotFound(sender);

            // Mangle
            string mangle = Mangler.Mangle(sender.GetType().Name, "NetworkUpdate", args.HasValue ? Mangler.PROPERTY_SET : Mangler.PROPERTY_GET);

            // Determine arguments
            object[] remoteArgs = args.HasValue ? new object[] { args.Property, args.Value } : new object[] { args.Property };

            // Send the broadcast message
            this.m_connection.SendBroadcastMessage(new RemoteCallMessage(senderID.ToString(), mangle, remoteArgs));

        }

        private void OnNetworkObjectMethodEvent<T>(T sender, string invokedMethod, params object[] args) {

        }

    }

}
