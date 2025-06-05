using System.IO;

using Battlegrounds.Models.Companies;

namespace Battlegrounds.Serializers;

public interface ICompanySerializer {

    void SerializeCompany(Stream destination, Company company);

}
