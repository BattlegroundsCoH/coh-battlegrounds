using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using BattlegroundsApp.Dialogs.CreateCompany;
using BattlegroundsApp.Dialogs.RenameCopyDialog;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

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

    public BattlegroundsApp.MVVM.EventCommand EditCompanyDirectly { get; }

    public ObservableCollection<Company> Companies { get; set; }

    public LocaleKey NameListViewHeader { get; }

    public LocaleKey StrengthListViewHeader { get; }

    public LocaleKey TypeListViewHeader { get; }

    public LocaleKey AllianceListViewHeader { get; }

    public LocaleKey CountryListViewHeader { get; }

    public Company SelectedCompany { get; set; }

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
        this.StrengthListViewHeader = new LocaleKey("CompanyView_Rating"); // TODO: Change the LOCALE string name to 'CompanyBrowserView_Strength' 
        this.TypeListViewHeader = new LocaleKey("CompanyView_Type");
        this.AllianceListViewHeader = new LocaleKey("CompanyView_Alliance");
        this.CountryListViewHeader = new LocaleKey("CompanyView_Country");

    }

    public void CreateButton() {

        ModGuid modGuid = ModManager.GetPackage("mod_bg").TuningGUID;
        Trace.TraceWarning("There is currently no method of setting tuning pack. This should be fixed ASAP.");

        if (CreateCompanyDialogViewModel.ShowCreateCompanyDialog(new LocaleKey("CompanyView_CreateCompanyDialog_Title"), out string companyName, out Faction companyFaction, out CompanyType companyType) 
            is CreateCompanyDialogResult.Create) {

            CompanyBuilderViewModel companyBuilder = new CompanyBuilderViewModel(companyName, companyFaction, companyType, modGuid);

            App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, companyBuilder);

        }

    }

    public void EditButton() {

        CompanyBuilderViewModel companyBuilder = new CompanyBuilderViewModel(SelectedCompany);

        App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, companyBuilder);

    }

    // TODO: CompanyTemplate.cs is broken
    public void RenameButton() {

        if (RenameCopyDialogViewModel.ShowRenameDialog(new LocaleKey("CompanyView_RenameCopyDialog_Rename_Title"), out string companyName)
            is RenameCopyDialogResult.Rename) {

            var builder = new CompanyBuilder();
            builder.CloneCompany(SelectedCompany, companyName, CompanyAvailabilityType.MultiplayerOnly).Commit();

            PlayerCompanies.DeleteCompany(SelectedCompany);
            PlayerCompanies.SaveCompany(builder.Result);

            UpdateCompanyList();

        }

    }

    public void DeleteButton() { 

        if (YesNoDialogViewModel.ShowYesNoDialog(new LocaleKey("CompanyView_YesNoDialog_Delete_Company_Title"), new LocaleKey("CompanyView_YesNoDialog_Delete_Company_Message"))
            is YesNoDialogResult.Confirm) {

            PlayerCompanies.DeleteCompany(SelectedCompany);

            UpdateCompanyList();

        }
    
    }

    // TODO: CompanyTemplate.cs is broken
    public void CopyButton() {

        if (RenameCopyDialogViewModel.ShowCopyDialog(new LocaleKey("CompanyView_RenameCopyDialog_Copy_Title"), out string companyName)
            is RenameCopyDialogResult.Copy) {

            var builder = new CompanyBuilder();
            builder.CloneCompany(SelectedCompany, companyName, CompanyAvailabilityType.MultiplayerOnly).Commit();

            PlayerCompanies.SaveCompany(builder.Result);

            UpdateCompanyList();

        }

    }

    public void ExportButton() { 
    
        // TODO

    }

    public void ImportButton() { 
    
        // TODO

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

    public bool UnloadViewModel() => true;

}
