using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Game {
    
    /// <summary>
    /// 
    /// </summary>
    public enum AIDifficulty {
    
        /// <summary>
        /// 
        /// </summary>
        Human,

        /// <summary>
        /// 
        /// </summary>
        AI_Easy,

        /// <summary>
        /// 
        /// </summary>
        AI_Standard,

        /// <summary>
        /// 
        /// </summary>
        AI_Hard,

        /// <summary>
        /// 
        /// </summary>
        AI_Expert,

    }

    /// <summary>
    /// Extension class to the <see cref="AIDifficulty"/> enum.
    /// </summary>
    public static class AIExtensions {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static bool AllowsPersistency(this AIDifficulty difficulty)
            => difficulty >= AIDifficulty.AI_Hard || difficulty == AIDifficulty.Human;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static string GetIngameDisplayName(this AIDifficulty difficulty)
            => difficulty switch
            {
                AIDifficulty.AI_Easy => "AI - Easy",
                AIDifficulty.AI_Expert => "AI - Expert",
                AIDifficulty.AI_Hard => "AI - Hard",
                AIDifficulty.AI_Standard => "AI - Standard",
                _ => string.Empty
            };

    }

}
