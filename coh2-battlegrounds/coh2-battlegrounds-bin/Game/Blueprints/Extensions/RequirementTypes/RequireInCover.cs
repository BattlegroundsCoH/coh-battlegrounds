﻿using System;
using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints.Extensions.RequirementTypes;

public class RequireInCover : RequirementExtension
{

    public string[] CoverTypes { get; }

    public RequireInCover(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason)
    {
        if (properties.TryGetValue("cover_type_table", out object? cover) && cover is Dictionary<string, object> table)
        {
            List<string> entries = new();
            foreach (KeyValuePair<string, object> pair in table)
            {
                if (pair.Value is string s && !string.IsNullOrEmpty(s))
                {
                    entries.Add(s);
                }
            }
            CoverTypes = entries.ToArray();
        }
        else
            CoverTypes = Array.Empty<string>();
    }

    public override bool IsTrue(Squad squad) => false;

}

