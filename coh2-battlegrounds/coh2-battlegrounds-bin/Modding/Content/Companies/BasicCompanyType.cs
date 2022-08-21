using System;
using System.Collections.Generic;

using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Modding.Content.Companies;

public class BasicCompanyType : FactionCompanyType {

    public BasicCompanyType()
        : this(new(), new()) {
    }
    public BasicCompanyType(Dictionary<string, TransportOption> DeployBlueprints, Dictionary<string, Phase> Phases) 
        : base("base", "undefined", 12, 12, 12, 4, 4, Company.DEFAULT_INITIAL, Array.Empty<string>(), new string[] { "DeployAndExit", "DeployAndStay" }, DeployBlueprints, Phases) {
    }

}
