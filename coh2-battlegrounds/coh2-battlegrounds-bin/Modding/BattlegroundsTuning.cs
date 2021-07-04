namespace Battlegrounds.Modding {

    /// <summary>
    /// The battlegrounds tuning mod. Implements <see cref="ITuningMod"/>. This class cannot be inherited.
    /// </summary>
    public sealed class BattlegroundsTuning : ITuningMod {

        public ModGuid Guid => ModGuid.FromGuid("142b113740474c82a60b0a428bd553d5");

        public string Name => "Battlegrounds";

        public string VerificationUpgrade => "bg_verify";

        public string TowUpgrade => "is_towed";

        public string TowingUpgrade => "is_towing";

    }

}
