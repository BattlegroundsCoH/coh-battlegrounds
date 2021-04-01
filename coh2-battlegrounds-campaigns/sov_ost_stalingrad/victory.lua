-- Stalingrad nodes
__stalingradNodes = {
    Map:FromName("landing_soviet"),
    Map:FromName("central_train_station"),
    Map:FromName("fallen_soldiers"),
    Map:FromName("southern_advances"),
    Map:FromName("southern_suburbs"),
    Map:FromName("central_approach"),
    Map:FromName("red_october_plant"),
    Map:FromName("tractor_plant"),
    Map:FromName("barricades_plant"),
    Map:FromName("pavlovs_house"),
    Map:FromName("timberyard"),
    Map:FromName("south_german_entrance"),
    Map:FromName("south_west_german_entrance"),
    Map:FromName("central_west_german_entrance"),
    Map:FromName("northern_german_entrance"),
    Map:FromName("workers_settlement"),
    Map:FromName("rail_exit")
};

-- Setup campaign
function CampaignSetup()

    -- Register Wincondition functions
    Battlegrounds_RegisterWincondition(TEAM_AXIS, HasAxisWon);
    Battlegrounds_RegisterWincondition(TEAM_ALLIES, HasAlliesWon);

end

-- Has axis won
function HasAxisWon()
    return g_axisOwnsCity and Team_GetPoints(TEAM_AXIS) >= 1 or Map:FromName("volga_crossing").owner == TEAM_AXIS;
end

-- Has allies won
function HasAlliesWon()
    return Date:IsEndDate() or (Team_GetReservesCount(TEAM_AXIS) == 0 and Team_GetFormationCount(TEAM_AXIS) == 0);
end

-- Check if unit can cross the volga
function CanCrossVolga(unit)
    if unit.Team == TEAM_AXIS then
        if DoesAxisOwnCity() then
            return FILTER_NEVER;
        else
            return FILTER_OK;
        end
    else
        return FILTER_OK;
    end
end

-- Returns true if axis team owns all city nodes
function DoesAxisOwnCity()
    return Team_OwnsAll(TEAM_AXIS, __stalingradNodes);
end
