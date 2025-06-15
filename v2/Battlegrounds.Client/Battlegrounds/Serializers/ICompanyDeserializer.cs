using System.IO;

using Battlegrounds.Models.Companies;

namespace Battlegrounds.Serializers;

public interface ICompanyDeserializer {

    Company DeserializeCompany(Stream source);

}
