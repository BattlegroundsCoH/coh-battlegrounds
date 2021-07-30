using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Battlegrounds.Functional;

namespace Battlegrounds.Game.Database.Extensions {

    public class VeterancyExtension {

        public readonly struct Rank {
            public readonly string LocaleStr;
            public readonly float ExperienceRequired;
            public Rank(string locStr, float exp) {
                this.LocaleStr = locStr;
                this.ExperienceRequired = exp;
            }
            public override string ToString() => $"{this.ExperienceRequired} : {this.LocaleStr}";
        }

        private Rank[] m_ranks;

        public float MaxExperience => this.m_ranks.Max(x => x.ExperienceRequired);

        public int MaxRank => this.m_ranks.Length;

        public VeterancyExtension(Rank[] entries) {
            this.m_ranks = entries;
        }

        public static VeterancyExtension FromJson(ref Utf8JsonReader reader) {
            List<Rank> ranks = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
                string locstr = string.Empty;
                float exp = 0;
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                    if (reader.ReadProperty() is "ScreenName") {
                        locstr = reader.GetString();
                    } else {
                        exp = reader.GetInt32();
                    }
                }
                ranks.Add(new(locstr, exp));
            }
            return new(ranks.ToArray());
        }

        public override string ToString() => $"0 < {string.Join(" < ", this.m_ranks.Select(x => x.ExperienceRequired))}";

    }

}
