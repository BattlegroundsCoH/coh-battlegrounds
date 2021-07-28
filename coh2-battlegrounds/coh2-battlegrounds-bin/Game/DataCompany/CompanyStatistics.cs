using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany {

    /// <summary>
    /// Class representing the win and loss statistics of a company.
    /// </summary>
    public class CompanyStatistics : ICloneable, IChecksumPropertyItem {

        #region Win/Lose counts

            [ChecksumProperty]
            public ulong TotalMatchCount { get; set; }

            [ChecksumProperty]
            public ulong TotalMatchLossCount { get; set; }

            [ChecksumProperty]
            public ulong TotalMatchWinCount { get; set; }

            [JsonIgnore]
            public double WinRate => this.TotalMatchCount > 0 ? this.TotalMatchWinCount / (double)this.TotalMatchCount : 0;

            [JsonIgnore]
            public double LossRate => this.TotalMatchCount > 0 ? this.TotalMatchLossCount / (double)this.TotalMatchCount : 0;

        #endregion

        #region Unit loss counts

            [ChecksumProperty]
            public ulong TotalInfantryLosses { get; set; }

            [ChecksumProperty]
            public ulong TotalVehicleLosses { get; set; }

            [JsonIgnore]
            public ulong TotalLosses => this.TotalInfantryLosses + this.TotalVehicleLosses;

        #endregion

        public object Clone() => JsonSerializer.Deserialize<CompanyStatistics>(JsonSerializer.Serialize(this));
        
    }

}
