namespace Battlegrounds.Core.Companies.Serializing;

public interface ICompanySerializer {

    bool Serialize(ICompany company, Stream outputStream);

    Task<bool> SerializeAsync(ICompany company, Stream outputStream);

    ICompany? Deserialise(Stream inputStream);

    Task<ICompany?> DeserializeAsync(Stream inputStream);

}
