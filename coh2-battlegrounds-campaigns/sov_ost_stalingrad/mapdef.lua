{
    nodes = {
        ["landing_soviet"] = {
            u = 428.0 / 1110.0,
            v = 290.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["central_train_station"] = {
            u = 600.0 / 1110.0,
            v = 450.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["fallen_soldiers"] = {
            u = 640.0 / 1110.0,
            v = 450.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["southern_advances"] = {
            u = 700.0 / 1110.0,
            v = 733.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "4p_rails_and_metal" -- The map to play when fighting a battle
        },
        ["southern_suburbs"] = {
            u = 694.0 / 1110.0,
            v = 610.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "2p_stalingrad" -- The map to play when fighting a battle
        },
        ["central_approach"] = {
            u = 520.0 / 1110.0,
            v = 520.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "8p_city17" -- The map to play when fighting a battle
        },
        ["red_october_plant"] = {
            u = 374.0 / 1110.0,
            v = 292.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["tractor_plant"] = {
            u = 150.0 / 1110.0,
            v = 95.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["barricades_plant"] = {
            u = 300.0 / 1110.0,
            v = 210.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["pavlovs_house"] = {
            u = 594.0 / 1110.0,
            v = 400.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["volga_crossing"] = {
            u = 485.0 / 1110.0,
            v = 210.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["soviet_entrance"] = {
            u = 510.0 / 1110.0,
            v = 0.0 / 792.0,
            leaf = true, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        }
    },
    transitions = {
        { "central_train_station", "fallen_soldiers", "binary" }, -- Binary = a <-> b | Unary = a -> b
        { "landing_soviet", "central_train_station", "binary" },
        { "landing_soviet", "pavlovs_house", "binary" },
        { "pavlovs_house", "fallen_soldiers", "binary" },
        { "southern_advances", "southern_suburbs", "binary" },
        { "southern_suburbs", "central_train_station", "binary" },
        { "soviet_entrance", "volga_crossing", "unary" },
        { "volga_crossing", "landing_soviet", "binary" },
        { "central_approach", "central_train_station", "binary" },
        { "landing_soviet", "red_october_plant", "binary" },
        { "red_october_plant", "barricades_plant", "binary" },
        { "barricades_plant", "tractor_plant", "binary" },
    }
}