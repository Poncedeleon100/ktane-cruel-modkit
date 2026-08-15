using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;

public class Puzzle
{
    protected CruelModkitScript Module;
    protected ComponentInfo Info;
    public byte Components;

    public Puzzle(CruelModkitScript module, ComponentInfo info, byte components)
    {
        this.Module = module;
        this.Info = info;
        this.Components = components;
    }

    public readonly List<int> WiresCut = new List<int>();
    public readonly bool[] BulbScrewedIn = { true, true };

    public virtual void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.WireSnip);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Wire {0} was cut when the component selection was [{1}] instead of [{2}].", Wire + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                Module.Log("Resetting wires...");

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

        Module.Shake(Module.Button.GetComponentInChildren<KMSelectable>(), 0.25f, Sound.BigButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnButtonRelease()
    {
        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.BigButtonRelease);
    }

    public virtual void OnSymbolPress(int Symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Symbols[Symbol].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Symbol {0} was pressed when the component selection was [{1}] instead of [{2}].", Symbol + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
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

        Module.Shake(Module.Alphabet[Alphabet].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Alphanumeric key {0} was pressed when the component selection was [{1}] instead of [{2}].", Alphabet + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
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

        Module.Shake(Module.Piano[Piano].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.PianoSounds[Piano + (Info.Piano * 12)]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} key on the piano was pressed when the component selection was [{1}] instead of [{2}].", PianoKeyNames[(PianoKeys)Piano], Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} arrow button was pressed when the component selection was [{1}] instead of [{2}].", ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnBulbButtonPress(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.BulbButtons[Button].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} button was pressed when the component selection was [{1}] instead of [{2}].", (Button == 0) == Info.BulbOLeft ? "O" : "I", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnBulbButtonRelease(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.ButtonRelease);
    }

    public virtual void OnBulbInteract(int Bulb)
    {
        if (Module.IsAnimating())
            return;

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbOn[Bulb]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Shake(Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents() && !BulbScrewedIn[Bulb])
            {
                Module.Strike("Strike! The {0} bulb was removed when the component selection was [{1}] instead of [{2}].", (Bulb + 1) == 1 ? "first" : "second", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public virtual void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.UtilityButton.GetComponentInChildren<KMSelectable>(), 0.5f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The ❖ button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
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
