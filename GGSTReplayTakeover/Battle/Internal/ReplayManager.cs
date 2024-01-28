using System.Runtime.InteropServices;

namespace GGSTReplayTakeover.Battle.Internal;

[StructLayout(LayoutKind.Explicit)]
public struct ReplayManager
{
    [FieldOffset(0x5)]
    public bool NextSeq;
    [FieldOffset(0x6)]
    public bool ChangeSeqDisable;
    [FieldOffset(0x7)]
    public bool PrevSeq;
    [FieldOffset(0x8)]
    public int PlayScale;
}