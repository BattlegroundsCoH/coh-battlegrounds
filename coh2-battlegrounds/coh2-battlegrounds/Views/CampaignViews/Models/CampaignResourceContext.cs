using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Battlegrounds.Campaigns.Controller;
using Battlegrounds.Functional;
using Battlegrounds.Gfx;
using Battlegrounds.Locale;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    /// <summary>
    /// Context class for keeping track of resources.
    /// </summary>
    public class CampaignResourceContext {

        private Dictionary<string, ImageSource> m_graphics;

        /// <summary>
        /// Get the width of the campaign map.
        /// </summary>
        public double MapWidth { get; }

        /// <summary>
        /// Get the height of the campaign map.
        /// </summary>
        public double MapHeight { get; }

        /// <summary>
        /// Get the underlying campaign controller.
        /// </summary>
        public ICampaignController Controller { get; }

        public CampaignResourceContext(double mapWidth, double mapHeight, ICampaignController controller) {
            this.MapWidth = mapWidth;
            this.MapHeight = mapHeight;
            this.Controller = controller;
        }

        public void InitializeGraphics(ICampaignController controller) {

            // Create graphics dictionary
            this.m_graphics = new() {
                ["victory_points"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_vp.png")),
                ["attrition_value"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_attrition.png")),
                ["unit_infantry"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_unittype_infantry.png")),
                ["unit_support"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_unittype_support.png")),
                ["unit_vehicle"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_unittype_vehicle.png")),
                ["unit_tank"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_unittype_tank.png")),
                ["unit_air"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/icon_unittype_air.png")),
                ["objt_1"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/obj_main.png")),
                ["objt_2"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/obj_secondary.png")),
                ["objt_3"] = new BitmapImage(new Uri("pack://application:,,,/Resources/campaign/obj_star.png")),
            };

            // For each GFX map
            controller.GfxMaps.ForEach(x => {

                // Loop over all resources
                x.Resources.ForEach(id => {

                    // Get the resource
                    GfxResource resource = x.GetResource(id);
                    var stream = resource.Open();

                    // Decode and store the image
                    this.m_graphics[id] = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];

                });

            });

        }

        public string GetString(LocaleKey localeKey)
            => this.Controller.Locale.GetString(localeKey);

        /// <summary>
        /// Get resource identified by <paramref name="resourceKey"/>.
        /// </summary>
        /// <param name="resourceKey">The lookup string to find resource with.</param>
        /// <returns>If resource if found, a <see cref="ImageSource"/> instance; Otherwise <see langword="null"/>.</returns>
        public ImageSource GetResource(string resourceKey) {
            if (this.m_graphics.TryGetValue(resourceKey, out ImageSource src)) {
                return src;
            } else {
                return null;
            }
        }

    }

}
