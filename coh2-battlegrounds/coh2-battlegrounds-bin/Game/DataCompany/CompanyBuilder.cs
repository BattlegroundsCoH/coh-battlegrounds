using System;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.DataCompany {

    /// <summary>
    /// Builder class for building a <see cref="Company"/>. Inherit if you wish to extend functionality. This class is intended to be used for method chaining (But is not required for use).
    /// </summary>
    public class CompanyBuilder {

        private Company m_companyTarget;
        private CompanyType m_companyType;
        private CompanyAvailabilityType m_availabilityType;
        private string m_companyName;
        private string m_companyUsername;
        private string m_companyAppVersion;
        private ModGuid m_companyGUID;

        /// <summary>
        /// Get the resulting <see cref="Company"/>. (Call <see cref="NewCompany(Faction)"/> and <see cref="Commit"/> to apply changes).
        /// </summary>
        public Company Result => this.m_companyTarget;

        /// <summary>
        /// Get if it is possible to add another unit.
        /// </summary>
        public bool CanAddUnit => this.m_companyTarget.Units.Length <= Company.MAX_SIZE;

        /// <summary>
        /// Get if it is possible to add another ability.
        /// </summary>
        public bool CanAddAbility => this.m_companyTarget.Abilities.Length <= Company.MAX_ABILITY;

        /// <summary>
        /// New instance of the <see cref="CompanyBuilder"/>.
        /// </summary>
        public CompanyBuilder()
            => this.m_companyTarget = null;

        /// <summary>
        /// Creates a new <see cref="Company"/> internally that the <see cref="CompanyBuilder"/> will modify while building.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that the company will belong to.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder NewCompany(Faction faction) {
            this.m_companyTarget = new Company(faction); // This is intentional
            this.m_companyName = "New Company";
            this.m_companyAppVersion = this.m_companyTarget.AppVersion;
            return this;
        }

        /// <summary>
        /// Attaches the <see cref="CompanyBuilder"/> to a specific <see cref="Company"/> instance to edit.
        /// </summary>
        /// <param name="companyTarget">The <see cref="Company"/> instance to edit.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder DesignCompany(Company companyTarget) {
            this.m_companyTarget = companyTarget;
            this.m_companyType = companyTarget.Type;
            this.m_companyName = companyTarget.Name;
            this.m_companyGUID = companyTarget.TuningGUID;
            this.m_companyAppVersion = companyTarget.AppVersion;
            return this;
        }

        /// <summary>
        /// Clones an existing <see cref="Company"/> instance using a <see cref="CompanyTemplate"/> to clone.
        /// </summary>
        /// <remarks>
        /// Company progression is lost when using this method.
        /// </remarks>
        /// <param name="company">The company to clone.</param>
        /// <param name="newName">The new name of the company.</param>
        /// <param name="companyAvailability">The <see cref="CompanyAvailabilityType"/> new availability type</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public CompanyBuilder CloneCompany(Company company, string newName, CompanyAvailabilityType companyAvailability) {
            var template = CompanyTemplate.FromCompany(company);
            this.m_companyTarget = CompanyTemplate.FromTemplate(template);
            this.m_companyName = newName;
            this.m_companyType = this.m_companyTarget.Type;
            this.m_companyGUID = this.m_companyTarget.TuningGUID;
            this.m_availabilityType = companyAvailability;
            return this;
        }

        /// <summary>
        /// Release the internal <see cref="Company"/> instance that's being edited.
        /// </summary>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder ReleaseCompany() {
            this.m_companyTarget = null;
            this.m_companyType = CompanyType.Unspecified;
            this.m_companyName = string.Empty;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ulong CalculateChecksum() => this.m_companyTarget.Checksum;

        /// <summary>
        /// Add a unit to the <see cref="Company"/> using a <see cref="UnitBuilder"/>.
        /// </summary>
        /// <param name="builder">The function to build the unit.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder AddUnit(Func<UnitBuilder, UnitBuilder> builder) {
            UnitBuilder bld = new();
            bld.SetModGUID(this.m_companyGUID.ToString());
            _ = this.AddAndCommitUnit(builder(bld));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public virtual Squad AddAndCommitUnit(UnitBuilder builder) {

            // If null, throw error
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder), "The given unit builder may not be null");
            }

            // If null, throw error
            if (this.m_companyTarget == null) {
                throw new ArgumentNullException("CompanyTarget", "Cannot add unit to a company that has not been created.");
            }

            // Set the mod GUID (So we get no conflicts between mods).
            builder.SetModGUID(this.m_companyGUID);

            // Add squad
            ushort sid = this.m_companyTarget.AddSquad(builder);

            // Ask for squad and return
            return this.m_companyTarget.GetSquadByIndex(sid);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ability"></param>
        public virtual void AddAndCommitAbility(Ability ability) => this.m_companyTarget.AddAbility(ability);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abp"></param>
        /// <returns></returns>
        public virtual Ability AddAndCommitAbility(AbilityBlueprint abp, AbilityCategory specialAbility, int maxUse) {

            // Create ability
            Ability sabp = new(abp, AbilityCategory.Default, 0);

            // Add ability
            this.m_companyTarget.AddAbility(sabp);

            // Return the special ability
            return sabp;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public virtual CompanyBuilder RemoveAbility(Ability ability) {
            _ = this.m_companyTarget.RemoveAbility(ability);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitID"></param>
        /// <returns></returns>
        public virtual CompanyBuilder RemoveUnit(uint unitID) {

            this.m_companyTarget.RemoveSquad((ushort)unitID);

            // Return self for method chaining
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squadId"></param>
        /// <returns></returns>
        public virtual UnitBuilder GetUnit(uint squadId) {
            if (this.m_companyTarget.Units.FirstOrDefault(x => x.SquadID == squadId) is Squad s) {
                return new UnitBuilder(s, true);
            } else {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Change the specified <see cref="CompanyType"/> of the <see cref="Company"/>.
        /// </summary>
        /// <param name="type">The new <see cref="CompanyType"/> to set.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder ChangeType(CompanyType type) {
            this.m_companyType = type;
            return this;
        }

        /// <summary>
        /// Change the name of the <see cref="Company"/>.
        /// </summary>
        /// <param name="name">The new name to set.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder ChangeName(string name) {
            this.m_companyName = name;
            return this;
        }

        /// <summary>
        /// Change the user of the company (Possibly obsolete - you may ignore it).
        /// </summary>
        /// <param name="name"></param>
        public virtual CompanyBuilder ChangeUser(string name) {
            this.m_companyUsername = name;
            return this;
        }

        /// <summary>
        /// Change the associated <see cref="Guid"/> of the <see cref="Company"/>. (This will decide from where the blueprints can be drawn from).
        /// </summary>
        /// <param name="tuningGUID">The string version of the mod GUID. (May contain '-' characters).</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder ChangeTuningMod(string tuningGUID) {
            this.m_companyGUID = ModGuid.FromGuid(tuningGUID);
            return this;
        }

        /// <summary>
        /// Change the associated <see cref="Guid"/> of the <see cref="Company"/>. (This will decide from where the blueprints can be drawn from).
        /// </summary>
        /// <param name="tuningGUID">The tuning mod GUID.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder ChangeTuningMod(ModGuid tuningGUID) {
            this.m_companyGUID = tuningGUID;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual CompanyBuilder ChangeAppVersion(string version) {
            this.m_companyAppVersion = version;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="availabilityType"></param>
        /// <returns></returns>
        public virtual CompanyBuilder ChangeAvailability(CompanyAvailabilityType availabilityType) {
            this.m_availabilityType = availabilityType;
            return this;
        }

        /// <summary>
        /// Get the amount of units in the specified deployment <paramref name="phase"/>.
        /// </summary>
        /// <param name="phase">The phase to fetch amount of units from.</param>
        /// <returns>The amount of units in specific phase</returns>
        public virtual int CountUnitsInPhase(DeploymentPhase phase)
            => this.m_companyTarget.Units.Count(x => x.DeploymentPhase == phase);

        /// <summary>
        /// Commit all unsaved changes to the <see cref="Company"/> target instance.
        /// </summary>
        /// <remarks>
        /// Result can be extracted from the <see cref="Result"/> property.
        /// </remarks>
        /// <returns>
        /// The calling <see cref="CompanyBuilder"/> instance.
        /// </returns>
        public virtual CompanyBuilder Commit() {

            // If null, throw error
            if (this.m_companyTarget == null) {
                throw new ArgumentNullException("Internal Company Target", "Unable to commit changes to undefined company");
            }

            // Update company fluff
            this.m_companyTarget.SetType(this.m_companyType);
            this.m_companyTarget.SetAvailability(this.m_availabilityType);
            this.m_companyTarget.SetAppVersion(this.m_companyAppVersion);
            this.m_companyTarget.Name = this.m_companyName;
            this.m_companyTarget.TuningGUID = this.m_companyGUID;
            this.m_companyTarget.Owner = this.m_companyUsername;

            // Return self.
            return this;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasAbility(string blueprint) => this.m_companyTarget.Abilities.Any(x => x.ABP.Name == blueprint);

        /// <summary>
        /// Get if the company under construction has a squad with specified <paramref name="blueprint"/>.
        /// </summary>
        /// <param name="blueprint">The name of the blueprint to check for.</param>
        /// <returns><see langword="true"/>, if <paramref name="blueprint"/> is found; Otherwise <see langword="false"/>.</returns>
        public bool HasUnit(string blueprint) => this.m_companyTarget.Units.Any(x => x.SBP.Name == blueprint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squad"></param>
        public void EachUnit(Action<Squad> action, Func<Squad, int> sort) => this.m_companyTarget.Units.OrderBy(sort).ForEach(action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void EachUnit(Action<Squad> action) => this.EachUnit(action, x => x.SquadID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void EachAbility(Action<Ability, bool> action) {
            _ = this.m_companyTarget.GetSpecialUnitAbilities().ForEach(x => action(x, true));
            _ = this.m_companyTarget.Abilities.ForEach(x => action(x, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void EachItem(Action<Blueprint> action) => this.m_companyTarget.Inventory.ForEach(action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blueprint"></param>
        public void AddEquipment(Blueprint blueprint)
            => this.m_companyTarget.AddInventoryItem(blueprint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        public CompanyBuilder RemoveEquipment(Blueprint equipment) {
            this.m_companyTarget.RemoveInventoryItem(equipment);
            return this;
        }

    }

}
