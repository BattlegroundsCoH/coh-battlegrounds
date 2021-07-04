using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.DataCompany {
    
    /// <summary>
    /// Builder class for building a <see cref="Squad"/> instance with serial-style methods. Can be cleared for re-use.
    /// </summary>
    public class UnitBuilder {

        ushort m_overrideIndex = ushort.MaxValue;
        bool m_hasOverrideIndex = false;

        byte m_vetrank;
        float m_vetexperience;
        string m_modGuid;
        SquadBlueprint m_blueprint;
        SquadBlueprint m_transportBlueprint;
        DeploymentMethod m_deploymentMethod;
        DeploymentPhase m_deploymentPhase;
        HashSet<UpgradeBlueprint> m_upgrades;
        HashSet<SlotItemBlueprint> m_slotitems;
        HashSet<Modifier> m_modifiers;
        UnitBuilder m_crewBuilder;
        CompanyBuilder m_builder;

        /// <summary>
        /// Get the current blueprint of the unit.
        /// </summary>
        public SquadBlueprint Blueprint => this.m_blueprint;

        /// <summary>
        /// New basic <see cref="UnitBuilder"/> instance of for building a <see cref="Squad"/>.
        /// </summary>
        public UnitBuilder() {
            this.m_modGuid = string.Empty;
            this.m_blueprint = null;
            this.m_transportBlueprint = null;
            this.m_crewBuilder = null;
            this.m_upgrades = new HashSet<UpgradeBlueprint>();
            this.m_slotitems = new HashSet<SlotItemBlueprint>();
            this.m_modifiers = new HashSet<Modifier>();
            this.m_deploymentMethod = DeploymentMethod.None;
            this.m_deploymentPhase = DeploymentPhase.PhaseNone;
        }

        /// <summary>
        /// New <see cref="UnitBuilder"/> instance based on the settings of an already built <see cref="Squad"/> instance.
        /// </summary>
        /// <param name="squad">The <see cref="Squad"/> instance to copy the unit data from.</param>
        /// <param name="overrideIndex">Should the built squad </param>
        /// <remarks>This will not modify the <see cref="Squad"/> instance.</remarks>
        public UnitBuilder(Squad squad, bool overrideIndex = true) {
            
            this.m_hasOverrideIndex = overrideIndex;
            this.m_overrideIndex = squad.SquadID;
            this.m_modifiers = squad.Modifiers.ToHashSet();
            this.m_upgrades = squad.Upgrades.Select(x => x as UpgradeBlueprint).ToHashSet();
            this.m_slotitems = squad.SlotItems.Select(x => x as SlotItemBlueprint).ToHashSet();
            this.m_vetexperience = squad.VeterancyProgress;
            this.m_vetrank = squad.VeterancyRank;
            this.m_blueprint = squad.SBP;
            this.m_transportBlueprint = squad.SupportBlueprint as SquadBlueprint;
            this.m_deploymentPhase = squad.DeploymentPhase;
            this.m_deploymentMethod = squad.DeploymentMethod;
            this.m_modGuid = string.Empty;
            
            if (squad.Crew != null) {
                this.m_crewBuilder = new UnitBuilder(squad.Crew, overrideIndex);
            }

        }
        
        /// <summary>
        /// New <see cref="UnitBuilder"/> instance based on the settings of an already built <see cref="Squad"/> instance.
        /// </summary>
        /// <param name="squad">The <see cref="Squad"/> instance to copy the unit data from.</param>
        /// <param name="builder"></param>
        /// <remarks>This will not modify the <see cref="Squad"/> instance.</remarks>
        public UnitBuilder(Squad squad, CompanyBuilder builder) {
            this.m_hasOverrideIndex = true;
            this.m_builder = builder;
            this.m_overrideIndex = squad.SquadID;
            this.m_modifiers = squad.Modifiers.ToHashSet();
            this.m_upgrades = squad.Upgrades.Select(x => x as UpgradeBlueprint).ToHashSet();
            this.m_slotitems = squad.SlotItems.Select(x => x as SlotItemBlueprint).ToHashSet();
            this.m_vetexperience = squad.VeterancyProgress;
            this.m_vetrank = squad.VeterancyRank;
            this.m_blueprint = squad.SBP;
            this.m_transportBlueprint = squad.SupportBlueprint as SquadBlueprint;
            this.m_deploymentPhase = squad.DeploymentPhase;
            this.m_deploymentMethod = squad.DeploymentMethod;
            this.m_modGuid = string.Empty;
            if (squad.Crew != null) {
                this.m_crewBuilder = new UnitBuilder(squad.Crew, true);
            }
        }

        /// <summary>
        /// Set the tuning pack GUID this unit should be based on.
        /// </summary>
        /// <param name="guid">The GUID (in coh2 string format).</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public UnitBuilder SetModGUID(string guid) {
            this.m_modGuid = guid;
            return this;
        }

        /// <summary>
        /// Copy the current <see cref="UnitBuilder"/> instance values into a new <see cref="UnitBuilder"/> instance.
        /// </summary>
        /// <returns>A cloned instance of the <see cref="UnitBuilder"/> instance.</returns>
        public virtual UnitBuilder Clone()
            => new UnitBuilder(this.Build(0), this.m_hasOverrideIndex);

        /// <summary>
        /// Set the veterancy rank of the <see cref="Squad"/> instance being built.
        /// </summary>
        /// <param name="level">The veterancy rank in byte-range to set.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetVeterancyRank(byte level) {
            this.m_vetrank = level;
            return this;
        }

        /// <summary>
        /// Set the veterancy progress of the <see cref="Squad"/> instance being built.
        /// </summary>
        /// <param name="experience">The veterancy progress to set.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetVeterancyExperience(float experience) {
            this.m_vetexperience = experience;
            return this;
        }

        /// <summary>
        /// Set the <see cref="SquadBlueprint"/> the <see cref="Squad"/> instance being built will use.
        /// </summary>
        /// <param name="sbp">The <see cref="SquadBlueprint"/> to set.</param>
        /// <remarks>This must be set before certain other methods.</remarks>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetBlueprint(SquadBlueprint sbp) {
            this.m_blueprint = sbp;
            return this;
        }

        /// <summary>
        /// Set the <see cref="SquadBlueprint"/> the <see cref="Squad"/> instance being built will use.
        /// </summary>
        /// /// <remarks>
        /// This must be called before certain other methods.
        /// </remarks>
        /// <param name="localBPID">The local property bag ID given to the blueprint.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetBlueprint(ushort localBPID) {
            if (localBPID == BlueprintManager.InvalidLocalBlueprint) {
                throw new ArgumentNullException(nameof(localBPID), "Cannot set unit blueprint to null!");
            }
            this.m_blueprint = BlueprintManager.GetCollection<SquadBlueprint>().FilterByMod(this.m_modGuid).Single(x => x.ModPBGID == localBPID);
            if (this.m_blueprint is null) {
                throw new ArgumentException($"Failed to find blueprint with mod PBGID {localBPID}", nameof(localBPID));
            }
            return this;
        }

        /// <summary>
        /// Set the <see cref="SquadBlueprint"/> the <see cref="Squad"/> instance being built will use.
        /// </summary>
        /// <remarks>
        /// This must be called before certain other methods.
        /// </remarks>
        /// <param name="sbpName">The blueprint name to use when finding the <see cref="Blueprint"/>.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetBlueprint(string sbpName) {
            this.m_blueprint = BlueprintManager.FromBlueprintName(sbpName, BlueprintType.SBP) as SquadBlueprint;
            return this;
        }

        /// <summary>
        /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
        /// </summary>
        /// <remarks>
        /// This must be called before certain other methods.
        /// </remarks>
        /// <param name="sbp">The <see cref="SquadBlueprint"/> to set.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetTransportBlueprint(SquadBlueprint sbp) {
            this.m_transportBlueprint = sbp;
            return this;
        }

        /// <summary>
        /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
        /// </summary>
        /// <remarks>
        /// This must be called before certain other methods.
        /// </remarks>
        /// <param name="localBPID">The local property bag ID given to the blueprint.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetTransportBlueprint(ushort localBPID) {
            if (localBPID == BlueprintManager.InvalidLocalBlueprint) {
                this.m_transportBlueprint = null;
                return this;
            }
            this.m_transportBlueprint = BlueprintManager.GetCollection<SquadBlueprint>()
                .FilterByMod(this.m_modGuid)
                .Single(x => x.ModPBGID == localBPID);
            if (this.m_transportBlueprint is null) {
                throw new ArgumentException($"Failed to find blueprint with mod PBGID {localBPID}", nameof(localBPID));
            }
            return this;
        }

        /// <summary>
        /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
        /// </summary>
        /// <remarks>
        /// This must be called before certain other methods.
        /// </remarks>
        /// <param name="sbpName">The blueprint name to use when finding the <see cref="Blueprint"/>.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetTransportBlueprint(string sbpName) {
            this.m_transportBlueprint = BlueprintManager.FromBlueprintName(sbpName, BlueprintType.SBP) as SquadBlueprint;
            return this;
        }

        /// <summary>
        /// Set the <see cref="DeploymentMethod"/> to use when the <see cref="Squad"/> instance being built is deployed.
        /// </summary>
        /// <param name="method">The <see cref="DeploymentMethod"/> to use when deploying.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetDeploymentMethod(DeploymentMethod method) {
            if (this.m_transportBlueprint == null && method >= DeploymentMethod.DeployAndExit) {
                throw new InvalidOperationException();
            }
            this.m_deploymentMethod = method;
            return this;
        }

        /// <summary>
        /// Set the <see cref="DeploymentPhase"/> the <see cref="Squad"/> instance being built may be deployed in.
        /// </summary>
        /// <param name="phase">The <see cref="DeploymentPhase"/> to set.</param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder SetDeploymentPhase(DeploymentPhase phase) {
            this.m_deploymentPhase = phase;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upb"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddUpgrade(UpgradeBlueprint upb) {
            this.m_upgrades.Add(upb);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upbs"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddUpgrade(UpgradeBlueprint[] upbs) {
            upbs.ForEach(x => this.m_upgrades.Add(x));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upb"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddUpgrade(string upb) {
            this.m_upgrades.Add(BlueprintManager.FromBlueprintName(upb, BlueprintType.UBP) as UpgradeBlueprint);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upbs"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddUpgrade(string[] upbs) {
            upbs.ForEach(x => this.AddUpgrade(x));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ibp"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddSlotItem(SlotItemBlueprint ibp) {
            this.m_slotitems.Add(ibp);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ibp"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddSlotItem(string ibp) {
            this.m_slotitems.Add(BlueprintManager.FromBlueprintName(ibp, BlueprintType.IBP) as SlotItemBlueprint);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder AddModifier(Modifier modifier) {
            this.m_modifiers.Add(modifier);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ubp"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder RemoveUpgrade(UpgradeBlueprint ubp) {
            this.m_upgrades.Remove(ubp);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ibp"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder RemoveSlotItem(SlotItemBlueprint ibp) {
            this.m_slotitems.Remove(ibp);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns>The modified instance the method is invoked with.</returns>
        public virtual UnitBuilder RemoveModifier(Modifier modifier) {
            this.m_modifiers.Remove(modifier);
            return this;
        }

        /// <summary>
        /// Create or get a new <see cref="UnitBuilder"/> instance representing the <see cref="Squad"/> crew of the current <see cref="UnitBuilder"/> instance.
        /// </summary>
        /// <returns>The vehicle crew <see cref="UnitBuilder"/> instance for  the (vehicle/crewable) <see cref="UnitBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public virtual UnitBuilder CreateAndGetCrew() {
            if (this.m_blueprint == null) {
                throw new ArgumentNullException();
            }
            if (!this.m_blueprint.HasCrew) {
                throw new InvalidOperationException();
            }
            if (this.m_crewBuilder == null) {
                this.m_crewBuilder = new UnitBuilder();
            }
            return this.m_crewBuilder;
        }

        /// <summary>
        /// Build the <see cref="Squad"/> instance using the data collected with the <see cref="UnitBuilder"/>. The ID will be copied from the original <see cref="Squad"/> if possible.
        /// </summary>
        /// <param name="ID">The unique ID to use when creating the <see cref="Squad"/> instance.</param>
        /// <returns>A <see cref="Squad"/> instance with all the parameters defined by the <see cref="UnitBuilder"/>.</returns>
        public virtual Squad Build(ushort ID) {
        
            if (this.m_hasOverrideIndex) {
                ID = this.m_overrideIndex;
            }

            Squad squad = new Squad(ID, null, this.m_blueprint);
            squad.SetDeploymentMethod(this.m_transportBlueprint, this.m_deploymentMethod, this.m_deploymentPhase);
            squad.SetVeterancy(this.m_vetrank, this.m_vetexperience);

            if (this.m_blueprint?.HasCrew ?? false && this.m_crewBuilder == null) {
                this.CreateAndGetCrew();
            }

            if (this.m_crewBuilder != null) {
                squad.SetCrew(this.m_crewBuilder.Build((ushort)(ID + 1)));
            }

            this.m_upgrades.ToArray().ForEach(x => squad.AddUpgrade(x));
            this.m_slotitems.ToArray().ForEach(x => squad.AddSlotItem(x));
            this.m_modifiers.ToArray().ForEach(x => squad.AddModifier(x));

            return squad;

        }

        /// <summary>
        /// Apply changes directly to the company.
        /// </summary>
        public virtual void Apply() {
            if (this.m_builder is not null && this.m_overrideIndex != ushort.MaxValue) {
                this.m_builder.Result.ReplaceSquad(this.m_overrideIndex, this.Build(0));
            } else {
                throw new InvalidOperationException("Cannot apply changes as this is a new unit.");
            }
        }

        /// <summary>
        /// Reset all the values set by the <see cref="UnitBuilder"/>.
        /// </summary>
        public virtual void Reset() {

            this.m_blueprint = null;
            this.m_crewBuilder = null;
            this.m_deploymentMethod = DeploymentMethod.None;
            this.m_deploymentPhase = DeploymentPhase.PhaseNone;
            this.m_hasOverrideIndex = false;
            this.m_modifiers.Clear();
            this.m_overrideIndex = 0;
            this.m_slotitems.Clear();
            this.m_transportBlueprint = null;
            this.m_upgrades.Clear();
            this.m_vetexperience = 0;
            this.m_vetrank = 0;

        }

        /// <summary>
        /// Clone self and resets the current instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public UnitBuilder GetAndReset() {
            UnitBuilder clone = this.Clone();
            this.Reset();
            return clone;
        }

    }

}
