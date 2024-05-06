using System.Text;

using Battlegrounds.Core.Companies.Builders;
using Battlegrounds.Core.Companies.Templates;
using Battlegrounds.Core.Games.Blueprints;
using Battlegrounds.Core.Games.Factions;
using Battlegrounds.Core.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Companies.Serializing;

public class BinaryCompanySerializer(ILogger<BinaryCompanySerializer> logger, ICompanyTemplateService companyTemplateService, IBlueprintService blueprintService) : ICompanySerializer {

    private static readonly Version current_version = Version.CoreVersion;
    //private static readonly Version basic_version = new Version(2, 0, 0);
    // Note: Please add more Version instances when major changes are made to company serialisation
    //       That way we can easily read outdated companies

    private readonly ILogger<BinaryCompanySerializer> _logger = logger;
    private readonly ICompanyTemplateService _companyTemplateService = companyTemplateService;
    private readonly IBlueprintService _blueprintService = blueprintService;

    public ICompany? Deserialise(Stream inputStream) => DeserializeAsync(inputStream).Result;

    public async Task<ICompany?> DeserializeAsync(Stream inputStream) => await Task.Run(() => {

        CompanyBuilder builder = new CompanyBuilder();

        using var binaryReader = new BinaryReader(inputStream);
        var major = binaryReader.ReadInt32();
        var minor = binaryReader.ReadInt32();
        var patch = binaryReader.ReadInt32();
        var version = new Version(major, minor, patch);

        if (current_version.IsGreaterThan(version)) {
            _logger.LogInformation("Loading outdated company {v1} (Current: {v2})", version, current_version);
        }

        var guidBytes = binaryReader.ReadBytes(16);
        builder.WithId(new Guid(guidBytes))
            .WithFaction(Faction.FromIndex(binaryReader.ReadByte()));

        byte[] templateId = binaryReader.ReadBytes(128);
        int cut = Array.FindIndex(templateId, b => b == 0);
        if (cut == -1) {
            throw new InvalidDataException("Cannot read company template name, since it exceeds 128 characters!");
        }
        string templateName = Encoding.ASCII.GetString(templateId[..cut]);
        if (_companyTemplateService.GetCompanyTemplate(templateName) is not ICompanyTemplate template) {
            _logger.LogError("Cannot load company with invalid company template id {id}", templateName);
            return null;
        }

        builder.WithTemplate(template);

        int nameLen = binaryReader.ReadInt32();
        builder.WithName(Encoding.UTF8.GetString(binaryReader.ReadBytes(nameLen)));

        int equipmentCount = binaryReader.ReadInt32();
        for (int i = 0; i < equipmentCount; i++) {
            byte ty = binaryReader.ReadByte();
            ushort equipmentIndex = binaryReader.ReadUInt16();
            ushort captureIndex = binaryReader.ReadUInt16();
            PropertyBagGroupId pbgId = new(binaryReader.ReadUInt64());
            switch (ty) {
                case 0:
                    if (_blueprintService.GetBlueprintById(builder.Faction.GameId, pbgId) is IBlueprint ibp) {
                        builder.WithEquipment(new WeaponEquipment(equipmentIndex, captureIndex, ibp));
                    }
                    break;
                case 1:
                    if (_blueprintService.GetBlueprintById<EntityBlueprint>(builder.Faction.GameId, pbgId) is EntityBlueprint ebp) {
                        builder.WithEquipment(new VehicleEquipment(equipmentIndex, captureIndex, ebp));
                    }
                    break;
                default:
                    _logger.LogError("Encountered invalid equipment type {type} while reading company {company}", ty, builder.Name);
                    return null;
            }
        }

        int squads = binaryReader.ReadInt32();
        for (int i = 0; i < squads; i++) {
            var sb = DeserializeSquad(binaryReader, builder);
            if (sb is not null) {
                builder.AddSquad(sb.Build());
            }
        }

        int phaseCount = binaryReader.ReadInt32();
        for (int i = 0; i < phaseCount; i++) {
            int priority = binaryReader.ReadInt32();
            int phaseUnitCount = binaryReader.ReadInt32();
            HashSet<ushort> phaseUnits = [];
            for (int j = 0; j < phaseUnitCount; j++) {
                phaseUnits.Add(binaryReader.ReadUInt16());
            }
            builder.WithPhase(new DeploymentPhase(priority, phaseUnits));
        }

        return builder.Build();

    });

