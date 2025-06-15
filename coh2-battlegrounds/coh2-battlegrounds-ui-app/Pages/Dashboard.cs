using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.DataLocal;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.UI.Application.Pages;

public enum CompanyDataType {
    Games,
    Wins,
    Losses,
    InfantryKills,
    VehicleKills,
    InfantryLosses,
    VehicleLosses,
    WinRate,
    Company,
    FactionGames
}

/// <summary>
/// 
/// </summary>
public sealed class Dashboard : ViewModelBase {

    #region Private member

    /// <summary>
    /// The name of the currently "loged" in player
    /// </summary>
    private string m_playerName = BattlegroundsContext.Steam.HasUser ? BattlegroundsContext.Steam.User.Name : "Unknown";

    /// <summary>
    /// The number of games played sumerized
    /// </summary>
    private ulong m_totalGames;

    /// <summary>
    /// The number of wins sumerized
    /// </summary>
    private ulong m_totalWins;

    /// <summary>
    /// The number of losses sumerized
    /// </summary>
    private ulong m_totalLosses;

    /// <summary>
    /// The number of infantry kills sumerized
    /// </summary>
    private ulong m_totalInfantryKills; // TODO

    /// <summary>
    /// The number of vehicle kills sumerized
    /// </summary>
    private ulong m_totalVehicleKills; // TODO

    /// <summary>
    /// The number of infantry losses sumerized
    /// </summary>
    private ulong m_totalInfantryLosses;

    /// <summary>
    /// The number of vehicles losses sumerized
    /// </summary>
    private ulong m_totalVehicleLosses;

    /// <summary>
    /// The total win/loss ration 
    /// </summary>
    private double m_winRate;

    /// <summary>
    /// The total amount of games played as Wehrmach
    /// </summary>
    private ulong m_wehrmachtGames;

    /// <summary>
    /// The total amount of games played as Soviets
    /// </summary>
    private ulong m_sovietGames;

    /// <summary>
    /// The total amount of games played as USF
    /// </summary>
    private ulong m_usfGames;

    /// <summary>
    /// The total amount of games played as UKF
    /// </summary>
    private ulong m_ukfGames;

    /// <summary>
    /// The total amount of games played as OKW
    /// </summary>
    private ulong m_okwGames;

    /// <summary>
    /// The most played company
    /// </summary>
    private Company? m_mostPlayedCompany;

    #endregion

    #region Public Properties

