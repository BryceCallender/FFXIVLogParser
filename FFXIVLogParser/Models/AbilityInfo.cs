using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models
{
    class AbilityInfo
    {
        public uint CastAmount { get; set; }
        public double AveragePerCast { get; set; }
        public uint HitCount { get; set; }
        public double AveragePerHit { get; set; }

        public DamageInfo DamageInformation { get; set; } = new DamageInfo();
        public HealingInfo HealingInformation { get; set; } = new HealingInfo();

        public AbilityInfo()
        {
            CastAmount = 0;
            AveragePerCast = 0;
            HitCount = 0;
            AveragePerHit = 0;
        }
    }
}
