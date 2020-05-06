using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    public struct Status
    {
        public uint EffectID { get; set; }
        public float Duration { get; set; }
        public uint ActorID { get; set; }
    }

    class NetworkStatusList
    {
        public DateTime Timestamp { get; set; }
        public uint TargetID { get; set; }
        public string TargetName { get; set; }

        public uint JobID { get; set; }
        public uint Level1 { get; set; }
        public uint Level2 { get; set; }
        public uint Level3 { get; set; }

        public Health Health { get; set; } = new Health();
        public Position Position { get; set; } = new Position();

        public List<Status> StatusList { get; set; } = new List<Status>();
    }
}
