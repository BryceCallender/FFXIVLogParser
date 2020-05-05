using FFXIVLogParser.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FFXIVLogParser
{
    class Parser
    {
        List<string>? bossNames;

        List<Encounter> encounters;

        Encounter currentEncounter;

        public Parser()
        {
            encounters = new List<Encounter>();
        }

        public void ParseLine(string line)
        {
            string[] lineContents = line.Split('|');

            int.TryParse(lineContents[0], out int messageType);

            switch ((LogMessageType)messageType)
            {
                case LogMessageType.LogLine:
                    break;
                case LogMessageType.ChangeZone:

                    string zoneName = lineContents[3];

                    if (ZoneData.zoneInfo.ContainsKey(zoneName))
                    {
                        bossNames = ZoneData.zoneInfo[zoneName];
                    }

                    Debug.WriteLine("Entered Boss Zone...");

                    currentEncounter = new Encounter();

                    break;
                case LogMessageType.ChangePrimaryPlayer:
                    break;
                case LogMessageType.AddCombatant:
                    currentEncounter.combatants.Add(ReadCombatant(lineContents));

                    Debug.WriteLine(currentEncounter.combatants.Count);

                    break;
                case LogMessageType.RemoveCombatant:
                    Combatant removeCombatant = ReadCombatant(lineContents);

                    currentEncounter.combatants.Remove(removeCombatant);

                    Debug.WriteLine(currentEncounter.combatants.Count);

                    if (currentEncounter.combatants.Count == currentEncounter.partyMembers.Count)
                    {
                        encounters.Add(currentEncounter);

                        Debug.WriteLine("Encounter has ended...");

                        currentEncounter.endTime = removeCombatant.Timestamp;
                        
                        currentEncounter.ResetEncounter();
                    }

                    break;
                case LogMessageType.AddBuff:
                    break;
                case LogMessageType.RemoveBuff:
                    break;
                case LogMessageType.FlyingText:
                    break;
                case LogMessageType.OutgoingAbility:
                    break;
                case LogMessageType.IncomingAbility:
                    break;
                case LogMessageType.PartyList:

                    uint.TryParse(lineContents[2], out uint size);

                    int start = 3; //Start of first memeber and going to start + size

                    for (int i = 0; i < size; i++)
                    {
                        currentEncounter.partyMembers.Add(Convert.ToUInt32(lineContents[start + i], 16));
                    }

                    break;
                case LogMessageType.PlayerStats:
                    break;
                case LogMessageType.CombatantHP:
                    break;
                case LogMessageType.ParsedPartyMember:
                    break;
                case LogMessageType.NetworkStartsCasting:
                    break;
                case LogMessageType.NetworkAbility:
                    break;
                case LogMessageType.NetworkAOEAbility:
                    break;
                case LogMessageType.NetworkCancelAbility:
                    break;
                case LogMessageType.NetworkDoT:
                    break;
                case LogMessageType.NetworkDeath:
                    break;
                case LogMessageType.NetworkBuff:
                    break;
                case LogMessageType.NetworkTargetIcon:
                    break;
                case LogMessageType.NetworkTargetMarker:
                    break;
                case LogMessageType.NetworkBuffRemove:
                    break;
                case LogMessageType.NetworkGauge:
                    break;
                case LogMessageType.NetworkWorld:
                    break;
                case LogMessageType.Network6D:
                    break;
                case LogMessageType.NetworkNameToggle:
                    break;
                case LogMessageType.NetworkTether:
                    break;
                case LogMessageType.NetworkLimitBreak:
                    break;
                case LogMessageType.NetworkEffectResult:
                    break;
                case LogMessageType.NetworkStatusList:
                    break;
                case LogMessageType.NetworkUpdateHp:
                    break;
                case LogMessageType.Settings:
                    string[] settings = lineContents[2].Split(',');

                    Debug.WriteLine("ACT Settings:");
                    foreach (string setting in settings)
                    {
                        Debug.WriteLine(setting.Trim());
                    }
                    break;
                case LogMessageType.Process:
                    break;
                case LogMessageType.Debug:
                    break;
                case LogMessageType.PacketDump:
                    break;
                case LogMessageType.Version:
                    Debug.WriteLine($"You are using {lineContents[2]}");
                    break;
                case LogMessageType.Error:
                    break;
                case LogMessageType.Timer:
                    break;
            }
        }

        private Combatant ReadCombatant(string[] lineContents)
        {
            return new Combatant
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ID = Convert.ToUInt32(lineContents[2], 16),
                Name = lineContents[3],
                JobID = Convert.ToUInt32(lineContents[4], 16),
                Level = Convert.ToUInt32(lineContents[5], 16),
                OwnerID = Convert.ToUInt32(lineContents[6], 16),
                WorldID = Convert.ToUInt32(lineContents[7], 16),
                WorldName = string.IsNullOrEmpty(lineContents[8]) ? "" : lineContents[8],
                BNpcNameID = Convert.ToUInt32(lineContents[9], 16),
                BNpcID = Convert.ToUInt32(lineContents[10], 16),
                Health = new Health
                {
                    CurrentHP = Convert.ToUInt32(lineContents[11], 16),
                    MaxHP = Convert.ToUInt32(lineContents[12], 16),
                    CurrentMP = Convert.ToUInt32(lineContents[13], 16),
                    MaxMP = Convert.ToUInt32(lineContents[14], 16),
                    CurrentTP = Convert.ToUInt32(lineContents[15], 16),
                    MaxTP = Convert.ToUInt32(lineContents[16], 16)
                },
                Position = new Position
                {
                    X = Convert.ToSingle(lineContents[17]),
                    Y = Convert.ToSingle(lineContents[18]),
                    Z = Convert.ToSingle(lineContents[19]),
                    Facing = Convert.ToSingle(lineContents[20]),
                }
            };
        }

        private NetworkAbility
    }
}
