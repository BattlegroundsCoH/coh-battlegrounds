using System;
using System.Text.Json;
using System.Threading;

using Battlegrounds;
using Battlegrounds.Modding;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Database.Management;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Battlegrounds.Game.Database.Extensions.VeterancyExtension;
using System.Numerics;
using System.Timers;

namespace coh2_battlegrounds_bin_tests { // NOTE: This test file tests serialisation and deserialisation
    // But the primary goal is to test that no checksum violations occur.

    [TestClass]
    public class ChecksumTest {

        ModPackage package;

        [TestInitialize]
        public void Setup() {

            // Tell BG to load itself
            if (DatabaseManager.DatabaseLoaded is false) {
                bool canContinue = false;
                Environment.CurrentDirectory = @"E:\coh2_battlegrounds\coh2-battlegrounds\coh2-battlegrounds\bin\Debug\net5.0-windows";
                BattlegroundsInstance.LoadInstance();
                DatabaseManager.LoadAllDatabases((a, b) => canContinue = true);
                while (!canContinue) {
                    Thread.Sleep(1);
                }
            }

            // Get package
            package = ModManager.GetPackage("mod_bg");

        }

        [TestMethod]
        public void NewSquadIsSerialised() {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg").Build(0);
            Assert.IsNotNull(squad);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(squad);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);

        }

