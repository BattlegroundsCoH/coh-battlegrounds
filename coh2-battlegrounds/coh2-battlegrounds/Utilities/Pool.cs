using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BattlegroundsApp.Utilities;

/// <summary>
/// Class representing a pool of <typeparamref name="T"/> elements, where only available elements are shown visually.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Pool<T> : ObservableCollection<T> {

    private readonly HashSet<T> m_items;

    public event EventHandler<T>? OnPick;

    public event EventHandler<T>? OnUnPick;

    /// <summary>
    /// Get the amount of available items.
    /// </summary>
    public int Available => this.m_items.Count;

    /// <summary>
    /// Get the amount of picked items.
    /// </summary>
    public int Picked => this.m_items.Count - this.Count;

    /// <summary>
    /// Initialise a new and empty <see cref="Pool{T}"/> instance.
    /// </summary>
    public Pool() {
        this.m_items = new();
    }

    /// <summary>
    /// Register a new item in the pool.
    /// </summary>
    /// <param name="item">The item to register in the pool.</param>
    public void Register(T item) {
        this.m_items.Add(item);
        base.Add(item);
    }

    /// <summary>
    /// Unregister an item from the pool.
    /// </summary>
    /// <param name="item">The item to unregister from the pool.</param>
    public void Unregister(T item) {
        this.m_items.Remove(item);
        base.Remove(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Pick(T item) {
        if (this.m_items.Contains(item) && this.Remove(item)) {
            this.OnPick?.Invoke(this, item);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public T Unpick(T item) {
        if (this.m_items.Contains(item)) {
            this.Add(item);
            this.OnUnPick?.Invoke(this, item);
        }
        return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CanPick(T item) => this.Contains(item);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool IsPicked(T item) => this.m_items.Contains(item) && !this.CanPick(item);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public T? Find(Predicate<T> predicate) {
        for (int i = 0; i < this.Count; i++) {
            if (predicate(this[i])) {
                return this[i];
            }
        }
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool Pick(Predicate<T> predicate) {
        if (this.Find(predicate) is T item) {
            return this.Pick(item);
        }
        return false;
    }

}
