using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVLogParser
{
    public enum JobType
    {
        Tank,
        DPS,
        Healer
    }

    struct JobInfo
    {
        public string JobName { get; set; }
        public JobType JobCategory { get; set; }
    }

    static class JobData
    {
        public static Dictionary<uint, JobInfo> jobInfo = new Dictionary<uint, JobInfo>()
        {
            //Tanks
            { 0x13, new JobInfo { JobName = "Paladin", JobCategory = JobType.Tank } },
            { 0x15, new JobInfo { JobName = "Warrior", JobCategory = JobType.Tank } },
            { 0x20, new JobInfo { JobName = "Dark Knight", JobCategory = JobType.Tank } },
            { 0x25, new JobInfo { JobName = "Gunbreaker", JobCategory = JobType.Tank } },

            //Healers
            { 0x18, new JobInfo { JobName = "White Mage", JobCategory = JobType.Healer } },
            { 0x1c, new JobInfo { JobName = "Scholar", JobCategory = JobType.Healer } },
            { 0x21, new JobInfo { JobName = "Astrologian", JobCategory = JobType.Healer } },

            //Melee DPS
            { 0x1e, new JobInfo { JobName = "Ninja", JobCategory = JobType.DPS } },
            { 0x22, new JobInfo { JobName = "Samurai", JobCategory = JobType.DPS } },
            { 0x16, new JobInfo { JobName = "Dragoon", JobCategory = JobType.DPS } },
            { 0x14, new JobInfo { JobName = "Monk", JobCategory = JobType.DPS } },

            //Ranged DPS
            { 0x17, new JobInfo { JobName = "Bard", JobCategory = JobType.DPS } },
            { 0x1f, new JobInfo { JobName = "Machinist", JobCategory = JobType.DPS } },
            { 0x26, new JobInfo { JobName = "Dancer", JobCategory = JobType.DPS } },

            //Magic Ranged DPS
            { 0x1b, new JobInfo { JobName = "Summoner", JobCategory = JobType.DPS } },
            { 0x23, new JobInfo { JobName = "Red Mage", JobCategory = JobType.DPS } },
            { 0x19, new JobInfo { JobName = "Black Mage", JobCategory = JobType.DPS } }
        };
    }
}
