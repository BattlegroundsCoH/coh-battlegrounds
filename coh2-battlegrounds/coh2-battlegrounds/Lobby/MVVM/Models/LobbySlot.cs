using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public abstract class LobbySlot : INotifyPropertyChanged {

        protected static readonly Dictionary<string, ImageSource> FactionIcons = new() {
            [Faction.Soviet.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIET.png")),
            [Faction.America.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionAEF.png")),
            [Faction.British.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionBRIT.png")),
            [Faction.OberkommandoWest.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionOKW.png")),
            [Faction.Wehrmacht.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionWEHR.png")),
            ["?"] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIET.png")),
            [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionLOCKED.png")),
        };

        protected static readonly Dictionary<string, ImageSource> FactionHoverIcons = new() {
            [Faction.Soviet.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIETHighlighted.png")),
            [Faction.America.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionAEFHighlighted.png")),
            [Faction.British.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionBRITHighlighted.png")),
            [Faction.OberkommandoWest.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionOKWHighlighted.png")),
            [Faction.Wehrmacht.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionWEHRHighlighted.png")),
            ["?"] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIETHighlighted.png")),
            [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionLOCKED.png")),
        };

        protected static readonly List<Company> AlliedCompanies = PlayerCompanies.FindAll(x => x.Army.IsAllied);

        protected static readonly List<Company> AxisCompanies = PlayerCompanies.FindAll(x => x.Army.IsAxis);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected LobbyAPIStructs.LobbySlot m_slot;

        private int m_selectedCompany;

        public Visibility IsSlotVisible => this.Slot.State == 3 ? Visibility.Collapsed : Visibility.Visible;

        public ImageSource? LeftIcon { get; set; }

        public ImageSource? LeftIconHover { get; set; }

        public LobbyTeam Team { get; }

        public LobbyAPIStructs.LobbySlot Slot => this.m_slot;

        public ObservableCollection<LobbyAPIStructs.LobbyCompany> SelectableCompanies { get; }

        public LobbyAPIStructs.LobbyCompany SelectedCompany => this.SelectableCompanies[0];

        public bool IsSelf => this.Slot.IsSelf();

        public bool IsSlotMouseOver { get; set; }

        public string LeftDisplayString => this.Slot.State switch {
            0 => string.Empty,
            1 => this.Slot.Occupant?.DisplayName ?? "FATAL ERROR",
            2 => "Locked",
            _ => string.Empty
        };

        public abstract Visibility IsCompanySelectorVisible { get; } // TODO: Disable if only one element can be picked.

        public Visibility IsCompanyInfoVisible => this.IsCompanySelectorVisible == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        public int SelectedCompanyIndex {
            get => this.m_selectedCompany;
            set {
                this.m_selectedCompany = value;
                this.OnLobbyCompanyChanged(value);
                this.PropertyChanged?.Invoke(this, new(nameof(LeftIcon)));
            }
        }

        public LobbySlot(LobbyAPIStructs.LobbySlot teamSlot, LobbyTeam team) {

            // Set values
            this.Team = team;
            this.m_slot = teamSlot;
            this.SelectableCompanies = new((teamSlot.TeamID == 0 ? AlliedCompanies : AxisCompanies).Select(FromCompany));

            // Set some defaults
            this.SetLeftIcon(FactionIcons[this.SelectedCompany.Army], FactionHoverIcons[this.SelectedCompany.Army]);

            // Get API
            if (teamSlot.API is LobbyAPI api) {
                api.OnLobbySlotUpdate += this.OnLobbySlotUpdate;
            } else {
                Trace.WriteLine("Invalid lobby API given -- slot wont update properly!!!", nameof(LobbySlot));
            }

        }

        private LobbyAPIStructs.LobbyCompany FromCompany(Company x)
            => new LobbyAPIStructs.LobbyCompany() {
                API = Slot.API ?? throw new Exception("API should always be set on company!"),
                Army = x.Army.Name,
                IsAuto = false,
                IsNone = false,
                Name = x.Name,
                Specialisation = x.Type.ToString(),
                Strength = (float)x.Rating
            };

        private void OnLobbySlotUpdate(LobbyAPIStructs.LobbySlot args) {
            if (args.TeamID == this.Team.Team.TeamID && args.SlotID == this.m_slot.SlotID) {

                // Update internal repr.
                this.m_slot = args;

                // Update state
                this.PropertyChanged?.Invoke(this, new(nameof(IsSelf)));
                this.PropertyChanged?.Invoke(this, new(nameof(IsSlotVisible)));
                this.PropertyChanged?.Invoke(this, new(nameof(IsCompanyInfoVisible)));
                this.PropertyChanged?.Invoke(this, new(nameof(IsCompanySelectorVisible)));

            }
        }

        protected void SetLeftIcon(ImageSource normal, ImageSource hover) {
            
            // Set icons
            this.LeftIcon = normal;
            this.LeftIconHover = hover;
            
            // Do property changed
            this.PropertyChanged?.Invoke(this, new(nameof(LeftIcon)));
            this.PropertyChanged?.Invoke(this, new(nameof(LeftIconHover)));

        }

        protected abstract void OnLobbyCompanyChanged(int newValue);

    }

}
