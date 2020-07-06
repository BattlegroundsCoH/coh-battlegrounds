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
        public readonly struct WinconditionOption {
            public string Title { get; }
            public int Value { get; }
            public WinconditionOption(string title, int val) {
                this.Title = title;
                this.Value = val;
            }
            public void Deconstruct(out string title, out int value) {
                title = this.Title;
                value = this.Value;
            }
            public override string ToString() => this.Title;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public Guid Guid { get; }

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
        /// <param name="name"></param>
        /// <param name="guid"></param>
        public Wincondition(string name, Guid guid) {
            this.Name = name;
            this.Guid = guid;
        }

        public override string ToString() => this.Name;

        public string ToJsonReference() => this.Guid.ToString();

    }

}
