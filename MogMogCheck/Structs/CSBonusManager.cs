using System.Runtime.InteropServices;
using FFXIVClientStructs.Interop.Attributes;

namespace MogMogCheck.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x178)]
public unsafe partial struct CSBonusManager
{
    [StaticAddress("48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 63 D0", 3, true)]
    public static partial CSBonusManager* Instance();

    [FieldOffset(0x08)] public ushort State; // 1 = Pending, 2 = Underway, 3 = Finished

    [FieldOffset(0x0C)] public uint BaseTime;
    [FieldOffset(0x10)] public uint SeasonTarget;
    [FieldOffset(0x14)] public byte IsOpenShop;
    [FieldOffset(0x15)] public byte IsOpenMission;

    [FieldOffset(0x18)] public uint SeedBase; // see "E8 ?? ?? ?? ?? 8B C0 33 D2 48 F7 F3 44 8B C2"

    [FieldOffset(0x2C)] public fixed byte WBAchieveFlag[20];
    [FieldOffset(0x40)] public fixed byte PMAchieveFlag[20];
    [FieldOffset(0x54)] public fixed byte MMAchieveFlag[4];

    [FieldOffset(0x7C)] public fixed byte WBReceiveFlag[20];
    [FieldOffset(0x90)] public fixed byte PMReceiveFlag[20];
    [FieldOffset(0xA4)] public fixed byte MMReceiveFlag[4];

    [FieldOffset(0xBA)] public byte WBCount;

    [FieldOffset(0xBC)] public byte PMCount;

    [FieldOffset(0xBE)] public byte MMCount;

#if DEBUG
    [MemberFunction("E8 ?? ?? ?? ?? 48 8B CB E8 ?? ?? ?? ?? 48 8B D8 E8")]
    public partial void RequestData();

    [MemberFunction("E8 ?? ?? ?? ?? 33 C0 E9 ?? ?? ?? ?? 48 03 CF 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 49 8B 0F")]
    public partial void PrintDebugInfo();
#endif
}
