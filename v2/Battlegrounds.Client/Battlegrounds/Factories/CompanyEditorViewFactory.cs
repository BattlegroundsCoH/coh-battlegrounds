using System.Windows.Controls;

using Battlegrounds.Models.Companies;
using Battlegrounds.ViewModels;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Factories;

public static class CompanyEditorViewFactory {

    public static UserControl CreateCompanyEditorView(IServiceProvider serviceProvider, CreateCompanyParameters createParameters) {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(createParameters, nameof(createParameters));
        var companyEditorViewModel = ActivatorUtilities.CreateInstance<CompanyEditorViewModel>(serviceProvider, new CompanyEditorViewModelContext(null, createParameters));
        var companyEditorView = ActivatorUtilities.CreateInstance<CompanyEditorView>(serviceProvider, companyEditorViewModel);
        return companyEditorView;
    }

    public static UserControl CreateCompanyEditorView(IServiceProvider serviceProvider, Company company) {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(company, nameof(company));
        var companyEditorViewModel = ActivatorUtilities.CreateInstance<CompanyEditorViewModel>(serviceProvider, new CompanyEditorViewModelContext(company, null));
        var companyEditorView = ActivatorUtilities.CreateInstance<CompanyEditorView>(serviceProvider, companyEditorViewModel);
        return companyEditorView;
    }

}
