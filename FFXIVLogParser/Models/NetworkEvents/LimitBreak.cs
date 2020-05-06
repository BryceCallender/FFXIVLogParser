using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models.NetworkEvents
{
    class LimitBreak
    {
        public DateTime Timestamp { get; set; }
        public uint LimitBreakGuage { get; set; }
        public uint MaxLimitBreakNumber { get; set; }
    }
}
