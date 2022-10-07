using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Extensions.RequirementTypes;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;

namespace BattlegroundsApp.Utilities;

/// <summary>
/// 
/// </summary>
public static class RequirementVisualiser {

    private static readonly ImageSource ImgFullfilled
        = new BitmapImage(new Uri($"pack://application:,,,/Resources/app/checkmark.png"));

    private static readonly ImageSource ImgNotFullfilled
        = new BitmapImage(new Uri($"pack://application:,,,/Resources/app/checkmark_not.png"));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="requirements"></param>
    public static void Visualize(StackPanel panel, RequirementExtension[] requirements, Squad squad) {

        // Get all where requirements can be verified
        requirements = requirements.Where(x => IsRequirementVerifiable(x, squad)).ToArray();

        // Bail if no requirements
        if (requirements.Length is 0) {
            return;
        }

        // Add "Requirements" text
        _ = panel.Children.Add(new TextBlock() { Text = "Requirements:" });

        // Loop over requirements
        foreach (RequirementExtension requirement in requirements) {
            if (AddRequirement(requirement, squad) is StackPanel container) {
                _ = panel.Children.Add(container);
            }
        }

    }

    public static bool IsRequirementVerifiable(RequirementExtension requirement, Squad squad) => requirement switch {
        IRequirementList ls => ls.Requirements.Length is not 0 && ls.Requirements.All(x => IsRequirementVerifiable(x, squad)),
        RequireSquadUpgrade su => squad.SBP.Upgrades.Contains(su.Upgrade),
        RequireSlotItem => true,
        _ => false
    };

    public static StackPanel AddRequirement(RequirementExtension requirement, Squad squad) {

        // Create panel
        StackPanel workPanel = new() {
            Orientation = Orientation.Horizontal
        };

        // Get if requirement should be tested
        bool isVerifiable = IsRequirementVerifiable(requirement, squad);
        bool isFullfilled = isVerifiable && requirement.IsTrue(squad);

        // Check if fullfilled or verifiable
        if (isVerifiable) {
            Image state = new() {
                Width = 24,
                Height = 24,
                Source = isFullfilled ? ImgFullfilled : ImgNotFullfilled
            };
            _ = workPanel.Children.Add(state);
        }

        // If there's a reason string
        if (!UcsString.IsNullOrEmpty(requirement.UIName)) {
            string ucsStr = GameLocale.GetString(requirement.UIName);
            _ = workPanel.Children.Add(new TextBlock() { Text = ucsStr });
        }

        // Is list of requirements?
        if (requirement is IRequirementList list) {

            // Verify there's content
            if (list.Requirements.Length is 0) {
                return new();
            }

            // Create container and add current workpanl
            StackPanel container = new();
            _ = container.Children.Add(workPanel);

            // Add child requirements
            foreach (RequirementExtension r in list.Requirements) {
                if (AddRequirement(r, squad) is StackPanel sub) {
                    _ = container.Children.Add(sub);
                }
            }

            // Update return value
            workPanel = container;

        }

        // Return panel
        return workPanel;

    }

}

