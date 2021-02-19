using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns {
    
    public enum CampaignType {
        Undefined,
        SingleplayerOnly,
        CooperativeOnly,
        CompetitiveOnly,
    }

    public enum CampaignTheatre {
        Undefined,
        East,
        West,
        EastWest
    }

    public record CampaignArmy(LocaleKey Name, LocaleKey Desc, Faction Army, int Min, int Max);

    public record CampaignDate(int Year, int Month, int Day);

    public class CampaignPackage {

        public CampaignType CampaignType { get; }

        public CampaignTheatre Theatre { get; }

        public bool LoadFromBinary(string binaryFilepath) => true;

    }

}
