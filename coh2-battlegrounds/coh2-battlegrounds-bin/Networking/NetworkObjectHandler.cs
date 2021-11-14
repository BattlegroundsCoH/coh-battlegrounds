using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;

namespace Battlegrounds.Networking {
    
    public sealed class NetworkObjectHandler<T> {

        private readonly BrokerHandler m_broker;
        private readonly IStaticInterface m_interface;
        private readonly IObjectPool m_objects;

        public NetworkObjectObserver Observer { get; }

        public NetworkObjectListener Listener { get; }

        public NetworkObjectHandler(INetworkObjectObservable<T> networkObject, BrokerHandler broker, IStaticInterface staticInterface, IObjectPool objectPool) {

            // Create observer and subscribe to broker notify events
            this.Observer = new(broker, objectPool);
            this.Observer.BrokerNotify += this.OnNetworkBrokerNotify;

            // Create listener
            this.Listener = new(broker, objectPool, staticInterface);

            // Set observer instance
            this.Observer.AddInstance(networkObject);

            // Set fields
            this.m_broker = broker;
            this.m_interface = staticInterface;
            this.m_objects = objectPool;

        }

        private void OnNetworkBrokerNotify<TSomething>(TSomething sender, ObservableValueChangedEventArgs args) {

            // Make sure there's a value
            if (!args.HasValue) {
                return;
            }

            // Send message and forget it
            this.m_broker.Update(args.Property switch {
                "LobbyPlaymode" => BrokerFirstVal.Mode,
                _ => BrokerFirstVal.Capacity
            }, args.Value);

        }

    }

}
