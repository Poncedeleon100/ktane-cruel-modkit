using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using UnityEditor.PackageManager;
using UnityEngine;
using KModkit;



public class TestPuzzle : Puzzle
{

    public TestPuzzle(CruelModkitScript Module, int ModuleID, ComponentInfo Info, bool Vanilla, bool[] Components) : base(Module, ModuleID, Info, Vanilla, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving the TestPuzzle.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Wires present: {1}.", ModuleID, Info.GetWireInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Wire LEDs present: {1}.", ModuleID, Info.GetWireLEDInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Button is {1}.", ModuleID, Info.GetButtonInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] LEDs present: {1}.", ModuleID, Info.GetLEDInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Symbols present: {1}.", ModuleID, Info.GetSymbolInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Alphanumeric keys present: {1}.", ModuleID, Info.GetAlphabetInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Arrows present: {1}.", ModuleID, Info.GetArrowsInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Identity is {1}, with item {2}, at the location {3}, and a rarity of {4}.", ModuleID, Info.IdentityNames[Info.Identity[0]], Info.IdentityItems[Info.Identity[1]], Info.IdentityLocations[Info.Identity[2]], Info.IdentityRarity[Info.Identity[3]]);
        Debug.LogFormat("[The Cruel Modkit #{0}] Bulb 1 is {1}, {2}, and {3}. Bulb 2 is {4}, {5}, and {6}. The O button is on the {7}.", ModuleID, Info.MainColors[Info.BulbColors[0]], Info.BulbInfo[0] ? "opaque" : "see-through", Info.BulbInfo[2] ? "on" : "off", Info.MainColors[Info.BulbColors[1]], Info.BulbInfo[1] ? "opaque" : "see-through", Info.BulbInfo[3] ? "on" : "off", Info.BulbInfo[4] ? "left" : "right");
        Debug.LogFormat("[The Cruel Modkit #{0}] Resistor strips present: {1}. Left letter is {2}, right letter is {3}.", ModuleID, Info.GetResistorInfo(), Info.ResistorText[0], Info.ResistorText[1]);
        Debug.LogFormat("[The Cruel Modkit #{0}] Widgets: Timer display is {1}. Word display is {2}. Number display is {3}. Morse code LED is {4}. Meter is {5} and at {6}%.", ModuleID, Info.TimerDisplay.ToString().PadLeft(2, '0'), Info.WordDisplay, Info.NumberDisplay, Info.Morse, (Info.MeterValue == 0) ? "none" : Info.MeterColors[Info.MeterColor], (100 * Math.Round(Info.MeterValue, 2)).ToString());
    }

    public override void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the component selection was [{2}] instead of [{3}].", ModuleID, Wire + 1, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            Debug.LogFormat("[The Cruel Modkit #{0}] Resetting wires...", ModuleID);
            Module.RegenWires();
            return;
        }

        Module.StartSolve();

        Debug.LogFormat("[The Cruel Modkit #{0}] Cutting wire {1}.", ModuleID, Wire + 1);
    }
}

