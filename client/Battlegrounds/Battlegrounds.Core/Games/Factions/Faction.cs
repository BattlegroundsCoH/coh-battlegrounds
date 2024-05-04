using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Core.Games.Factions;

public static class Faction {

    // CoH1 Factions
    // TODO: Implement?

    // CoH2 Factions
    public static readonly CoH2Faction Soviets = new CoH2Faction(4, "Soviets", "allies");
    public static readonly CoH2Faction Ostheer = new CoH2Faction(5, "Ostheer", "axis");
    public static readonly CoH2Faction AEF = new CoH2Faction(6, "American Expeditionary Forces", "allies");
    public static readonly CoH2Faction OKW = new CoH2Faction(7, "Oberkommando West", "axis");
    public static readonly CoH2Faction UKF = new CoH2Faction(8, "United Kingdom Forces", "allies");

    // CoH3 Factions
    public static readonly CoH3Faction British = new CoH3Faction(9, "British", "allies");
    public static readonly CoH3Faction American = new CoH3Faction(10, "American", "allies");
    public static readonly CoH3Faction Wehrmacht = new CoH3Faction(11, "Wehrmacht", "axis");
    public static readonly CoH3Faction AfrikaKorps = new CoH3Faction(12, "Afrika Korps", "axis");

    public static IFaction FromIndex(byte index) => index switch {
        4 => Soviets,
        5 => Ostheer,
        6 => AEF,
        7 => OKW,
        8 => UKF,
        9 => British,
        10 => American,
        11 => Wehrmacht,
        12 => AfrikaKorps,
        _ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid faction index"),
    };

}
