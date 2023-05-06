using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Util;

/// <summary>
/// Represents a search tree that merges multiple search trees into a single one.
/// </summary>
/// <typeparam name="T">The type of values stored in the search tree.</typeparam>
public sealed class MergedSearchTree<T> : SearchTree<T> {

    private readonly IList<SearchTree<T>> _trees;

    /// <summary>
    /// Gets the number of search trees merged into this instance.
    /// </summary>
    public override int Count => _trees.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergedSearchTree{T}"/> class
    /// by merging the specified search trees.
    /// </summary>
    /// <param name="trees">The search trees to be merged.</param>
    public MergedSearchTree(params SearchTree<T>[] trees) {
        _trees = trees;
    }

    /// <inheritdoc/>
    public override T? Lookup(string path) {
        for (int i = 0; i < _trees.Count; i++) {
            if (_trees[i].Lookup(path, out T? val)) {
                return val;
            }
        }
        return default;
    }

    /// <inheritdoc/>
    public override bool Lookup(string path, [NotNullWhen(true)] out T? value) {
        for (int i = 0; i < _trees.Count; i++) {
            if (_trees[i].Lookup(path, out T? val)) {
                value = val;
                return true;
            }
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/> because adding nodes to a merged search tree is not supported.
    /// </summary>
    /// <param name="path">The path of the node to add.</param>
    /// <param name="value">The value of the node to add.</param>
    /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
    public override void Add(string path, T value) => throw new NotSupportedException();

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/> because removing nodes from a merged search tree is not supported.
    /// </summary>
    /// <param name="path">The path of the node to remove.</param>
    /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
    public override void Remove(string path) => throw new NotSupportedException();

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/> because clearing a merged search tree is not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
    public override void Clear() => throw new NotSupportedException();

    /// <inheritdoc/>
    public override IEnumerator<T> GetEnumerator() => TraverseAll().GetEnumerator();

    private IEnumerable<T> TraverseAll() {
        foreach (var t in _trees) {
            foreach (var val in t) {
                yield return val;
            }
        }
    }

}
