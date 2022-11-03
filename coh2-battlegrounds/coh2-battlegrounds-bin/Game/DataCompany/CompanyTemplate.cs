using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Util;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Readonly class representing a template for generating a <see cref="Company"/> instance.
/// </summary>
public sealed class CompanyTemplate {

    private record CompanyUnit(ulong PBGID, ulong TPBGID, byte DMODE, int AMOUNT);

    private readonly CompanyUnit[][] m_units;
    private readonly string m_guid;
    private readonly string m_army;
    private readonly string m_type;

    /// <summary>
    /// Get the company army set by this template
    /// </summary>
    public string TemplateArmy => this.m_army;

    private CompanyTemplate(string guid, string army, string companytype)
        : this(guid, army, companytype, new CompanyUnit[4][]){
    }

    private CompanyTemplate(string guid, string army, string companytype, CompanyUnit[][] units) {
        this.m_army = army;
        this.m_type = companytype;
        this.m_guid = guid;
        this.m_units = units;
    }

    /// <summary>
    /// Convert the <see cref="CompanyTemplate"/> into a unique <see cref="string"/> representation that can be decoded back into a <see cref="CompanyTemplate"/> instance.
    /// </summary>
    /// <returns>A <see cref="string"/> representation of the <see cref="CompanyTemplate"/>.</returns>
    public override string ToString() {

        // Create string builder and encode basic information
        StringBuilder sb = new();
        sb.Append($"{this.m_guid}{EncodeArmy(this.m_army)}");

        // Get valid non-zero chunks
        StringBuilder subBuilder = new();
        subBuilder.Append(this.m_type);
        for (int i = 0; i < 4; i++) {
            subBuilder.Append('[');
            subBuilder.Append(string.Join('-', this.m_units[i].Map(UnitToString)));
            subBuilder.Append(']');
        }

        // Return in string
        return sb.Append(StringCompression.CompressString(subBuilder.ToString())).ToString();

    }

    private string UnitToString(CompanyUnit unit)
        => $"{unit.PBGID:X}?{unit.TPBGID:X}?{unit.DMODE}?{unit.AMOUNT:X}";

    /// <summary>
    /// Create a <see cref="CompanyTemplate"/> from a <see cref="Company"/> instance. Certain settings are not saved from the <see cref="Company"/>.
    /// </summary>
    /// <param name="company">The <see cref="Company"/> to create a template of.</param>
    /// <returns>A full <see cref="CompanyTemplate"/> containing basic <see cref="Company"/> data.</returns>
    public static CompanyTemplate FromCompany(Company company) {

        // Create template with base params
        CompanyTemplate template = new CompanyTemplate(company.TuningGUID, company.Army.Name, company.Type.Id);

        // Define container
        Dictionary<DeploymentPhase, Dictionary<CompanyUnit, int>> templateSetup = new() {
            [DeploymentPhase.PhaseInitial] = new(),
            [DeploymentPhase.PhaseStandard] = new(),
            [DeploymentPhase.PhaseStandard] = new(),
        };

        // Loop over units and put them into their respective phases
        for (int i = 0; i < company.Units.Length; i++) {

            // Create unit
            var unit = new CompanyUnit(
                company.Units[i].Blueprint.PBGID.UniqueIdentifier, 
                company.Units[i].SupportBlueprint?.PBGID.UniqueIdentifier ?? 0, 
                (byte)company.Units[i].DeploymentMethod, 0);
            
            // Increment or define count
            if (templateSetup[company.Units[i].DeploymentPhase].ContainsKey(unit)) {
                templateSetup[company.Units[i].DeploymentPhase][unit]++;
            } else {
                templateSetup[company.Units[i].DeploymentPhase][unit] = 1;
            }

        }

        // Insert
        for (byte i = 1; i <= 4; i++) {
            DeploymentPhase phase = (DeploymentPhase)i;
            template.m_units[i - 1] = templateSetup[phase].Map((k, v) => k with { AMOUNT = v });
        }

        // Return template
        return template;

    }

