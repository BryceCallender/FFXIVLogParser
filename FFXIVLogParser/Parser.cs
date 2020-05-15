using FFXIVLogParser.Models;
using FFXIVLogParser.Models.NetworkEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FFXIVLogParser
{
    class Parser
    {
        List<BossInfo> bossInformation;

        public List<Encounter> encounters;

        Encounter currentEncounter;

        private string zoneName;

        public DirectoryInfo encounterDirectoryInfo;

        private bool firstAOELine;

        public Parser(DirectoryInfo summaryDirectoryInfo, string path)
        {
            encounters = new List<Encounter>();
            currentEncounter = new Encounter("");
            bossInformation = new List<BossInfo>();

            string dateOfLog = path.Split("_")[2];
            dateOfLog = dateOfLog.Substring(0, dateOfLog.IndexOf('.'));
            encounterDirectoryInfo = Directory.CreateDirectory(Path.Combine(summaryDirectoryInfo.FullName, dateOfLog));

            zoneName = "";
        }

        public void ParseLine(string line)
        {
            string[] lineContents = line.Split('|');

            int.TryParse(lineContents[0], out int messageType);

            if((LogMessageType)messageType != LogMessageType.NetworkAOEAbility)
            {
                firstAOELine = true;
            }

            switch ((LogMessageType)messageType)
            {
                case LogMessageType.LogLine:
                    break;
                case LogMessageType.ChangeZone:

                    zoneName = lineContents[3];

                    if (ZoneData.zoneInfo.ContainsKey(zoneName))
                    {
                        bossInformation = ZoneData.zoneInfo[zoneName];
                        currentEncounter.zoneName = zoneName;
                        currentEncounter.bosses = bossInformation;
                        currentEncounter.combatants.Clear();

                        Debug.WriteLine($"Entered {zoneName}...");
                    }
                    else
                    {
                        Debug.WriteLine($"Entered {zoneName}...");

                        currentEncounter = new Encounter(zoneName);
                    }

                    break;
                case LogMessageType.ChangePrimaryPlayer:
                    break;
                case LogMessageType.AddCombatant:
                    Combatant addCombatant = ReadCombatant(lineContents);

                    currentEncounter.combatants.Add(addCombatant);

                    if(addCombatant.ID.ToString("X8").StartsWith("10"))
                    {
                        if(!currentEncounter.partyMemberIDs.Contains(addCombatant.ID))
                        {
                            currentEncounter.partyMemberIDs.Add(addCombatant.ID);
                        }
                    }

                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if (boss.Name == addCombatant.Name && boss.Level == addCombatant.Level && (boss.MaxHP == addCombatant.Health.MaxHP || boss.MaxHP == addCombatant.Health.CurrentHP))
                        {
                            currentEncounter.endedEncounter = false;
                        }
                    }

                    break;
                case LogMessageType.RemoveCombatant:
                    Combatant removeCombatant = ReadCombatant(lineContents);

                    currentEncounter.combatants.Remove(removeCombatant);
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
                        currentEncounter.partyMemberIDs.Clear();

                        uint.TryParse(lineContents[2], out uint size);

                        int start = 3; //Start of first memeber and going to start + size

                        try
                        {
                            for (int i = 0; i < size; i++)
                            {
                                currentEncounter.partyMemberIDs.Add(Convert.ToUInt32(lineContents[start + i], 16));
                            }
                        }
                        catch(OverflowException ex)
                        {
                            Debug.WriteLine("Someone seemed to have disconnected!");
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

                    if(combatant != null && !currentEncounter.startedEncounter && currentEncounter.bosses.Any(boss => boss.Name == networkAbilityCast.TargetName))
                    {
                        currentEncounter.events.Add(new ReportEvent
                        {
                            EventTime = networkAbilityCast.Timestamp.TimeOfDay,
                            EventDescription = $"{networkAbilityCast.ActorName} casts {networkAbilityCast.SkillName} on {networkAbilityCast.TargetName}",
                            EventType = EventType.Summary | EventType.DamageDone
                        });
                    }

                    if (combatant != null)
                    {
                        if (!combatant.AbilityInfo.ContainsKey(networkAbilityCast.SkillName))
                        {
                            combatant.AbilityInfo.Add(networkAbilityCast.SkillName, new AbilityInfo());
                        }

                        combatant.AbilityInfo[networkAbilityCast.SkillName].CastAmount++;
                    }


                    break;
                case LogMessageType.NetworkAbility:
                case LogMessageType.NetworkAOEAbility:
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
                        if (!combatant.AbilityInfo.ContainsKey(ability.SkillName))
                        {
                            combatant.AbilityInfo.Add(ability.SkillName, new AbilityInfo());
                        }

                        if(((LogMessageType)messageType == LogMessageType.NetworkAOEAbility && firstAOELine) || (LogMessageType)messageType == LogMessageType.NetworkAbility)
                        {
                            combatant.AbilityInfo[ability.SkillName].CastAmount++;
                            firstAOELine = false;
                        }
                        
                    }

                    if (!currentEncounter.startedEncounter && bossInformation.Any(boss => boss.Name == ability.TargetName))
                    {
                        currentEncounter.startTime = ability.Timestamp;
                        Debug.WriteLine($"Start of fight: {ability.Timestamp}");
                        currentEncounter.startedEncounter = true;

                        currentEncounter.AdjustTimeSpans(); //Makes time negative since actions happened before the start
                    }

                    if(currentEncounter.startedEncounter) 
                    {
                        currentEncounter.events.Add(new ReportEvent
                        {
                            EventTime = ability.Timestamp.Subtract(currentEncounter.startTime),
                            EventDescription = $"{ability.ActorName} prepares {ability.SkillName} on {ability.TargetName} {ability.GetAbilityDamageInformation()}",
                            EventType = EventType.Summary | (ability.AbilityState == AbilityState.Damage? EventType.DamageDone : EventType.Healing)
                        });

                        currentEncounter.abilities.Add(ability);
                    }
                    
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
                        currentEncounter.events.Add(new ReportEvent
                        {
                            EventTime = death.Timestamp.Subtract(currentEncounter.startTime),
                            EventDescription = $"{death.ActorName} dies."
                        });
                    }

                    break;
                case LogMessageType.NetworkBuff:
                    NetworkBuff networkBuff = ReadNetworkBuff(lineContents);

                    if(currentEncounter.startedEncounter)
                    {
                        currentEncounter.events.Add(new ReportEvent
                        {
                            EventTime = networkBuff.Timestamp.Subtract(currentEncounter.startTime),
                            EventDescription = $"{networkBuff.TargetName} gains {networkBuff.SkillName} from {networkBuff.ActorName}",
                            EventType = EventType.Summary
                        });
                    }

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

                    if(currentEncounter.startedEncounter)
                    {
                        if(lineContents[3] == "40000010") //Ending Wipe
                        {
                            currentEncounter.endedEncounter = true;
                            currentEncounter.endTime = DateTime.Parse(lineContents[1]);

                            encounters.Add(currentEncounter);

                            Debug.WriteLine($"A wipe has occurred... It took {currentEncounter.endTime.Subtract(currentEncounter.startTime)}");

                            currentEncounter.DumpSummaryToFile(encounterDirectoryInfo);

                            currentEncounter = (Encounter)currentEncounter.Clone();
                        }
                    }

                    break;
                case LogMessageType.NetworkNameToggle:
                    break;
                case LogMessageType.NetworkTether:
                    break;
                case LogMessageType.NetworkLimitBreak:
                    LimitBreak limitBreak = ReadLimitBreak(lineContents);

                    if(currentEncounter.startedEncounter)
                    {
                        currentEncounter.events.Add(new ReportEvent
                        {
                            EventTime = limitBreak.Timestamp.Subtract(currentEncounter.startTime),
                            EventDescription = $"The limit break guage has been updated to {limitBreak.LimitBreakGuage}. {limitBreak.MaxLimitBreakNumber} bars are available.",
                            EventType = EventType.Summary
                        });
                    }
                    
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

                    if (currentEncounter.startedEncounter && !currentEncounter.endedEncounter && currentEncounter.AreRequiredBossesDead())
                    {
                        currentEncounter.endedEncounter = true;
                        currentEncounter.endTime = result.Timestamp;
                        currentEncounter.isCleared = true;

                        encounters.Add(currentEncounter);

                        Debug.WriteLine($"Encounter cleared!!! It took {currentEncounter.endTime.Subtract(currentEncounter.startTime)}");

                        currentEncounter.DumpSummaryToFile(encounterDirectoryInfo);

                        currentEncounter = (Encounter)currentEncounter.Clone();
                       
                    }

                    if (currentEncounter.startedEncounter)
                    {
                        combatant = currentEncounter.GetCombatantFromID(result.ActorID);

                        if (combatant != null)
                        {
                            int healthChange = combatant.Health - result.Health;

                            combatant.Health.CurrentHP -= (uint)healthChange;

                            NetworkAbility networkAbility = currentEncounter.abilities.Where(ability => ability.Damage == (uint)Math.Abs(healthChange)).FirstOrDefault(); 

                            if(networkAbility != null)
                            {
                                currentEncounter.events.Add(new ReportEvent
                                {
                                    EventTime = result.Timestamp.Subtract(currentEncounter.startTime),
                                    EventDescription = $"{networkAbility.ActorName} {networkAbility.SkillName} {networkAbility.TargetName} {networkAbility.Damage}",
                                    EventType = EventType.Summary | (networkAbility.AbilityState == AbilityState.Damage ? EventType.DamageDone : EventType.Healing)
                                });

                                currentEncounter.abilities.Remove(networkAbility);

                                Combatant caster = currentEncounter.GetCombatantFromID(networkAbility.ActorID);

                                AbilityInfo abilityInfo = caster.AbilityInfo[networkAbility.SkillName];

                                abilityInfo.HitCount++;

                                if(networkAbility.AbilityState == AbilityState.Damage)
                                {
                                    abilityInfo.DamageInformation.TotalDamageDone += (uint)healthChange;
                                    abilityInfo.DamageInformation.DPS = abilityInfo.DamageInformation.TotalDamageDone / (result.Timestamp.Subtract(currentEncounter.startTime).TotalSeconds);
                                }
                                else if(networkAbility.AbilityState == AbilityState.Healing)
                                {
                                    abilityInfo.HealingInformation.TotalHealingDone += (uint)Math.Abs(healthChange);
                                    abilityInfo.HealingInformation.HPS = abilityInfo.HealingInformation.TotalHealingDone / (result.Timestamp.Subtract(currentEncounter.startTime).TotalSeconds);
                                }
                                
                            }
                        }
                    }

                    break;
                case LogMessageType.NetworkStatusList:
                    NetworkStatusList networkStatusList = ReadNetworkStatusList(lineContents);

                    if (currentEncounter.startedEncounter)
                    {
                        combatant = currentEncounter.GetCombatantFromID(networkStatusList.TargetID);

                        if (combatant != null)
                        {
                            int healthChange = combatant.Health - networkStatusList.Health;

                            combatant.Health.CurrentHP -= (uint)healthChange;

                        }
                    }


                    foreach (BossInfo boss in currentEncounter.bosses)
                    {
                        if (networkStatusList.TargetName == boss.Name && networkStatusList.Health.CurrentHP == 0 && boss.HasToDie)
                        {
                            boss.IsDead = true;
                        }
                    }

                    //Figure out the damage the skill inflicted and then go back and credit the ability for that damage

                    if (currentEncounter.startedEncounter && !currentEncounter.endedEncounter && currentEncounter.AreRequiredBossesDead())
                    {
                        currentEncounter.endedEncounter = true;
                        currentEncounter.endTime = networkStatusList.Timestamp;
                        currentEncounter.isCleared = true;

                        encounters.Add(currentEncounter);

                        Debug.WriteLine($"Encounter cleared!!! It took {currentEncounter.endTime.Subtract(currentEncounter.startTime)}");

                        currentEncounter.DumpSummaryToFile(encounterDirectoryInfo);

                        currentEncounter = (Encounter)currentEncounter.Clone();
                    }

                    break;
                case LogMessageType.NetworkUpdateHp:
                    combatant = ReadNetworkUpdateHP(lineContents);

                    if(currentEncounter.startedEncounter)
                    {
                        Combatant oldCombatant = currentEncounter.GetCombatantFromID(combatant.ID);
                        if (oldCombatant != null)
                        {
                            oldCombatant.Health = combatant.Health;
                        }
                    }

                    break;
                case LogMessageType.Settings:
                    string[] settings = lineContents[2].Split(',');

                    //Debug.WriteLine("ACT Settings:");
                    foreach (string setting in settings)
                    {
                        //Debug.WriteLine(setting.Trim());
                    }

                    break;
                case LogMessageType.Process:
                    break;
                case LogMessageType.Debug:
                    break;
                case LogMessageType.PacketDump:
                    break;
                case LogMessageType.Version:
                    //Debug.WriteLine($"You are using {lineContents[2]}");
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
            Combatant combatant = new Combatant
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

            if(JobData.jobInfo.ContainsKey(combatant.JobID))
            {
                combatant.JobInformation = JobData.jobInfo[combatant.JobID];
            }


            return combatant;
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
                Flags = Convert.ToUInt32(lineContents[8], 16),
                DamageHex = lineContents[9],
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

            while (index < lineContents.Length - 1)
            {
                //Read in 3 at a time
                //statuses.Add(new Status
                //{
                //    EffectID = lineContents.Length <= 4 ? 0 : Convert.ToUInt32(lineContents[index].Substring(0, lineContents[index].Length - 4), 16),
                //    OtherInfo = Convert.ToUInt32(lineContents[index].Substring(lineContents[index].Length - 4), 16),
                //    Duration = Convert.ToUInt32(lineContents[index + 1], 16),
                //    ActorID = Convert.ToUInt32(lineContents[index + 2], 16)
                //});
                index += 3;
            }


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
                StatusList = statuses
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
                SkillID = Convert.ToUInt32(lineContents[2], 16),
                SkillName = lineContents[3],
                Duration = Convert.ToSingle(lineContents[4]),
                ActorID = Convert.ToUInt32(lineContents[5], 16),
                ActorName = lineContents[6],
                TargetID = Convert.ToUInt32(lineContents[7], 16),
                TargetName = lineContents[8],
                TargetMaxHP = string.IsNullOrEmpty(lineContents[10]) ? 0 : Convert.ToUInt32(lineContents[10]),
                TargetMaxMP = string.IsNullOrEmpty(lineContents[11]) ? 0 : Convert.ToUInt32(lineContents[11])
            };
        }

        private Combatant ReadNetworkUpdateHP(string[] lineContents)
        {
            return new Combatant
            {
                ID = Convert.ToUInt32(lineContents[2], 16),
                Name = lineContents[3],
                Health = new Health
                {
                    CurrentHP = string.IsNullOrEmpty(lineContents[4]) ? 0 : Convert.ToUInt32(lineContents[4]),
                    MaxHP = string.IsNullOrEmpty(lineContents[5]) ? 0 : Convert.ToUInt32(lineContents[5]),
                    CurrentMP = string.IsNullOrEmpty(lineContents[6]) ? 0 : Convert.ToUInt32(lineContents[6]),
                    MaxMP = string.IsNullOrEmpty(lineContents[7]) ? 0 : Convert.ToUInt32(lineContents[7]),
                    CurrentTP = string.IsNullOrEmpty(lineContents[8]) ? 0 : Convert.ToUInt32(lineContents[8]),
                    MaxTP = string.IsNullOrEmpty(lineContents[9]) ? 0 : Convert.ToUInt32(lineContents[9])
                }
            };
        }

        #endregion
    }
}
