using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.Roles.Host;

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

        public ILobbyTeamSlot NetworkInterface { get; set; }

        public string LeftDisplayString => this.NetworkInterface.SlotState switch {
            TeamSlotState.Open => "Open",
            TeamSlotState.Locked => "Locked",
            TeamSlotState.Occupied => this.NetworkInterface.SlotOccupant.Name,
            TeamSlotState.Disabled => "Disabled",
            _ => throw new NotImplementedException()
        };

        public bool IsSelf => this.NetworkInterface.SlotOccupant?.IsSelf ?? false;

        public bool IsOpen => this.NetworkInterface.SlotState is TeamSlotState.Open;

        public bool IsLocked => this.NetworkInterface.SlotState is TeamSlotState.Locked;

        public bool IsDisabled => this.NetworkInterface.SlotState is TeamSlotState.Disabled;

        public bool IsAIOccupant => this.NetworkInterface.SlotOccupant is ILobbyAIParticipant;

        public Visibility IsSlotVisible => this.IsDisabled ? Visibility.Hidden : Visibility.Visible;

        public Visibility LeftIconVisibility => this.IsOpen ? Visibility.Hidden : Visibility.Visible;

        public Visibility CompanyVisibility => !(this.IsOpen || this.IsLocked) ? Visibility.Visible : Visibility.Hidden;

        public int SelectedCompanyIndex { get; set; }

        public ImageSource LeftIcon { get; set; }

        public LobbyCompanyItem SelectedCompany { get => this.m_selfCompanySelected; set => this.OnCompanySelectionChanged(value); }

        public LobbySlotContextMenu SlotContextMenu { get; }

        public bool IsProxyInterface => this.NetworkInterface is not HostedLobbyTeamSlot;

        public LobbySlot(ILobbyTeamSlot teamSlot, LobbyTeam team) {

            // Store reference to network interface
            this.NetworkInterface = teamSlot;
            this.NetworkInterface.ValueChanged += this.NetworkInterface_ValueChanged;

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

        private void NetworkInterface_ValueChanged(ILobbyTeamSlot sender, ObservableValueChangedEventArgs e) {
            if (e.Property == nameof(ILobbyTeamSlot.SlotState)) {
                this.RefreshVisuals();
            } else if (e.Property == nameof(ILobbyParticipant.Company)) {
                this.RefreshCompany();
            }
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

                // Set slot faction
                this.UpdateSlotFaction(val.Army.Name);

                // Update slot company
                if (!this.NetworkInterface.SlotOccupant.SetCompany(val)) {
                    Trace.WriteLine("Failed to update local company", nameof(LobbySlot));
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
                        this.UpdateSlotFaction(this.NetworkInterface.SlotOccupant.Company.Faction);
                    }
                }
            });
        }

        private void Lock() {

            if (this.NetworkInterface.SlotState is TeamSlotState.Disabled) {
                return;
            }

            if (!this.NetworkInterface.SetState(TeamSlotState.Locked)) {
                // Meh?
            }

        }

        private void Unlock() {

            if (this.NetworkInterface.SlotState is TeamSlotState.Disabled) {
                return;
            }

            if (!this.NetworkInterface.SetState(TeamSlotState.Open)) {
                // Meh?
            }

        }

    }

}
