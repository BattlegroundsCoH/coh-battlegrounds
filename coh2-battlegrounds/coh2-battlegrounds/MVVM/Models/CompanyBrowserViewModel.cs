using Battlegrounds.Locale;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.MVVM.Models;

public class CompanyBrowserButton {
    public ICommand Click { get; init; }
    public LocaleKey Text { get; init; }
    public LocaleKey localeKey { get; init; }
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

    public bool SingleInstanceOnly => true;

    public CompanyBrowserViewModel() {

        // Create create
        this.Create = new() {
            Click = new RelayCommand(this.CreateButton),
            Text = new("")
        };

        // Create edit
        this.Edit = new() {
            Click = new RelayCommand(this.EditButton),
            Text = new("")
        };

        // Create rename
        this.Rename = new() {
            Click = new RelayCommand(this.RenameButton),
            Text = new("")
        };

        // Create delete
        this.Delete = new() {
            Click = new RelayCommand(this.DeleteButton),
            Text = new("")
        };

        // Create copy
        this.Copy = new() {
            Click = new RelayCommand(this.CopyButton),
            Text = new("")
        };

        // Create export
        this.Export = new() {
            Click = new RelayCommand(this.ExportButton),
            Text = new("")
        };

        // Create import
        this.Import = new() {
            Click = new RelayCommand(this.ImportButton),
            Text = new("")
        };

        // Create double-click
        this.EditCompanyDirectly = new EventCommand<MouseButtonEventArgs>(this.EditCompany);

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

}
