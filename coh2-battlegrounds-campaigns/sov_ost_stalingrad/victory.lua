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
    return false;
end

-- Has allies won
function HasAlliesWon()
    return false;
end

-- Check if unit can cross the volga
function CanCrossVolga(unit)
    if unit.Team == TEAM_AXIS then
        if Team_OwnsAll(unit.Team, __stalingradNodes) then
            return FILTER_NEVER;
        else
            return FILTER_OK;
        end
    else
        return FILTER_OK;
    end
end
