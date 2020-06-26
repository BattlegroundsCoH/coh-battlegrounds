namespace Battlegrounds.Game {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Represents a game event
    /// </summary>
    public enum GameEventType : byte {

        /**
         * Entity/Generic Commands 
         */

        CMD_DefaultAction,
        CMD_Stop,
        CMD_Destroy,
        CMD_BuildSquad,
        CMD_InstantBuildSquad,
        CMD_CancelProduction,
        CMD_BuildStructure,
        CMD_Move,
        CMD_Face,
        CMD_Attack,
        CMD_AttackMove,
        CMD_RallyPoint,
        CMD_Capture,
        CMD_Ability,
        CMD_Evacuate,
        CMD_Upgrade,
        CMD_InstantUpgrade,
        CMD_ChooseResource,
        CMD_Load,
        CMD_Unload,
        CMD_UnloadSquads,
        CMD_AttackStop,
        CMD_AttackForced,
        CMD_SetHoldHeading,
        CMD_Halt,
        CMD_Fidget,
        CMD_Paradrop,
        CMD_DefuseMine,
        CMD_Casualty,
        CMD_Death,
        CMD_InstantDeath,
        CMD_Projectile,
        CMD_PlaceCharge,
        CMD_BuildEntity,
        CMD_RescueCasualty,
        CMD_AttackFromHold,
        CMD_Vault,
        CMD_COUNT,

        /**
         * Squad Commands 
         */

        SCMD_Move,
        SCMD_Stop,
        SCMD_Destroy,
        SCMD_BuildStructure,
        SCMD_Capture,
        SCMD_Attack,
        SCMD_ReinforceUnit,
        SCMD_Upgrade,
        SCMD_CancelProduction,
        SCMD_AttackMove,
        SCMD_Ability,
        SCMD_Load,
        SCMD_InstantLoad,
        SCMD_UnloadSquads,
        SCMD_Unload,
        SCMD_SlotItemRemove,
        SCMD_Retreat,
        SCMD_CaptureTeamWeapon,
        SCMD_SetMoveType,
        SCMD_InstantReinforceUnit,
        SCMD_InstantUpgrade,
        SCMD_SetCamouflageStance,
        SCMD_PlaceCharge,
        SCMD_DefuseCharge,
        SCMD_PickUpSlotItem,
        SCMD_DefuseMine,
        SCMD_DoPlan,
        SCMD_Patrol,
        SCMD_Surprise,
        SCMD_InstantSetupTeamWeapon,
        SCMD_AbandonTeamWeapon,
        SCMD_StationaryAttack,
        SCMD_RevertFieldSupport,
        SCMD_Face,
        SCMD_BuildSquad,
        SCMD_RallyPoint,
        SCMD_RescueCasualty,
        SCMD_Recrew,
        SCMD_Merge,
        SCMD_Pilfer,
        SCMD_COUNT,

        /**
         * Player Commands 
         */

        PCMD_ConstructStructure,
        PCMD_ManpowerDonation,
        PCMD_FuelDonation,
        PCMD_MunitionDonation,
        PCMD_CheatResources,
        PCMD_CheatRevealAll,
        PCMD_CheatKillSelf,
        PCMD_Ability,
        PCMD_CheatBuildTime,
        PCMD_CriticalHit,
        PCMD_Upgrade,
        PCMD_InstantUpgrade,
        PCMD_ConstructFence,
        PCMD_ConstructField,
        PCMD_UpgradeRemove,
        PCMD_SlotItemRemove,
        PCMD_CancelProduction,
        PCMD_DetonateCharges,
        PCMD_AIPlayer,
        PCMD_AIPlayer_ObjectiveNotification,
        PCMD_SetCommander,
        PCMD_COUNT,
        
        _UNKNOWN1,

        PCMD_BroadcastMessage,



        EVENT_MAX

    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
