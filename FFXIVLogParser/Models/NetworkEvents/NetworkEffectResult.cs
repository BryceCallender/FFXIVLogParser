using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class NetworkEffectResult
    {
        public DateTime Timestamp { get; set; }
        public uint ActorID { get; set; }
        public string ActorName { get; set; }
        public uint Sequence { get; set; }
        public Health Health { get; set; } = new Health();
        public Position Position { get; set; } = new Position();
    }
}
