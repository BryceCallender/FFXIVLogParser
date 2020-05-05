using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser
{
    public static class ZoneData
    {
        public static Dictionary<string, List<string>> zoneInfo = new Dictionary<string, List<string>>()
        {
            { "Eden's Verse: Fulmination (Savage)", new List<string>() { "Ramuh" } },
            { "Eden's Verse: Furor (Savage)", new List<string>() { "Ifrit", "Garuda", "Raktapaksa" } },
            { "Eden's Verse: Iconoclasm (Savage)", new List<string>() { "The Idol of Darkness", "Idolatry" } },
            { "Eden's Verse: Refulgence (Savage)", new List<string>() { "Shiva" } }
        }; //Area gives boss name's could be one and could be many like Ifrit and Garuda E6
    }
}