        [TestMethod]
        public void VeteranSquadIsSerialised() {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg").SetVeterancyRank(1).Build(0);
            Assert.IsNotNull(squad);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(squad);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);
            Assert.AreEqual(squad.VeterancyRank, deserialised.VeterancyRank);

        }

        [TestMethod]
        [DataRow(200.0f)]
        [DataRow(200.5f)]
        [DataRow(800.0f)]
        [DataRow(9000.0f)]
        [DataRow(1700.0f)]
        [DataRow(58.789f)]
        [DataRow(7890.11f)]
        public void VeteranWithExperienceSquadIsSerialised(float exp) {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg")
                .SetVeterancyRank(1).SetVeterancyExperience(exp).Build(0);
            Assert.IsNotNull(squad);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(squad);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);
            Assert.AreEqual(squad.VeterancyRank, deserialised.VeterancyRank);
            Assert.AreEqual(squad.VeterancyProgress, deserialised.VeterancyProgress);

        }

        [TestMethod]
        [DataRow(DeploymentPhase.PhaseNone)]
        [DataRow(DeploymentPhase.PhaseInitial)]
        [DataRow(DeploymentPhase.PhaseA)]
        [DataRow(DeploymentPhase.PhaseB)]
        [DataRow(DeploymentPhase.PhaseC)]
        public void SquadIsSerialisedWithDeploymentPhases(DeploymentPhase phase) {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg").SetVeterancyRank(1)
                .SetDeploymentPhase(phase).Build(0);
            Assert.IsNotNull(squad);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(squad);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);
            Assert.AreEqual(squad.VeterancyRank, deserialised.VeterancyRank);
            Assert.AreEqual(squad.DeploymentPhase, deserialised.DeploymentPhase);

        }

        [TestMethod]
        [DataRow("", DeploymentMethod.None)]
        [DataRow("zis_6_transport_truck_bg", DeploymentMethod.DeployAndExit)]
        [DataRow("zis_6_transport_truck_bg", DeploymentMethod.DeployAndStay)]
        [DataRow("zis_6_transport_truck_bg", DeploymentMethod.Glider)]
        [DataRow("zis_6_transport_truck_bg", DeploymentMethod.Paradrop)]
        public void SquadIsSerialisedWithDeploymentModes(string support, DeploymentMethod mode) {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg").SetVeterancyRank(1)
                .SetDeploymentPhase(DeploymentPhase.PhaseInitial).SetTransportBlueprint(support).SetDeploymentMethod(mode).Build(0);
            Assert.IsNotNull(squad);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(squad);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);
            Assert.AreEqual(squad.VeterancyRank, deserialised.VeterancyRank);
            Assert.AreEqual(squad.DeploymentPhase, deserialised.DeploymentPhase);
            Assert.AreEqual(squad.DeploymentMethod, deserialised.DeploymentMethod);
            Assert.AreEqual(squad.SupportBlueprint, deserialised.SupportBlueprint);

        }

        [TestMethod]
        [DataRow(11.0)]
        [DataRow(18.9)]
        [DataRow(7899.4)]
        [DataRow(290.008)]
        [DataRow(74999.0)]
        public void SquadIsSerialisedWithCombatTime(double minutes) {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg")
                .SetVeterancyRank(1).SetCombatTime(TimeSpan.FromMinutes(minutes)).Build(0);
            Assert.IsNotNull(squad);
            Assert.AreEqual(TimeSpan.FromMinutes(minutes), squad.CombatTime);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(squad);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);
            Assert.AreEqual(squad.VeterancyRank, deserialised.VeterancyRank);
            Assert.AreEqual(squad.CombatTime, deserialised.CombatTime);

        }

        [TestMethod]
        [DataRow("conscript_squad_bg", "", DeploymentMethod.None, DeploymentPhase.PhaseInitial, 0, 0, 0)]
        [DataRow("conscript_squad_bg", "", DeploymentMethod.None, DeploymentPhase.PhaseB, 0, 899, 0)]
        [DataRow("shock_troops_bg", "", DeploymentMethod.None, DeploymentPhase.PhaseInitial, 0, 0, 0)]
        [DataRow("conscript_squad_bg", "zis_6_transport_truck_bg", DeploymentMethod.DeployAndStay, DeploymentPhase.PhaseInitial, 5, 0, 789.0)]
        [DataRow("conscript_squad_bg", "zis_6_transport_truck_bg", DeploymentMethod.DeployAndExit, DeploymentPhase.PhaseC, 3, 0, 0)]
        [DataRow("shock_troops_bg", "", DeploymentMethod.None, DeploymentPhase.PhaseInitial, 1, 0, 0)]
        [DataRow("conscript_squad_bg", "", DeploymentMethod.None, DeploymentPhase.PhaseA, 2, 275.9f, 2789.0)]
        [DataRow("conscript_squad_bg", "", DeploymentMethod.None, DeploymentPhase.PhaseA, 4, 0, 0)]
        public void RandomSquad(string bp, string sbp, DeploymentMethod method, DeploymentPhase phase, int rank, double xp, double time) {

            // Create squad
            var squad = new UnitBuilder().SetModGUID(package.TuningGUID).SetBlueprint(bp)
                .SetVeterancyRank((byte)rank).SetVeterancyExperience((float)xp).SetDeploymentPhase(phase)
                .SetTransportBlueprint(sbp).SetDeploymentMethod(method).SetCombatTime(TimeSpan.FromMinutes(time)).Build(0);
            Assert.IsNotNull(squad);

            // Serialise
            string js = JsonSerializer.Serialize(squad);
            Assert.IsNotNull(js);

            // Deserialise
            var deserialised = JsonSerializer.Deserialize<Squad>(js);
            Assert.IsNotNull(deserialised);
            Assert.AreEqual(squad.SquadID, deserialised.SquadID);
            Assert.AreEqual(squad.Blueprint, deserialised.Blueprint);
            Assert.AreEqual(squad.VeterancyRank, deserialised.VeterancyRank);
            Assert.AreEqual(squad.VeterancyProgress, deserialised.VeterancyProgress);
            Assert.AreEqual(squad.SupportBlueprint, deserialised.SupportBlueprint);
            Assert.AreEqual(squad.DeploymentMethod, deserialised.DeploymentMethod);
            Assert.AreEqual(squad.DeploymentPhase, deserialised.DeploymentPhase);
            Assert.AreEqual(squad.CombatTime, deserialised.CombatTime);

        }

        [TestMethod]
        public void CanSaveEmptyCompany() {

            // Create company
            Company company = new(Faction.Soviet);
            company.Name = "Test";
            company.CalculateChecksum();

            // Get the checksum
            string chksum = company.Checksum;

            // Serialise
            string js = JsonSerializer.Serialize(company);
            Assert.IsNotNull(js);

            // Deserialise and verify
            Company deserialised = JsonSerializer.Deserialize<Company>(js);
            Assert.AreEqual(chksum, deserialised.Checksum);

        }

        [TestMethod]
        public void CanSaveFullyPopulatedSovietCompany() {

            // Create unit builder
            UnitBuilder ub = new();

            // Create company
            Company company = new(Faction.Soviet);
            company.Name = "Test";
            for (int i = 0; i < Company.MAX_SIZE; i++) {
                company.AddSquad(ub.SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg").GetAndReset());
            }
            company.CalculateChecksum();

            // Get the checksum
            string chksum = company.Checksum;

            // Serialise
            string js = JsonSerializer.Serialize(company);
            Assert.IsNotNull(js);

            // Deserialise and verify
            Company deserialised = JsonSerializer.Deserialize<Company>(js);
            Assert.AreEqual(chksum, deserialised.Checksum);

        }

        [TestMethod]
        public void CanSavePopulatedSovietCompanyWithDeployPhases() {

            // Create unit builder
            UnitBuilder ub = new();

            // Create company
            Company company = new(Faction.Soviet);
            company.Name = "Test";
            for (int i = 0; i < Company.MAX_INITIAL; i++) {
                company.AddSquad(ub.SetModGUID(package.TuningGUID).SetBlueprint("conscript_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            }
            for (int i = 0; i < Company.MAX_SIZE - Company.MAX_INITIAL; i++) {
                company.AddSquad(ub.SetModGUID(package.TuningGUID).SetBlueprint("guards_troops_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            }
            company.CalculateChecksum();

            // Get the checksum
            string chksum = company.Checksum;

            // Serialise
            string js = JsonSerializer.Serialize(company);
            Assert.IsNotNull(js);

            // Deserialise and verify
            Company deserialised = JsonSerializer.Deserialize<Company>(js);
            Assert.AreEqual(chksum, deserialised.Checksum);

        }

        [TestMethod]
        public void CanSaveRandomGeneratedCompanies() {

            // Create unit builder
            UnitBuilder ub = new();
            Random random = new();

            string[] unitPool = { "conscript_squad_bg", "guards_troops_bg", "shock_troops_bg", "pm-82_41_mortar_squad_bg" };
            DeploymentPhase[] phases = Enum.GetValues(typeof(DeploymentPhase)) as DeploymentPhase[];

            for (int i = 0; i < 100; i++) {

                // Create company
                Company company = new(Faction.Soviet);
                company.Name = "Test";
                company.TuningGUID = ModGuid.FromGuid("142b113740474c82a60b0a428bd553d5");
                company.UpdateStatistics(x => new() { 
                    TotalInfantryLosses = (ulong)random.Next(0, ushort.MaxValue),
                    TotalMatchCount = (ulong)random.Next(0, 200),
                    TotalVehicleLosses = (ulong)random.Next(0, ushort.MaxValue),
                    TotalMatchWinCount = (ulong)random.Next(0, 100),
                    TotalMatchLossCount = (ulong)random.Next(0, 100),
                });
                for (int j = 0; j < Company.MAX_SIZE; j++) {
                    int bp = random.Next(0, unitPool.Length);
                    int ph = random.Next(0, phases.Length);
                    TimeSpan tm = TimeSpan.FromMinutes(120.0 * random.NextDouble());
                    company.AddSquad(ub.SetModGUID(package.TuningGUID).SetBlueprint(unitPool[bp]).SetDeploymentPhase(phases[ph])
                        .SetCombatTime(tm).GetAndReset());
                }
                company.CalculateChecksum();

                // Get the checksum
                string chksum = company.Checksum;

                // Serialise
                string js = JsonSerializer.Serialize(company);
                Assert.IsNotNull(js);

                // Deserialise and verify
                Company deserialised = JsonSerializer.Deserialize<Company>(js);
                Assert.AreEqual(chksum, deserialised.Checksum);

            }

        }

    }

}
