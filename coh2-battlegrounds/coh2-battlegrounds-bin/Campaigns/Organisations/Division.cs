using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Json;
using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns.Organisations {
    
    public class Division : IJsonObject {

        public LocaleKey Name { get; init; }

        public List<Regiment> Regiments { get; }

        public uint DivisionUid { get; }

        public string TemplateName { get; init; }

        // See Regiment.cs
        public Army EleemntOf { get; }

        public Division(uint uid, Army army, LocaleKey name) {
            this.DivisionUid = uid;
            this.Name = name;
            this.EleemntOf = army;
            this.Regiments = new List<Regiment>();
        }

        public string ToJsonReference() => this.DivisionUid.ToString();

    }

}
