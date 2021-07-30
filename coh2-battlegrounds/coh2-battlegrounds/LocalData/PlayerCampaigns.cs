using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.Campaigns;

namespace BattlegroundsApp.LocalData {

    public static class PlayerCampaigns {

        public static List<CampaignPackage> CampaignPackages { get; private set; }

        public static void GetInstalledCampaigns() {

            // Create list
            CampaignPackages = new List<CampaignPackage>();

            // Get the campaign folder
            string campaignFolder = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "campaigns");
            if (!Directory.Exists(campaignFolder)) {
                Trace.WriteLine("Failed to locate campaign directory - SKIPPING LOADING PLAYER CAMPAIGNS.", nameof(PlayerCampaigns));
                return;
            }

            // Get files
            string[] files = Directory.GetFiles(campaignFolder, "*.dat");

            // Load packages
            for (int i = 0; i < files.Length; i++) {
                CampaignPackage package = new CampaignPackage();
                if (!package.LoadFromBinary(files[i])) {
                    Trace.WriteLine($"Failed to load campaign file @ {files[i]}", nameof(PlayerCampaigns));
                } else {
                    CampaignPackages.Add(package);
                }
            }

        }

        public static void LoadActiveCampaigns() {

        }

    }

}
