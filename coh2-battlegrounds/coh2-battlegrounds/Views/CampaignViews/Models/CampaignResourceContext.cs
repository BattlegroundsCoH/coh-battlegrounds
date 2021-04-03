using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Battlegrounds.Campaigns.Controller;
using Battlegrounds.Functional;
using Battlegrounds.Gfx;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    /// <summary>
    /// 
    /// </summary>
    public class CampaignResourceContext {

        private Dictionary<string, ImageSource> m_graphics;

        /// <summary>
        /// 
        /// </summary>
        public double MapWidth { get; }

        /// <summary>
        /// 
        /// </summary>
        public double MapHeight { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICampaignController Controller { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapWidth"></param>
        /// <param name="mapHeight"></param>
        public CampaignResourceContext(double mapWidth, double mapHeight, ICampaignController controller) {
            this.MapWidth = mapWidth;
            this.MapHeight = mapHeight;
            this.Controller = controller;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public void InitializeGraphics(ICampaignController controller) {

            // Create graphics dictionary
            this.m_graphics = new Dictionary<string, ImageSource>();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public ImageSource GetResource(string resourceKey) {
            if (this.m_graphics.TryGetValue(resourceKey, out ImageSource src)) {
                return src;
            } else {
                return null;
            }
        }

    }

}
