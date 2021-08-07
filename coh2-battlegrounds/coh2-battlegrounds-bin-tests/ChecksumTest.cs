using System;
using System.Text.Json;
using System.Threading;

using Battlegrounds;
using Battlegrounds.Modding;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Database.Management;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {

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

    }

}
