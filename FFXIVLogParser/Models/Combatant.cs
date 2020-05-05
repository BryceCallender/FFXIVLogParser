using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFXIVLogParser.Models
{
    class Combatant : IEquatable<Combatant>
    {
        public uint ID { get; set; }
        public string? Name { get; set; }
        public uint JobID { get; set; }
        public uint Level { get; set; }
        public uint OwnerID { get; set; }
        public uint WorldID { get; set; }
        public string? WorldName { get; set; }
        public uint BNpcNameID { get; set; }
        public uint BNpcID { get; set; }

        public Health Health { get; set; } = new Health();
        public Position Position { get; set; } = new Position();

        public DateTime Timestamp { get; set; }

        public bool Equals(Combatant other)
        {
            return ID.Equals(other.ID);
        }
    }
}
