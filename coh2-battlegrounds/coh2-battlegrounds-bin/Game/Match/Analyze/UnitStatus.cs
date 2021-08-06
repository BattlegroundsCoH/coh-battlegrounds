using System;
using System.Collections.Generic;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Analyze {

    public class UnitStatus { // Maybe convert into an interface in the future

        public Player PlayerOwner { get; }
        public ushort UnitID { get; }
        public List<Blueprint> CapturedSlotItems { get; }

        public bool IsDead;
        public bool IsDeployed;
        public bool IsWithdrawn;
        public byte VetChange;
        public float VetExperience;
        public TimeSpan FirstSeen;
        public TimeSpan LastSeen;

        public TimeSpan CombatTime => this.LastSeen - this.FirstSeen;

        public UnitStatus(Player owner, ushort id) {
            this.PlayerOwner = owner;
            this.UnitID = id;
            this.IsDead = false;
            this.IsDeployed = false;
            this.IsWithdrawn = false;
            this.VetChange = 0;
            this.VetExperience = 0.0f;
            this.FirstSeen = TimeSpan.Zero;
            this.LastSeen = TimeSpan.Zero;
            this.CapturedSlotItems = new List<Blueprint>();
        }

        public bool MakeDead(TimeSpan stamp) {
            if (this.IsDeployed) {
                this.LastSeen = stamp;
                this.IsDead = true;
                this.IsDeployed = false;
                return true;
            } else {
                return false;
            }
        }

        public bool Deploy(TimeSpan stamp) {
            if (!this.IsDead && !this.IsDeployed) {
                this.IsDeployed = true;
                if (this.FirstSeen == TimeSpan.Zero) {
                    this.FirstSeen = stamp;
                }
                return true;
            } else {
                return false;
            }
        }

        public bool Callback(TimeSpan stamp, byte vChange, float xp) {
            if (!this.IsDead && this.IsDeployed && !this.IsWithdrawn && xp >= this.VetExperience) {
                this.IsDeployed = false;
                this.IsWithdrawn = true;
                this.VetChange = vChange;
                this.VetExperience = xp;
                this.LastSeen = stamp;
                return true;
            } else {
                return false;
            }
        }

    }

}
