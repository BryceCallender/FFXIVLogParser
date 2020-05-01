using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FFXIVLogParser
{
    class Parser
    {
        public void ParseLine(string line)
        {
            string[] lineContents = line.Split('|');

            int.TryParse(lineContents[0], out int messageType);

            switch ((LogMessageType)messageType)
            {
                case LogMessageType.LogLine:
                    break;
                case LogMessageType.ChangeZone:
                    break;
                case LogMessageType.ChangePrimaryPlayer:
                    break;
                case LogMessageType.AddCombatant:
                    break;
                case LogMessageType.RemoveCombatant:
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
                    foreach(string setting in settings)
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
    }
}
