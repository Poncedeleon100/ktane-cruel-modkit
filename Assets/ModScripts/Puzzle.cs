using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Debug;

public class Puzzle
{
    private CruelModkitScript module;
    private int moduleId;
    private ComponentInfo info;
    private bool vanilla;
    public bool[] Components;

    public Puzzle(CruelModkitScript module, int moduleId, ComponentInfo info, bool vanilla, bool[] components)
    {
        this.module = module;
        this.moduleId = moduleId;
        this.info = info;
        this.vanilla = vanilla;
        Components = components;
    }

    public void OnWireCut(int wire)
    {
        if (module.IsAnimating())
            return;

        module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, module.transform);

        if (module.IsModuleSolved())
            return;

        if (!module.CheckValidComponents())
        {
            Log($"[Cruel Modkit #{moduleId}] ");
        }
    }
}
