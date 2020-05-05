using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class NetworkDoT
    {
        public DateTime Timestamp { get; set; }
        public uint TargetID { get; set; }
        public string TargetName { get; set; }
        public bool IsDamage { get; set; }
        public uint BuffID { get; set; }
        public uint Damage { get; set; }
        public Health TargetHealth { get; set; } = new Health();
        public Position TargetPosition { get; set; } = new Position();
    }
}
