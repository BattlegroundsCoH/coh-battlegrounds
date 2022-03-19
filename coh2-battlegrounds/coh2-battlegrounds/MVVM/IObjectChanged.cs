namespace BattlegroundsApp.MVVM;

/// <summary>
/// Event handler for objects that have changed.
/// </summary>
/// <param name="sender">The object that triggered the change.</param>
/// <param name="newObject">The object that was changed.</param>
public delegate void ObjectChangedEventHandler(object sender, IObjectChanged newObject);

/// <summary>
/// Interface for handling objects that can change and should notify other objects.
/// </summary>
public interface IObjectChanged {

    /// <summary>
    /// Event triggered when object is changed.
    /// </summary>
    event ObjectChangedEventHandler ObjectChanged;

}
