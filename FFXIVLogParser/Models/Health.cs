using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFXIVLogParser.Models
{
    class Health : IEquatable<Health>
    {
        public uint CurrentHP { get; set; }
        public uint MaxHP { get; set; }

        public uint CurrentMP { get; set; }
        public uint MaxMP { get; set; }

        public uint CurrentTP { get; set; }
        public uint MaxTP { get; set; }

        public bool Equals(Health other)
        {
            return CurrentHP == other.CurrentHP && CurrentMP == other.CurrentMP;
        }
    }
}
