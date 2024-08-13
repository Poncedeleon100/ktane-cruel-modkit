using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using UnityEditor.PackageManager;
using UnityEngine;
using KModkit;
using Mono.Cecil;



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
        Debug.LogFormat("[The Cruel Modkit #{0}] Bulb 1 is {1}, {2}, and {3}. Bulb 2 is {4}, {5}, and {6}. The O button is on the {7}.", ModuleID, Info.MainColors[Info.BulbColors[0]], Info.BulbInfo[0] ? "opaque" : "see-through", Info.BulbInfo[2] ? "on" : "off", Info.MainColors[Info.BulbColors[1]], Info.BulbInfo[1] ? "opaque" : "see-through", Info.BulbInfo[3] ? "on" : "off", Info.BulbInfo[4] ? "left" : "right");
        Debug.LogFormat("[The Cruel Modkit #{0}] Identity is {1}, with item {2}, at the location {3}, and a rarity of {4}.", ModuleID, Info.IdentityNames[Info.Identity[0]], Info.IdentityItems[Info.Identity[1]], Info.IdentityLocations[Info.Identity[2]], Info.IdentityRarity[Info.Identity[3]]);
        Debug.LogFormat("[The Cruel Modkit #{0}] Resistor 1 colors are (Reading from {1}): {2}. Left letter is {3}, right letter is {4}.", ModuleID, Info.ResistorReversed[0] ? "right to left" : "left to right", Info.GetResistorInfo(0), Info.ResistorText[0], Info.ResistorText[1]);
        Debug.LogFormat("[The Cruel Modkit #{0}] Resistor 2 colors are (Reading from {1}): {2}. Left letter is {3}, right letter is {4}.", ModuleID, Info.ResistorReversed[1] ? "right to left" : "left to right", Info.GetResistorInfo(1), Info.ResistorText[2], Info.ResistorText[3]);
        Debug.LogFormat("[The Cruel Modkit #{0}] Widgets: Timer display is {1}. Word display is {2}. Number display is {3}. Morse code LED is {4}. Meter is {5} and at {6}%.", ModuleID, Info.TimerDisplay.ToString().PadLeft(2, '0'), Info.WordDisplay, Info.NumberDisplay, Info.Morse, (Info.MeterValue == 0) ? "none" : Info.MeterColors[Info.MeterColor], (100 * Math.Round(Info.MeterValue, 2)).ToString());
    }

    List<int> WiresCut = new List<int>();

    public override void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        /*if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the component selection was [{2}] instead of [{3}].", ModuleID, Wire + 1, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            Debug.LogFormat("[The Cruel Modkit #{0}] Resetting wires...", ModuleID);
            Module.RegenWires();
            Debug.LogFormat("[The Cruel Modkit #{0}] Wires present: {1}.", ModuleID, Info.GetWireInfo());
            Debug.LogFormat("[The Cruel Modkit #{0}] Wire LEDs present: {1}.", ModuleID, Info.GetWireLEDInfo());
            return;
        }*/

        Module.StartSolve();

        Debug.LogFormat("[The Cruel Modkit #{0}] Cut wire {1}.", ModuleID, Wire + 1);
        WiresCut.Add(Wire);
        if (WiresCut.Count == 7)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] All wires cut. Module solved.", ModuleID);
            Module.StopAllCoroutines();
            foreach (GameObject Symbols in Module.Symbols)
                Symbols.transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[0];
            foreach (GameObject Alphabet in Module.Alphabet)
                Alphabet.transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[0];
            foreach (GameObject Arrow in Module.Arrows)
                Arrow.transform.Find("ArrowLight").gameObject.SetActive(false);
            for (int i = 0; i < 2; i++)
                Module.Bulbs[i].transform.Find("BulbLight").GetComponentInChildren<Light>().enabled = Module.Bulbs[i].transform.Find("BulbLight2").GetComponentInChildren<Light>().enabled = false;
            Module.MorseLight.enabled = false;
            Module.MorseMesh.material = Module.MorseMats[0];
            Module.Solve();
            return;
        }
    }
}