    private SquadBuilder? DeserializeSquad(BinaryReader reader, CompanyBuilder companyBuilder) {

        ushort index = reader.ReadUInt16();
        SquadBuilder builder = new SquadBuilder(index, companyBuilder);

        ulong pbgid = reader.ReadUInt64();
        SquadBlueprint? sbp = _blueprintService.GetBlueprintById<SquadBlueprint>(companyBuilder.Faction.GameId, new PropertyBagGroupId(pbgid));
        if (sbp is not null) {
            builder.WithBlueprint(sbp);
        } else {
            _logger.LogWarning("Encountered invalid property bag group id {pbgid} while loading squad {index} for company '{company}'", pbgid, index, companyBuilder.Name);
        }

        builder.WithExperience(reader.ReadSingle());
        
        byte nameLen = reader.ReadByte();
        if (nameLen > 0) {
            builder.WithName(Encoding.UTF8.GetString(reader.ReadBytes(nameLen)));
        }

        byte itemCount = reader.ReadByte();
        for (int i = 0; i < itemCount; i++) {
            ulong itemPbgid = reader.ReadUInt64();
            if (_blueprintService.GetBlueprintById(companyBuilder.Faction.GameId, new PropertyBagGroupId(itemPbgid)) is IBlueprint bp) {
                builder.WithItem(bp);
            } else {
                _logger.LogWarning("Encountered invalid property bag group id {pbgid} while loading item/weapon for squad {index} in company '{company}'", itemPbgid, index, companyBuilder.Name);
            }
        }

        byte upgradeCount = reader.ReadByte();
        for (int i = 0; i < upgradeCount; i++) {
            ulong upgradePbgid = reader.ReadUInt64();
            if (_blueprintService.GetBlueprintById<UpgradeBlueprint>(companyBuilder.Faction.GameId, new PropertyBagGroupId(upgradePbgid)) is UpgradeBlueprint upg) {
                builder.WithUpgrade(upg);
            } else {
                _logger.LogWarning("Encountered invalid property bag group id {pbgid} while loading upgrade for squad {index} in company '{company}'", upgradePbgid, index, companyBuilder.Name);
            }
        }

        byte crewData = reader.ReadByte();
        if (crewData > 0) {
            throw new NotImplementedException();
        }

        byte transportData = reader.ReadByte();
        if (transportData > 0) {
            throw new NotImplementedException();
        }

        return builder.Blueprint is null ? null : builder;

    }

    public bool Serialize(ICompany company, Stream outputStream) => SerializeAsync(company, outputStream).Result;

    public async Task<bool> SerializeAsync(ICompany company, Stream outputStream) => await Task.Run(() => {

        using BinaryWriter binaryWriter = new BinaryWriter(outputStream, Encoding.UTF8, true);
        binaryWriter.Write(current_version.Major);
        binaryWriter.Write(current_version.Minor);
        binaryWriter.Write(current_version.Patch);
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
            switch (equipment) {
                case WeaponEquipment we:
                    binaryWriter.Write((byte)0);
                    binaryWriter.Write(we.ItemIndex);
                    binaryWriter.Write(we.Capturer);
                    binaryWriter.Write(we.EquipmentBlueprint.Pbgid.Ppbgid);
                    break;
                case VehicleEquipment ve:
                    binaryWriter.Write((byte)1);
                    binaryWriter.Write(ve.ItemIndex);
                    binaryWriter.Write(ve.Capturer);
                    binaryWriter.Write(ve.EquipmentBlueprint.Pbgid.Ppbgid);
                    break;
                default:
                    _logger.LogError("Cannot serialize company {company} with equipment type {type}", company.Name, equipment.GetType().Name);
                    return false;
            }
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
            if (squadNameEncoded.Length >= 256) {
                _logger.LogWarning("Truncating custom unit name to 255 characters {idx} '{name}'", squad.SquadId, squad.Name);
                squadNameEncoded = squadNameEncoded[.. byte.MaxValue];
            }
            binaryWriter.Write((byte)squadNameEncoded.Length); // So custom names may only be < 256 characters
            binaryWriter.Write(squadNameEncoded);
        }
        
        binaryWriter.Write((byte)squad.Items.Count);
        foreach (var item in squad.Items) {
            binaryWriter.Write(item.Pbgid.Ppbgid);
        }
        
        binaryWriter.Write((byte)squad.Upgrades.Count);
        foreach (var upgrade in squad.Upgrades) {
            binaryWriter.Write(upgrade.Pbgid.Ppbgid);
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
