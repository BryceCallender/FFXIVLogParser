using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser.Models
{
    class ReportEvent
    {
        public TimeSpan EventTime { get; set; }
        public string EventDescription { get; set; }
    }
}
