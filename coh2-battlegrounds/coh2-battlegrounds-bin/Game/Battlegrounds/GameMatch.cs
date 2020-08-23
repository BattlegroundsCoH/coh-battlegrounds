using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Data;

namespace Battlegrounds.Game.Battlegrounds {

    /// <summary>
    /// Representation of a match wherein the results of the match can be evaluated and verified within the <see cref="Session"/> the match was played.
    /// </summary>
    public class GameMatch {

        private ReplayFile m_matchRecord;
        private Session m_gameSession;
        private PlayerResult[] m_matchPlayerResults;
        private bool m_hasVictor;

        /// <summary>
        /// 
        /// </summary>
        public PlayerResult[] Players => this.m_matchPlayerResults;

        /// <summary>
        /// Does this match have a player or a team that has won?
        /// </summary>
        public bool HasVictor => m_hasVictor;

        /// <summary>
        /// Was the <see cref="GameMatch"/> played with the session?
        /// </summary>
        public bool PlayedWithSession { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameMatch(Session session) {
            this.m_matchPlayerResults = new PlayerResult[0];
            this.m_gameSession = session;
            this.m_hasVictor = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="replayFilepath"></param>
        /// <returns></returns>
        public bool LoadMatch(string replayFilepath) {

            // Create instance
            this.m_matchRecord = new ReplayFile(replayFilepath);

            // Load the replay
            if (!this.m_matchRecord.LoadReplay()) {
                return false;
            }

            // Return true
            return true;

        }

        /// <summary>
        /// Evaluate the result of the match based on the replay file
        /// </summary>
        public virtual void EvaluateResult() {

            // Get the players
            Player[] players = this.m_matchRecord.Players;

            // Setup variables
            this.m_matchPlayerResults = new PlayerResult[players.Length];

            // Run Setup results
            for (int i = 0; i < this.m_matchPlayerResults.Length; i++) {
                this.m_matchPlayerResults[i] = new PlayerResult(players[i]) {
                    IsOnWinningTeam = false
                };
            }

            // Create containers for storing all spawned entities and squads
            HashSet<Squad> allSquads = new HashSet<Squad>();
            HashSet<Entity> allEntities = new HashSet<Entity>();

            // Get the tick array
            var ticks = this.m_matchRecord.Ticks;

            // Run through the array
            for (int i = 0; i < ticks.Length; i++) {

                // If there's something to work with
                if (ticks[i].Events.Count > 0) {

                    // Loop through all the gave events in tick
                    foreach (GameEvent e in ticks[i].Events) {

                        // It is an event we can work with?
                        if (e.Type < (byte)GameEventType.EVENT_MAX) {

                            // Is it a broadcast message?
                            if (e.EventType == GameEventType.PCMD_BroadcastMessage) {
                                this.ParseBroadcastMessage(e.AttachedMessage, this.m_matchPlayerResults.FirstOrDefault(x => x.Player.ID == e.PlayerID), allSquads);
                            }

                        }

                    }

                }

            }

            // Update all companies
            this.UpdateCompanies();

        }

        private Squad FindFirstSquad(PlayerResult player, HashSet<Squad> allsquads, ushort squadID)
            => allsquads.FirstOrDefault(x => x.SquadID == squadID && x.PlayerOwner == player.Player);

        private void ParseBroadcastMessage(string msg, PlayerResult player, HashSet<Squad> allsquads) {

            if (msg.Length > 0) {

                Match messageMatchResult = Regex.Match(msg, @"(?<cmdtype>\w)\[(?<content>(?<msg>(\w|_|-|:|\.|\d)+)|,|\s)*\]");

                if (messageMatchResult.Success) {

                    char msgtype = char.ToUpper(messageMatchResult.Groups["cmdtype"].Value[0]); // Always bump it to upper (incase it's forgotten in Scar script)
                    string[] values = messageMatchResult.Groups["content"].Captures.ToList().Where(x => x.Value != "," && x.Value != " ").Select(x => x.Value).ToArray();

                    if (msgtype == 'D') {

                        Squad squad = new Squad(ushort.Parse(values[0]), player.Player, null);
                        allsquads.Add(squad);
                        player.AddSquad(squad);

                        Console.WriteLine(player.Player.Name + " deployed " + squad.SquadID);

                    } else if (msgtype == 'K') {

                        ushort squadID = ushort.Parse(values[0]);
                        Squad squad = FindFirstSquad(player, allsquads, squadID);

                        if (squad != null) {

                            allsquads.Remove(squad);
                            player.RemoveSquad(squad);

                            Console.WriteLine(player.Player.Name + " lost " + squad.SquadID);

                        }

                    } else if (msgtype == 'V') { // Victory marker
                        this.m_hasVictor = true;
                        if (int.Parse(values[0]) - 1 == player.Player.ID) {
                            player.IsOnWinningTeam = true;
                            Console.WriteLine(player.Player.Name + " was on the winning team.");
                        }
                    } else if (msgtype == 'R') {

                        ushort squadID = ushort.Parse(values[0]);

                        if (byte.TryParse(values[1], out byte vetChange)) {

                            float vetExp = float.Parse(values[2]);

                            Squad squad = FindFirstSquad(player, allsquads, squadID);
                            if (squad != null) {
                                Console.WriteLine(squadID + " was withdrawn (or survived to end of match) by " + player.Player.Name);
                                if (vetChange > 0) {
                                    squad.SetVeterancy((byte)(squad.VeterancyRank + vetChange), vetExp);
                                    Console.WriteLine(squadID + " increased veterancy rank by " + vetChange);
                                }
                            } else {
                                // error?
                            }

                        } else {
                            Console.WriteLine("Failed to properly detect withdrawal of unit " + squadID + " by " + player.Player.Name);
                        }

                    } else if (msgtype == 'U') { // Upgrade?

                        Console.WriteLine();


                    } else if (msgtype == 'S') { // Surrender

                        Console.WriteLine();


                    } else if (msgtype == 'W') { // Withdraw

                        Console.WriteLine();


                    } else if (msgtype == 'T') { // Captured Equipment

                        // Parse the blueprint type
                        BlueprintType bp = Enum.Parse<BlueprintType>(values[2]);

                        // Get the captured item
                        Blueprint item = BlueprintManager.FromBlueprintName(values[0], bp);

                        if (item != null) {

                            // Add captured item
                            player.CapturedItems.Add(item);

                            // Log the capture
                            Console.WriteLine($"{player.Player.Name} captured \"{item.Name}\"");

                        } else {

                            Console.WriteLine($"{player.Player.Name} captured unknwon item \"{values[0]}\" (Not logged!)");

                        }

                    } else if (msgtype == 'I') { // Slot Item

                        ushort squadID = ushort.Parse(values[0]);
                        string slot_item_bp = values[1];

                        Squad squad = FindFirstSquad(player, allsquads, squadID);
                        if (squad != null) {
                            Blueprint bp = BlueprintManager.FromBlueprintName(slot_item_bp, BlueprintType.IBP);
                            if (bp != null) {
                                squad.AddSlotItem(bp);
                                Console.WriteLine(squadID + " picked up " + slot_item_bp);
                            } else {
                                Console.WriteLine(squadID + " picked up unknown item " + slot_item_bp);
                            }
                        } else {
                            Console.WriteLine($"Invalid squad {squadID} picked up {slot_item_bp}");
                            // error?
                        }

                    } else if (msgtype == 'G') {

                        // Verify GUID
                        this.PlayedWithSession = values[0].CompareTo(this.m_gameSession.SessionID.ToString()) == 0;

                        // Verify the session
                        if (!this.PlayedWithSession) {
                            Console.WriteLine("Fatal error during match validation!");
                            Console.WriteLine($"\"{values[0]}\" != \"{this.m_gameSession.SessionID}\"");
                        } else {
                            Console.WriteLine("Match GUID's were verified.");
                        }

                    } else {

                        Console.WriteLine($"Failed to parse regex-matched message: \"{msg}\"");

                    }

                } else {
                    if (msg.Contains("[") && msg.Contains("]")) { // Was it a potential message we should've parsed?
                        Console.WriteLine($"Failed to parse: \"{msg}\"");
                    }
                }

            } else {
                // Some sort of error?
            }

        }

        private void UpdateCompanies() {

            if (!this.m_gameSession.AllowPersistency) {
                Console.WriteLine("The game session didn't allow for persistency - thus no changes are applied to the companies.");
                return;
            }

            for (int i = 0; i < this.m_matchPlayerResults.Length; i++) {

                // Find the player
                Company company = this.m_gameSession.FindCompany(this.m_matchPlayerResults[i].Player.Name, this.m_matchPlayerResults[i].Player.Army);

                if (company == null) {
                    Console.WriteLine($"Unable to find company for player '{this.m_matchPlayerResults[i].Player.Name}'");
                    continue;
                }

                // Remove all the squads lost
                foreach (Squad squad in this.m_matchPlayerResults[i].Losses) {
                    if (!company.RemoveSquad(squad.SquadID)) {
                        Console.WriteLine("Lost a squad that was not deployed by player!");
                    }
                }

                // Update squads
                foreach (Squad squad in this.m_matchPlayerResults[i].Alive) {

                    Squad companySquad = company.GetSquadByIndex(squad.SquadID);
                    if (companySquad != null) {
                        companySquad.ApplyBattlefieldSquad(squad);
                    } else {
                        Console.WriteLine("Failed to update non-existant squad.");
                    }

                }

                // Add captured items
                foreach (Blueprint bp in this.m_matchPlayerResults[i].CapturedItems) {
                    company.AddInventoryItem(bp);
                }

            }

            Console.WriteLine("Applied all company changes.");

        }

    }

}
