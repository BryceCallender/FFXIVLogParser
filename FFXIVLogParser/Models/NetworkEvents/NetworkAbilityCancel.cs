using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class NetworkAbilityCancel
    {
        public DateTime Timestamp { get; set; }
        public uint ActorID { get; set; }
        public string ActorName { get; set; }
        public uint SkillID { get; set; }
        public string SkillName { get; set; }
        public bool Cancelled { get; set; }
        public bool Interrupted { get; set; }
    }
}
