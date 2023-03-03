using System;

namespace Battlegrounds.UI;

/// <summary>
/// 
/// </summary>
public interface IViewManager {
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    void RegisterFactory<T>(Func<object?, T> factory) where T : IViewModel;

}
