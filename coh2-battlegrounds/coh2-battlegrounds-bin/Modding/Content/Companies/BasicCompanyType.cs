using System;
using System.Collections.Generic;

namespace Battlegrounds.Modding.Content.Companies;

public class BasicCompanyType : FactionCompanyType {

    public BasicCompanyType()
        : this(Array.Empty<TransportOption>(), new()) {
    }
    public BasicCompanyType(TransportOption[] DeployBlueprints, Dictionary<string, CommandLevel> Phases) 
        : base("base", new UI("", null, null), Array.Empty<CompanyAbility>(), 12, 12, 12, 4, 4, BattlegroundsDefine.COMPANY_DEFAULT_INITIAL, 
            Array.Empty<string>(), 
            new string[] { "DeployAndExit", "DeployAndStay" }, DeployBlueprints, Phases, new(), "", "", false) {
    }

}
