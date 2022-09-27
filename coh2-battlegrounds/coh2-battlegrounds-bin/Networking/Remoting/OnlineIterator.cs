using System;
using System.Collections;
using System.Collections.Generic;

namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// Supports the iterator pattern over an online connection.
/// </summary>
/// <typeparam name="T">The collection element type.</typeparam>
public class OnlineIterator<T> : IEnumerator<T> {

    private readonly struct OnlineIteratorResponse {
        public readonly T[] Data { get; init; }
        public readonly bool IsDone { get; init; }
    }

    private readonly RemoteCall m_remoter;
    private readonly List<T> m_buffered;
    private T? m_current;

    private bool m_isDone;

    public T Current => this.m_current ?? throw new Exception();

    object? IEnumerator.Current => this.m_current;

    public ulong IteratorUID { get; }

    internal OnlineIterator(ulong iteratorUID, RemoteCall remoter) {
        this.IteratorUID = iteratorUID;
        this.m_remoter = remoter;
        this.m_buffered = new();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        // TODO: Send dispose to server
    }

    public bool MoveNext() {

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
            var result = this.m_remoter.Call<OnlineIteratorResponse>("Iterator_MoveNext", this.IteratorUID);
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
        throw new NotSupportedException("Cannot reset an online iterator.");
    }

}
