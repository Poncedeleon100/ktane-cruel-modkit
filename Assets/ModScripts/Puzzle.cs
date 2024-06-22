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
            //Module.RegenWires();
            return;
        }

        Module.StartSolve();
    }
}
