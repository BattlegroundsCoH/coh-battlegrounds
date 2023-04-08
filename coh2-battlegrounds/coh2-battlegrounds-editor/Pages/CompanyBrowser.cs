using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Battlegrounds.Editor.Modals;
using Battlegrounds.Game.DataCompany;

using Battlegrounds.UI;
using Battlegrounds.UI.Modals;
using Battlegrounds.UI.Modals.Prompts;

using static Battlegrounds.DataLocal.Companies;
using static Battlegrounds.UI.AppContext;
using Battlegrounds.Locale;

namespace Battlegrounds.Editor.Pages;

public record CompanyBrowserButton(ICommand? Click, LocaleKey? Tooltip = null);

public class CompanyBrowser : ViewModelBase {

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

    public CompanyBrowser() {

        // TODO: Change the LOCALE strings names to 'CompanyBrowserView_...'

        // Create create
        this.Create = new(new RelayCommand(this.CreateButton));

        // Create edit
        this.Edit = new(new RelayCommand(this.EditButton));

        // Create rename
        this.Rename = new(new RelayCommand(this.RenameButton));

        // Create delete
        this.Delete = new(new RelayCommand(this.DeleteButton));

        // Create copy
        this.Copy = new(new RelayCommand(this.CopyButton));

        // Create export
        this.Export = new(new RelayCommand(this.ExportButton));

        // Create import
        this.Import = new(new RelayCommand(this.ImportButton));

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

        // Do modal
        CreateCompany.Show((vm, result) => {

            // Check return value
            if (result is not ModalDialogResult.Confirm) {
                return;
            }

            // Check return value
            if (vm.SelectedType.Type is null) {
                Trace.WriteLine($"Fatal error: Tried to create new company with no valid type: '{vm.SelectedType.Name}'", nameof(CompanyBrowser));
                return;
            }

            // Create view model
            CompanyEditor editor = new CompanyEditor(vm.SelectedName, vm.SelectedFaction.Self, vm.SelectedType.Type, vm.Package.TuningGUID);

            // Display it
            GetViewManager().UpdateDisplay(AppDisplayTarget.Right, editor);

        });

    }

    public void EditButton() {

        // Make sure there's a company to edit
        if (this.SelectedCompany is null) {
            return;
        }

        // Goto edit view
        GetViewManager().UpdateDisplay(AppDisplayTarget.Right, new CompanyEditor(SelectedCompany));

    }

    public void RenameButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Do rename modal
        RenamePrompt.Show("Rename Company", (_, _, x) => {

            // Delete old company
            DeleteCompany(this.SelectedCompany);

            // Save new company
            SaveCompany(CompanyBuilder.EditCompany(this.SelectedCompany).ChangeName(x).Commit().Result);

            // Trigger refresh of company
            UpdateCompanyList();

        }, "Rename", this.SelectedCompany.Name);

    }

    public void DeleteButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Do modal
        YesNoPrompt.Show((vm, resault) => {

            // Check return value
            if (resault is not ModalDialogResult.Confirm) {
                return;
            }

            // Delete the company
            DeleteCompany(SelectedCompany);

            // Update company list
            UpdateCompanyList();

        }, "Delete Company", "This action can not be undone. Are you sure?");

    }

    public void CopyButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Do rename modal
        RenamePrompt.Show("Copy Company", (_, _, x) => {

            // 'edit' company but immediately commit and save result
            SaveCompany(CompanyBuilder.EditCompany(this.SelectedCompany).ChangeName(x).Commit().Result);

            // Trigger refresh of company
            UpdateCompanyList();

        }, "Copy", this.SelectedCompany.Name);

    }

    public void ExportButton() {

        // Safeguard
        if (this.SelectedCompany is null)
            return;

        // Show export
        ImportExport.ShowExport("Export Company", CompanyTemplate.FromCompany(this.SelectedCompany).ToString(), this.SelectedCompany.Name);

    }

    public void ImportButton() {

        // Do import
        ImportExport.ShowImport("Import Company", (name, template) => {

            try {

                // Create company from template
                if (CompanyTemplate.FromTemplate(name, template, out Company? company)) {

                    // Save
                    SaveCompany(company);

                    // Trigger refresh of company
                    UpdateCompanyList();

                    // Bail now
                    return;

                }

            } catch (Exception e) { // Catch any error
                Trace.WriteLine(e, nameof(CompanyBrowser));
            }

            // Catch all situation
            OKPrompt.Show(OKPrompt.Nothing, "Import Failed", "Failed to create company from the given template string (Error dumped to log).");

        });

    }

    public void EditCompany(object sender, MouseButtonEventArgs args)
        => this.EditButton();

    public void UpdateCompanyList() {

        // Clear all companies
        this.Companies.Clear();

        // Locad companies
        LoadAll();

        // Update companies
        foreach (var company in GetAllCompanies()) {
            this.Companies.Add(company);
        }

    }

}
