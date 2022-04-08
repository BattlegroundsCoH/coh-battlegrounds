using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Compiler;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyParticipantModel : LobbyModel {

    private bool m_hasDownloadedGamemode = false;

    public override LobbyButton StartMatchButton { get; }

    public override LobbyDropdown<Scenario> MapDropdown { get; }

    public override LobbyDropdown<IGamemode> GamemodeDropdown { get; }

    public override LobbyDropdown<IGamemodeOption> GamemodeOptionDropdown { get; }

    public override LobbyDropdown<OnOffOption> WeatherDropdown { get; }

    public override LobbyDropdown<OnOffOption> SupplySystemDropdown { get; }

    public override LobbyDropdown<ModPackage> ModPackageDropdown { get; }

    public LobbyParticipantModel(LobbyAPI handle, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) : base(handle, allies, axis) {

        // Define start match buttnn
        this.StartMatchButton = new(false, new(() => { }), Visibility.Hidden);

        // Init dropdowns 
        this.MapDropdown = new(false, Visibility.Hidden, new(), x => { });
        this.GamemodeDropdown = new(false, Visibility.Hidden, new(), x => { });
        this.GamemodeOptionDropdown = new(false, Visibility.Hidden, new(), x => { });
        this.WeatherDropdown = new(false, Visibility.Hidden, new(), x => { });
        this.SupplySystemDropdown = new(false, Visibility.Hidden, new(), x => { });
        this.ModPackageDropdown = new(false, Visibility.Hidden, new(), x => { });

    }

    private void OnMatchStartupCancelled(LobbyMatchStartupCancelledEventArgs e) {

        // Inform user
        if (this.m_chatModel is not null) {
            this.m_chatModel.SystemMessage($"{e.CancelName} has cancelled the match.", Colors.Gray);
        }

        // Invoke on GUI
        Application.Current.Dispatcher.Invoke(() => {

            // Allow exit lobby
            //this.ExitLobby.Enabled = true;

        });

    }

    private void OnMatchBegin() {

        // Inform user
        if (this.m_chatModel is not null) {
            this.m_chatModel.SystemMessage($"The match is starting", Colors.Gray);
        }

        // Invoke on GUI
        Application.Current.Dispatcher.Invoke(() => {

            // Allow exit lobby
            //this.ExitLobby.Enabled = false;

        });

    }

    private void OnLaunchGame() {

        // Create overwatch strategy
        var overwatch = new MemberOverwatchStrategy();

        // Run task
        Task.Run(async () => {

            // Get time
            DateTime time = DateTime.Now;

            // Wait for gamemode
            while (!this.m_hasDownloadedGamemode) {
                if ((DateTime.Now - time).TotalSeconds > 15.0) {
                    if (this.m_chatModel is not null) {
                        this.m_chatModel.SystemMessage($"Failed to download gamemode file!", Colors.Gray);
                    }
                    // TODO: Report to host
                    return;
                }
                await Task.Delay(100);
            }

            // Inform user
            if (this.m_chatModel is not null) {
                this.m_chatModel.SystemMessage($"Laucnhing game", Colors.Gray);
            }

            // Begin
            overwatch.Launch();

            // Wait for exit
            overwatch.WaitForExit();

            // set received to false (may need to do some checksum stuff here so clients can reconnect if needed)
            this.m_hasDownloadedGamemode = false;

            // TODO: Check for bugsplats etc. and report accordingly

        });

    }

    private void OnResultsReleased(ServerAPI obj) {

        // Instruct download
        Task.Run(() => {
            obj.DownloadCompany(this.m_handle.Self.ID, (status, data) => {

                // Check status
                if (status is DownloadResult.DOWNLOAD_SUCCESS) {

                    // Load it
                    var company = CompanySerializer.GetCompanyFromJson(Encoding.UTF8.GetString(data));

                    // Save it
                    PlayerCompanies.SaveCompany(company);

                } else {
                    Trace.WriteLine($"Failed to download company results!", nameof(LobbyHostModel));
                }


                // Invoke on GUI - now allow to leave
                Application.Current.Dispatcher.Invoke(() => {

                    // Allow exit lobby
                    //this.ExitLobby.Enabled = true;

                });


            });
        });

    }

    private void OnGamemodeReleased(ServerAPI obj) {

        // Start background thread
        Task.Run(() => {

            // Log download operation
            Trace.WriteLine("Starting download of gamemode.", nameof(LobbyHostModel));

            // Download
            obj.DownloadGamemode((status, data) => {

                if (status is DownloadResult.DOWNLOAD_SUCCESS) {

                    // File sga to gamemode file
                    File.WriteAllBytes(WinconditionCompiler.GetArchivePath(), data);

                    // Set as true
                    this.m_hasDownloadedGamemode = true;

                    // Inform user
                    if (this.m_chatModel is not null) {
                        this.m_chatModel.SystemMessage($"Gamemode downloaded", Colors.Gray);
                    }

                } else {

                    // Inform user
                    if (this.m_chatModel is not null) {
                        this.m_chatModel.SystemMessage($"Failed to download gamemode", Colors.DarkRed);
                    }

                    Trace.WriteLine($"Failed to download gamemode! (E = {status})", nameof(LobbyHostModel));

                }

            });

        });

    }

    private void OnCompanyRequested(ServerAPI obj) {

        // Log request
        Trace.WriteLine("Received request to upload company file", nameof(LobbyHostModel));

        // Get self
        ulong selfid = this.m_handle.Self.ID;
        var self = this.m_handle.Allies.GetSlotOfMember(selfid) ?? this.m_handle.Axis.GetSlotOfMember(selfid);
        if (self is not null && self.Occupant is not null) {

            // Make sure there's a company
            if (self.Occupant.Company is null) {
                return;
            }

            // Get company name
            string companyName = self.Occupant.Company.Name;

            // Get company faction
            Faction faction = Faction.FromName(self.Occupant.Company.Army);

            // Get company
            var company = PlayerCompanies.FromNameAndFaction(companyName, faction);
            if (company is null) {
                Trace.WriteLine($"Failed to upload company json file (Company '{companyName}' not found).", nameof(LobbyHostModel));
                return;
            }

            // Get company json
            string companyJson = CompanySerializer.GetCompanyAsJson(company, indent: false);
            if (string.IsNullOrEmpty(companyJson)) {
                Trace.WriteLine($"Failed to upload company json file (Company '{companyName}' not found).", nameof(LobbyHostModel));
                return;
            }

            // Upload file
            if (obj.UploadCompany(selfid, companyJson, (a, b) => Trace.WriteLine($"Upload company progress {a}/{b}", nameof(LobbyHostModel))) is not UploadResult.UPLOAD_SUCCESS) {
                Trace.WriteLine("Failed to upload company json file.", nameof(LobbyHostModel));
            }

        } else {

            // Log request
            Trace.WriteLine("Failed to find self-instance and cannot upload company file.", nameof(LobbyHostModel));

        }

    }

}
