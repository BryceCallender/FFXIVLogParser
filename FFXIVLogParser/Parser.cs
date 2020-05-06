using FFXIVLogParser.Models;
using FFXIVLogParser.Models.NetworkEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FFXIVLogParser
{
    class Parser
    {
        List<BossInfo> bossInformation;

        List<Encounter> encounters;

        Encounter currentEncounter;

        public Parser()
        {
            encounters = new List<Encounter>();
            currentEncounter = new Encounter();
            bossInformation = new List<BossInfo>();
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
                        bossInformation = ZoneData.zoneInfo[zoneName];
                        currentEncounter.zoneName = zoneName;
                        currentEncounter.bosses = bossInformation;

                        Debug.WriteLine("Entered Boss Zone...");
                    }
                    else
                    {
                        Debug.WriteLine($"Entered {zoneName}...");

                        currentEncounter = new Encounter();
                    }

                    break;
                case LogMessageType.ChangePrimaryPlayer:
                    break;
                case LogMessageType.AddCombatant:
                    currentEncounter.combatants.Add(ReadCombatant(lineContents));

                    //Debug.WriteLine(currentEncounter.combatants.Count);

                    break;
                case LogMessageType.RemoveCombatant:
                    Combatant removeCombatant = ReadCombatant(lineContents);

                    currentEncounter.combatants.Remove(removeCombatant);

                    //Debug.WriteLine(currentEncounter.combatants.Count);
                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if (boss.Name == removeCombatant.Name && boss.Level == removeCombatant.Level && boss.MaxHP == removeCombatant.Health.MaxHP)
                        {
                            if (bossInformation.Count == 1)
                            {
                                currentEncounter.endTime = removeCombatant.Timestamp;

                                currentEncounter.isCleared = removeCombatant.Health.CurrentHP == 0;

                                encounters.Add(currentEncounter);

                                Debug.WriteLine("Encounter has ended...");

                                currentEncounter.ResetEncounter();
                                break;
                            }
                            else
                            {
                                //Debug.WriteLine(line);
                                currentEncounter.bosses.Remove(boss);
                                break;
                            }
                        }
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

                    //If the encounter hasnt started yet form the party memebers
                    //This is to avoid the struggles of someone dcing and changing the party size which could end up crashing this algorithm
                    if (!string.IsNullOrEmpty(currentEncounter.zoneName) && !currentEncounter.startedEncounter)
                    {
                        currentEncounter.partyMembers.Clear();

                        uint.TryParse(lineContents[2], out uint size);

                        int start = 3; //Start of first memeber and going to start + size

                        for (int i = 0; i < size; i++)
                        {
                            currentEncounter.partyMembers.Add(Convert.ToUInt32(lineContents[start + i], 16));
                        }
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
                    if (bossInformation.Count == 0)
                        break;

                    NetworkAbility ability = ReadAbilityUsed(lineContents);

                    if (!currentEncounter.startedEncounter && bossInformation.Any(boss => boss.Name == ability.TargetName))
                    {
                        currentEncounter.startTime = ability.Timestamp;
                        Debug.WriteLine($"Start of fight: { ability.Timestamp}");
                        currentEncounter.startedEncounter = true;
                    }

                    if(bossInformation.Any(boss => boss.Name == ability.TargetName)) 
                    {
                        currentEncounter.networkAbilities.Enqueue(ability);
                        //Debug.WriteLine($"{ability.SkillName} used: {ability.Timestamp.Subtract(currentEncounter.startTime)}");
                    }
                    
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
                    CurrentHP = Convert.ToUInt32(lineContents[11]),
                    MaxHP = Convert.ToUInt32(lineContents[12]),
                    CurrentMP = Convert.ToUInt32(lineContents[13]),
                    MaxMP = Convert.ToUInt32(lineContents[14]),
                    CurrentTP = Convert.ToUInt32(lineContents[15]),
                    MaxTP = Convert.ToUInt32(lineContents[16])
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

        private NetworkAbility ReadAbilityUsed(string[] lineContents)
        {
            return new NetworkAbility
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ActorID = Convert.ToUInt32(lineContents[2], 16),
                ActorName = lineContents[3],
                SkillID = Convert.ToUInt32(lineContents[4], 16),
                SkillName = lineContents[5],
                TargetID = Convert.ToUInt32(lineContents[6], 16),
                TargetName = string.IsNullOrEmpty(lineContents[7]) ? "" : lineContents[7],
                ActorHealth = new Health
                {
                    CurrentHP = string.IsNullOrEmpty(lineContents[24]) ? 0 : Convert.ToUInt32(lineContents[24]),
                    MaxHP = string.IsNullOrEmpty(lineContents[25]) ? 0 : Convert.ToUInt32(lineContents[25]),
                    CurrentMP = string.IsNullOrEmpty(lineContents[26]) ? 0 : Convert.ToUInt32(lineContents[26]),
                    MaxMP = string.IsNullOrEmpty(lineContents[27]) ? 0 : Convert.ToUInt32(lineContents[27]),
                    CurrentTP = string.IsNullOrEmpty(lineContents[28]) ? 0 : Convert.ToUInt32(lineContents[28]),
                    MaxTP = string.IsNullOrEmpty(lineContents[29]) ? 0 : Convert.ToUInt32(lineContents[29]),
                },
                ActorPosition = new Position
                {
                    X = string.IsNullOrEmpty(lineContents[30]) ? 0 : Convert.ToSingle(lineContents[30]),
                    Y = string.IsNullOrEmpty(lineContents[31]) ? 0 : Convert.ToSingle(lineContents[31]),
                    Z = string.IsNullOrEmpty(lineContents[32]) ? 0 : Convert.ToSingle(lineContents[32]),
                    Facing = string.IsNullOrEmpty(lineContents[33]) ? 0 : Convert.ToSingle(lineContents[33]),
                },
                TargetHealth = new Health
                {
                    CurrentHP = string.IsNullOrEmpty(lineContents[34]) ? 0 : Convert.ToUInt32(lineContents[34]),
                    MaxHP = string.IsNullOrEmpty(lineContents[35]) ? 0 : Convert.ToUInt32(lineContents[35]),
                    CurrentMP = string.IsNullOrEmpty(lineContents[36]) ? 0 : Convert.ToUInt32(lineContents[36]),
                    MaxMP = string.IsNullOrEmpty(lineContents[37]) ? 0 : Convert.ToUInt32(lineContents[37]),
                    CurrentTP = string.IsNullOrEmpty(lineContents[38]) ? 0 : Convert.ToUInt32(lineContents[38]),
                    MaxTP = string.IsNullOrEmpty(lineContents[39]) ? 0 : Convert.ToUInt32(lineContents[39])
                },
                TargetPosition = new Position
                {
                    X = string.IsNullOrEmpty(lineContents[40]) ? 0 : Convert.ToSingle(lineContents[40]),
                    Y = string.IsNullOrEmpty(lineContents[41]) ? 0 : Convert.ToSingle(lineContents[41]),
                    Z = string.IsNullOrEmpty(lineContents[42]) ? 0 : Convert.ToSingle(lineContents[42]),
                    Facing = string.IsNullOrEmpty(lineContents[43]) ? 0 : Convert.ToSingle(lineContents[43]),
                }
            };
        }
    }
}
