using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Battlegrounds.Functional;

namespace Battlegrounds.Util;

/// <summary>
/// Represents a search tree that maps string paths to values of type T.
/// </summary>
/// <typeparam name="T">The type of values stored in the tree.</typeparam>
public class SearchTree<T> : IEnumerable<T> {

    private readonly Node _root;

    /// <summary>
    /// Gets the number of values in the tree.
    /// </summary>
    public virtual int Count => _root.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchTree{T}"/> class with a root node named "Root".
    /// </summary>
    public SearchTree() {
        _root = new Node("");
    }

    /// <summary>
    /// Adds a value with the specified <paramref name="path"/> to the tree.
    /// </summary>
    /// <param name="path">The path to the value.</param>
    /// <param name="value">The value to add.</param>
    public virtual void Add(string path, T value) {
        var node = new Node(path) { Value = value };
        _root.AddNode(path, node);
    }

    /// <summary>
    /// Looks up the value associated with the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to the value.</param>
    /// <returns>The value associated with the path, or <c>null</c> if the path is not found in the tree.</returns>
    public virtual T? Lookup(string path) {
        var node = _root.GetNode(path);
        if (node != null) {
            return node.Value;
        }
        return default;
    }

    /// <summary>
    /// Looks up the value associated with the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to the value.</param>
    /// <param name="value">The value associated with the path, or <c>null</c> if the path is not found in the tree.</param>
    /// <returns><c>true</c> if the path is found in the tree and has a non-null value; otherwise, <c>false</c>.</returns>
    public virtual bool Lookup(string path, [NotNullWhen(true)] out T? value) {
        var node = _root.GetNode(path);
        if (node != null && node.Value != null) {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Determines whether the tree contains the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path is found in the tree; otherwise, <c>false</c>.</returns>
    public virtual bool Contains(string path) => _root.GetNode(path) is not null;

    /// <summary>
    /// Returns an enumerator that iterates through the values in the tree.
    /// </summary>
    /// <returns>An enumerator that iterates through the values in the tree.</returns>
    public IEnumerator<T> GetEnumerator() 
        => TraverseInternal(_root).Select(node => node.Value).Where(x => x is not null).GetEnumerator()!;

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    /// <summary>
    /// Traversers the entire tree and invokes <paramref name="action"/> on each element.
    /// </summary>
    /// <param name="action">The action to invoke on each element.</param>
    public void Traverse(Action<T> action) {
        var itt = GetEnumerator();
        while (itt.MoveNext()) {
            action(itt.Current);
        }
    }

    private IEnumerable<Node> TraverseInternal(Node node) {
        yield return node;
        foreach (var child in node.Children.Values) {
            foreach (var descendant in TraverseInternal(child)) {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Removes the value associated with the specified <paramref name="path"/> from the tree.
    /// </summary>
    /// <param name="path">The path to the value to remove.</param>
    public virtual void Remove(string path) {
        var segments = path.Split('.');
        var current = _root;
        for (int i = 0; i < segments.Length - 1; i++) {
            var segment = segments[i];
            if (!current.Children.TryGetValue(segment, out var child)) {
                return;
            }
            current = child;
        }
        current.Children.Remove(segments[^1]);
    }

    /// <summary>
    /// Removes all values from the tree.
    /// </summary>
    public virtual void Clear() {
        _root.Children.Clear();
    }

    /// <summary>
    /// Creates a new search tree that contains all the elements from this search tree and the specified search trees.
    /// </summary>
    /// <param name="trees">The search trees to merge with this search tree.</param>
    /// <returns>A new search tree that contains all the elements from this search tree and the specified search trees.</returns>
    public virtual SearchTree<T> Merge(params SearchTree<T>[] trees) {
        return new MergedSearchTree<T>(trees.Append(this));
    }

    private class Node {
        public string Name { get; set; }
        public T? Value { get; set; }
        public Dictionary<string, Node> Children { get; set; }
        public int Count => Children.Count > 0 ? Children.Aggregate(0, (state, val) => state + val.Value.Count) : 1;

        public Node(string name) {
            Name = name;
            Children = new Dictionary<string, Node>();
        }

        public Node? GetNode(string path) {
            var segments = path.Split('.');
            var current = this;
            for (int i = 0; i < segments.Length; i++) {
                if (!current.Children.TryGetValue(segments[i], out var child)) {
                    return null;
                }
                current = child;
            }
            return current;
        }

        public void AddNode(string path, Node node) {
            var segments = path.Split('.');
            var current = this;
            for (int i = 0; i < segments.Length - 1; i++) {
                var segment = segments[i];
                if (!current.Children.TryGetValue(segment, out var child)) {
                    child = new Node(segment);
                    current.Children[segment] = child;
                }
                current = child;
            }
            current.Children[segments[segments.Length - 1]] = node;
        }

    }
}
