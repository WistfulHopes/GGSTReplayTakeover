using System.Runtime.InteropServices;
using GGSTReplayTakeover.Battle.Internal;
using UE4SSDotNetFramework.Framework;

namespace GGSTReplayTakeover.Battle;

public class RedGameStateBattle : ObjectReference
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RedGameStateBattleInternal
    {
        [FieldOffset(0x298)] public IntPtr GameCommon;
        [FieldOffset(0xBA0)] public unsafe BattleObjectManager* ObjectManager;
        [FieldOffset(0xBB8)] public unsafe BattleState* BattleState;
    }

    public RedGameStateBattleInternal Instance => Marshal.PtrToStructure<RedGameStateBattleInternal>(Pointer);

    public RedGameStateBattle(IntPtr pointer) : base(pointer)
    {
        Pointer = pointer;
    }
}