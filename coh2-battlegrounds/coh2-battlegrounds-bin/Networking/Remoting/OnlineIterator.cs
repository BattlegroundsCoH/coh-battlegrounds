using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// 
/// </summary>
public class OnlineIterator {

    private RemoteCall? m_remoter;

    /// <summary>
    /// 
    /// </summary>
    internal RemoteCall? Remoter {
        get => this.m_remoter;
        set => this.m_remoter = value;
    }

}

/// <summary>
/// Supports the iterator pattern over an online connection.
/// </summary>
/// <typeparam name="T">The collection element type.</typeparam>
public sealed class OnlineIterator<T> : OnlineIterator, IEnumerator<T> {

    private readonly struct OnlineIteratorResponse {
        public readonly T[] Data { get; init; }
        public readonly bool IsDone { get; init; }
        [JsonConstructor]
        public OnlineIteratorResponse(T[] Data, bool IsDone) {
            this.Data = Data;
            this.IsDone = IsDone;
        }

    }

    private readonly List<T> m_buffered;
    private T? m_current;

    private bool m_isDone;
    private bool m_isDisposed;

    public T Current 
        => this.m_isDisposed ? throw new ObjectDisposedException(nameof(OnlineIterator<T>)) : (this.m_current ?? throw new Exception());

    object? IEnumerator.Current => this.m_current;

    public ulong IteratorUID { get; }

    public OnlineIterator(ulong iteratorUID) {
        this.IteratorUID = iteratorUID;
        this.m_buffered = new();
    }

    public void Dispose() {
        
        // Bail if remoter not defined or already disposed
        if (this.Remoter is null || this.m_isDisposed) {
            return;
        }

        // Suppress this
        GC.SuppressFinalize(this);
        
        // Inform server we're done
        this.Remoter.Call("Iterator_Done", this.IteratorUID);

        // Clear buffered if any
        this.m_buffered.Clear();

        // Set disposed flag
        this.m_isDisposed = true;

        // Mark null
        this.Remoter = null;

    }

    public bool MoveNext() {

        // Error if disposed
        if (this.m_isDisposed || this.Remoter is null) {
            throw new ObjectDisposedException(nameof(OnlineIterator<T>));
        }

        // Decide if we should move to next buffered element or make a request online
        if (this.m_buffered.Count > 0) {
            
            // Set current as first buffered element
            this.m_current = this.m_buffered[0];
            
            // Remove first
            this.m_buffered.RemoveAt(0);
            
            // Return true => can advance to next
            return true;

        } else {

            // Return false if last request was marked done and we've exhausted the buffer
            if (this.m_isDone) {
                return false;
            }

            // Send request for more
            var result = this.Remoter.Call<OnlineIteratorResponse>("Iterator_MoveNext", this.IteratorUID);
            this.m_isDone = result.IsDone;
            
            // Check if any was received
            if (result.Data.Length > 0) {

                // Set current as first element
                this.m_current = result.Data[0];

                // Add remaining to buffer
                if (result.Data.Length > 1) {
                    this.m_buffered.AddRange(result.Data[1..]);
                }

                // Return true => can return more
                return true;

            }

            // Return false
            return false;
        }

    }

    public void Reset() {

        // Error if disposed
        if (this.m_isDisposed || this.Remoter is null) {
            throw new ObjectDisposedException(nameof(OnlineIterator<T>));
        }

        // Invoke reset on server
        this.Remoter.Call("Iterator_Reset", this.IteratorUID);

        // Clear buffer
        this.m_buffered.Clear();

    }

}
