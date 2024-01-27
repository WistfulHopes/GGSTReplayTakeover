using System.Runtime.InteropServices;

namespace GGSTReplayTakeover.Battle.Internal;

[StructLayout(LayoutKind.Explicit)]
public struct BattleObjectManager
{
    [FieldOffset(0x37A8)]
    public BattleInputAnalyzer InputAnalyzerP1;
    [FieldOffset(0x3922)]
    public BattleInputAnalyzer InputAnalyzerP2;
}