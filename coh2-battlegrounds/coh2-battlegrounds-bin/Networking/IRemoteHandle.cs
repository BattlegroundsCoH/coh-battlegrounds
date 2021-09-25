using System;
using System.Runtime.CompilerServices;

using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Query;

namespace Battlegrounds.Networking;

/// <summary>
/// Interface for invoking and fetching remote properties and methods.
/// </summary>
public interface IRemoteHandle {

    /// <summary>
    /// Get the value of a property that is stored remotely.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <typeparam name="TCaller">The caller type.</typeparam>
    /// <param name="objId">The ID of the object storing the remote property.</param>
    /// <param name="propertyName">The name of the property to get value of.</param>
    /// <returns>If remote value is found, the remote value; Otherwise default value.</returns>
    TProperty GetRemoteProperty<TProperty, TCaller>(IObjectID objId, [CallerMemberName] string propertyName = "");

    /// <summary>
    /// Invoke a method on a remote object.
    /// </summary>
    /// <typeparam name="TReturnValue">The return value. Use <see cref="Remoting.VoidValue"/> for void calls.</typeparam>
    /// <typeparam name="TCaller">The caller type.</typeparam>
    /// <param name="objId">The ID of the object storing the remote property.</param>
    /// <param name="methodName">The name of the method to invoke remotely.</param>
    /// <param name="remoteCallArgs">The arguments to use for remote invokation.</param>
    /// <returns>If call received response, the returned value. If void method was invoked, <see cref="Remoting.VoidValue"/> is returned.</returns>
    TReturnValue RemoteCall<TReturnValue, TCaller>(IObjectID objId, string methodName, params object[] remoteCallArgs);

    /// <summary>
    /// Get a remote object.
    /// </summary>
    /// <typeparam name="TRemoteObject">The type of the remot object.</typeparam>
    /// <param name="getObjectString">The string to use to get remote object reference.</param>
    /// <param name="objectCtor">The constructor to invoke when receiving ID of remote object.</param>
    /// <returns>If request was a succes, the value constructed by <paramref name="objectCtor"/>; Otherwise default value.</returns>
    TRemoteObject GetRemotableObject<TRemoteObject>(string getObjectString, Func<IObjectID, IRemoteHandle, TRemoteObject> objectCtor);

    /// <summary>
    /// Send a command query to remote location and retrieve query result.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>The result of the command query.</returns>
    CommandQueryResult Query(CommandQuery query);

}
