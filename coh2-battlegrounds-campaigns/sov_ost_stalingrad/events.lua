-- Handler for node ownership change events
function CampaignEvent_NodeOwnershipChanged(node)

    -- Check who owns the whole city
    local ownsCity = DoesAxisOwnCity();

    -- Check if ownership has changed
    if g_axisOwnsCity ~= ownsCity and g_axisOwnsCity ~= nil then
        local announcement = {};
        if g_axisOwnsCity then
            announcement.title = "";
            announcement.desc = "";
            announcement.icon = "";
        else
            announcement.title = "";
            announcement.desc = "";
            announcement.icon = "";
        end
        Battlegrounds_Announce(announcement);
    end

    -- Update global variable
    g_axisOwnsCity = ownsCity;

end

-- Register campaign handler
Battlegrounds_RegisterCampaignEventHandler(CampaignEvent_NodeOwnershipChanged, ET_Ownership);
