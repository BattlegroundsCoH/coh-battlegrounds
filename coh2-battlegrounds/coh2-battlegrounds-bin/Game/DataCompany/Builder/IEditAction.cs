using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Game.DataCompany.Builder;

/// <summary>
/// Interface for an editing action that can be applied and undone.
/// </summary>
public interface IEditAction<T> where T : class {

    /// <summary>
    /// 
    /// </summary>
    public readonly struct ActionResult {

        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 
        /// </summary>
        public T Target { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="target"></param>
        public ActionResult(bool success, T target) { 
            this.Success = success;
            this.Target = target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="success"></param>
        public void Deconstruct(out T target, out bool success) {
            target = this.Target;
            success = this.Success;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator T(ActionResult result) => result.Target;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator bool(ActionResult result) => result.Success;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
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
