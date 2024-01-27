using System.Runtime.InteropServices;

namespace GGSTReplayTakeover.Common;

[StructLayout(LayoutKind.Explicit)]
public struct SystemRed
{
    [FieldOffset(0x1F8)] public IntPtr BattleKeyP1;
}