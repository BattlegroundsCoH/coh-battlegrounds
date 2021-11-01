using System;
using System.Diagnostics;

using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Class that listens for incoming events and dispatches them to targetted destination.
    /// </summary>
    public sealed class NetworkObjectListener {

        public BrokerHandler Broker { get; }

        public IObjectPool SharedObjects { get; }

        public IStaticInterface StaticInterface { get; }

        public NetworkObjectListener(BrokerHandler broker, IObjectPool objectPool, IStaticInterface staticInterface) {
            this.Broker = broker;
            this.SharedObjects = objectPool;
            this.StaticInterface = staticInterface;
        }

    }

}
