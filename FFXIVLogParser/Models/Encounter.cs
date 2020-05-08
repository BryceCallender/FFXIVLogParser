using FFXIVLogParser.Models.NetworkEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFXIVLogParser.Models
{
    class Encounter
    {
        public List<Combatant> combatants;

        public List<uint> partyMembers; //Query combatant list to find the party members

        public List<NetworkAbility> networkAbilities;
        public List<NetworkAbilityCast> networkCastingAbilities;

        public List<ReportEvent> summaryEvents; //Events Time and Description

        public DateTime startTime;
        public DateTime endTime;

        public string zoneName;
        public List<BossInfo> bosses;

        public bool startedEncounter;
        public bool endedEncounter;

        public bool isCleared;

        public Encounter()
        {
            combatants = new List<Combatant>();
            partyMembers = new List<uint>();
            networkAbilities = new List<NetworkAbility>();
            networkCastingAbilities = new List<NetworkAbilityCast>();
            startedEncounter = false;
            zoneName = "";
            bosses = new List<BossInfo>();
            summaryEvents = new List<ReportEvent>();
        }

        public void ResetEncounter()
        {
            combatants.Clear();
            networkAbilities.Clear();
            summaryEvents.Clear();
            startedEncounter = false;
            
            if (ZoneData.zoneInfo.ContainsKey(zoneName))
            {
                bosses = ZoneData.zoneInfo[zoneName];
            }
            else
            {
                zoneName = "";
                bosses.Clear();
            }
        }

        public bool AreRequiredBossesDead()
        {
            return bosses.Where(boss => boss.HasToDie).All(boss => boss.IsDead); //Returns true if all the bosses who have to die are dead
        }

        public Combatant GetCombatantFromID(uint id)
        {
            return combatants.Where(combatant => combatant.ID == id).FirstOrDefault();
        }

        public void AdjustTimeSpans()
        {
            foreach(ReportEvent reportEvent in summaryEvents)
            {
                reportEvent.EventTime = reportEvent.EventTime.Subtract(startTime.TimeOfDay);
            }
        }
    }
}
