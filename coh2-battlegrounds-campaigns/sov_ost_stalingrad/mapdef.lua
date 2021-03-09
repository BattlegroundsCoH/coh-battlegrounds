{
    nodes = {
        ["landing_soviet"] = {
            u = 428.0 / 1110.0,
            v = 290.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["central_train_station"] = {
            u = 600.0 / 1110.0,
            v = 450.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["fallen_soldiers"] = {
            u = 640.0 / 1110.0,
            v = 450.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["southern_advances"] = {
            u = 700.0 / 1110.0,
            v = 733.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 2, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "4p_rails_and_metal" -- The map to play when fighting a battle
        },
        ["southern_suburbs"] = {
            u = 694.0 / 1110.0,
            v = 610.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "2p_coh2_resistance" -- The map to play when fighting a battle
        },
        ["central_approach"] = {
            u = 520.0 / 1110.0,
            v = 520.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 4, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = {
                { scenario = "8p_coh2_city_17_spring_frontline", winter = false },
                { scenario = "8p_coh2_city_17_winter_battlefield", winter = true },
            }
        },
        ["red_october_plant"] = {
            u = 374.0 / 1110.0,
            v = 292.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["tractor_plant"] = {
            u = 150.0 / 1110.0,
            v = 95.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "8p_tank_factory" -- The map to play when fighting a battle
        },
        ["barricades_plant"] = {
            u = 300.0 / 1110.0,
            v = 210.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["pavlovs_house"] = {
            u = 594.0 / 1110.0,
            v = 400.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "6p_coh2_pavlov_s_house" -- The map to play when fighting a battle
        },
        ["volga_crossing"] = {
            u = 485.0 / 1110.0,
            v = 210.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["soviet_entrance"] = {
            u = 510.0 / 1110.0,
            v = 0.0,
            leaf = true, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 8, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "" -- The map to play when fighting a battle
        },
        ["timberyard"] = {
            u = 1004.0 / 1110.0,
            v = 724.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 2, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "timberyard" -- The map to play when fighting a battle
        },
        ["south_german_entrance"] = {
            u = 1.0,
            v = 750.0 / 792.0,
            leaf = true, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 8, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "divide map" -- The map to play when fighting a battle
        },
        ["south_west_german_entrance"] = {
            u = 688.0 / 1110.0,
            v = 1.0,
            leaf = true, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 8, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "divide map" -- The map to play when fighting a battle
        },
        ["central_west_german_entrance"] = {
            u = 410.0 / 1110.0,
            v = 1.0,
            leaf = true, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 8, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "divide map" -- The map to play when fighting a battle
        },
        ["northern_german_entrance"] = {
            u = 0.0,
            v = 433.0 / 792.0,
            leaf = true, -- Is endpoint
            owner = TEAM_AXIS, -- Node owner
            value = 1, -- Victory Point value
            capacity = 8, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "divide map" -- The map to play when fighting a battle
        },
        ["workers_settlement"] = {
            u = 121.0 / 1110.0,
            v = 172.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 1, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "anotherstalingrad" -- The map to play when fighting a battle
        },
        ["rail_exit"] = {
            u = 215.0 / 1110.0,
            v = 524.0 / 792.0,
            leaf = false, -- Is endpoint
            owner = TEAM_ALLIES, -- Node owner
            value = 1, -- Victory Point value
            capacity = 4, -- Max companies per side
            attrition = 0.1, -- The amount of attrition a company suffers
            map = "6_8_brody" -- The map to play when fighting a battle
        }
        -- TODO: Mamaev Kurgan (The mountain in the middle - sorta splitting the city in two)
    },
    transitions = {
        { "central_train_station", "fallen_soldiers", "binary" }, -- Binary = a <-> b | Unary = a -> b
        { "landing_soviet", "central_train_station", "binary" },
        { "landing_soviet", "pavlovs_house", "binary" },
        { "pavlovs_house", "fallen_soldiers", "binary" },
        { "southern_advances", "southern_suburbs", "binary" },
        { "southern_suburbs", "central_train_station", "binary" },
        { "soviet_entrance", "volga_crossing", "unary" }, -- soviet_entrance has no map - soviets will always be able to move in from this node
        { "volga_crossing", "landing_soviet", "binary" },
        { "central_approach", "central_train_station", "binary" },
        { "landing_soviet", "red_october_plant", "binary" },
        { "red_october_plant", "barricades_plant", "binary" },
        { "barricades_plant", "tractor_plant", "binary" },
        { "south_german_entrance", "timberyard", "binary" },
        { "timberyard", "southern_suburbs", "binary" },
        { "timberyard", "fallen_soldiers", "binary" },
        { "timberyard", "central_train_station", "binary" },
        { "south_west_german_entrance", "southern_advances", "unary" },
        { "central_west_german_entrance", "central_approach", "unary" },
        { "central_west_german_entrance", "rail_exit", "unary" },
        { "northern_german_entrance", "workers_settlement", "unary" },
        { "northern_german_entrance", "rail_exit", "unary" },
        { "workers_settlement", "tractor_plant", "binary" },
        { "rail_exit", "red_october_plant", "binary" },
        { "rail_exit", "central_approach", "binary" },
    }
}
