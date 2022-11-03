using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

using Battlegrounds.AI;
using Battlegrounds.DataLocal;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Networking.LobbySystem.Json;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Lobby.Helpers;
using Battlegrounds.Lobby.Lookups;

namespace Battlegrounds.Lobby.Components;

public abstract class PlayerSlot : INotifyPropertyChanged {

    protected static readonly Func<List<Company>> AlliedCompanies = () => Companies.FindAll(x => x.Army.IsAllied);
    protected static readonly Func<List<Company>> AxisCompanies = () => Companies.FindAll(x => x.Army.IsAxis);

    private static readonly Func<string> LOCSTR_SLOT_OPEN = () => BattlegroundsInstance.Localize.GetString("TeamPlayerCard_Open_Slot");
    private static readonly Func<string> LOCSTR_SLOT_LOCKED = () => BattlegroundsInstance.Localize.GetString("TeamPlayerCard_Locked_Slot");
    private static readonly Func<string> LOCSTR_SLOT_JOINING = () => BattlegroundsInstance.Localize.GetString("TeamPlayerCard_Joining_Slot");

    protected static readonly ILobbyCompany DummyCompany = new JsonLobbyCompany(false, false, "None", "soviet", 0.0f, "");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected ILobbySlot m_slot;

    private int m_selectedCompany;

    public ILobbyHandle Handle { get; }

    public Visibility IsSlotVisible => this.Slot.State == 3 ? Visibility.Collapsed : Visibility.Visible;

    public ImageSource? LeftIcon { get; set; }

    public ImageSource? LeftIconHover { get; set; }

    public Team Team { get; }

    public ILobbySlot Slot => this.m_slot;

    public ObservableCollection<ILobbyCompany> SelectableCompanies { get; }

    public ILobbyCompany SelectedCompany
        => this.SelectedCompanyIndex >= 0 ? this.SelectableCompanies[this.SelectedCompanyIndex] : DummyCompany;

    public abstract SlotContextMenu ContextMenu { get; }

    public bool IsSelf => this.Slot.IsSelf();

    public bool IsSlotMouseOver { get; set; }

    public string LeftDisplayString => this.Slot.State switch {
        0 => LOCSTR_SLOT_OPEN(),
        1 => this.Slot.IsOccupied ? GetOccupantName(this.Slot.Occupant) : "ERROR",
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

    public PlayerSlot(ILobbySlot teamSlot, Team team) {

        // Set values
        this.Team = team;
        this.Handle = teamSlot.Handle;
        this.m_slot = teamSlot;

        // Check if self or AI, then set contents
        this.SelectableCompanies = new() { teamSlot.Occupant is null ? DummyCompany : teamSlot.Occupant.Company ?? DummyCompany };

        // Do initial view
        this.RefreshCompanyInfo();

        // Subscribe to slot updates
        this.Handle.OnLobbySlotUpdate += this.OnLobbySlotUpdate;

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
            this.SetLeftDisplay(VisualsLookup.FactionIcons[this.SelectedCompany.Army], VisualsLookup.FactionHoverIcons[this.SelectedCompany.Army]);

        } else if (this.m_slot.State == 0) {

            // Hide
            this.SetLeftDisplay(null, null);

        } else {

            // Show lock
            this.SetLeftDisplay(VisualsLookup.FactionIcons[string.Empty], VisualsLookup.FactionHoverIcons[string.Empty]);

        }

    }

    protected ILobbyCompany FromCompany(Company x) {
        var company = new JsonLobbyCompany(false, false, x.Name, x.Army.Name, (float)x.Rating, x.Type.ToString());
        company.SetHandle(this.Handle);
        return company;
    }

    public void OnLobbySlotUpdate(ILobbySlot args) {
        Application.Current.Dispatcher.Invoke(() => {
            if (args.TeamID == this.Team.Model.TeamID && args.SlotID == this.m_slot.SlotID) {

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

    protected static string GetOccupantName(ILobbyMember mem) => mem.Role switch {
        3 => BattlegroundsInstance.Localize.GetEnum((AIDifficulty)mem.AILevel),
        _ => mem.State is LobbyMemberState.Joining ? LOCSTR_SLOT_JOINING() : mem.DisplayName
    };

    protected abstract void OnLobbyCompanyChanged(int newValue);

    public abstract void OnLobbyCompanyChanged(ILobbyCompany company);

    protected void NotifyProperty(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);

}
