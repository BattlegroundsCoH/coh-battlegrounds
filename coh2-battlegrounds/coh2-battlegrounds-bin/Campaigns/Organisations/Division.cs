using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns.Organisations {

    public class Division {

        public LocaleKey Name { get; init; }

        public List<Regiment> Regiments { get; }

        public uint DivisionUid { get; }

        public int MaxMove { get; init; }

        public string TemplateName { get; init; }

        // See Regiment.cs
        public Army EleemntOf { get; }

        public Division(uint uid, Army army, LocaleKey name) {
            this.DivisionUid = uid;
            this.Name = name;
            this.EleemntOf = army;
            this.Regiments = new List<Regiment>();
        }

    }

}
