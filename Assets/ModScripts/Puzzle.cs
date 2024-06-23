using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Puzzle
{
    private CruelModkitScript Module;
    private int ModuleID;
    private ComponentInfo Info;
    private bool Vanilla;
    public bool[] Components;

    public Puzzle(CruelModkitScript Module, int ModuleID, ComponentInfo Info, bool Vanilla, bool[] Components)
    {
        this.Module = Module;
        this.ModuleID = ModuleID;
        this.Info = Info;
        this.Vanilla = Vanilla;
        Components = Components;
    }

    public virtual void OnWireCut(int Wire)
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
    }

    public virtual void OnButtonPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
        Module.Button.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();
    }

    public virtual void OnButtonRelease()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Module.transform);

        return;
    }

    public virtual void OnAdventurePress(int Button)
    {
        string AdventureButton = String.Empty;
        switch (Button)
        {
            case 0:
                AdventureButton = "Attack";
                break;
            case 1:
                AdventureButton = "Defend";
                break;
            case 2:
                AdventureButton = "Item";
                break;
            case 3:
                AdventureButton = "Left";
                break;
            case 4:
                AdventureButton = "Up";
                break;
            case 5:
                AdventureButton = "Down";
                break;
            case 6:
                AdventureButton = "Right";
                break;
        }

        if (Module.IsAnimating())
            return;
        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Adventure[Button].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button on Adventure was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, AdventureButton, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();
    }

    public virtual void OnSymbolPress(int Symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Symbols[Symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Symbol + 1, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            Module.CauseButtonStrike(true, Symbol);
            return;
        }

        Module.StartSolve();
    }

    public virtual void OnAlphabetPress(int Alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Alphabet[Alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Alphabet + 1, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            Module.CauseButtonStrike(false, Alphabet);
            return;
        }
    }
}
