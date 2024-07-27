using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using UnityEditor.PackageManager;
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
        this.Components = Components;
    }

    private readonly int[] IdentityDisplay = new int[3];
    private readonly int[] ResistorDisplay = new int[4];

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
        if (Module.IsAnimating())
            return;
        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Adventure[Button].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button on Adventure was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Info.AdventureNames[Button], Module.GetOnComponents(), Module.GetTargetComponents());
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

        Module.StartSolve();
    }

    public virtual void OnPianoPress(int Piano)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlaySoundAtTransform(Module.PianoSounds[Piano + (Info.Piano * 12)].name, Module.transform);
        Module.Piano[Piano].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Info.PianoKeyNames[Piano], Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();
    }

    public virtual void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Info.ArrowDirections[Arrow], Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();
    }

    public virtual void OnIdentityPress(int Identity)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Identity[Identity].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        ChangeIdentityDisplay(Identity);

        return;
    }

    public virtual void OnResistorPress(int Resistor)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Resistor[Resistor].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        ChangeResistorDisplay(Resistor);

        return;
    }

    public virtual void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.UtilityButton.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();
    }

    public IEnumerator HandleArrowDelayFlash()
    {
        yield return null;
        for (int i = 0; i < Module.Arrows.Length; i++)
        {
            Module.Arrows[i].transform.Find("ArrowLight").gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < Module.Arrows.Length; i++)
        {
            Module.Arrows[i].transform.Find("ArrowLight").gameObject.SetActive(false);
        }
    }

    public IEnumerator HandleArrowDelayFlashSingle(int Arrow)
    {
        if (Arrow < 0 || Arrow >= 9) yield break;
        yield return null;
        Module.Arrows[Arrow].transform.Find("ArrowLight").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Module.Arrows[Arrow].transform.Find("ArrowLight").gameObject.SetActive(false);
    }

    public IEnumerator CurrentFlashingArrow;

    void ChangeIdentityDisplay(int Button)
    {
        IdentityDisplay[Button] += 1;
        if (IdentityDisplay[Button] >= 3)
            IdentityDisplay[Button] -= 3;
        if (Button == 0)
            Module.Identity[0].transform.Find("IdentityFaceIcon").GetComponentInChildren<Renderer>().material = Module.IdentityMats[Info.Identity[0][IdentityDisplay[Button]]];
        else
            Module.Identity[Button].transform.Find("IdentityText").GetComponentInChildren<TextMesh>().text = Info.IdentityItems[Info.Identity[Button][IdentityDisplay[Button]]];
    }

    void ChangeResistorDisplay(int Button)
    {
        ResistorDisplay[Button] += 1;
        if (ResistorDisplay[Button] >= 3)
            ResistorDisplay[Button] -= 3;
        Module.Resistor[Button].transform.GetComponentInChildren<Renderer>().material = Module.ResistorMats[Info.ResistorColors[Button][ResistorDisplay[Button]]];
    }
}
