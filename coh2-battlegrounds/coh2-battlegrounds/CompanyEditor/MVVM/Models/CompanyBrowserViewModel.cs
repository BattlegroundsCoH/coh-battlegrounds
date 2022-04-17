using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using BattlegroundsApp.Dialogs.CreateCompany;
using BattlegroundsApp.Dialogs.RenameCopyDialog;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals;
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

    public LocaleKey StrengthListViewHeader { get; }

    public LocaleKey TypeListViewHeader { get; }

    public LocaleKey AllianceListViewHeader { get; }

    public LocaleKey CountryListViewHeader { get; }

    public Company SelectedCompany { get; set; }

    public bool SingleInstanceOnly => true;

    public bool KeepAlive => true;

    public CompanyBrowserViewModel() {

        // TODO: Change the LOCALE strings names to 'CompanyBrowserView_...'

        // Create create
        this.Create = new() {
            Click = new RelayCommand(this.CreateButton)
        };

        // Create edit
        this.Edit = new() {
            Click = new RelayCommand(this.EditButton)
        };

        // Create rename
        this.Rename = new() {
            Click = new RelayCommand(this.RenameButton)
        };

        // Create delete
        this.Delete = new() {
            Click = new RelayCommand(this.DeleteButton)
        };

        // Create copy
        this.Copy = new() {
            Click = new RelayCommand(this.CopyButton)
        };

        // Create export
        this.Export = new() {
            Click = new RelayCommand(this.ExportButton)
        };

        // Create import
        this.Import = new() {
            Click = new RelayCommand(this.ImportButton)
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

        // Grab mod GUID (TODO: Allow user to pick in modal dialog)
        ModGuid modGuid = ModManager.GetPackage("mod_bg").TuningGUID;

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do modal
        Modals.Dialogs.MVVM.Models.CreateCompanyDialogViewModel.ShowModal(mControl, (vm, resault) => {
            
            // Check return value
            if (resault is not ModalDialogResult.Confirm) {
                return;
            }

            // Create view model
            CompanyBuilderViewModel companyBuilder = new CompanyBuilderViewModel(vm.CompanyName, vm.CompanyFaction, vm.CompanyType, modGuid);

            // Display it
            App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, companyBuilder);

        });

    }

    public void EditButton() {

        if (this.SelectedCompany is null) {
            return;
        }

        CompanyBuilderViewModel companyBuilder = new CompanyBuilderViewModel(SelectedCompany);

        App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, companyBuilder);

    }

    // TODO: CompanyTemplate.cs is broken
    public void RenameButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        if (RenameCopyDialogViewModel.ShowRenameDialog(new LocaleKey("CompanyView_RenameCopyDialog_Rename_Title"), out string companyName)
            is RenameCopyDialogResult.Rename) {

            // Delete old company
            PlayerCompanies.DeleteCompany(this.SelectedCompany);

            // Save new company
            PlayerCompanies.SaveCompany(CompanyBuilder.EditCompany(this.SelectedCompany).ChangeName(companyName).Commit().Result);

            // Trigger refresh of company
            UpdateCompanyList();

        }

    }

    public void DeleteButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do modal
        Modals.Dialogs.MVVM.Models.YesNoDialogViewModel.ShowModal(mControl, (vm, resault) => {

            // Check return value
            if (resault is not ModalDialogResult.Confirm) {
                return;
            }

            PlayerCompanies.DeleteCompany(SelectedCompany);

            UpdateCompanyList();

        }, "Delete Company", "This action can not be undone. Are you sure?");

    }

    public void CopyButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        if (RenameCopyDialogViewModel.ShowCopyDialog(new LocaleKey("CompanyView_RenameCopyDialog_Copy_Title"), out string companyName)
            is RenameCopyDialogResult.Copy) {

            // 'edit' company but immediately commit and save result
            PlayerCompanies.SaveCompany(CompanyBuilder.EditCompany(this.SelectedCompany).ChangeName(companyName).Commit().Result);

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

    public void Swapback() {

    }

}
