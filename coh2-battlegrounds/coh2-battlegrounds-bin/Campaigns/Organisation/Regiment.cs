using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Json;

namespace Battlegrounds.Campaigns.Organisation {
    
    public class Regiment : IJsonObject {

        public string Name { get; }

        public string ToJsonReference() => throw new NotSupportedException();

    }

}
