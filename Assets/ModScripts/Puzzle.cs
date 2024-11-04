using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ComponentInfo;

public class Puzzle
{
    protected CruelModkitScript Module;
    protected int ModuleID;
    protected ComponentInfo Info;
    protected bool Vanilla;
    public byte Components;

    public Puzzle(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components)
    {
        this.Module = Module;
        this.ModuleID = ModuleID;
        this.Info = Info;
        this.Components = Components;
    }

    public readonly List<int> WiresCut = new List<int>();
    public readonly bool[] BulbScrewedIn = { true, true };

    public virtual void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the component selection was [{2}] instead of [{3}].", ModuleID, Wire + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Debug.LogFormat("[The Cruel Modkit #{0}] Resetting wires...", ModuleID);

                Info.GenerateWireInfo();
                Info.GenerateWireLEDInfo();
                Module.RegenWires();

                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnButtonPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
        Module.Button.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnButtonRelease()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Module.transform);

        return;
    }

    public virtual void OnSymbolPress(int Symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Symbols[Symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Symbol + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Module.StartCoroutine(Module.ButtonStrike(true, Symbol));
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnAlphabetPress(int Alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Alphabet[Alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Alphabet + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnPianoPress(int Piano)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlaySoundAtTransform(Module.PianoSounds[Piano + (Info.Piano * 12)].name, Module.transform);
        Module.Piano[Piano].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, PianoKeyNames[(PianoKeys)Piano], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnBulbButtonPress(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Bulbs[Button].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, (Button == 2) == Info.BulbInfo[4] ? "O" : "I", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnBulbButtonRelease(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Module.transform);
    }

    public virtual void OnBulbInteract(int Bulb)
    {
        if (Module.IsAnimating())
            return;

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbInfo[Bulb + 2]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Audio.PlaySoundAtTransform(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1].name, Module.transform);
        Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents() && !BulbScrewedIn[Bulb])
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the component selection was [{2}] instead of [{3}].", ModuleID, (Bulb + 1) == 1 ? "first" : "second", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.UtilityButton.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual IEnumerator AnimateButtonPress(Transform Object, Vector3 Offset, int Index = 0)
    {
        switch (Index)
        {
            case 0:
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition += Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition -= Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            case 1:
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition += Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            case 2:
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition -= Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
        }
    }

    /// <summary>
    /// Briefly flashes the lights of every arrow at the same time.
    /// </summary>
    public IEnumerator HandleArrowFlashAll()
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

    /// <summary>
    /// Briefly flashes the light of a single arrow.
    /// </summary>
    public IEnumerator HandleArrowFlash(int Arrow)
    {
        if (Arrow < 0 || Arrow >= 9) yield break;
        yield return null;
        Module.Arrows[Arrow].transform.Find("ArrowLight").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Module.Arrows[Arrow].transform.Find("ArrowLight").gameObject.SetActive(false);
    }

    public IEnumerator CurrentFlashingArrow;
}
