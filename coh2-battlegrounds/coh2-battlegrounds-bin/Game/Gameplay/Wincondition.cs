using System;
using Battlegrounds.Json;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class Wincondition : IJsonObject, IWinconditionMod {
    
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public ModGuid Guid { get; }

        /// <summary>
        /// 
        /// </summary>
        public WinconditionOption[] Options { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DefaultOptionIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint DisplayShortDescription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint[] DisplayGoals { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        public Wincondition(string name, Guid guid) : this(name, ModGuid.FromGuid(guid)) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        public Wincondition(string name, ModGuid guid) {
            this.Name = name;
            this.Guid = guid;
            this.DefaultOptionIndex = 0;
        }

        public override string ToString() => this.Name;

        public string ToJsonReference() => this.Guid.ToString();

    }

}
