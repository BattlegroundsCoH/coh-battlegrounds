using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.UI;

/// <summary>
/// 
/// </summary>
public interface IUIModule {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controller"></param>
    void RegisterMenuCallbacks(IMenuController controller);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewManager"></param>
    void RegisterViewFactories(IViewManager viewManager);

}
