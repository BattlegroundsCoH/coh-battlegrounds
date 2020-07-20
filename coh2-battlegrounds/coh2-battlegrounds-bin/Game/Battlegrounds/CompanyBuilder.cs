using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// 
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
        /// 
        /// </summary>
        public Company Result => this.m_companyTarget;

        /// <summary>
        /// 
        /// </summary>
        public bool CanUndoSquad => this.m_uncommittedSquads.Count > 0;

        /// <summary>
        /// 
        /// </summary>
        public bool CanRedoSquad => this.m_redo.Count > 0;

        public CompanyBuilder() {
            this.m_companyTarget = null;
            this.m_uncommittedSquads = new Stack<UnitBuilder>();
            this.m_redo = new Stack<UnitBuilder>();
        }

        /// <summary>
        /// 
        /// </summary>
        public CompanyBuilder NewCompany(Faction faction) {
            this.m_companyTarget = new Company();
            this.m_companyTarget.SetArmy(faction);
            this.m_companyName = "New Company";
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyTarget"></param>
        public CompanyBuilder DesignCompany(Company companyTarget) {
            this.m_companyTarget = companyTarget;
            this.m_companyType = companyTarget.Type;
            this.m_companyName = companyTarget.Name;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public CompanyBuilder AddUnit(UnitBuilder builder) {

            // If null, throw error
            if (builder == null) {
                throw new ArgumentNullException();
            }

            // If null, throw error
            if (this.m_companyTarget == null) {
                throw new ArgumentNullException();
            }

            // Add if we can
            if (this.m_uncommittedSquads.Count + 1 + this.m_companyTarget.Units.Length < Company.MAX_SIZE) {
                this.m_uncommittedSquads.Push(builder);
                this.m_redo.Clear();
            }
            
            return this;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public CompanyBuilder ChangeType(CompanyType type) {
            this.m_companyType = type;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CompanyBuilder ChangeName(string name) {
            this.m_companyName = name;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CompanyBuilder ChangeUser(string name) {
            this.m_companyUsername = name;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuningGUID"></param>
        /// <returns></returns>
        public CompanyBuilder ChangeTuningMod(string tuningGUID) {
            this.m_companyGUID = tuningGUID.Replace("-", "");
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Commit() {

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
        /// 
        /// </summary>
        public void UndoSquad() {
            if (this.m_uncommittedSquads.Count > 0)
                this.m_redo.Push(this.m_uncommittedSquads.Pop());
        }

        /// <summary>
        /// 
        /// </summary>
        public void RedoSquad() {
            if (this.m_redo.Count > 0)
                this.m_uncommittedSquads.Push(this.m_redo.Pop());
        }

    }

}
