using Battlegrounds.Game.DataCompany;
using Battlegrounds.Locale;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.MVVM.Models;

public class CompanyBrowserButton {
    public ICommand Click { get; init; }
    public LocaleKey Text { get; init; }
    public LocaleKey Tooltip { get; init; }
}

public class CompanyBrowserViewModel : IViewModel {

    public CompanyBrowserButton Create { get; }

    public CompanyBrowserButton Edit { get; }

    public CompanyBrowserButton Rename { get; }

    public CompanyBrowserButton Delete { get; }

    public CompanyBrowserButton Copy { get; }

    public CompanyBrowserButton Export { get; }

    public CompanyBrowserButton Import { get; }

    public EventCommand EditCompanyDirectly { get; }

    public ObservableCollection<Company> Companies { get; set; }

    public LocaleKey NameListViewHeader { get; }

    public LocaleKey StrengthViewHeader { get; }

    public LocaleKey TypeViewHeader { get; }

    public LocaleKey AllianceViewHeader { get; }

    public LocaleKey CountryViewHeader { get; }

    public bool SingleInstanceOnly => true;

    public CompanyBrowserViewModel() {

        // TODO: Change the LOCALE strings names to 'CompanyBrowserView_...'

        // Create create
        this.Create = new() {
            Click = new RelayCommand(this.CreateButton),
            Text = new("CompanyView_Create")
        };

        // Create edit
        this.Edit = new() {
            Click = new RelayCommand(this.EditButton),
            Text = new("CompanyView_Edit")
        };

        // Create rename
        this.Rename = new() {
            Click = new RelayCommand(this.RenameButton),
            Text = new("CompanyView_Rename")
        };

        // Create delete
        this.Delete = new() {
            Click = new RelayCommand(this.DeleteButton),
            Text = new("CompanyView_Delete")
        };

        // Create copy
        this.Copy = new() {
            Click = new RelayCommand(this.CopyButton),
            Text = new("CompanyView_Copy")
        };

        // Create export
        this.Export = new() {
            Click = new RelayCommand(this.ExportButton),
            Text = new("CompanyView_Export")
        };

        // Create import
        this.Import = new() {
            Click = new RelayCommand(this.ImportButton),
            Text = new("CompanyView_Import")
        };

        // Create double-click
        this.EditCompanyDirectly = new EventCommand<MouseButtonEventArgs>(this.EditCompany);

        // Create company container
        this.Companies = new();

        // Define locales
        this.NameListViewHeader = new LocaleKey("CompanyView_Name");
        this.StrengthViewHeader = new LocaleKey("CompanyView_Rating"); // TODO: Change the LOCALE string name to 'CompanyBrowserView_Strength' 
        this.TypeViewHeader = new LocaleKey("CompanyView_Type");
        this.AllianceViewHeader = new LocaleKey("CompanyView_Alliance");
        this.CountryViewHeader = new LocaleKey("CompanyView_Country");

    }

    public void CreateButton() {

    }

    public void EditButton() { 
    
    }

    public void RenameButton() {

    }

    public void DeleteButton() { 
    
    }

    public void CopyButton() {

    }

    public void ExportButton() { 
    
    }

    public void ImportButton() { 
    
    }

    public void EditCompany(object sender, MouseButtonEventArgs args)
        => this.EditButton();

    public void UpdateCompanyList() {

        // Clear all companies
        Companies.Clear();

        // Locad companies
        PlayerCompanies.LoadAll();

        // Update companies
        foreach (var company in PlayerCompanies.GetAllCompanies()) {
            Companies.Add(company);
        }

    }

}
