using System;
using System.Collections.Generic;

using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Modding.Content.Companies;

public class BasicCompanyType : FactionCompanyType {

    public BasicCompanyType()
        : this(Array.Empty<TransportOption>(), new()) {
    }
    public BasicCompanyType(TransportOption[] DeployBlueprints, Dictionary<string, Phase> Phases) 
        : base("base", new UI("", null, null), 12, 12, 12, 4, 4, Company.DEFAULT_INITIAL, 
            Array.Empty<string>(), 
            new string[] { "DeployAndExit", "DeployAndStay" }, DeployBlueprints, Phases, "", "", false) {
    }

}
