using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models
{
    enum EventType
    {
        Summary = 0x1,
        DamageDone = 0x2,
        DamageTaken = 0x4,
        Healing = 0x8,
    }

    class ReportEvent
    {
        public TimeSpan EventTime { get; set; }
        public string EventDescription { get; set; }
        public EventType EventType { get; set; } = EventType.Summary;
    }
}
