{
    display = {
        fe_name = "campaign_name",
        fe_desc = "campaign_desc",
        fe_location = "campaign_location",
        theatre = "east",
        modes = {
            "singleplayer",
            "cooperative",
            "competitive",
        },
        start = "german",
        max_players = 8,
        start_date = { year = 1942, month = 9, day = 13 },
        end_date = { year = 1943, month = 2, day = 2 },
        turn_time = { months = 0.25 },
    },
    armies = {
        ["soviet"] = {
            fe_army_name = "soviet_army",
            fe_army_desc = "",
            army_file = "soviets.lua",
            min_players = 1,
            max_players = 4,
        },
        ["german"] = {
            fe_army_name = "german_army",
            fe_army_desc = "",
            army_file = "germans.lua",
            min_players = 1,
            max_players = 4,
        }
    },
    resources = {
        {
            type = MAP,
            file = "map.png",
        },
        {
            type = LOCALE,
            lang = "ALL",
            file = "locale.loc",
        },
        {
            type = SCRIPT,
            file = "victory.lua",
        },
        {
            type = SCRIPT,
            file = "events.lua",
        },
        {
            type = GFX,
            file = "gfx.lua",
        },
    }
}
