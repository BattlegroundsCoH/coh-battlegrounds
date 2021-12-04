using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbySlot : INotifyPropertyChanged {

        private static readonly Dictionary<string, ImageSource> FactionIcons = new() {
            [Faction.Soviet.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIET.png")),
            [Faction.America.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionAEF.png")),
            [Faction.British.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionBRIT.png")),
            [Faction.OberkommandoWest.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionOKW.png")),
            [Faction.Wehrmacht.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionWEHR.png")),
            [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionLOCKED.png")),
        };

        private static readonly Dictionary<string, ImageSource> FactionHoverIcons = new() {
            [Faction.Soviet.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIETHighlighted.png")),
            [Faction.America.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionAEFHighlighted.png")),
            [Faction.British.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionBRITHighlighted.png")),
            [Faction.OberkommandoWest.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionOKWHighlighted.png")),
            [Faction.Wehrmacht.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionWEHRHighlighted.png")),
            [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionLOCKED.png")),
        };

        public event PropertyChangedEventHandler PropertyChanged;

        private LobbyCompanyItem m_selfCompanySelected;
        private readonly LobbyTeam m_teamModel;

        public LobbyAPIStructs.LobbySlot NetworkInterface { get; set; }

        public string LeftDisplayString => this.NetworkInterface.State switch {
            0 => "Open",
            1 => this.NetworkInterface.Occupant.DisplayName,
            2 => "Locked",
            3 => "Disabled",
            _ => throw new NotImplementedException()
        };

        public bool IsSelf => this.NetworkInterface.IsSelf();

        public bool IsOpen => this.NetworkInterface.State is 0;

        public bool IsLocked => this.NetworkInterface.State is 2;

        public bool IsDisabled => this.NetworkInterface.State is 3;

        public bool IsAIOccupant => this.NetworkInterface.IsAI();

        public Visibility IsSlotVisible => this.IsDisabled ? Visibility.Hidden : Visibility.Visible;

        public Visibility LeftIconVisibility => this.IsOpen ? Visibility.Hidden : Visibility.Visible;

        public Visibility CompanyVisibility => !(this.IsOpen || this.IsLocked) ? Visibility.Visible : Visibility.Hidden;

        public int SelectedCompanyIndex { get; set; }

        public ImageSource LeftIcon { get; set; }

        public LobbyCompanyItem SelectedCompany { get => this.m_selfCompanySelected; set => this.OnCompanySelectionChanged(value); }

        public LobbySlotContextMenu SlotContextMenu { get; }

        public bool IsProxyInterface => this.NetworkInterface.API.IsHost;

        public int TeamID => this.m_teamModel.Interface.TeamID;

        public LobbySlot(LobbyAPIStructs.LobbySlot teamSlot, LobbyTeam team) {

            // Store reference to network interface
            this.NetworkInterface = teamSlot;

            // Store reference to lobby team
            this.m_teamModel = team;

            // If local machine
            if (this.IsSelf) {
                this.SelectedCompanyIndex = 0;
            } else if (this.IsLocked) {
                this.UpdateSlotFaction(string.Empty);
            }

            // Create context menu
            this.SlotContextMenu = new(this) {
                ShowPlayerCard = new TargettedRelayCommand<LobbySlot>(this, this.m_teamModel.ShowPlayercard),
                KickOccupant = new TargettedRelayCommand<LobbySlot>(this, this.m_teamModel.KickOccupant),
                LockSlot = new RelayCommand(this.Lock),
                UnlockSlot = new RelayCommand(this.Unlock),
                AddAIPlayer = new TargettedRelayCommand<LobbySlot, string>(this, this.m_teamModel.AddAIPlayer)
            };

        }

        private void OnCompanySelectionChanged(LobbyCompanyItem val) {

            // Bail if value is null
            if (val is null) {
                return;
            }

            // Check if self
            if (this.IsSelf || this.IsAIOccupant) {

                // Update selected value
                this.m_selfCompanySelected = val;

                if (val.Army is not null) {

                    // Set slot faction
                    this.UpdateSlotFaction(val.Army.Name);

                }

                if (this.IsAIOccupant) {
                    this.NetworkInterface.API.SetAICompany(this.TeamID, this.NetworkInterface.SlotID, new());
                } else {
                    this.NetworkInterface.API.SetCompany(this.NetworkInterface.Occupant.MemberID, new());
                }

            }

        }

        public void UpdateSlotFaction(string faction)
            => this.UpdateSlotLeftIcon(this.IsSelf ? FactionHoverIcons[faction] : FactionIcons[faction]);

        private void UpdateSlotLeftIcon(ImageSource img) {
            Application.Current.Dispatcher.Invoke(() => {
                this.LeftIcon = img;
                this.PropertyChanged?.Invoke(this, new(nameof(this.LeftIcon)));
            });
        }

        public void RefreshVisuals() {
            Application.Current.Dispatcher.Invoke(() => {
                this.PropertyChanged?.Invoke(this, new(nameof(this.IsSlotVisible)));
                if (this.IsSlotVisible is Visibility.Visible) {
                    this.RefreshCompany();
                    this.PropertyChanged?.Invoke(this, new(nameof(this.LeftIcon)));
                    this.PropertyChanged?.Invoke(this, new(nameof(this.LeftIconVisibility)));
                    this.PropertyChanged?.Invoke(this, new(nameof(this.LeftDisplayString)));
                }
                this.SlotContextMenu.RefreshAvailability();
            });
        }

        public void RefreshCompany() {
            Application.Current.Dispatcher.Invoke(() => {
                this.PropertyChanged?.Invoke(this, new(nameof(this.CompanyVisibility)));
                if (this.CompanyVisibility is Visibility.Visible) {
                    this.PropertyChanged?.Invoke(this, new(nameof(this.SelectedCompanyIndex)));
                    if (this.LeftIcon is null || this.LeftIcon == FactionIcons[string.Empty]) {
                        this.UpdateSlotFaction(this.NetworkInterface.Occupant.Company.Army);
                    }
                }
            });
        }

        private void Lock() {

            // Do async
            Task.Run(() => {

                // Invoke API unlock slot function
                this.NetworkInterface.API.LockSlot(this.TeamID, this.NetworkInterface.SlotID);

                // Refresh
                this.RefreshVisuals();

            });

        }

        private void Unlock() {

            Task.Run(() => {

                // Invoke API unlock slot function
                this.NetworkInterface.API.UnlockSlot(this.TeamID, this.NetworkInterface.SlotID);

                // Refresh
                this.RefreshVisuals();

            });

        }

    }

}