    /// <summary>
    /// Convert a <see cref="string"/> representation of a <see cref="CompanyTemplate"/> into its respective <see cref="CompanyTemplate"/> representation.
    /// </summary>
    /// <param name="tmpString">The <see cref="string"/> to convert into a <see cref="CompanyTemplate"/>.</param>
    /// <returns>The <see cref="CompanyTemplate"/> represented by the input <see cref="string"/>.</returns>
    public static CompanyTemplate? FromString(string tmpString) {

        try {

            // Get cut position and length for name
            string decompressed = StringCompression.DecompressString(tmpString[(ModGuid.FIXED_LENGTH + 1)..]);

            // Grab first section
            int a = decompressed.IndexOf('[');
            int b = decompressed.IndexOf(']');

            // Grab type
            string typ = decompressed[..a];

            // Loop over all phases
            CompanyUnit[][] units = new CompanyUnit[4][];
            for (int i = 0; i < 4; i++) {

                // Grab decompressed
                string sub = decompressed[(a + 1)..b];

                // Split individually
                string[] phaseUnitStrs = sub.Split('-', StringSplitOptions.RemoveEmptyEntries);
                units[i] = new CompanyUnit[phaseUnitStrs.Length];

                // Parse
                for (int j = 0; j < units[i].Length; j++) {

                    // Split into separate data
                    string[] data = phaseUnitStrs[j].Split('?');

                    // Set
                    units[i][j] = new(
                        ulong.Parse(data[0], NumberStyles.HexNumber),
                        ulong.Parse(data[1], NumberStyles.HexNumber),
                        byte.Parse(data[2]),
                        int.Parse(data[3], NumberStyles.HexNumber));

                }

                // Update decompress string
                a = decompressed.IndexOf('[', a + 1);
                b = decompressed.IndexOf(']', b + 1);

            }

            // Create template and return
            return new CompanyTemplate(
                tmpString[0..ModGuid.FIXED_LENGTH],
                DecodeArmy(tmpString[ModGuid.FIXED_LENGTH]),
                typ, units);

        } catch (Exception ex) {
            Trace.WriteLine(ex, nameof(CompanyTemplate));
            return null;
        }

    }

    /// <summary>
    /// Convert a <see cref="string"/> representation of a <see cref="CompanyTemplate"/> into a new <see cref="Company"/>.
    /// </summary>
    /// <param name="templateName">The name of the company to create from the template</param>
    /// <param name="template">The template <see cref="string"/> to generate <see cref="Company"/> instance from.</param>
    /// <returns>A <see cref="Company"/> instance based on the <see cref="CompanyTemplate"/>.</returns>
    public static bool FromTemplate(string templateName, string template, [NotNullWhen(true)] out Company? company) 
        => FromTemplate(templateName, FromString(template) ?? throw new Exception("Cannot parse template string"), out company);

    /// <summary>
    /// Convert a <see cref="CompanyTemplate"/> into a new <see cref="Company"/>.
    /// </summary>
    /// <param name="templateName">The name of the company to create from the template</param>
    /// <param name="template">The <see cref="CompanyTemplate"/> to generate new <see cref="Company"/> instance from.</param>
    /// <returns>A <see cref="Company"/> instance based on the <see cref="CompanyTemplate"/>.</returns>
    public static bool FromTemplate(string templateName, CompanyTemplate template, [NotNullWhen(true)] out Company? company) {

        // Get mod guid
        var guid = ModGuid.FromGuid(template.m_guid);

        // Get type
        var typ = ModManager.GetPackageFromGuid(guid)?.GetCompanyType(template.m_type);
        if (typ is null) {
            company = null;
            return false;
        }

        // Create Company Builder
        var builder = CompanyBuilder.NewCompany(templateName, typ, CompanyAvailabilityType.AnyMode, Faction.FromName(template.m_army), guid);

        // Create Units
        for (int i = 0; i < template.m_units.Length; i++) {
            for (int j = 0; j < template.m_units[i].Length; j++) {
                var sbp = BlueprintManager.FromPPbgid<SquadBlueprint>(new BlueprintUID(template.m_units[i][j].PBGID, guid));
                var tsbp = template.m_units[i][j].TPBGID is 0 ? null : BlueprintManager.FromPPbgid<SquadBlueprint>(new BlueprintUID(template.m_units[i][j].TPBGID, guid));
                for (int k = 0; k < template.m_units[i][j].AMOUNT; k++) {
                    
                    // Create unit with basics
                    var ub = UnitBuilder.NewUnit(sbp)
                        .SetDeploymentPhase((DeploymentPhase)(i + 1))
                        .SetDeploymentMethod((DeploymentMethod)template.m_units[i][j].DMODE);

                    // Add transport if possible
                    if (tsbp is not null)
                        ub.SetTransportBlueprint(tsbp);
                    
                    // Add unit
                    builder.AddUnit(ub);

                }
            }
        }

        // Commit changes and get result
        company = builder.Commit().Result;
        return true;

    }

    private static readonly (char, string)[] ArmyEncodingDic = new (char, string)[] {
        ('A', Faction.America.Name),
        ('U', Faction.British.Name),
        ('O', Faction.OberkommandoWest.Name),
        ('W', Faction.Wehrmacht.Name),
        ('S', Faction.Soviet.Name),
    };

    private static char EncodeArmy(string army) {
        int encodeID = Array.FindIndex(ArmyEncodingDic, x => x.Item2.CompareTo(army) == 0);
        if (encodeID > -1) {
            return ArmyEncodingDic[encodeID].Item1;
        } else {
            return 'E';
        }
    }

    private static string DecodeArmy(char character) {
        int encodeID = Array.FindIndex(ArmyEncodingDic, x => x.Item1 == character);
        if (encodeID > -1) {
            return ArmyEncodingDic[encodeID].Item2;
        } else {
            throw new ArgumentException("Invalid army character", nameof(character));
        }
    }

}
