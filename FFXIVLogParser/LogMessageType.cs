namespace FFXIVLogParser
{
    public enum LogMessageType
    {
        LogLine = 0,
        ChangeZone = 1,
        ChangePrimaryPlayer = 2,
        AddCombatant = 3,
        RemoveCombatant = 4,
        AddBuff = 5,
        RemoveBuff = 6,
        FlyingText = 7,
        OutgoingAbility = 8,
        IncomingAbility = 10, // 0x0000000A
        PartyList = 11, // 0x0000000B
        PlayerStats = 12, // 0x0000000C
        CombatantHP = 13, // 0x0000000D
        ParsedPartyMember = 14, // 0x0000000E
        NetworkStartsCasting = 20, // 0x00000014
        NetworkAbility = 21, // 0x00000015
        NetworkAOEAbility = 22, // 0x00000016
        NetworkCancelAbility = 23, // 0x00000017
        NetworkDoT = 24, // 0x00000018
        NetworkDeath = 25, // 0x00000019
        NetworkBuff = 26, // 0x0000001A
        NetworkTargetIcon = 27, // 0x0000001B
        NetworkTargetMarker = 29, // 0x0000001D
        NetworkBuffRemove = 30, // 0x0000001E
        NetworkGauge = 31, // 0x0000001F
        NetworkWorld = 32, // 0x00000020
        Network6D = 33, // 0x00000021
        NetworkNameToggle = 34, // 0x00000022
        NetworkTether = 35, // 0x00000023
        NetworkLimitBreak = 36, // 0x00000024
        NetworkEffectResult = 37, // 0x00000025
        NetworkStatusList = 38, // 0x00000026
        NetworkUpdateHp = 39, // 0x00000027
        Settings = 249, // 0x000000F9
        Process = 250, // 0x000000FA
        Debug = 251, // 0x000000FB
        PacketDump = 252, // 0x000000FC
        Version = 253, // 0x000000FD
        Error = 254, // 0x000000FE
        Timer = 255, // 0x000000FF
    }
}
