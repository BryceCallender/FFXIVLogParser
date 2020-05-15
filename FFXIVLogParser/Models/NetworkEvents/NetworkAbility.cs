using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    enum AbilityState
    {
        Damage,
        Healing,
        Buff,
        None
    }

    class NetworkAbility: IEquatable<NetworkAbility>
    {
        public DateTime Timestamp { get; set; }
        public uint ActorID { get; set; }
        public string ActorName { get; set; }
        public uint SkillID { get; set; }
        public string SkillName { get; set; }
        public uint TargetID { get; set; }
        public string TargetName { get; set; }
        public Health ActorHealth { get; set; } = new Health();
        public Position ActorPosition { get; set; } = new Position();
        public Health TargetHealth { get; set; } = new Health();
        public Position TargetPosition { get; set; } = new Position();

        public uint Flags { get; set; }
        public ulong Damage { get; set; } 
        public AbilityState AbilityState { get; set; } = AbilityState.None;

        public string DamageHex { get; set; }

        public void SetAbilityDamage()
        {
            if(DamageHex.Length > 4)
            {
                char damageModifier = DamageHex[DamageHex.Length - 4];
                if (damageModifier == '1')
                {
                    Damage = 0;
                }
                else if (damageModifier == '4')
                {
                    //ABCD where each letter is 2 hex values
                    DamageHex = DamageHex.PadLeft(8, '0');

                    string A = DamageHex[0..2];
                    string B = DamageHex[2..4];
                    string D = DamageHex[6..8];

                    int bVal = Convert.ToInt32(B, 16);
                    int dVal = Convert.ToInt32(D, 16);

                    int newVal = bVal - dVal;

                    string largeDamageString = D + A + newVal.ToString("X2");// D A (B - D)

                    Damage = Convert.ToUInt64(largeDamageString, 16);
                }
                else
                {
                    Damage = Convert.ToUInt32(DamageHex.Substring(0, DamageHex.Length - 4), 16);
                }
            }
            
        }


        public string GetAbilityDamageInformation()
        {
            StringBuilder damageContent = new StringBuilder();

            SetAbilityDamage();

            //0x01 = dodge
            //0x05 = blocked damage
            //0x06 = parried damage
            //0x33 = instant death

            if ((Flags & 0x3) == 0x3)
            {
                damageContent.Append($"{Damage}");
                AbilityState = AbilityState.Damage;

                if ((Flags & 0x100) == 0x100)
                {
                    damageContent.Append(" (CRITICAL)");
                }
                else if ((Flags & 0x200) == 0x200)
                {
                    damageContent.Append(" (DIRECT HIT)");
                }
                else if ((Flags & 0x300) == 0x300)
                {
                    damageContent.Append(" (CRITIAL DIRECT HIT)");
                }
            }
            //Check if we have healing
            else if ((Flags.ToString().Length == 1 && (Flags & 0x4) == 0x4) || (Flags & 0x10004) == 0x10004)
            {
                damageContent.Append($"+{Damage}");
                AbilityState = AbilityState.Healing;

                //Check if its a crit heal or not
                if ((Flags & 0x10004) == 0x10004)
                {
                    damageContent.Append(" (CRITICAL)");
                }
            }
            else
            {
                AbilityState = AbilityState.Buff;
            }

            return damageContent.ToString();
        }

        public bool Equals(NetworkAbility other)
        {
            return SkillID == other.SkillID && SkillName == other.SkillName;
        }
    }
}
