using System;
using System.Collections;

namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// 
/// </summary>
internal static class OnlineIteratorFactory {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elementType"></param>
    /// <param name="iteratorId"></param>
    /// <param name="caller"></param>
    /// <returns></returns>
    /// <exception cref="ContextMarshalException"></exception>
    public static IEnumerator CreateIterator(Type elementType, ulong iteratorId, RemoteCall caller) {

        // Create generic type
        var ittTy = typeof(OnlineIterator<>).MakeGenericType(elementType);

        // Try create using public constructor with only the iterator Id
        if (Activator.CreateInstance(ittTy, iteratorId) is IEnumerator itt) { 
            
            // Check if we got an iterator, set remoter if success
            if (itt is OnlineIterator oitt) {
                oitt.Remoter = caller;
            } else {
                throw new ContextMarshalException();
            }
            
            // Return iterator
            return itt;

        }

        // Failed to create enumerator => Error
        throw new ContextMarshalException();

    }

}
