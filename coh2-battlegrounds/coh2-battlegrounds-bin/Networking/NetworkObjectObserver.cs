using System;
using System.Collections.Generic;

using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.LobbySystem.Roles;
using Battlegrounds.Networking.Remoting.Objects;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Class that observes network objects and triggers remote events.
    /// </summary>
    public sealed class NetworkObjectObserver {

        private readonly HashSet<object> m_observedObjects;
        private readonly BrokerHandler m_broker;
        private readonly IObjectPool m_pool;

        public event ObservableValueChangedHandler<object> BrokerNotify;

        public ICollection<string> MethodFilter { get; }

        public NetworkObjectObserver(BrokerHandler broker, IObjectPool objectPool) {
            this.m_broker = broker;
            this.m_pool = objectPool;
            this.m_observedObjects = new();
            this.MethodFilter = new List<string>();
        }

        public void AddInstance<T>(INetworkObjectObservable<T> observable) {
            if (this.m_observedObjects.Add(observable)) {
                observable.ValueChanged += this.OnNetworkObjectValueEvent;
                observable.MethodInvoked += this.OnNetworkObjectRemoteMethodEvent;
            }
        }

        private void OnNetworkObjectValueEvent<T>(T sender, ObservableValueChangedEventArgs args) {
            
            // If GUI only, bail
            if (args.GUIOnly) {
                return;
            }

            // If broker, notify; otherwise invoke remote
            if (args.IsBrokerEvent) {
                this.BrokerNotify?.Invoke(sender, args);
            } else {
                this.OnNetworkObjectRemoteValueEvent(sender, args);
            }

        }

        private void OnNetworkObjectRemoteValueEvent<T>(T sender, ObservableValueChangedEventArgs args) {

            // Get ID of sender
            var senderID = this.m_pool.RegisterIfNotFound(sender);

            // Determine arguments
            object[] remoteArgs = args.HasValue ? new object[] { args.Value } : Array.Empty<object>();
            
            // Update args
            for (int i = 0; i < remoteArgs.Length; i++) {
                if (remoteArgs[i] is IRemoteReference remoteRef) {
                    remoteArgs[i] = remoteRef.ObjectID;
                }
            }

            // Send the broadcast message
            this.m_broker.RemoteCall(senderID, args.Property, remoteArgs, Broadcast: true);

        }

        private void OnNetworkObjectRemoteMethodEvent<T>(T sender, string invokedMethod, params object[] args) {

            // Make sure it's not part of the method filter
            if (this.MethodFilter.Contains(invokedMethod)) {
                return;
            }

            // Get ID of sender
            var senderID = this.m_pool.RegisterIfNotFound(sender);

            // Update args
            for (int i = 0; i < args.Length; i++) {
                if (args[i] is IRemoteReference remoteRef) {
                    args[i] = remoteRef.ObjectID;
                }
            }

            // Send the broadcast message
            this.m_broker.RemoteCall(senderID, invokedMethod, args, Broadcast: true);

        }

    }

}
