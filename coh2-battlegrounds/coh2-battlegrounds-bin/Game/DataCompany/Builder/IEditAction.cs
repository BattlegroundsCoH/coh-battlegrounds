using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Game.DataCompany.Builder;

/// <summary>
/// 
/// </summary>
public interface IEditAction<T> where T : class {

    public readonly struct ActionResult {
        public bool Success { get; }
        public T Target { get; }
        public ActionResult(bool success, T target) { 
            this.Success = success;
            this.Target = target;
        }
        public void Deconstruct(out T target, out bool success) {
            target = this.Target;
            success = this.Success;
        }
        public static implicit operator T(ActionResult result) => result.Target;
        public static implicit operator bool(ActionResult result) => result.Success;
        public static implicit operator ActionResult(T val) => val is null ? new ActionResult(false, val) : new ActionResult(true, val);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    ActionResult Apply(T target);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    ActionResult Undo(T target);

}