    /// <summary>
    /// The name of the currently "loged" in player
    /// </summary>
    public string PlayerName {
        get => this.m_playerName;
        set {
            this.m_playerName = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of wins sumerized
    /// </summary>
    public ulong TotalWins {
        get => this.m_totalWins;
        set {
            this.m_totalWins = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of losses sumerized
    /// </summary>
    public ulong TotalLosses {
        get => this.m_totalLosses;
        set {
            this.m_totalLosses = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of games played sumerized
    /// </summary>
    public ulong TotalGamesPlayed {
        get {
            return this.m_totalGames;
        }
        set {
            this.m_totalGames = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The total win/loss ration 
    /// </summary>
    public double WinRate {
        get => this.m_winRate;
        set {
            this.m_winRate = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of infantry kills sumerized
    /// </summary>
    public ulong TotalInfantryKills {
        get => this.m_totalInfantryKills;
        set {
            this.m_totalInfantryKills = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of vehicle kills sumerized
    /// </summary>
    public ulong TotalVehicleKills {
        get => this.m_totalVehicleKills;
        set {
            this.m_totalVehicleKills = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of infantry losses sumerized
    /// </summary>
    public ulong TotalInfantryLosses {
        get => this.m_totalInfantryLosses;
        set {
            this.m_totalInfantryLosses = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The number of vehicle losses sumerized
    /// </summary>
    public ulong TotalVehicleLosses {
        get => this.m_totalVehicleLosses;
        set {
            this.m_totalVehicleLosses = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The total kill/death ratio
    /// </summary>
    public double KillDeathRatio {
        get {
            return (this.TotalInfantryLosses + this.TotalVehicleLosses) > 0 ? (this.TotalInfantryKills + this.TotalVehicleKills) / (this.TotalInfantryLosses + this.TotalVehicleLosses) : 0;
        }
        set {
            this.Notify();
        }
    }

    /// <summary>
    /// The total games played as Wehrmacht
    /// </summary>
    public ulong WehrmachtGames {
        get => this.m_wehrmachtGames;
        set {
            this.m_wehrmachtGames = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The total games played as Soviets
    /// </summary>
    public ulong SovietGames {
        get => this.m_sovietGames;
        set {
            this.m_sovietGames = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The total games played as USF
    /// </summary>
    public ulong USFGames {
        get => this.m_usfGames;
        set {
            this.m_usfGames = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The total games played as UKF
    /// </summary>
    public ulong UKFGames {
        get => this.m_ukfGames;
        set {
            this.m_ukfGames = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The total games played as OKW
    /// </summary>
    public ulong OKWGames {
        get => this.m_okwGames;
        set {
            this.m_okwGames = value;
            this.Notify();
        }
    }

    /// <summary>
    /// The most played company
    /// </summary>
    public Company? MostPlayedCompany {
        get => this.m_mostPlayedCompany;
        set {
            this.m_mostPlayedCompany = value;
            this.Notify();
        }
    }

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => false;

    #endregion

    /// <summary>
    /// Default constructor
    /// </summary>
    public Dashboard() {
        Companies.PlayerCompaniesLoaded += OnPlayerCompaniesLoaded;
    }

    public void UpdateSteamUser() {
        this.PlayerName = BattlegroundsContext.Steam.User.Name;
    }

    private void OnPlayerCompaniesLoaded() {

        this.TotalGamesPlayed = this.GetCompanyData<ulong>(CompanyDataType.Games);
        this.TotalWins = this.GetCompanyData<ulong>(CompanyDataType.Wins);
        this.TotalLosses = this.GetCompanyData<ulong>(CompanyDataType.Losses);
        this.TotalInfantryLosses = this.GetCompanyData<ulong>(CompanyDataType.InfantryLosses);
        this.TotalVehicleLosses = this.GetCompanyData<ulong>(CompanyDataType.VehicleLosses);
        this.WinRate = this.TotalGamesPlayed > 0 ? Math.Round(this.TotalWins / (double)this.TotalGamesPlayed * 100) : 0.0;
        this.MostPlayedCompany = this.GetCompanyData<Company>(CompanyDataType.Company);

        this.WehrmachtGames = this.GetCompanyData<ulong>(CompanyDataType.FactionGames, "ger");
        this.SovietGames = this.GetCompanyData<ulong>(CompanyDataType.FactionGames, "sov");
        this.USFGames = this.GetCompanyData<ulong>(CompanyDataType.FactionGames, "usa");
        this.UKFGames = this.GetCompanyData<ulong>(CompanyDataType.FactionGames, "ukf");
        this.OKWGames = this.GetCompanyData<ulong>(CompanyDataType.FactionGames, "okw");

    }

    private T? GetCompanyData<T>(CompanyDataType type, string faction = "") => type switch {
        CompanyDataType.Games or
        CompanyDataType.Wins or
        CompanyDataType.Losses or
        CompanyDataType.InfantryLosses or
        CompanyDataType.VehicleLosses => (T)Convert.ChangeType(this.SumerizeData(type), typeof(T)),
        CompanyDataType.FactionGames => (T)Convert.ChangeType(this.SumerizeData(type, faction), typeof(T)),
        CompanyDataType.WinRate => (T)Convert.ChangeType(this.DataAverage(type), typeof(T)),
        CompanyDataType.Company => (T?)Convert.ChangeType(this.GetMostPlayedCompany(), typeof(T)),
        _ => default,
    };

    // TODO : Add returns for Infantry & Vehicle kills
    private ulong SumerizeData(CompanyDataType type, string faction = "") => type switch {
        CompanyDataType.Games => Companies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchCount),
        CompanyDataType.Wins => Companies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchWinCount),
        CompanyDataType.Losses => Companies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchLossCount),
        CompanyDataType.InfantryLosses => Companies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalInfantryLosses),
        CompanyDataType.VehicleLosses => Companies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalVehicleLosses),
        CompanyDataType.FactionGames => Companies.GetAllCompanies().Where(x => x.Army == Faction.FromName(faction))
                                                       .Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchCount),
        _ => 0,
    };

    // TODO : Add return for KD rate
    private double DataAverage(CompanyDataType type) => type switch {
        CompanyDataType.WinRate => Companies.GetAllCompanies().Select(x => x.Statistics.WinRate).Average(),
        _ => throw new Exception()
    };

    private Company? GetMostPlayedCompany() => Companies.GetAllCompanies().Select(x => (x, x.Units.Aggregate(TimeSpan.Zero, (a, b) => a + b.CombatTime)))
                                                                                .OrderByDescending(y => y.Item2)
                                                                                .Select(z => z.x).FirstOrDefault();

}
