using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFXIVLogParser.Models
{
    class Health : IEquatable<Health>
    {
        public uint CurrentHP { get; set; } = 0;
        public uint MaxHP { get; set; } = 0;

        public uint CurrentMP { get; set; } = 0;
        public uint MaxMP { get; set; } = 0;

        public uint CurrentTP { get; set; } = 0;
        public uint MaxTP { get; set; } = 0;
         
        public bool Equals(Health other)
        {
            return CurrentHP == other.CurrentHP && CurrentMP == other.CurrentMP;
        }

        public static int operator -(Health a, Health b)
        {
            return (int)a.CurrentHP - (int)b.CurrentHP;
        }
    }
}
