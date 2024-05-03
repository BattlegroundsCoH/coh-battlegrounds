using System.Text;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Companies.Serializing;

public class BinaryCompanySerializer(ILogger<BinaryCompanySerializer> logger) : ICompanySerializer {

    private readonly Version basic_version = new Version(2, 0, 0);
    // Note: Please add more Version instances when major changes are made to company serialisation
    //       That way we can easily read outdated companies

    private readonly ILogger<BinaryCompanySerializer> _logger = logger;

    public ICompany? Deserialise(Stream inputStream) => DeserializeAsync(inputStream).Result;

    public Task<ICompany?> DeserializeAsync(Stream inputStream) {
        throw new NotImplementedException();
    }

    public bool Serialize(ICompany company, Stream outputStream) => SerializeAsync(company, outputStream).Result;

    public async Task<bool> SerializeAsync(ICompany company, Stream outputStream) => await Task.Run(() => {

        var coreVersion = Version.CoreVersion; // Always use latest version when saving

        using BinaryWriter binaryWriter = new BinaryWriter(outputStream);
        binaryWriter.Write(coreVersion.Major);
        binaryWriter.Write(coreVersion.Minor);
        binaryWriter.Write(coreVersion.Build);
        binaryWriter.Write(company.Id.ToByteArray());

        binaryWriter.Write(company.Faction.FactionIndex); // This also carries information about what game the company is for

        byte[] template = new byte[128];
        Encoding.ASCII.GetBytes(company.Template.Id).CopyTo(template, 0);
        binaryWriter.Write(template);

        var nameEncoded = Encoding.UTF8.GetBytes(company.Name);
        binaryWriter.Write(nameEncoded.Length);
        binaryWriter.Write(nameEncoded);

        binaryWriter.Write(company.Equipment.Count);
        for (int i = 0; i < company.Equipment.Count; i++) {
            var equipment = company.Equipment[i];
        }

        binaryWriter.Write(company.Squads.Count);
        for (int i = 0; i < company.Squads.Count; i++) {
            SerializeSquad(company.Squads[i], binaryWriter);            
        }

        binaryWriter.Write(company.DeploymentPhases.Count);
        for (int i = 0;i < company.DeploymentPhases.Count; i++) {
            var phase = company.DeploymentPhases[i];
            binaryWriter.Write(phase.Priority);
            binaryWriter.Write(phase.Squads.Count);
            foreach (var squad in phase.Squads)
                binaryWriter.Write(squad);
        }

        return true;

    });

    private void SerializeSquad(ISquad squad, BinaryWriter binaryWriter) {
        binaryWriter.Write(squad.SquadId);
        binaryWriter.Write(squad.Blueprint.Pbgid.Ppbgid);
        binaryWriter.Write(squad.Experience);
        if (string.IsNullOrEmpty(squad.Name)) {
            binaryWriter.Write(false);
        } else {
            var squadNameEncoded = Encoding.UTF8.GetBytes(squad.Name);
            binaryWriter.Write((byte)squadNameEncoded.Length); // So custom names may only be < 256 characters
            binaryWriter.Write(squadNameEncoded);
        }
        binaryWriter.Write((byte)squad.Items.Count);
        foreach (var item in squad.Items) {
            throw new NotImplementedException();
        }
        binaryWriter.Write((byte)squad.Upgrades.Count);
        foreach (var upgrade in squad.Upgrades) {
            throw new NotImplementedException();
        }
        if (squad.Crew is null) {
            binaryWriter.Write(false);
        } else {
            throw new NotImplementedException();
        }
        if (squad.Transport is null) {
            binaryWriter.Write(false);
        } else {
            throw new NotImplementedException();
        }
    }

}
