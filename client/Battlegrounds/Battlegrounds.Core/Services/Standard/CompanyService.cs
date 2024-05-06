using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Companies.Serializing;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services.Standard;

public class CompanyService(ILogger<CompanyService> logger, ICompanySerializer companySerializer) : ICompanyService {

    private readonly ILogger<CompanyService> _logger = logger;
    private readonly ICompanySerializer _companySerializer = companySerializer;

    private readonly Dictionary<Guid, ICompany> _companies = [];

    public Task<ICompany?> DownloadSyncedCompany(string companyId) {
        throw new NotImplementedException();
    }

    public IList<ICompany> GetCompanies(string alliance) => _companies.Values.Where(x => x.Faction.Alliance == alliance).ToList();

    public IList<ICompany> GetCompanies() => [.. _companies.Values];

    public ICompany? GetCompany(string companyId) => Guid.TryParse(companyId, out var guid) ? GetCompany(guid) : throw new ArgumentException("Expected UUID4 compliant id", nameof(companyId));

    public ICompany? GetCompany(Guid companyId) => _companies.TryGetValue(companyId, out var company) ? company : null;

    public async Task<ICompany?> LoadCompanyFromStream(Stream inputStream) {
        try {
            return await _companySerializer.DeserializeAsync(inputStream);
        } catch (Exception e) {
            _logger.LogError(e, "Failed to load company from stream");
            return null;
        } finally {
            await inputStream.DisposeAsync();
        }
    }

    public void RegisterCompany(ICompany company) {
        _companies[company.Id] = company;
    }

    public async Task<bool> SaveCompany(ICompany company, Stream outputStream) {
        try {
            return await _companySerializer.SerializeAsync(company, outputStream);
        } catch (Exception e) {
            _logger.LogError(e, "Failed saving company {id} - {company}", company.Id, company.Name);
            return false;
        } finally {
            await outputStream.DisposeAsync();
        }
    }

    public async Task<bool> SynchronizeCompany(ICompany company) {
        MemoryStream outputStream = new MemoryStream();
        if (!await SaveCompany(company, outputStream)) {
            _logger.LogWarning("Failed synchronozing company {id} with cloud", company.Id);
            return false;
        }
        // TODO: Upload to cloud
        throw new NotImplementedException();
    }

}
