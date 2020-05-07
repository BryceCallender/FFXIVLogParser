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
        public List<uint> partyMembers;

        public List<NetworkAbility> networkAbilities;

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
            startedEncounter = false;
            zoneName = "";
            bosses = new List<BossInfo>();
        }

        public void ResetEncounter()
        {
            combatants.Clear();
            networkAbilities.Clear();
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
    }
}
