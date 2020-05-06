using FFXIVLogParser.Models.NetworkEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models
{
    class Encounter
    {
        public List<Combatant> combatants;
        public List<uint> partyMembers;

        public Queue<NetworkAbility> networkAbilities;

        public DateTime startTime;
        public DateTime endTime;

        public string zoneName;
        public List<BossInfo> bosses;

        public bool startedEncounter;

        public bool isCleared;

        public Encounter()
        {
            combatants = new List<Combatant>();
            partyMembers = new List<uint>();
            networkAbilities = new Queue<NetworkAbility>();
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
    }
}
