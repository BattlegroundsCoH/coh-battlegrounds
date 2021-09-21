using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;

namespace Battlegrounds.Networking {
    
    public sealed class NetworkObjectHandler<T> {

        private readonly IConnection m_connection;
        private readonly IStaticInterface m_interface;
        private readonly IObjectPool m_objects;

        public NetworkObjectObserver Observer { get; }

        public NetworkObjectListener Listener { get; }

        public NetworkObjectHandler(INetworkObjectObservable<T> networkObject, IConnection connection, IStaticInterface staticInterface, IObjectPool objectPool) {

            // Create observer and subscribe to broker notify events
            this.Observer = new(connection, objectPool);
            this.Observer.BrokerNotify += this.OnNetworkBrokerNotify;

            // Create listener
            this.Listener = new(connection, objectPool, staticInterface);

            // Set connection handler
            connection.SetRequestHandler(this.Listener);

            // Set observer instance
            this.Observer.AddInstance(networkObject);

            // Set fields
            this.m_connection = connection;
            this.m_interface = staticInterface;
            this.m_objects = objectPool;

        }

        private void OnNetworkBrokerNotify<TSomething>(TSomething sender, ObservableValueChangedEventArgs args) {

            // Make sure there's a value
            if (!args.HasValue) {
                return;
            }

            // Send message and forget it
            this.m_connection.SendBrokerMessage(new StringMessage($"{args.Property}:{args.Value}"), false);

        }

    }

}
