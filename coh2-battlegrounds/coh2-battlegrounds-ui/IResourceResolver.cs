using System;
using System.Windows;

namespace Battlegrounds.UI;

/// <summary>
/// Interface for resolving resources.
/// </summary>
public interface IResourceResolver {

    /// <summary>
    /// Try and find a <see cref="DataTemplate"/> from the <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to find the associated <see cref="DataTemplate"/>.</param>
    /// <returns>The associated <see cref="DataTemplate"/>; <see langword="null"/> if no associated template is found.</returns>
    DataTemplate? TryFindDataTemplate(Type type);
    
}
