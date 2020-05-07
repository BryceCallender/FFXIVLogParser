using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser
{
    public class BossInfo
    {
        public string Name { get; set; }
        public uint Level { get; set; }
        public uint MaxHP { get; set; }
        public bool HasToDie { get; set; } = false;
        public bool IsDead { get; set; } = false;
    }

    public static class ZoneData
    {
        public static Dictionary<string, List<BossInfo>> zoneInfo = new Dictionary<string, List<BossInfo>>()
        {
            { "Eden's Gate: Resurrection (Savage)", new List<BossInfo>() { new BossInfo { Name = "Eden Prime", Level = 80, MaxHP = 29709520, HasToDie = true } } },
            { "Eden's Gate: Inundation (Savage)", new List<BossInfo>() { new BossInfo { Name = "Leviathan", Level = 80, MaxHP = 47397000, HasToDie = true } } },


            { "Eden's Verse: Fulmination (Savage)", new List<BossInfo>() { new BossInfo { Name = "Ramuh", Level = 80, MaxHP = 54990880, HasToDie = true } } },
            { "Eden's Verse: Furor (Savage)", new List<BossInfo>() { new BossInfo { Name = "Ifrit", Level = 80, MaxHP = 26215240 }, new BossInfo { Name = "Garuda", Level = 80, MaxHP = 26215240 } , new BossInfo { Name = "Raktapaksa", Level = 80, MaxHP = 52430480, HasToDie = true } } },
            { "Eden's Verse: Iconoclasm (Savage)", new List<BossInfo>() { new BossInfo { Name = "The Idol of Darkness", Level = 80, HasToDie = true } } },
            { "Eden's Verse: Refulgence (Savage)", new List<BossInfo>() {  new BossInfo { Name = "Shiva", Level = 80, HasToDie = true } } }
        }; //Area gives boss name's could be one and could be many like Ifrit and Garuda E6
    }
}
