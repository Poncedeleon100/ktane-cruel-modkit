using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AVInput : Puzzle
{
    readonly List<int> bulb1Notes = new List<int>();
    readonly List<int> bulb2Notes = new List<int>();
    readonly bool[] bulbStates = new bool[2];
    readonly bool[] bulbSolved = new bool[2];
    int lastPress = -1;
    List<int> scaleInput = new List<int>();
    List<int> uniquePresses = new List<int>();
    readonly int[] bulb1Actions = new int[12];
    readonly int[] bulb2Actions = new int[12];

    public AVInput(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving AV Input.", ModuleID);

        bulbStates = new bool[2] { Info.BulbInfo[2], Info.BulbInfo[3] };
        for (int i = 0; i < 5; i++)
        {
            int note = Random.Range(0, 12);
            while (bulb1Notes.Contains(note))
                note = Random.Range(0, 12);

            bulb1Notes.Add(note);
            bulb1Actions[note] = 2;

            note = Random.Range(0, 12);
            while (bulb2Notes.Contains(note))
                note = Random.Range(0, 12);

            bulb2Notes.Add(note);
            bulb2Actions[note] = 2;
        }
        bulb1Notes.Sort();
        bulb2Notes.Sort();
        Debug.LogFormat("[The Cruel Modkit #{0}] Left bulb's scale is {1}.", ModuleID, LogScale(bulb1Notes));
        Debug.LogFormat("[The Cruel Modkit #{0}] Right bulb's scale is {1}.", ModuleID, LogScale(bulb2Notes));

        for (int i = 0; i < 12; i++)
        {
            if (bulb1Actions[i] != 2)
                bulb1Actions[i] = Random.Range(0, 2);
            if (bulb2Actions[i] != 2)
                bulb2Actions[i] = Random.Range(0, 2);
        }
        Debug.LogFormat("[The Cruel Modkit #{0}] Left bulb's key actions are {1}.", ModuleID, bulb1Actions.Select(x => new string[] { "Off", "On", "Toggle" }[x]).Join(", "));
        Debug.LogFormat("[The Cruel Modkit #{0}] Right bulb's key actions are {1}.", ModuleID, bulb2Actions.Select(x => new string[] { "Off", "On", "Toggle" }[x]).Join(", "));
    }
    
    public override void OnPianoPress(int Piano)
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
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Info.PianoKeyNames[Piano], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
        if (Piano == lastPress & !BulbScrewedIn.Contains(false))
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Pressed the {1} key twice in a row. Turning both bulbs off.", ModuleID, Info.PianoKeyNames[Piano]);
            ChangeBulb(0, false);
            ChangeBulb(1, false);
            Module.CauseStrike();
            return;
        }

        if (BulbScrewedIn[0] && BulbScrewedIn[1])
        {
            if (!bulbSolved[0])
            {
                if (bulb1Notes.Contains(Piano))
                    ChangeBulb(0, !bulbStates[0]);
                else
                    ChangeBulb(0, bulb1Actions[Piano] == 1);
            }
            if (!bulbSolved[1])
            {
                if (bulb2Notes.Contains(Piano))
                    ChangeBulb(1, !bulbStates[1]);
                else
                    ChangeBulb(1, bulb2Actions[Piano] == 1);
            }
        }
        else
        {
            scaleInput.Add(Piano);
        }
        lastPress = Piano;
        if (!uniquePresses.Contains(Piano)) uniquePresses.Add(Piano);
    }

    public override void OnBulbInteract(int Bulb)
    {
        if (bulbSolved[Bulb] || !BulbScrewedIn[1 - Bulb] || Module.IsAnimating())
            return;

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbInfo[Bulb + 2]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Audio.PlaySoundAtTransform(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1].name, Module.transform);
        Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            if (BulbScrewedIn[Bulb]) return;
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the component selection was [{2}] instead of [{3}].", ModuleID, (Bulb + 1) == 1 ? "first" : "second", Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();

        if (!BulbScrewedIn[Bulb])
            return;

        if (scaleInput.OrderBy(x => x).SequenceEqual(Bulb == 0 ? bulb1Notes : bulb2Notes))
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Inputted the correct scale {1} for the {2} bulb. Permanently turning it off.", ModuleID, LogScale(scaleInput), Bulb == 0 ? "left" : "right");
            bulbSolved[Bulb] = true;
            Info.BulbInfo[Bulb + 2] = false;
            lastPress = -1;
            if (bulbSolved.All(b => b))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Inputted the correct scale for both bulbs. Module solved.", ModuleID);
                Module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Inputted the incorrect scale {1} for the {2} bulb.", ModuleID, LogScale(scaleInput), Bulb == 0 ? "left" : "right");
            Module.CauseStrike();
        }
        scaleInput = new List<int>();
    }

    public override void OnBulbButtonPress(int Button)
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

        if (uniquePresses.Count != 12)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Tried to reset the module before pressing every key at least once.", ModuleID);
            Module.CauseStrike();
            return;
        }
        if (new List<int>() { 0, 2, 4, 5, 7, 9, 11 }.Contains(lastPress))
        {
            if (Info.BulbInfo[4] == (Button == 2))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Incorrectly pressed the O key for resetting after the white key {1}.", ModuleID, Info.PianoKeyNames[lastPress]);
                Module.CauseStrike();
                return;
            }
            else
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Correctly pressed the I key for resetting after the white key {1}.", ModuleID, Info.PianoKeyNames[lastPress]);
                ChangeBulb(0, false);
                ChangeBulb(1, false);
                lastPress = -1;
            }
        }
        else
        {
            if (Info.BulbInfo[4] == (Button == 2))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Correctly pressed the O key for resetting after the black key {1}.", ModuleID, Info.PianoKeyNames[lastPress]);
                ChangeBulb(0, false);
                ChangeBulb(1, false);
                lastPress = -1; 
            }
            else
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Incorrectly pressed the I key for resetting after the black key {1}.", ModuleID, Info.PianoKeyNames[lastPress]);
                Module.CauseStrike();
                return;
            }
        }
        uniquePresses = new List<int>();
    }

    private void ChangeBulb(int Bulb, bool State)
    {
        Module.Bulbs[Bulb].transform.Find("BulbLight").GetComponentInChildren<Light>().enabled = Module.Bulbs[Bulb].transform.Find("BulbLight2").GetComponentInChildren<Light>().enabled = State;
        bulbStates[Bulb] = State;
    }

    private string LogScale (List<int> Scale)
    {
        return Scale.Select(x => Info.PianoKeyNames[x]).Join(", ");
    }
}
