using System;

using Battlegrounds.Editor.Pages;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.UI;

namespace Battlegrounds.Editor;

public class EditorModule : IUIModule {
    
    public void RegisterMenuCallbacks(IMenuController controller) {
        
        // Set menu callback
        controller.SetMenuCallback(MenuButton.Companies, this.OpenCompanyBrowser);

    }

    private void OpenCompanyBrowser(AppViewManager appViewManager) {

        // Update display to company browser
        appViewManager.UpdateDisplay<CompanyBrowser>(AppDisplayTarget.Right, browser => {
            if (browser is CompanyBrowser vm) {
                vm.UpdateCompanyList();
            }
        });

    }

    public void RegisterViewFactories(IViewManager viewManager) {
        
        // Register factory
        viewManager.RegisterFactory(x => x is Company c ? new CompanyEditor(c) : throw new Exception());

    }

}
