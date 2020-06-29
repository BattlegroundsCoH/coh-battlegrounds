using System;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Steam;

namespace coh2_battlegrounds_console {
    
    class Program {
        
        static void Main(string[] args) {

            Battlegrounds.Game.Database.BlueprintManager.CreateDatabase();

            string latest = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec";

            // Test this
            //WinconditionCompiler.CompileToSga("temp_build", "session.scar");

            Company testCompany = new Company(SteamUser.FromID(76561198003529969UL), "26th Rifle Guards Division", Battlegrounds.Game.Gameplay.Faction.Soviet);
            testCompany.AddSquad("conscript_squad_mp", 0, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" }, null, false);
            testCompany.AddSquad("conscript_squad_mp", 2, 0, null, null, false);
            testCompany.AddSquad("conscript_squad_mp", 3, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" }, null, false);
            testCompany.AddSquad("t_34_85_squad_mp", 2, 0, null, null, false);
            testCompany.AddSquad("t_34_85_squad_mp", 2, 0, null, null, false);
            testCompany.AddSquad("t_34_85_squad_mp", 3, 0, null, null, false);

            Company[] companies = new Company[] {
                testCompany
            };

            Session session = Session.CreateSession("", companies, new Battlegrounds.Game.Gameplay.Wincondition(), true);

            /*GameMatch match = new GameMatch(session);
            if (match.LoadMatch($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec")) {
                match.EvaluateResult();
            } else {
                Console.WriteLine("Failed to load replayfile...");
            }*/

            SessionManager.PlaySession<SessionCompiler<CompanyCompiler>, CompanyCompiler>(session, (a,b) => { Console.WriteLine(a); }, null);

            //Battlegrounds.Game.CoH2Launcher.Launch();

            Console.Read();

        }

    }

}
