using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Json;

namespace Battlegrounds.Campaigns.Organisation {
    
    public class Division : IJsonObject {

        public string Name { get; init; }

        public Regiment[] Regiments { get; init; }

        public string ToJsonReference() => throw new NotSupportedException();

    }

}
