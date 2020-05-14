using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class NetworkAbility
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


        public string GetAbilityDamageInformation(uint value)
        {
            string damageContent = "";

            BitArray bits = new BitArray(new int[] { Convert.ToInt32(value) });

            if (bits.And(new BitArray(new int[] { 4 }))
            {

            }

            return damageContent;
        }

    }
}
