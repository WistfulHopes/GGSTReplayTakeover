﻿namespace GGSTReplayTakeover.Common.Enums;

[Flags]
public enum GameKeyFlag
{
    Up = 0x1,
    Right = 0x2,
    Down = 0x4,
    Left = 0x8,
    LStickUp = 0x10,
    LStickRight = 0x20,
    LStickDown = 0x40,
    LStickLeft = 0x80,
    RStickUp = 0x100,
    RStickRight = 0x200,
    RStickDown = 0x400,
    RStickLeft = 0x800,
    StkMask = 0xF,
    StkDMask = 0x4,
    StkLrMask = 0xA,
    X = 0x1000,
    Y = 0x2000,
    A = 0x4000,
    B = 0x8000,
    R1 = 0x10000,
    R2 = 0x20000,
    L1 = 0x40000,
    L2 = 0x80000,
    L3 = 0x100000,
    R3 = 0x200000,
    Start = 0x400000,
    Select = 0x800000,
    Swall = 0xFFF000,
    Null = 0x0,
}