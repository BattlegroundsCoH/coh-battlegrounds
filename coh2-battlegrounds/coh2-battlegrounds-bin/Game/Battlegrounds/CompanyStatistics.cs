using Battlegrounds.Json;

namespace Battlegrounds.Game.Battlegrounds {

    public class CompanyStatistics : IJsonObject {

        #region Win/Lose counts

        public ulong TotalMatchCount { get; set; }

        public ulong TotalMatchLossCount { get; set; }

        public ulong TotalMatchWinCount { get; set; }

        [JsonIgnore]
        public double WinRate => this.TotalMatchCount > 0 ? this.TotalMatchWinCount / (double)this.TotalMatchCount : 0;

        [JsonIgnore]
        public double LossRate => this.TotalMatchCount > 0 ? this.TotalMatchLossCount / (double)this.TotalMatchCount : 0;

        #endregion

        #region Unit loss counts

        public ulong TotalInfantryLosses { get; set; }

        public ulong TotalVehicleLosses { get; set; }

        [JsonIgnore]
        public ulong TotalLosses => this.TotalInfantryLosses + this.TotalVehicleLosses;

        #endregion

        public string ToJsonReference() => "";

    }

}
