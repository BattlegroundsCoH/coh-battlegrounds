using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Companies.Serializing;

public class BinaryCompanySerializer(ILogger<BinaryCompanySerializer> logger) : ICompanySerializer {
    
    private readonly ILogger<BinaryCompanySerializer> _logger = logger;

    public ICompany? Deserialise(Stream inputStream) => DeserializeAsync(inputStream).Result;

    public Task<ICompany?> DeserializeAsync(Stream inputStream) {
        throw new NotImplementedException();
    }

    public bool Serialize(ICompany company, Stream outputStream) => SerializeAsync(company, outputStream).Result;

    public Task<bool> SerializeAsync(ICompany company, Stream outputStream) {
        throw new NotImplementedException();
    }

}
