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

        public List<Encounter> encounters;

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

                        Debug.WriteLine($"Entered {zoneName}...");
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
                    Combatant addCombatant = ReadCombatant(lineContents);

                    currentEncounter.combatants.Add(ReadCombatant(lineContents));

                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if (boss.Name == addCombatant.Name && boss.Level == addCombatant.Level && (boss.MaxHP == addCombatant.Health.MaxHP || boss.MaxHP == addCombatant.Health.CurrentHP))
                        {
                            currentEncounter.endedEncounter = false;
                        }

                    }
                        //Debug.WriteLine(currentEncounter.combatants.Count);

                    break;
                case LogMessageType.RemoveCombatant:
                    Combatant removeCombatant = ReadCombatant(lineContents);

                    currentEncounter.combatants.Remove(removeCombatant);

                    //Debug.WriteLine(currentEncounter.combatants.Count);
                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if (!currentEncounter.endedEncounter && boss.Name == removeCombatant.Name && boss.Level == removeCombatant.Level && (boss.MaxHP == removeCombatant.Health.MaxHP || boss.MaxHP == removeCombatant.Health.CurrentHP))
                        {
                            if (currentEncounter.bosses.Count == 1)
                            {
                                currentEncounter.endTime = removeCombatant.Timestamp;
                                currentEncounter.endedEncounter = true;
                                currentEncounter.isCleared = removeCombatant.Health.CurrentHP == 0;

                                encounters.Add(currentEncounter);

                                Debug.WriteLine($"Encounter has ended... It took {currentEncounter.endTime.Subtract(currentEncounter.startTime)}");

                                currentEncounter.ResetEncounter();
                                break;
                            }
                            else
                            {
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
                    NetworkAbilityCast networkAbilityCast = ReadNetworkCast(lineContents);

                    currentEncounter.networkCastingAbilities.Add(networkAbilityCast);

                    Combatant combatant = currentEncounter.GetCombatantFromID(networkAbilityCast.ActorID);

                    //They started casting so we count this as a skill usage even if cancelled or interruped
                    if(combatant != null)
                    {
                        //combatant.AbilityInfo[networkAbilityCast.SkillID].CastAmount++;
                    }

                    if(combatant != null && !currentEncounter.startedEncounter && currentEncounter.bosses.Any(boss => boss.Name == networkAbilityCast.TargetName))
                    {
                        currentEncounter.summaryEvents.Add(new ReportEvent
                        {
                            EventTime = networkAbilityCast.Timestamp.TimeOfDay,
                            EventDescription = $"{networkAbilityCast.ActorName} prepares {networkAbilityCast.SkillName} on {networkAbilityCast.TargetName}"
                        });
                    }

                    break;
                case LogMessageType.NetworkAbility:
                    if (bossInformation.Count == 0)
                    { 
                        break;
                    }

                    NetworkAbility ability = ReadAbilityUsed(lineContents);

                    combatant = currentEncounter.GetCombatantFromID(ability.ActorID);

                    //Check if the ability used is a new skill if it is lets keep track of it from the combatant side
                    //Itll be updated in the NetworkEffectResult since thats when the information is able to be deteced
                    if(combatant != null)
                    {
                        if(combatant.AbilityInfo.ContainsKey(ability.SkillID))
                        {
                            combatant.AbilityInfo.Add(ability.SkillID, new AbilityInfo());
                        }
                    }

                    if (!currentEncounter.startedEncounter && bossInformation.Any(boss => boss.Name == ability.TargetName))
                    {
                        currentEncounter.startTime = ability.Timestamp;
                        Debug.WriteLine($"Start of fight: {ability.Timestamp}");
                        currentEncounter.startedEncounter = true;

                        currentEncounter.AdjustTimeSpans();
                    }

                    if(currentEncounter.bosses.Any(boss => boss.Name == ability.TargetName)) 
                    {
                        currentEncounter.networkAbilities.Add(ability);

                        if(combatant != null)
                        {
                            //Skill information in here
                        }

                        //Debug.WriteLine($"{ability.SkillName} used: {ability.Timestamp.Subtract(currentEncounter.startTime)}");
                    }
                    
                    break;
                case LogMessageType.NetworkAOEAbility:
                    break;
                case LogMessageType.NetworkCancelAbility:
                    NetworkAbilityCancel networkAbilityCancel = ReadNetworkSkillCancel(lineContents);



                    break;
                case LogMessageType.NetworkDoT:
                    break;
                case LogMessageType.NetworkDeath:
                    NetworkDeath death = ReadNetworkDeath(lineContents);

                    if(currentEncounter.startedEncounter)
                    {
                        currentEncounter.summaryEvents.Add(new ReportEvent
                        {
                            EventTime = death.Timestamp.Subtract(currentEncounter.startTime),
                            EventDescription = $"{death.ActorName} died"
                        });
                    }

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
                    LimitBreak limitBreak = ReadLimitBreak(lineContents);

                    currentEncounter.summaryEvents.Add(new ReportEvent
                    {
                        EventTime = limitBreak.Timestamp.Subtract(currentEncounter.startTime),
                        EventDescription = $"The limit break guage has been updated to {limitBreak.LimitBreakGuage}. {limitBreak.MaxLimitBreakNumber} bars are available"
                    });

                    break;
                case LogMessageType.NetworkEffectResult:
                    NetworkEffectResult result = ReadNetworkEffectResult(lineContents);

                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if(result.ActorName == boss.Name && result.Health.CurrentHP == 0 && boss.HasToDie)
                        {
                            boss.IsDead = true;
                        }
                    }

                    //Figure out the damage the skill inflicted and then go back and credit the ability for that damage

                    if (!currentEncounter.endedEncounter && currentEncounter.AreRequiredBossesDead())
                    {
                        currentEncounter.endedEncounter = true;
                        currentEncounter.endTime = result.Timestamp;
                        currentEncounter.isCleared = true;

                        encounters.Add(currentEncounter);

                        Debug.WriteLine($"Encounter cleared!!! It took {currentEncounter.endTime.Subtract(currentEncounter.startTime)}");

                        currentEncounter.ResetEncounter();
                    }

                    break;
                case LogMessageType.NetworkStatusList:
                    NetworkStatusList networkStatusList = ReadNetworkStatusList(lineContents);

                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if (networkStatusList.TargetName == boss.Name && networkStatusList.Health.CurrentHP == 0 && boss.HasToDie)
                        {
                            boss.IsDead = true;
                        }
                    }

                    //Figure out the damage the skill inflicted and then go back and credit the ability for that damage

                    if (!currentEncounter.endedEncounter && currentEncounter.AreRequiredBossesDead())
                    {
                        currentEncounter.endedEncounter = true;
                        currentEncounter.endTime = networkStatusList.Timestamp;
                        currentEncounter.isCleared = true;

                        encounters.Add(currentEncounter);

                        Debug.WriteLine($"Encounter cleared!!! It took {currentEncounter.endTime.Subtract(currentEncounter.startTime)}");

                        currentEncounter.ResetEncounter();
                    }

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

        #region Reading Network Data Functions

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

        private NetworkEffectResult ReadNetworkEffectResult(string[] lineContents)
        {
            return new NetworkEffectResult
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ActorID = Convert.ToUInt32(lineContents[2], 16),
                ActorName = lineContents[3],
                Health = new Health
                {
                    CurrentHP = string.IsNullOrEmpty(lineContents[5]) ? 0 : Convert.ToUInt32(lineContents[5]),
                    MaxHP = string.IsNullOrEmpty(lineContents[6]) ? 0 : Convert.ToUInt32(lineContents[6]),
                    CurrentMP = string.IsNullOrEmpty(lineContents[7]) ? 0 : Convert.ToUInt32(lineContents[7]),
                    MaxMP = string.IsNullOrEmpty(lineContents[8]) ? 0 : Convert.ToUInt32(lineContents[8]),
                    CurrentTP = string.IsNullOrEmpty(lineContents[9]) ? 0 : Convert.ToUInt32(lineContents[9]),
                    MaxTP = string.IsNullOrEmpty(lineContents[10]) ? 0 : Convert.ToUInt32(lineContents[10])
                },
                Position = new Position
                {
                    X = string.IsNullOrEmpty(lineContents[11]) ? 0 : Convert.ToSingle(lineContents[11]),
                    Y = string.IsNullOrEmpty(lineContents[12]) ? 0 : Convert.ToSingle(lineContents[12]),
                    Z = string.IsNullOrEmpty(lineContents[13]) ? 0 : Convert.ToSingle(lineContents[13]),
                    Facing = string.IsNullOrEmpty(lineContents[14]) ? 0 : Convert.ToSingle(lineContents[14]),
                }
            };
        }

        private NetworkAbilityCast ReadNetworkCast(string[] lineContents)
        {
            return new NetworkAbilityCast
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ActorID = Convert.ToUInt32(lineContents[2], 16),
                ActorName = lineContents[3],
                SkillID = Convert.ToUInt32(lineContents[4], 16),
                SkillName = lineContents[5],
                TargetID = Convert.ToUInt32(lineContents[6], 16),
                TargetName = lineContents[7],
                CastDuration = Convert.ToSingle(lineContents[8])
            };
        }

        private NetworkAbilityCancel ReadNetworkSkillCancel(string[] lineContents)
        {
            return new NetworkAbilityCancel
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ActorID = Convert.ToUInt32(lineContents[2], 16),
                ActorName = lineContents[3],
                SkillID = Convert.ToUInt32(lineContents[4], 16),
                SkillName = lineContents[5],
                Cancelled = lineContents[6] == "Cancelled",
                Interrupted = lineContents[6] == "Interrupted"
            };
        }

        private LimitBreak ReadLimitBreak(string[] lineContents)
        {
            return new LimitBreak
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                LimitBreakGuage = Convert.ToUInt32(lineContents[2], 16),
                MaxLimitBreakNumber = Convert.ToUInt16(lineContents[3])
            };
        }

        private NetworkDeath ReadNetworkDeath(string[] lineContents)
        {
            return new NetworkDeath
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ActorID = Convert.ToUInt32(lineContents[2], 16),
                ActorName = lineContents[3],
                TargetID = Convert.ToUInt32(lineContents[4], 16),
                TargetName = lineContents[5]
            };
        }

        private NetworkDoT ReadNetworkDoT(string[] lineContents)
        {
            return new NetworkDoT
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                TargetID = Convert.ToUInt32(lineContents[2], 16),
                TargetName = lineContents[3],
                IsDamage = lineContents[4] == "DoT",
                Damage = Convert.ToUInt32(lineContents[5]),
                BuffID = Convert.ToUInt32(lineContents[6], 16),
                TargetHealth = new Health
                {
                    CurrentHP = string.IsNullOrEmpty(lineContents[7]) ? 0 : Convert.ToUInt32(lineContents[7]),
                    MaxHP = string.IsNullOrEmpty(lineContents[8]) ? 0 : Convert.ToUInt32(lineContents[8]),
                    CurrentMP = string.IsNullOrEmpty(lineContents[9]) ? 0 : Convert.ToUInt32(lineContents[9]),
                    MaxMP = string.IsNullOrEmpty(lineContents[10]) ? 0 : Convert.ToUInt32(lineContents[10]),
                    CurrentTP = string.IsNullOrEmpty(lineContents[11]) ? 0 : Convert.ToUInt32(lineContents[11]),
                    MaxTP = string.IsNullOrEmpty(lineContents[12]) ? 0 : Convert.ToUInt32(lineContents[12])
                },
                TargetPosition = new Position
                {
                    X = string.IsNullOrEmpty(lineContents[13]) ? 0 : Convert.ToSingle(lineContents[13]),
                    Y = string.IsNullOrEmpty(lineContents[14]) ? 0 : Convert.ToSingle(lineContents[14]),
                    Z = string.IsNullOrEmpty(lineContents[15]) ? 0 : Convert.ToSingle(lineContents[15]),
                    Facing = string.IsNullOrEmpty(lineContents[16]) ? 0 : Convert.ToSingle(lineContents[16]),
                }
            };
        }

        private NetworkStatusList ReadNetworkStatusList(string[] lineContents)
        {
            List<Status> statuses = new List<Status>();

            int index = 15;

            //while(index < lineContents.Length - 1)
            //{
            //    //Read in 3 at a time
            //    statuses.Add(new Status
            //    {
            //        EffectID = Convert.ToUInt32(lineContents[index].Substring(0,lineContents[index].Length - 4), 16),

            //    });
            //}


            return new NetworkStatusList
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                TargetID = Convert.ToUInt32(lineContents[2], 16),
                TargetName = lineContents[3],
                JobID = Convert.ToUInt32(lineContents[4].Substring(0, 2), 16),
                Level1 = Convert.ToUInt32(lineContents[4].Substring(2, 2), 16),
                Level2 = Convert.ToUInt32(lineContents[4].Substring(4, 2), 16),
                Level3 = Convert.ToUInt32(lineContents[4].Substring(6, 2), 16),
                Health = new Health
                {
                    CurrentHP = string.IsNullOrEmpty(lineContents[5]) ? 0 : Convert.ToUInt32(lineContents[5]),
                    MaxHP = string.IsNullOrEmpty(lineContents[6]) ? 0 : Convert.ToUInt32(lineContents[6]),
                    CurrentMP = string.IsNullOrEmpty(lineContents[7]) ? 0 : Convert.ToUInt32(lineContents[7]),
                    MaxMP = string.IsNullOrEmpty(lineContents[8]) ? 0 : Convert.ToUInt32(lineContents[8]),
                    CurrentTP = string.IsNullOrEmpty(lineContents[9]) ? 0 : Convert.ToUInt32(lineContents[9]),
                    MaxTP = string.IsNullOrEmpty(lineContents[10]) ? 0 : Convert.ToUInt32(lineContents[10])
                },
                Position = new Position
                {
                    X = string.IsNullOrEmpty(lineContents[11]) ? 0 : Convert.ToSingle(lineContents[11]),
                    Y = string.IsNullOrEmpty(lineContents[12]) ? 0 : Convert.ToSingle(lineContents[12]),
                    Z = string.IsNullOrEmpty(lineContents[13]) ? 0 : Convert.ToSingle(lineContents[13]),
                    Facing = string.IsNullOrEmpty(lineContents[14]) ? 0 : Convert.ToSingle(lineContents[14]),
                },
            };
        }

        private NetworkAOEAbility ReadNetworkAOEAbility(string[] lineContents)
        {
            return new NetworkAOEAbility
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

        private NetworkBuff ReadNetworkBuff(string[] lineContents)
        {
            return new NetworkBuff
            {
                Timestamp = DateTime.Parse(lineContents[1]),
                ActorID = Convert.ToUInt32(lineContents[2], 16),
                ActorName = lineContents[3],
                Duration = Convert.ToSingle(lineContents[4]),
                TargetID = Convert.ToUInt32(lineContents[5], 16),
                TargetName = lineContents[6],
                TargetMaxHP = Convert.ToUInt32(lineContents[10]),
                TargetMaxMP = Convert.ToUInt32(lineContents[11])
            };
        }

        #endregion
    }
}
