using System;
using System.Collections.Generic;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Builder class for building a <see cref="Company"/>. Inherit if you wish to extend functionality. This class is intended to be used for method chaining (But is not required for use).
    /// </summary>
    public class CompanyBuilder {

        private Company m_companyTarget;
        private CompanyType m_companyType;
        private string m_companyName;
        private string m_companyGUID;
        private string m_companyUsername;
        private Stack<UnitBuilder> m_uncommittedSquads;
        private Stack<UnitBuilder> m_redo;

        /// <summary>
        /// Get the resulting <see cref="Company"/>. (Call <see cref="NewCompany(Faction)"/> and <see cref="Commit"/> to apply changes).
        /// </summary>
        public Company Result => this.m_companyTarget;

        /// <summary>
        /// Get if any uncomitted changes to squads can be undone.
        /// </summary>
        public bool CanUndoSquad => this.m_uncommittedSquads.Count > 0;

        /// <summary>
        /// Get if any undone changes can be redone.
        /// </summary>
        public bool CanRedoSquad => this.m_redo.Count > 0;

        /// <summary>
        /// New instance of the <see cref="CompanyBuilder"/>.
        /// </summary>
        public CompanyBuilder() {
            this.m_companyTarget = null;
            this.m_uncommittedSquads = new Stack<UnitBuilder>();
            this.m_redo = new Stack<UnitBuilder>();
        }

        /// <summary>
        /// Creates a new <see cref="Company"/> internally that the <see cref="CompanyBuilder"/> will modify while building.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that the company will belong to.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder NewCompany(Faction faction) {
#pragma warning disable CS0618 // Type or member is obsolete
            this.m_companyTarget = new Company(); // This is intentional
#pragma warning restore CS0618 // Type or member is obsolete
            this.m_companyTarget.SetArmy(faction);
            this.m_companyName = "New Company";
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
        /// Add a unit to the <see cref="Company"/> using a <see cref="UnitBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="UnitBuilder"/> instance containing specific <see cref="Squad"/> data.</param>
        /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
        public virtual CompanyBuilder AddUnit(UnitBuilder builder) {

            // If null, throw error
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder), "The given unit builder may not be null");
            }

            // If null, throw error
            if (this.m_companyTarget == null) {
                throw new NullReferenceException("Cannot add unit to a company that has not been created.");
            }

            // Set the mod GUID (So we get no conflicts between mods).
            builder.SetModGUID(this.m_companyGUID);

            // Add if we can
            if (this.m_uncommittedSquads.Count + 1 + this.m_companyTarget.Units.Length < Company.MAX_SIZE) {
                this.m_uncommittedSquads.Push(builder);
                this.m_redo.Clear();
            }
            
            // Return self for method chaining
            return this;

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
            this.m_companyGUID = tuningGUID.Replace("-", "");
            return this;
        }

        /// <summary>
        /// Commit all unsaved changes to the <see cref="Company"/> target instance (Result can be extracted from the <see cref="Result"/> property).
        /// </summary>
        public virtual void Commit() {

            // If null, throw error
            if (this.m_companyTarget == null) {
                throw new ArgumentNullException();
            }

            // Update company fluff
            this.m_companyTarget.SetType(this.m_companyType);
            this.m_companyTarget.Name = this.m_companyName;
            this.m_companyTarget.TuningGUID = this.m_companyGUID;
            this.m_companyTarget.Owner = this.m_companyUsername;

            // While there are squads to add
            while(this.m_uncommittedSquads.Count > 0) {

                // Pop top element of uncommited
                UnitBuilder unit = this.m_uncommittedSquads.Pop();

                // Add squad
                this.m_companyTarget.AddSquad(unit);

            }

            // Clear the redo
            this.m_redo.Clear();

        }

        /// <summary>
        /// Undo adding a squad.
        /// </summary>
        public void UndoSquad() {
            if (this.m_uncommittedSquads.Count > 0)
                this.m_redo.Push(this.m_uncommittedSquads.Pop());
        }

        /// <summary>
        /// Redo adding a squad.
        /// </summary>
        public void RedoSquad() {
            if (this.m_redo.Count > 0)
                this.m_uncommittedSquads.Push(this.m_redo.Pop());
        }

    }

}
