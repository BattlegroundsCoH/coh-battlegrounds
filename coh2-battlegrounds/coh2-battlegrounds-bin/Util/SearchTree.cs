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
    /// Get or set the delimiter between path search terms.
    /// </summary>
    public char PathDelimiter { get; set; } = '.';

    /// <summary>
    /// Gets the keys of the search tree.
    /// </summary>
    public virtual ICollection<string> Keys => TraverseInternal(_root).Select(x => x.Path).ToList();

    /// <summary>
    /// Gets all the values of the search tree.
    /// </summary>
    public virtual ICollection<T?> Values => TraverseInternal(_root).Select(x => x.Value).ToList();

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchTree{T}"/> class with a root node named "Root".
    /// </summary>
    public SearchTree() {
        _root = new Node("", null);
    }

    /// <summary>
    /// Adds a value with the specified <paramref name="path"/> to the tree.
    /// </summary>
    /// <param name="path">The path to the value.</param>
    /// <param name="value">The value to add.</param>
    public virtual void Add(string path, T value) {
        var node = new Node(path, null) { Value = value };
        _root.AddNode(path, node, PathDelimiter);
    }

    /// <summary>
    /// Looks up the value associated with the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path to the value.</param>
    /// <returns>The value associated with the path, or <c>null</c> if the path is not found in the tree.</returns>
    public virtual T? Lookup(string path) {
        var node = _root.GetNode(path, PathDelimiter);
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
        var node = _root.GetNode(path, PathDelimiter);
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
    public virtual bool Contains(string path) => _root.GetNode(path, PathDelimiter) is not null;

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

    /// <summary>
    /// Gets the path of the node that contains the specified value.
    /// </summary>
    /// <param name="value">The value to search for in the tree.</param>
    /// <returns>The path of the node that contains the specified value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="KeyNotFoundException ">Thrown when the specified value is not found in the tree.</exception>
    public virtual string GetPath(T value) {
        if (value is null) {
            throw new ArgumentNullException(nameof(value));
        }
        if (FindNode(value) is Node n) {
            return n.Path;
        }
        throw new KeyNotFoundException();
    }

    /// <summary>
    /// Gets the siblings of the node that contains the specified value.
    /// </summary>
    /// <param name="value">The value to search for in the tree.</param>
    /// <returns>The list of sibling nodes of the node that contains the specified value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="KeyNotFoundException ">Thrown when the specified value is not found in the tree or its parent is null.</exception>
    public virtual IList<T?> GetSiblings(T value) {
        if (value is null) {
            throw new ArgumentNullException(nameof(value));
        }
        Node node = FindNode(value) ?? throw new KeyNotFoundException();
        if (node.Parent is null) {
            return new List<T?>();
        }
        return node.Parent.Children.Values.Select(x => x.Value).Where(x => !x?.Equals(value) ?? false).ToList();
    }

    /// <summary>
    /// Finds the node that contains the specified value.
    /// </summary>
    /// <param name="value">The value to search for in the tree.</param>
    /// <returns>The node that contains the specified value, or null if it is not found.</returns>
    protected Node? FindNode(T value) {
        foreach (var child in TraverseInternal(_root)) {
            if (child.Value is not null && child.Value.Equals(value)) {
                return child;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns a dictionary that contains all nodes in the tree, keyed by their paths.
    /// </summary>
    /// <returns>A dictionary that contains all nodes in the tree, keyed by their paths.</returns>
    public IDictionary<string, T?> ToDictionary() {
        var result = new Dictionary<string, T?>();
        foreach (var child in TraverseInternal(_root)) {
            result.Add(child.Path, child.Value);
        }
        return result;
    }

    /// <summary>
    /// Represents a node in the <see cref="SearchTree{T}"/>.
    /// </summary>
    protected class Node {

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value stored at the node.
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// Gets the child nodes of the current node.
        /// </summary>
        public Dictionary<string, Node> Children { get; set; }

        /// <summary>
        /// Gets the number of nodes in the subtree rooted at the current node.
        /// </summary>
        public int Count => Children.Count > 0 ? Children.Aggregate(0, (state, val) => state + val.Value.Count) : 1;

        /// <summary>
        /// Gets or sets the parent node of the current node.
        /// </summary>
        public Node? Parent { get; set; }

        /// <summary>
        /// Gets the path of the current node in the tree.
        /// </summary>
        public string Path => Parent is null ? this.Name : (string.IsNullOrEmpty(this.Parent.Path) ? this.Name : $"{this.Parent.Path}.{this.Name}");

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class with the specified name and parent node.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="parentNode">The parent node of the node.</param>
        public Node(string name, Node? parentNode) {
            Name = name;
            Parent = parentNode;
            Children = new Dictionary<string, Node>();
        }

        /// <summary>
        /// Gets the node at the specified path.
        /// </summary>
        /// <param name="path">The path of the node to get.</param>
        /// <param name="delimiter">The delimiter used in the path.</param>
        /// <returns>The node at the specified path, or <c>null</c> if the path is not valid.</returns>
        public Node? GetNode(string path, char delimiter) {
            var segments = path.Split(delimiter);
            var current = this;
            for (int i = 0; i < segments.Length; i++) {
                if (!current.Children.TryGetValue(segments[i], out var child)) {
                    return null;
                }
                current = child;
            }
            return current;
        }

        /// <summary>
        /// Adds a node at the specified path.
        /// </summary>
        /// <param name="path">The path of the node to add.</param>
        /// <param name="node">The node to add.</param>
        /// <param name="delimiter">The delimiter used in the path.</param>
        public void AddNode(string path, Node node, char delimiter) {
            var segments = path.Split(delimiter);
            var current = this;
            for (int i = 0; i < segments.Length - 1; i++) {
                var segment = segments[i];
                if (!current.Children.TryGetValue(segment, out var child)) {
                    child = new Node(segment, current);
                    current.Children[segment] = child;
                }
                current = child;
            }
            node.Parent = current;
            current.Children[segments[segments.Length - 1]] = node;
        }

    }
}
