using System;
using Battlegrounds.Verification;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Steam;

namespace coh2_battlegrounds_console {
    
    class Program {
        
        static void Main(string[] args) {

            // Important this is done
            Battlegrounds.Game.Database.BlueprintManager.CreateDatabase();

            Company company = Company.ReadCompanyFromFile("test_company.json");

            // Create a dummy company
            Company testCompany = new Company(SteamUser.FromID(76561198003529969UL), "26th Rifle Guards Division", Battlegrounds.Game.Gameplay.Faction.Soviet);
            testCompany.AddSquad("conscript_squad_mp", 0, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" }, null, false);
            testCompany.AddSquad("conscript_squad_mp", 2, 0, null, null, false);
            testCompany.AddSquad("conscript_squad_mp", 3, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" }, null, false);
            testCompany.AddSquad("conscript_squad_mp", 3, 0, new string[] { "ppsh-41_sub_machine_gun_upgrade_mp" }, null, false);
            testCompany.AddSquad("t_34_85_squad_mp", 2, 0, null, null, false);
            testCompany.AddSquad("t_34_85_squad_mp", 2, 0, null, null, false);
            testCompany.AddSquad("t_34_85_squad_mp", 3, 0, null, null, false);

            Company[] companies = new Company[] {
                testCompany
            };

            string bCheck = testCompany.GetChecksum();

            Session session = Session.CreateSession("2p_angoville", companies, new Battlegrounds.Game.Gameplay.Wincondition(), true);

            /*GameMatch m = new GameMatch(session);
            m.LoadMatch($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec");
            m.EvaluateResult();
            */
            //SessionManager.PlaySession<SessionCompiler<CompanyCompiler>, CompanyCompiler>(session, (a,b) => { Console.WriteLine(a); }, null);

            // Save json
            testCompany.SaveToFile("test_company.json");

            Console.Read();

        }

    }

}
