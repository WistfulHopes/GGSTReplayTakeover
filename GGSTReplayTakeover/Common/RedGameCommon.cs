using UE4SSDotNetFramework.Framework;

namespace GGSTReplayTakeover.Common;

public class RedGameCommon : ObjectReference
{
    public RedGameCommon(IntPtr pointer)
        : base(pointer)
    {
        Pointer = pointer;
    }
    
    public RedGameCommon(ObjectReference? @base)
    {
        if (@base is not null) Pointer = @base.Pointer;
    }

    public enum GameMode {
        DebugBattle = 0x0,
        Advertise = 0x1,
        MaintenanceVs = 0x2,
        Arcade = 0x3,
        Mom = 0x4,
        Sparring = 0x5,
        Versus = 0x6,
        VersusPreinstall = 0x7,
        Training = 0x8,
        Tournament = 0x9,
        RannyuVersus = 0xA,
        Event = 0xB,
        Survival = 0xC,
        Story = 0xD,
        MainMenu = 0xE,
        Tutorial = 0xF,
        LobbyTutorial = 0x10,
        Challenge = 0x11,
        Kentei = 0x12,
        Mission = 0x13,
        Gallery = 0x14,
        Library = 0x15,
        Network = 0x16,
        Replay = 0x17,
        LobbySub = 0x18,
        MainMenuQuickBattle = 0x19,
        Undecided = 0x1A,
        Invalid = 0x1B,
    }

    public static GetGameModeFunc? GetGameModeInternal;
    public delegate GameMode GetGameModeFunc(IntPtr @this);

    public GameMode GetGameMode()
    {
        if (Pointer == 0) return GameMode.Invalid;
        return GetGameModeInternal?.Invoke(Pointer) ?? GameMode.Invalid;
    }
}