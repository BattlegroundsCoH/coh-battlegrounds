using Battlegrounds;
using Battlegrounds.Game.DataCompany;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.MVVM;
using System;
using System.Linq;

namespace BattlegroundsApp.Dashboard.MVVM.Models;

public enum CompanyDataType {
    Games,
    Wins,
    Losses,
    InfantryKills,
    VehicleKills,
    InfantryLosses,
    VehicleLosses,
    WinRate,
    Company
}

public class DashboardViewModel : ViewModels.ViewModelBase, IViewModel {

    #region Private member

    /// <summary>
    /// The name of the currently "loged" in player
    /// </summary>
    private string m_playerName = BattlegroundsInstance.IsFirstRun ? "" : BattlegroundsInstance.Steam.User.Name;

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
            OnPropertyChanged(nameof(PlayerName));
        }
    }

    /// <summary>
    /// The number of wins sumerized
    /// </summary>
    public ulong TotalWins { 
        get => this.m_totalWins;
        set {
            this.m_totalWins = value;
            OnPropertyChanged(nameof(TotalWins));
        }
    }

    /// <summary>
    /// The number of losses sumerized
    /// </summary>
    public ulong TotalLosses { 
        get => this.m_totalLosses;
        set {
            this.m_totalLosses = value;
            OnPropertyChanged(nameof(TotalLosses));
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
            OnPropertyChanged(nameof(TotalGamesPlayed));
        }
    }

    /// <summary>
    /// The total win/loss ration 
    /// </summary>
    public double WinRate {
        get => this.m_winRate;
        set {
            this.m_winRate = value;
            OnPropertyChanged(nameof(WinRate));
        }
    }

    /// <summary>
    /// The number of infantry kills sumerized
    /// </summary>
    public ulong TotalInfantryKills { 
        get => this.m_totalInfantryKills;
        set {
            this.m_totalInfantryKills = value;
            OnPropertyChanged(nameof(TotalInfantryKills));
        }
    }

    /// <summary>
    /// The number of vehicle kills sumerized
    /// </summary>
    public ulong TotalVehicleKills { 
        get => this.m_totalVehicleKills;
        set {
            this.m_totalVehicleKills = value;
            OnPropertyChanged(nameof(TotalVehicleKills));
        }
    }

    /// <summary>
    /// The number of infantry losses sumerized
    /// </summary>
    public ulong TotalInfantryLosses { 
        get => this.m_totalInfantryLosses;
        set {
            this.m_totalInfantryLosses = value;
            OnPropertyChanged(nameof(TotalInfantryLosses));
        }
    }

    /// <summary>
    /// The number of vehicle losses sumerized
    /// </summary>
    public ulong TotalVehicleLosses { 
        get => this.m_totalVehicleLosses;
        set {
            this.m_totalVehicleLosses = value;
            OnPropertyChanged(nameof(TotalVehicleLosses));
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
            OnPropertyChanged(nameof(KillDeathRatio));
        }
    }

    /// <summary>
    /// The most played company
    /// </summary>
    public Company? MostPlayedCompany { 
        get => this.m_mostPlayedCompany;
        set {
            this.m_mostPlayedCompany = value;
            OnPropertyChanged(nameof(MostPlayedCompany));
        }
    }

    public bool SingleInstanceOnly => true;

    public bool KeepAlive => false;

    #endregion

    /// <summary>
    /// Default constructor
    /// </summary>
    public DashboardViewModel() {

        PlayerCompanies.PlayerCompaniesLoaded += OnPlayerCompaniesLoaded;

    }

    private void OnPlayerCompaniesLoaded() {

        this.TotalGamesPlayed = this.GetCompanyData<ulong>(CompanyDataType.Games);
        this.TotalWins = this.GetCompanyData<ulong>(CompanyDataType.Wins);
        this.TotalLosses = this.GetCompanyData<ulong>(CompanyDataType.Losses);
        this.TotalInfantryLosses = this.GetCompanyData<ulong>(CompanyDataType.InfantryLosses);
        this.TotalVehicleLosses = this.GetCompanyData<ulong>(CompanyDataType.VehicleLosses);
        this.WinRate = this.GetCompanyData<double>(CompanyDataType.WinRate);
        this.MostPlayedCompany = this.GetCompanyData<Company>(CompanyDataType.Company);

    }

    private T? GetCompanyData<T>(CompanyDataType type) => type switch {
        CompanyDataType.Games or
        CompanyDataType.Wins or
        CompanyDataType.Losses or
        CompanyDataType.InfantryLosses or
        CompanyDataType.VehicleLosses => (T)Convert.ChangeType(this.SumerizeData(type), typeof(T)),
        CompanyDataType.WinRate => (T)Convert.ChangeType(this.DataAverage(type), typeof(T)),
        CompanyDataType.Company => (T?)Convert.ChangeType(this.GetMostPlayedCompany(), typeof(T)),
        _ => default,
    };

    // TODO : Add returns for Infantry & Vehicle kills
    private ulong SumerizeData(CompanyDataType type) => type switch {
        CompanyDataType.Games => PlayerCompanies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchCount),
        CompanyDataType.Wins => PlayerCompanies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchWinCount),
        CompanyDataType.Losses => PlayerCompanies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalMatchLossCount),
        CompanyDataType.InfantryLosses => PlayerCompanies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalInfantryLosses),
        CompanyDataType.VehicleLosses => PlayerCompanies.GetAllCompanies().Aggregate(0ul, (a, b) => a + b.Statistics.TotalVehicleLosses),
        _ => 0,
    };

    // TODO : Add return for KD rate
    private double DataAverage(CompanyDataType type) => type switch {
        CompanyDataType.WinRate => PlayerCompanies.GetAllCompanies().Select(x => x.Statistics.WinRate).Average(),
    };

    private Company? GetMostPlayedCompany() => PlayerCompanies.GetAllCompanies().Select(x => (x, x.Units.Aggregate(TimeSpan.Zero, (a, b) => a + b.CombatTime)))
                                                                                .OrderByDescending(y => y.Item2)
                                                                                .Select(z => z.x).FirstOrDefault();
    
    public void UnloadViewModel(OnModelClosed closeCallback, bool destroy) => closeCallback(false);

    public void Swapback() {

    }

}
