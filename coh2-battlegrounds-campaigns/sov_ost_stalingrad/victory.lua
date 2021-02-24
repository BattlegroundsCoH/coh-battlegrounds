function CampaignSetup()

    -- Register Wincondition functions
    Battlegrounds_RegisterWincondition("axis", HasAxisWon);
    Battlegrounds_RegisterWincondition("allies", HasAlliesWon);

end

function HasAxisWon()
    return false;
end

function HasAlliesWon()
    return false;
end

g_stalingradNodes = {
    "",
    "",
    ""
};

function CanCrossVolga(regiment)
    if regiment.owner == "axis" then
        return Team_OwnsAll(regiment.owner, g_stalingradNodes);
    else
        return true;
    end
end
