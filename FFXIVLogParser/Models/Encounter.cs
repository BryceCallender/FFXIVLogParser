using FFXIVLogParser.Models.NetworkEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FFXIVLogParser.Models
{
    struct AbilityReportEvent
    {
        public ReportEvent Event { get; set; }
        public NetworkAbility Ability { get; set; }
    }

    class Encounter: ICloneable
    {
        public List<Combatant> combatants;

        public List<uint> partyMemberIDs; //Query combatant list to find the party members

        public List<NetworkAbilityCast> networkCastingAbilities;

        public List<ReportEvent> events; //Events Time and Description

        public DateTime startTime;
        public DateTime endTime;

        public string zoneName;
        public List<BossInfo> bosses;

        public bool startedEncounter;
        public bool endedEncounter;

        public bool isCleared;

        private uint encounterNumber;
        private string fileLocation;

        public Encounter(string zoneName)
        {
            combatants = new List<Combatant>();
            partyMemberIDs = new List<uint>();
            networkCastingAbilities = new List<NetworkAbilityCast>();
            startedEncounter = false;
            this.zoneName = zoneName;
            bosses = new List<BossInfo>();
            events = new List<ReportEvent>();
            encounterNumber = 1;
            fileLocation = "";
        }

        public void ResetEncounter()
        {
            combatants.RemoveAll(combatant => combatant.JobInformation.JobName == null);
            events.Clear();
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

            encounterNumber++;
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
            foreach(ReportEvent reportEvent in events)
            {
                reportEvent.EventTime = reportEvent.EventTime.Subtract(startTime.TimeOfDay);
            }
        }

        public List<Combatant> GetPartyCombatants()
        {
            return combatants.Where(combatant => partyMemberIDs.Contains(combatant.ID)).OrderBy(jobType => jobType.JobInformation.JobCategory).ToList();
        }

        public string GetEncounterFileLocation()
        {
            return fileLocation;
        }

        public void DumpSummaryToFile(DirectoryInfo directoryInfo)
        {
            using StreamWriter streamWriter = File.CreateText(Path.Combine(directoryInfo.FullName, $"Encounter {encounterNumber}.txt"));

            fileLocation = Path.Combine(directoryInfo.FullName, $"Encounter {encounterNumber}.html");

            foreach (ReportEvent report in events)
            {
                streamWriter.WriteLine($"{report.EventTime}\t\t{report.EventDescription}");
            }
        }

        public object Clone()
        {
            Encounter newEncounter = new Encounter(zoneName)
            {
                partyMemberIDs = new List<uint>(partyMemberIDs),
                combatants = new List<Combatant>(GetPartyCombatants())
            };

            if (ZoneData.zoneInfo.ContainsKey(zoneName))
            {
                newEncounter.bosses = ZoneData.zoneInfo[zoneName];
            }

            return newEncounter;
        }
    }
}
