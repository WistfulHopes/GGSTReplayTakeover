using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GGSTReplayTakeover.Battle;
using GGSTReplayTakeover.Battle.Internal;
using GGSTReplayTakeover.Battle.Internal.Enums;
using GGSTReplayTakeover.Common.Enums;
using UE4SSDotNetFramework.Framework;

namespace GGSTReplayTakeover.Common;

public static class Main
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern short GetKeyState(int key);

    private static IntPtr _updateBattleHook;
    private static UpdateBattleFunc? _updateBattleOrig;
    unsafe delegate void UpdateBattleFunc(RedGameStateBattle.RedGameStateBattleInternal* @this, float arg);
    private static unsafe delegate* unmanaged[Cdecl]<RedGameStateBattle.RedGameStateBattleInternal*, float, void> _updateBattlePtr;
    
    private static IntPtr _updateBattleInputHook;
    private static UpdateBattleInputFunc? _updateBattleInputOrig;
    unsafe delegate void UpdateBattleInputFunc(BattleInputAnalyzer* @this, RecFlag recFlag);
    private static unsafe delegate* unmanaged[Cdecl]<BattleInputAnalyzer*, RecFlag, void> _updateBattleInputPtr;

    private static IntPtr _updateSystemHook;
    private static UpdateSystemFunc? _updateSystemOrig;
    delegate void UpdateSystemFunc(IntPtr @this);
    private static unsafe delegate* unmanaged[Cdecl]<IntPtr, void> _updateSystemPtr;

    private static IntPtr _updateReplayHook;
    private static UpdateReplayFunc? _updateReplayOrig;
    delegate void UpdateReplayFunc(IntPtr @this);
    private static unsafe delegate* unmanaged[Cdecl]<IntPtr, void> _updateReplayPtr;

    private static GameKeyToRecFlagFunc? _gameKeyToRecFlag;
    delegate RecFlag GameKeyToRecFlagFunc(PadId padId, GameKeyFlag flag, bool isReverse);

    private static GetKeyOnFlagFunc? _getKeyOnFlag;
    delegate GameKeyFlag GetKeyOnFlagFunc(IntPtr @this);

    private static bool _scanSuccess = true;
    private static unsafe SystemRed* _system;
    private static unsafe BattleInputAnalyzer* _analyzer;
    private static unsafe RedGameStateBattle.RedGameStateBattleInternal* _gameState;
    private static bool _isTakeover;
    private static RedGameCommon? _gameCommon;
    private static RedGameCommon.GameMode _lastMode = RedGameCommon.GameMode.Invalid;
    
    private static bool CheckMode(RedGameCommon.GameMode cmp)
    {
        _gameCommon = new RedGameCommon(ObjectReference.Find("REDGameCommon"));
        _lastMode = _gameCommon.GetGameMode();
        return cmp == _lastMode;
    }

    [UnmanagedCallersOnly(CallConvs = new []{typeof(CallConvCdecl)})]
    private static unsafe void UpdateBattleHook(RedGameStateBattle.RedGameStateBattleInternal* @this, float deltaTime)
    {
        _gameState = @this;
        
        if (!CheckMode(RedGameCommon.GameMode.Replay))
        {
            _updateBattleOrig!(@this, deltaTime);
            return;
        }

        if ((GetKeyState(0x71) & 0x8000) == 0x8000)
        {
            _isTakeover = true;
            _analyzer = &_gameState->ObjectManager->InputAnalyzerP1;
        }

        if ((GetKeyState(0x72) & 0x8000) == 0x8000)
        {
            _isTakeover = true;
            _analyzer = &_gameState->ObjectManager->InputAnalyzerP2;
        }

        if ((GetKeyState(0x73) & 0x8000) == 0x8000)
        {
            _isTakeover = false;
            _analyzer = null;
        }
        
        _updateBattleOrig!(@this, deltaTime);
    }

    [UnmanagedCallersOnly(CallConvs = new []{typeof(CallConvCdecl)})]
    private static unsafe void UpdateBattleInputHook(BattleInputAnalyzer* @this, RecFlag recFlag)
    {
        if (_lastMode != RedGameCommon.GameMode.Replay || @this != _analyzer)
        {
            _updateBattleInputOrig!(@this, recFlag);
            return;
        }
            
        const PadId pad = PadId.PadId1Con;
        var gameKeyFlag = _getKeyOnFlag!(_system->BattleKeyP1);
        var newRecFlag = _gameKeyToRecFlag!(pad, gameKeyFlag, false);

        _updateBattleInputOrig!(@this, newRecFlag);
    }

    [UnmanagedCallersOnly(CallConvs = new []{typeof(CallConvCdecl)})]
    private static unsafe void UpdateSystemHook(IntPtr @this)
    {
        _updateSystemOrig!(@this);
        if (_system == null) _system = (SystemRed*)@this;
    }

    [UnmanagedCallersOnly(CallConvs = new []{typeof(CallConvCdecl)})]
    private static void UpdateReplayHook(IntPtr @this)
    {
        if (!_isTakeover) _updateReplayOrig!(@this);
    }
    
    public static void StartMod()
    {
        var updateBattleAddr = Hooking.SigScan("40 53 57 41 54 41 55 48 81 EC 88 00 00 00");
        if (updateBattleAddr != 0)
        {
            unsafe
            {
                IntPtr funcPtr = 0;
                _updateBattlePtr = &UpdateBattleHook;
                void Del(RedGameStateBattle.RedGameStateBattleInternal* @this, float f) => _updateBattlePtr(@this, f);

                _updateBattleHook = Hooking.Hook(updateBattleAddr, Marshal.GetFunctionPointerForDelegate((UpdateBattleFunc)Del), ref funcPtr);
                _updateBattleOrig = Marshal.GetDelegateForFunctionPointer<UpdateBattleFunc>(funcPtr);
            }
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of UpdateBattle");
            _scanSuccess = false;
        }

        var updateBattleInputAddr = Hooking.SigScan("0F B7 41 02 44 0F B7 D2");
        if (updateBattleInputAddr != 0)
        {
            unsafe
            {
                IntPtr funcPtr = 0;
                _updateBattleInputPtr = &UpdateBattleInputHook;
                void Del(BattleInputAnalyzer* @this, RecFlag flag) => _updateBattleInputPtr(@this, flag);

                _updateBattleInputHook = Hooking.Hook(updateBattleInputAddr, Marshal.GetFunctionPointerForDelegate((UpdateBattleInputFunc)Del), ref funcPtr);
                _updateBattleInputOrig = Marshal.GetDelegateForFunctionPointer<UpdateBattleInputFunc>(funcPtr);
            }
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of UpdateBattleInput");
            _scanSuccess = false;
        }
                
        var updateSystemAddr = Hooking.SigScan("48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 33 DB 48 8B F1 8B FB");
        if (updateSystemAddr != 0)
        {
            unsafe
            {
                IntPtr funcPtr = 0;
                _updateSystemPtr = &UpdateSystemHook;
                void Del(IntPtr @this) => _updateSystemPtr(@this);

                _updateSystemHook = Hooking.Hook(updateSystemAddr, Marshal.GetFunctionPointerForDelegate((UpdateSystemFunc)Del), ref funcPtr);
                _updateSystemOrig = Marshal.GetDelegateForFunctionPointer<UpdateSystemFunc>(funcPtr);
            }
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of UpdateSystem");
            _scanSuccess = false;
        }

        var updateReplayAddr = Hooking.SigScan("40 53 41 56 48 83 EC 68 45 32 F6");
        if (updateReplayAddr != 0)
        {
            unsafe
            {
                IntPtr funcPtr = 0;
                _updateReplayPtr = &UpdateReplayHook;
                void Del(IntPtr @this) => _updateReplayPtr(@this);

                _updateReplayHook = Hooking.Hook(updateReplayAddr, Marshal.GetFunctionPointerForDelegate((UpdateReplayFunc)Del), ref funcPtr);
                _updateReplayOrig = Marshal.GetDelegateForFunctionPointer<UpdateReplayFunc>(funcPtr);
            }
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of UpdateReplay");
            _scanSuccess = false;
        }

        var getGameModeAddr = Hooking.SigScan("0F B6 81 F0 02 00 00 C3");
        if (getGameModeAddr != 0)
        {
            RedGameCommon.GetGameModeInternal = Marshal.GetDelegateForFunctionPointer<RedGameCommon.GetGameModeFunc>(getGameModeAddr);
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of GetGameMode");
            _scanSuccess = false;
        }
        
        var gameKeyToRecFlagAddr = Hooking.SigScan("89 54 24 10 89 4C 24 08 53 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 38");
        if (gameKeyToRecFlagAddr != 0)
        {
            _gameKeyToRecFlag = Marshal.GetDelegateForFunctionPointer<GameKeyToRecFlagFunc>(gameKeyToRecFlagAddr);
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of GameKeyToRecFlag");
            _scanSuccess = false;
        }
        
        var getKeyOnFlagAddr = Hooking.SigScan("48 8B 41 60 8B 40 18");
        if (getKeyOnFlagAddr != 0)
        {
            _getKeyOnFlag = Marshal.GetDelegateForFunctionPointer<GetKeyOnFlagFunc>(getKeyOnFlagAddr);
        }
        else
        {
            Debug.Log(LogLevel.Warning, "Failed to get addr of GetKeyOnFlag");
            _scanSuccess = false;
        }

        if (!_scanSuccess) Debug.Log(LogLevel.Warning, "One or more functions weren't found! Please wait for an update.");
    }

    public static void StopMod()
    {
        Hooking.Unhook(_updateBattleHook);
        Hooking.Unhook(_updateBattleInputHook); 
        Hooking.Unhook(_updateSystemHook);
        Hooking.Unhook(_updateReplayHook);
    }
}