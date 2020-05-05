using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class NetworkDeath
    {
        public DateTime Timestamp { get; set; }
        public uint ActorID { get; set; }
        public string ActorName { get; set; }
        public uint TargetID { get; set; }
        public string TargetName { get; set; }
    }
}
