using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// 
    /// </summary>
    public class CompanyBuilder {

        private Company m_companyTarget;
        private Stack<UnitBuilder> m_uncommittedSquads;
        private Stack<UnitBuilder> m_redo;

        public Company Result => this.m_companyTarget;

        public CompanyBuilder() {
            this.m_companyTarget = null;
            this.m_uncommittedSquads = new Stack<UnitBuilder>();
            this.m_redo = new Stack<UnitBuilder>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void NewCompany() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyTarget"></param>
        public void DesignCompany(Company companyTarget)
            => this.m_companyTarget = companyTarget;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public void AddUnit(UnitBuilder builder) {
            this.m_uncommittedSquads.Push(builder);
            this.m_redo.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Commit() {

            while(this.m_uncommittedSquads.Count > 0) {

                // Pop top element of uncommited
                UnitBuilder unit = this.m_uncommittedSquads.Pop();

                // Add squad
                this.m_companyTarget.AddSquad(unit);

            }

            this.m_redo.Clear();   

        }

        public void Undo() {
            if (this.m_uncommittedSquads.Count > 0)
                this.m_redo.Push(this.m_uncommittedSquads.Pop());
        }

        public void Redo() {
            if (this.m_redo.Count > 0)
                this.m_uncommittedSquads.Push(this.m_redo.Pop());
        }

    }

}
