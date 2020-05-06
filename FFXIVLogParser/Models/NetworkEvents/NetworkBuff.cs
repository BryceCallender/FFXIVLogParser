using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class NetworkBuff
    {
        public DateTime Timestamp { get; set; }
        public uint SkillID { get; set; }
        public string SkillName { get; set; }
        public float Duration { get; set; }
        public uint ActorID { get; set; }
        public string ActorName { get; set; }
        public uint TargetID { get; set; }
        public string TargetName { get; set; }
        public uint TargetMaxHP { get; set; }
        public uint TargetMaxMP { get; set; }
    }
}
