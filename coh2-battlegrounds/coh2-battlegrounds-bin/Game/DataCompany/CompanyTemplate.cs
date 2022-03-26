using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Util;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany {

    /// <summary>
    /// Readonly class representing a template for generating a <see cref="Company"/> instance.
    /// </summary>
    public sealed class CompanyTemplate {

        private struct CompanyUnit {
            public ushort PBGID { get; init; }
            public ushort TPBGID { get; init; }
            public byte DMODE { get; init; }
            public byte PHASE { get; init; }
            public bool FILLED { get; init; } // If there's any relevant data
            public override string ToString() => FILLED ? $"[{this.PBGID}, {this.TPBGID} | {this.DMODE}, {this.PHASE}]" : "NONE";
            public string Collapse() => $"{this.PBGID}?{this.TPBGID}?{this.DMODE}?{this.PHASE}";
        }

        private CompanyUnit[] m_units;
        private string m_guid;
        private string m_name;
        private string m_army;
        private string m_checksum;

        /// <summary>
        /// Get the company name set by this template.
        /// </summary>
        public string TemplateName => this.m_name;

        /// <summary>
        /// Get the company army set by this template
        /// </summary>
        public string TemplateArmy => this.m_army;

        /// <summary>
        /// Get the amount of actual units in this template.
        /// </summary>
        public int TemplateUnitCount => this.m_units.Count(x => x.FILLED);

        private CompanyTemplate() {
            this.m_units = new CompanyUnit[Company.MAX_SIZE];
            this.m_checksum = string.Empty;
        }

        public string GetChecksum() => this.ToString().Aggregate((uint)0, (a, b) => a + b + (b % (a + 1))).ToString("X8");

        public bool VerifyChecksum() {

            // Backup and reset checksum
            string checksum = this.m_checksum;
            this.m_checksum = string.Empty;

            // Calculate checksum
            string newChecksum = this.GetChecksum();
            bool result = newChecksum.CompareTo(checksum) == 0;

            // Restore checksum
            this.m_checksum = checksum;

            // Return result
            return result;

        }

        /// <summary>
        /// Convert the <see cref="CompanyTemplate"/> into a unique <see cref="string"/> representation that can be decoded back into a <see cref="CompanyTemplate"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="CompanyTemplate"/>.</returns>
        public override string ToString() {

            // Create string builder and encode basic information
            StringBuilder sb = new StringBuilder();
            sb.Append($"{this.m_guid}{EncodeArmy(this.m_army)}{this.m_name.Length:X}-{this.m_name.Replace(' ', '@')}-");

            // Get valid non-zero chunks
            sb.Append(StringCompression.CompressString($"{string.Join('-', this.m_units.Where(x => x.FILLED).Select(x => x.Collapse()))}<{this.m_checksum}>"));

            // Return in string
            return sb.ToString();

        }

        public string ToTemplateString() {

            // Get the checksum
            this.m_checksum = string.Empty;
            this.m_checksum = GetChecksum();

            // Return template with checksum
            return this.ToString();

        }

        /// <summary>
        /// Create a <see cref="CompanyTemplate"/> from a <see cref="Company"/> instance. Certain settings are not saved from the <see cref="Company"/>.
        /// </summary>
        /// <param name="company">The <see cref="Company"/> to create a template of.</param>
        /// <returns>A full <see cref="CompanyTemplate"/> containing basic <see cref="Company"/> data.</returns>
        public static CompanyTemplate FromCompany(Company company) {

            // Create template with base params
            CompanyTemplate template = new CompanyTemplate {
                m_army = company.Army.Name,
                m_guid = company.TuningGUID,
                m_name = company.Name,
            };

            // Get unit enumerator and add all units (in a basic format).
            var unitEnumerator = company.Units.GetEnumerator() as IEnumerator<Squad>;
            for (int i = 0; i < Company.MAX_SIZE; i++) {
                if (unitEnumerator.MoveNext()) {
                    template.m_units[i] = new CompanyUnit() {
                        FILLED = true,
                        PBGID = unitEnumerator.Current.Blueprint.ModPBGID,
                        TPBGID = (unitEnumerator.Current.SupportBlueprint is not null) ? unitEnumerator.Current.SupportBlueprint.ModPBGID : BlueprintManager.InvalidLocalBlueprint,
                        DMODE = (byte)unitEnumerator.Current.DeploymentMethod,
                        PHASE = (byte)unitEnumerator.Current.DeploymentPhase,
                    };
                } else {
                    template.m_units[i] = new CompanyUnit() { FILLED = false };
                }
            }

            // Return template
            return template;

        }

        /// <summary>
        /// Convert a <see cref="string"/> representation of a <see cref="CompanyTemplate"/> into its respective <see cref="CompanyTemplate"/> representation.
        /// </summary>
        /// <param name="tmpString">The <see cref="string"/> to convert into a <see cref="CompanyTemplate"/>.</param>
        /// <returns>The <see cref="CompanyTemplate"/> represented by the input <see cref="string"/>.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="OverflowException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static CompanyTemplate FromString(string tmpString) {

            try {

                // Create template from first parameters of tmpString
                CompanyTemplate template = new CompanyTemplate {
                    m_guid = tmpString[0..ModGuid.FIXED_LENGTH],
                    m_army = DecodeArmy(tmpString[ModGuid.FIXED_LENGTH]),
                };

                // Get cut position and length for name
                int cut = tmpString.IndexOf('-');
                int nameLength = int.Parse(tmpString[(ModGuid.FIXED_LENGTH + 1)..cut], System.Globalization.NumberStyles.HexNumber);

                // Get company name
                int nameEnd = cut + 1 + nameLength;
                template.m_name = tmpString[(cut + 1)..nameEnd].Replace('@', ' ');

                // Decompress string
                tmpString = tmpString[(nameEnd + 1)..];
                tmpString = StringCompression.DecompressString(tmpString);

                // Get checksum
                int checksumBegin = tmpString.LastIndexOf('<') + 1;
                int checksumEnd = tmpString.LastIndexOf('>');
                template.m_checksum = tmpString[checksumBegin..checksumEnd];
                tmpString = tmpString[0..(checksumBegin - 1)];

                // Get each individual unit
                string[] units = tmpString.Split('-');
                for (int i = 0; i < Company.MAX_SIZE; i++) {
                    if (i < units.Length) {
                        string[] components = units[i].Split('?');
                        template.m_units[i] = new CompanyUnit() {
                            FILLED = true,
                            PBGID = ushort.Parse(components[0]),
                            TPBGID = ushort.Parse(components[1]),
                            DMODE = byte.Parse(components[2]),
                            PHASE = byte.Parse(components[3]),
                        };
                    } else {
                        template.m_units[i] = new CompanyUnit() { FILLED = false };
                    }
                }

                //// Verify checksum
                //if (!template.VerifyChecksum()) {
                //    throw new ChecksumViolationException(template.m_checksum, "0");
                //}

                // Return template
                return template;

            } catch (Exception e) {
                throw new FormatException("Template string was of incorrect format", e);
            }

        }

        /// <summary>
        /// Convert a <see cref="string"/> representation of a <see cref="CompanyTemplate"/> into a new <see cref="Company"/>.
        /// </summary>
        /// <param name="template">The template <see cref="string"/> to generate <see cref="Company"/> instance from.</param>
        /// <returns>A <see cref="Company"/> instance based on the <see cref="CompanyTemplate"/>.</returns>
        public static Company FromTemplate(string template) => FromTemplate(FromString(template));

        /// <summary>
        /// Convert a <see cref="CompanyTemplate"/> into a new <see cref="Company"/>.
        /// </summary>
        /// <param name="template">The <see cref="CompanyTemplate"/> to generate new <see cref="Company"/> instance from.</param>
        /// <returns>A <see cref="Company"/> instance based on the <see cref="CompanyTemplate"/>.</returns>
        public static Company FromTemplate(CompanyTemplate template) {

            // Create Company Builder
            CompanyBuilder builder = new CompanyBuilder()
                .NewCompany(Faction.FromName(template.m_army))
                .ChangeTuningMod(template.m_guid)
                .ChangeName(template.m_name);

            // Create Units
            for (int i = 0; i < template.m_units.Length; i++) {
                /*if (template.m_units[i].FILLED) {
                    UnitBuilder unit = new UnitBuilder()
                        .SetModGUID(template.m_guid)
                        .SetBlueprint(template.m_units[i].PBGID)
                        .SetDeploymentPhase((DeploymentPhase)template.m_units[i].PHASE)
                        .IfTrue(x => template.m_units[i].TPBGID != BlueprintManager.InvalidLocalBlueprint).Then(x => x.SetTransportBlueprint(template.m_units[i].TPBGID))
                        .SetDeploymentMethod((DeploymentMethod)template.m_units[i].DMODE);
                    builder.AddAndCommitUnit(unit);
                }*/
            }

            // Commit changes and get result
            return builder.Commit().Result;

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

        public bool VerifyChecksum(string checksum) => throw new NotImplementedException();
        public void CalculateChecksum() => throw new NotImplementedException();
    }

}
