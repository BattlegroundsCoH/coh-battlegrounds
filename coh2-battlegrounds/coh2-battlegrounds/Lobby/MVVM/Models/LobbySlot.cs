using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking.Lobby;

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

        public ILobbyTeamSlot NetworkInterface { get; set; }

        public string LeftDisplayString => this.NetworkInterface.SlotState switch {
            LobbyTeamSlotState.OCCUPIED => this.NetworkInterface.SlotOccupant.Name,
            LobbyTeamSlotState.OPEN => "Open",
            LobbyTeamSlotState.LOCKED => "Locked",
            _ => "Invalid Slot State"
        };

        public bool IsSelf => this.NetworkInterface.SlotOccupant?.IsLocalMachine ?? false;

        public bool IsOpen => this.NetworkInterface.SlotState is LobbyTeamSlotState.OPEN;

        public bool IsLocked => this.NetworkInterface.SlotState is LobbyTeamSlotState.LOCKED;

        public Visibility LeftIconVisibility => this.IsOpen ? Visibility.Hidden : Visibility.Visible;

        public Visibility CompanyVisibility => !(this.IsOpen || this.IsLocked) ? Visibility.Visible : Visibility.Hidden;

        public int SelectedCompanyIndex { get; set; }

        public ImageSource LeftIcon { get; set; }

        public LobbyCompanyItem SelectedCompany { get => this.m_selfCompanySelected; set => this.OnCompanySelectionChanged(value); }

        public LobbySlotContextMenu SlotContextMenu { get; }

        public LobbySlot(ILobbyTeamSlot teamSlot) {

            // Store reference to network interface
            this.NetworkInterface = teamSlot;

            // If local machine
            if (this.IsSelf) {
                this.SelectedCompanyIndex = 0;
            } else if (this.IsLocked) {
                this.UpdateSlotFaction(string.Empty);
            }

            // Create context menu
            this.SlotContextMenu = new(this);

        }

        private void OnCompanySelectionChanged(LobbyCompanyItem val) {

            // Bail if value is null
            if (val is null) {
                return;
            }

            // Check if self
            if (this.IsSelf) {

                // Update selected value
                this.m_selfCompanySelected = val;

                // Set slot faction
                this.UpdateSlotFaction(val.Army.Name);

                // TODO: Inform everyone of selection change

            }

        }

        private void UpdateSlotFaction(string faction)
            => this.UpdateSlotLeftIcon(this.IsSelf ? FactionHoverIcons[faction] : FactionIcons[faction]);

        private void UpdateSlotLeftIcon(ImageSource img) {
            Application.Current.Dispatcher.Invoke(() => {
                this.LeftIcon = img;
                this.PropertyChanged?.Invoke(this, new(nameof(this.LeftIcon)));
            });
        }

    }

}
