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

using Battlegrounds;
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

        private static readonly Func<string> LOCSTR_SLOT_OPEN = () => BattlegroundsInstance.Localize.GetString("TeamPlayerCard_Open_Slot");
        private static readonly Func<string> LOCSTR_SLOT_LOCKED = () => BattlegroundsInstance.Localize.GetString("TeamPlayerCard_Locked_Slot");

        protected static readonly LobbyAPIStructs.LobbyCompany DummyCompany = new() { Army = "soviet", IsNone = true, Name = "None", Specialisation = "", Strength = 0 };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected LobbyAPIStructs.LobbySlot m_slot;

        private int m_selectedCompany;

        public LobbyAPI Handle { get; }

        public Visibility IsSlotVisible => this.Slot.State == 3 ? Visibility.Collapsed : Visibility.Visible;

        public ImageSource? LeftIcon { get; set; }

        public ImageSource? LeftIconHover { get; set; }

        public LobbyTeam Team { get; }

        public LobbyAPIStructs.LobbySlot Slot => this.m_slot;

        public ObservableCollection<LobbyAPIStructs.LobbyCompany> SelectableCompanies { get; }

        public LobbyAPIStructs.LobbyCompany SelectedCompany 
            => this.SelectedCompanyIndex >= 0 ? this.SelectableCompanies[this.SelectedCompanyIndex] : DummyCompany;

        public abstract LobbyContextMenu ContextMenu { get; }

        public bool IsSelf => this.Slot.IsSelf();

        public bool IsSlotMouseOver { get; set; }

        public string LeftDisplayString => this.Slot.State switch {
            0 => LOCSTR_SLOT_OPEN(),
            1 => this.Slot.Occupant?.DisplayName ?? "FATAL ERROR",
            2 => LOCSTR_SLOT_LOCKED(),
            _ => string.Empty
        };

        public abstract Visibility IsCompanySelectorVisible { get; } // TODO: Disable if only one element can be picked.

        public Visibility IsCompanyInfoVisible 
            => this.IsCompanySelectorVisible == Visibility.Collapsed && this.Slot.IsOccupied ? Visibility.Visible : Visibility.Collapsed;

        public int SelectedCompanyIndex {
            get => this.m_selectedCompany;
            set {
                this.m_selectedCompany = value;
                this.OnLobbyCompanyChanged(value);
                this.PropertyChanged?.Invoke(this, new(nameof(LeftIcon)));
                this.PropertyChanged?.Invoke(this, new(nameof(SelectedCompanyIndex)));
            }
        }

        public LobbySlot(LobbyAPIStructs.LobbySlot teamSlot, LobbyTeam team) {

            // Set values
            this.Team = team;
            this.Handle = teamSlot.API ?? throw new Exception("Expected valid API handle but got none!");
            this.m_slot = teamSlot;

            // Check if self or AI, then set contents
            this.SelectableCompanies = new() { teamSlot.Occupant is null ? DummyCompany : teamSlot.Occupant.Company ?? DummyCompany };

            // Do initial view
            this.RefreshCompanyInfo();

            // Get API
            if (teamSlot.API is LobbyAPI api) {
                api.OnLobbySlotUpdate += this.OnLobbySlotUpdate;
            } else {
                Trace.WriteLine("Invalid lobby API given -- slot wont update properly!!!", nameof(LobbySlot));
            }

        }

        protected void RefreshCompanyInfo() {
            
            // Invoke view-specific code
            this.RefreshCompanyView();

            // Trigger LHS icon refresh
            this.RefreshLHSIcon();

        }

        protected abstract void RefreshCompanyView();

        protected void RefreshLHSIcon() {

            // Set icons
            if (this.m_slot.IsOccupied && this.SelectedCompanyIndex >= 0) {

                // Set some defaults
                this.SetLeftDisplay(FactionIcons[this.SelectedCompany.Army], FactionHoverIcons[this.SelectedCompany.Army]);

            } else if (this.m_slot.State == 0) {

                // Hide
                this.SetLeftDisplay(null, null);

            } else {

                // Show lock
                this.SetLeftDisplay(FactionIcons[string.Empty], FactionHoverIcons[string.Empty]);

            }

        }

        protected LobbyAPIStructs.LobbyCompany FromCompany(Company x)
            => new LobbyAPIStructs.LobbyCompany() {
                API = Slot.API ?? throw new Exception("API should always be set on company!"),
                Army = x.Army.Name,
                IsAuto = false,
                IsNone = false,
                Name = x.Name,
                Specialisation = x.Type.ToString(),
                Strength = (float)x.Rating
            };

        public void OnLobbySlotUpdate(LobbyAPIStructs.LobbySlot args) {
            Application.Current.Dispatcher.Invoke(() => {
                if (args.TeamID == this.Team.Team.TeamID && args.SlotID == this.m_slot.SlotID) {

                    // Update internal repr.
                    this.m_slot = args;

                    // Update init view
                    this.RefreshCompanyInfo();

                    // Update context menu
                    this.ContextMenu.RefreshMenu();

                    // Update state
                    this.PropertyChanged?.Invoke(this, new(nameof(IsSelf)));
                    this.PropertyChanged?.Invoke(this, new(nameof(IsSlotVisible)));
                    this.PropertyChanged?.Invoke(this, new(nameof(IsCompanyInfoVisible)));
                    this.PropertyChanged?.Invoke(this, new(nameof(IsCompanySelectorVisible)));

                }
            });
        }

        protected void SetLeftDisplay(ImageSource? normal, ImageSource? hover) {
            
            // Set icons
            this.LeftIcon = normal;
            this.LeftIconHover = hover;
            
            // Do property changed
            this.PropertyChanged?.Invoke(this, new(nameof(LeftIcon)));
            this.PropertyChanged?.Invoke(this, new(nameof(LeftIconHover)));
            this.PropertyChanged?.Invoke(this, new(nameof(LeftDisplayString)));

        }

        protected void SetCompany() {

            // Update company view
            this.RefreshLHSIcon();
            
            // Notify lobby
            this.Handle.SetCompany(this.Slot.TeamID, this.Slot.SlotID, this.SelectedCompany);

        }

        protected abstract void OnLobbyCompanyChanged(int newValue);

        public abstract void OnLobbyCompanyChanged(LobbyAPIStructs.LobbyCompany company);

        protected void NotifyProperty(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);

    }

}
