using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Game.Database.json {

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    class JsonIgnoreAttribute : Attribute {}

}
