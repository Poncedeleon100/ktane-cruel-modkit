using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Debug;

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

    public void OnWireCut(int wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Log($"[Cruel Modkit #{ModuleID}] ");
        }
    }
}
