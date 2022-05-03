using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Controls.Dashboard;

/// <summary>
/// Interaction logic for CompanyCard.xaml
/// </summary>
public partial class CompanyCard : UserControl {

    private static readonly Dictionary<Faction, ImageSource> Flags = new() {
        [Faction.Soviet] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/soviet.png")),
        [Faction.Wehrmacht] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/german.png")),
        [Faction.America] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/aef.png")),
        [Faction.OberkommandoWest] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/west_german.png")),
        [Faction.British] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/british.png"))
    };

    private static readonly Dictionary<string, ImageSource> Icons = new() {
        [nameof(CompanyType.Infantry)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_infantry.png")),
        [nameof(CompanyType.Armoured)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_armoured.png")),
        [nameof(CompanyType.Motorized)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_motorized.png")),
        [nameof(CompanyType.Mechanized)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_mechanized.png")),
        [nameof(CompanyType.Airborne)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_airborne.png")),
        [nameof(CompanyType.Artillery)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_artillery.png")),
        [nameof(CompanyType.TankDestroyer)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_td.png")),
        [nameof(CompanyType.Engineer)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_engineer.png")),
        [nameof(CompanyType.Unspecified)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_unspecified.png")),
        [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_unspecified.png"))
    };

    public static readonly DependencyProperty CompanyProperty 
        = DependencyProperty.Register(nameof(Company), typeof(Company), typeof(CompanyCard),
                                      new FrameworkPropertyMetadata(null,
                                      (a, b) => a.Cast<CompanyCard>(x => x.Company = b.NewValue as Company)));

    public Company? Company {
        get => this.GetValue(CompanyProperty) as Company;
        set {
            this.SetValue(CompanyProperty, value);
            this.TrySetCompanyData();
        }
    }

    public CompanyCard() {
        InitializeComponent();
    }

    private void TrySetCompanyData() => this.TrySetData(Company ?? null);

    private void TrySetData(Company company) {

        // Do nothing if company is null
        if (company is null) {
            companyName.Text = "No Company Data";
            winRateValue.Text = "N/A";
            infantryKillsValue.Text = "N/A";
            vehicleKillsValue.Text = "N/A";
            killDeathRatioValue.Text = "N/A";
            return;
        }

        // Set company faction
        factionIcon.Source = Flags[company.Army];

        // Set company name
        companyName.Text = company.Name;

        // Set company type
        typeIcon.Source = Icons.GetValueOrDefault(company.Type.ToString(), Icons[string.Empty]);

        // Set company data
        winRateValue.Text = company.Statistics.WinRate is 0 ? "N/A" : company.Statistics.WinRate.ToString();
        infantryKillsValue.Text = "N/A"; // TODO : This data is not currently being tracked
        vehicleKillsValue.Text = "N/A"; // TODO : This data is not currently being tracked
        killDeathRatioValue.Text = "N/A"; // TODO : This data is not currently being tracked


    }

}
