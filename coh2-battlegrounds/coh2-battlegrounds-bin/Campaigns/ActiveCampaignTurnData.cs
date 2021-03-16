using System;

using static Battlegrounds.BattlegroundsInstance;

namespace Battlegrounds.Campaigns {
    
    public class ActiveCampaignTurnData {

        private DateTime m_date;
        private DateTime m_endDate;
        private TimeSpan m_turnStep;

        private DateTime m_winterStart;
        private DateTime m_winterEnd;

        private CampaignArmyTeam m_currentTurn;
        private CampaignArmyTeam m_initialTurn;

        public CampaignArmyTeam CurrentTurn => this.m_currentTurn;

        public string Date => $"{this.m_date.Year}, {Localize.GetString($"Month_{this.m_date.Month}")} {Localize.GetNumberSuffix(this.m_date.Day)}";

        public bool IsWinter => this.m_date > this.m_winterStart && this.m_date < this.m_winterEnd;

        public ActiveCampaignTurnData(CampaignArmyTeam inTurn, (int year, int month, int day)[] dates, double advance) {

            // Set team turn
            this.m_initialTurn = this.m_currentTurn = inTurn;

            // Calculate advance days and hours
            int days = (int)(advance / 24);
            int hours = (int)(advance % 24);

            // Set turn step and date
            this.m_date = new DateTime(dates[0].year, dates[0].month, dates[0].day);
            this.m_endDate = new DateTime(dates[1].year, dates[1].month, dates[1].day);

            // Set step data
            this.m_turnStep = new TimeSpan(days, hours, 0, 0);
            
        }

        public void SetWinterDates((int year, int month, int day)[] dates) {
            this.m_winterStart = new DateTime(dates[0].year, dates[0].month, dates[0].day);
            this.m_winterEnd = new DateTime(dates[1].year, dates[1].month, dates[1].day);
        }

        public bool EndTurn(out bool wasRound) { 
            if (this.m_currentTurn == CampaignArmyTeam.TEAM_ALLIES) {
                this.m_currentTurn = CampaignArmyTeam.TEAM_AXIS;
            } else {
                this.m_currentTurn = CampaignArmyTeam.TEAM_ALLIES;
            }
            if (this.m_currentTurn == this.m_initialTurn) {
                this.m_date += this.m_turnStep;
                wasRound = true;
            } else {
                wasRound = false;
            }
            return this.m_date <= this.m_endDate;
        }

    }

}
