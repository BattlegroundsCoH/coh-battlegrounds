using System;
using System.IO;
using System.Linq;
using System.Threading;
using Battlegrounds;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Online;
using Battlegrounds.Online.Services;
using Battlegrounds.Steam;

namespace coh2_battlegrounds_console {
    
    class Program {
        
        static void Main(string[] args) {

            SteamUser loc = SteamUser.FromLocalInstall();

            BattlegroundsInstance.LoadInstance();
            BattlegroundsInstance.LocalSteamuser = SteamUser.FromID(76561198003529969UL);

            // Important this is done
            DatabaseManager.LoadAllDatabases(null);

            while (!DatabaseManager.DatabaseLoaded) {
                Thread.Sleep(1);
            }

            // Create a dummy company
            Company testCompany = new Company(BattlegroundsInstance.LocalSteamuser, "26th Rifle Division", Faction.Soviet);
            testCompany.AddSquad("conscript_squad_mp", 0, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" });
            testCompany.AddSquad("conscript_squad_mp", 2, 0);
            testCompany.AddSquad("conscript_squad_mp", 0, 120.0f);
            testCompany.AddSquad("conscript_squad_mp", 3, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" });
            testCompany.AddSquad("conscript_squad_mp", 3, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" });
            testCompany.AddSquad("t_34_85_squad_mp", 2, 0);
            testCompany.AddSquad("t_34_85_squad_mp", 2, 0);
            testCompany.AddSquad("t_34_85_squad_mp", 3, 0);
            testCompany.AddSquad("t_34_85_squad_mp", 3, 0);

            Company[] companies = new Company[] {
                testCompany
            };

            SessionInfo sessionInfo = new SessionInfo() {
                SelectedGamemode = WinconditionList.GetWinconditionByName("Victory Points"),
                SelectedGamemodeOption = 1,
                SelectedScenario = ScenarioList.FromRelativePath("2p_angoville"),
                SelectedTuningMod = new BattlegroundsTuning(),
                Allies = new string[] { "CoDiEx" },
                Axis = new string[] {},
                FillAI = true,
            };

            Session session = Session.CreateSession(sessionInfo, companies);

            /*GameMatch m = new GameMatch(session);
            m.LoadMatch($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec");
            m.EvaluateResult();
            */
            
            SessionManager.PlaySession<SessionCompiler<CompanyCompiler>, CompanyCompiler>(session, (a,b) => { Console.WriteLine(a); }, null, null);

            // Save json
            testCompany.SaveToFile("test_company.json");

            /*
            LobbyHub hub = new LobbyHub();
            if (!hub.CanConnect()) {
                Console.WriteLine("Unable to reach server hub");
            } else {

                var lobbies = hub.GetConnectableLobbies();
                if (lobbies.Count == 0) {
                    hub.User = BattlegroundsInstance.LocalSteamuser;
                    HostTest(hub);
                } else {
                    hub.User = SteamUser.FromID(76561198157626935UL);
                    ClientTest(hub, lobbies.First());
                }

            }
            */
            BattlegroundsInstance.SaveInstance();
            
            Console.ReadLine();

        }

        private static void HostTest(LobbyHub hub) {

            Console.WriteLine("Running hosting test");

            ManagedLobby.Host(hub, "Battlegrounds Test", string.Empty, OnMessageLoop);

        }

        private static void ClientTest(LobbyHub hub, ConnectableLobby lob) {

            Console.WriteLine("Running client test");

            ManagedLobby.Join(hub, lob, string.Empty, OnMessageLoop);

        }

        private static void OnMessageLoop(ManagedLobbyStatus status, ManagedLobby result) {

            static void OnCompanyFileReceived(string from, string name, bool received, byte[] content) {
                Console.WriteLine("Received company data");
                if (received) {
                    File.WriteAllBytes("hello.json", content);
                }
            }

            if (status.Success) {

                Console.WriteLine("Connection was established!");

                result.OnPlayerEvent += (a, b, c) => {
                    if (a == ManagedLobbyPlayerEventType.Message) {
                        Console.WriteLine($"{b}: {c}");
                        Console.WriteLine("Testing launch feature");
                        result.CompileAndStartMatch(x => Console.WriteLine(x));
                        //result.GetCompanyFileFrom(b, OnCompanyFileReceived);
                    } else {
                        string word = (a == ManagedLobbyPlayerEventType.Leave) ? "Left" : (a == ManagedLobbyPlayerEventType.Kicked ? "Was kicked" : "Joined");
                        Console.WriteLine($"{b} {word}");
                    }
                };

                result.OnLocalDataRequested += (a) => {
                    if (a.CompareTo("CompanyData") == 0) {
                        return Company.ReadCompanyFromFile("test_company.json");
                    } else if (a.CompareTo("Gamemode") == 0) {
                        return WinconditionList.GetWinconditionByName("Victory Points");
                    } else {
                        return null;
                    }
                };

                result.OnDataRequest += (a, b, c, d) => {
                    if (c.CompareTo("CompanyData") == 0) {
                        Console.WriteLine("Received request for company data using identifier " + d);
                        result.SendFile(b, "test_company.json", d);
                    } 
                };

                if (!result.IsHost) {
                    result.SendChatMessage("Hello World");
                }

            } else {
                Console.WriteLine(status.Message);
            }


        }

    }

}
