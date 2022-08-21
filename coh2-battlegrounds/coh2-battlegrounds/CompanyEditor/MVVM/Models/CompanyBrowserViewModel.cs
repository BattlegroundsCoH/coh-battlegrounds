using Battlegrounds.Game.DataCompany;
using Battlegrounds.Locale;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public class CompanyBrowserButton {
    public ICommand? Click { get; init; }
    public LocaleKey? Tooltip { get; init; }
}

public class CompanyBrowserViewModel : ViewModelBase {

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

    public Company? SelectedCompany { get; set; }

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => true;

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

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do modal
        CreateCompanyDialogViewModel.ShowModal(mControl, (vm, result) => {
            
            // Check return value
            if (result is not ModalDialogResult.Confirm) {
                return;
            }

            // Check return value
            if (vm.SelectedType.Type is null) {
                Trace.WriteLine($"Fatal error: Tried to create new company with no valid type: '{vm.SelectedType.Name}'", nameof(CompanyBrowserViewModel));
                return;
            }

            // Create view model
            CompanyBuilderViewModel companyBuilder = new CompanyBuilderViewModel(vm.SelectedName, vm.SelectedFaction, vm.SelectedType.Type, vm.Package.TuningGUID);

            // Display it
            App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, companyBuilder);

        });

    }

    public void EditButton() {

        // Make sure there's a company to edit
        if (this.SelectedCompany is null) {
            return;
        }

        // Goto edit view
        App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, new CompanyBuilderViewModel(SelectedCompany));

    }

    public void RenameButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do rename modal
        RenameDialogViewModel.ShowModal(mControl, "Rename Company", x => {

            // Delete old company
            PlayerCompanies.DeleteCompany(this.SelectedCompany);

            // Save new company
            PlayerCompanies.SaveCompany(CompanyBuilder.EditCompany(this.SelectedCompany).ChangeName(x).Commit().Result);

            // Trigger refresh of company
            UpdateCompanyList();

        }, "Rename", this.SelectedCompany.Name);

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
        YesNoDialogViewModel.ShowModal(mControl, (vm, resault) => {

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

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do rename modal
        RenameDialogViewModel.ShowModal(mControl, "Copy Company", x => {

            // 'edit' company but immediately commit and save result
            PlayerCompanies.SaveCompany(CompanyBuilder.EditCompany(this.SelectedCompany).ChangeName(x).Commit().Result);

            // Trigger refresh of company
            UpdateCompanyList();

        }, "Copy", this.SelectedCompany.Name);

    }

    public void ExportButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Show export
        ImportExportCompanyDialogViewModel.ShowExport(mControl, "Export Company", CompanyTemplate.FromCompany(this.SelectedCompany).ToString(), this.SelectedCompany.Name);

    }

    public void ImportButton() {

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do import
        ImportExportCompanyDialogViewModel.ShowImport(mControl, "Import Company", (name, template) => {

            // Create company
            if (CompanyTemplate.FromTemplate(name, template, out Company? company)) {

                // Save
                PlayerCompanies.SaveCompany(company);

                // Trigger refresh of company
                UpdateCompanyList();

            } else {
                OKDialogViewModel.ShowModal(mControl, (_, _) => { }, "Import Failed", "Failed to create company from the given template string.");
            }

        });

    }

    public void EditCompany(object sender, MouseButtonEventArgs args)
        => this.EditButton();

    public void UpdateCompanyList() {

        // Clear all companies
        this.Companies.Clear();

        // Locad companies
        PlayerCompanies.LoadAll();

        // Update companies
        foreach (var company in PlayerCompanies.GetAllCompanies()) {
            this.Companies.Add(company);
        }

    }

}
