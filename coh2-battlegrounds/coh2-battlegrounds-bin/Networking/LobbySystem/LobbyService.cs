using System;
using System.Collections.Generic;

using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.DataStructures;
using Battlegrounds.Networking.LobbySystem.Roles.Host;
using Battlegrounds.Networking.Proxy;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Requests;

namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// 
    /// </summary>
    public class LobbyService : IStaticInterface {

        private ILobby m_lobby;
        private readonly Dictionary<string, IObjectID> m_staticObjects;

        public IObjectPool ObjectPool { get; }

        /// <summary>
        /// Get the lobby instance associated with this element.
        /// </summary>
        public ILobby Lobby => this.m_lobby;

        /// <summary>
        /// Initialize a new <see cref="LobbyService"/> with an <paramref name="objectPool"/>.
        /// </summary>
        /// <param name="objectPool">The object pool to keep track of static elements.</param>
        public LobbyService(IObjectPool objectPool) {
            this.ObjectPool = objectPool;
            this.m_staticObjects = new Dictionary<string, IObjectID>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IObjectID Create<T>(string identifier, params object[] args) {
            if (typeof(T) == typeof(HostedLobby)) {
                this.m_lobby = new HostedLobby(args[0] as string);
                return this.m_staticObjects[identifier] = this.ObjectPool.Register(this.Lobby);
            } else if (typeof(T) == typeof(SynchronizedTimer)) {
                SynchronizedTimer timer = new(args[0] as IRequestHandler, (TimeSpan)args[1], (TimeSpan)args[2]);
                return this.m_staticObjects[identifier] = this.ObjectPool.Register(timer);
            } else {
                try {
                    var instance = Activator.CreateInstance(typeof(T), args);
                    return this.m_staticObjects[identifier] = this.ObjectPool.Register(instance);
                } catch { }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public IObjectID FromID(string objectType) => this.m_staticObjects[objectType];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="trackable"></param>
        public void Publish(IRequestHandler handler, IProxyTrackable trackable) => handler.Connection.SendMessage(new PublishMessage(trackable), false);

    }
}
