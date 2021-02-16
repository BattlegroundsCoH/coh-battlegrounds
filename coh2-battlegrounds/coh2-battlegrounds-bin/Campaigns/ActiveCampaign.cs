using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Json;

namespace Battlegrounds.Campaigns {
    
    public class ActiveCampaign : IJsonObject {

        private ActiveCampaign() { }

        public string ToJsonReference() => throw new NotSupportedException();

        public static ActiveCampaign FromPackage(CampaignPackage package) {
            return null;
        }

    }

}
