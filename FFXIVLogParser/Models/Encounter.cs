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

        public Encounter()
        {
            combatants = new List<Combatant>();
            partyMembers = new List<uint>();
            networkAbilities = new Queue<NetworkAbility>();
        }

        public void ResetEncounter()
        {
            combatants.Clear();
            networkAbilities.Clear();
        }
    }
}
