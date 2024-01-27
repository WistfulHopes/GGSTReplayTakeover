using System.Runtime.InteropServices;

namespace GGSTReplayTakeover.Battle.Internal;

[StructLayout(LayoutKind.Explicit)]
public struct BattleState
{
    [FieldOffset(0x118)]
    public int RoundCount;
}