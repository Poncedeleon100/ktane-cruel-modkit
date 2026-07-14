using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;
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

    public AVInput(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving AV Input.");

        bulbStates = new bool[2] { Info.BulbOn[0], Info.BulbOn[1] };
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
        Module.Log("Left bulb's scale is {0}.", LogScale(bulb1Notes));
        Module.Log("Right bulb's scale is {0}.", LogScale(bulb2Notes));

        for (int i = 0; i < 12; i++)
        {
            if (bulb1Actions[i] != 2)
                bulb1Actions[i] = Random.Range(0, 2);
            if (bulb2Actions[i] != 2)
                bulb2Actions[i] = Random.Range(0, 2);
        }
        Module.Log("Left bulb's key actions are {0}.", bulb1Actions.Select(x => new string[] { "Off", "On", "Toggle" }[x]).Join(", "));
        Module.Log("Right bulb's key actions are {0}.", bulb2Actions.Select(x => new string[] { "Off", "On", "Toggle" }[x]).Join(", "));
    }
    
    public override void OnPianoPress(int Piano)
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

        if (Piano == lastPress & !BulbScrewedIn.Contains(false))
        {
            Module.Strike("Strike! Pressed the {0} key twice in a row. Turning both bulbs off.", PianoKeyNames[(PianoKeys)Piano]);
            ChangeBulb(0, false);
            ChangeBulb(1, false);
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

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], false);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Shake(Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                if (BulbScrewedIn[Bulb]) return;
                Module.Strike("Strike! The {0} bulb was removed when the component selection was [{1}] instead of [{2}].", (Bulb + 1) == 1 ? "first" : "second", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        if (!BulbScrewedIn[Bulb])
            return;

        if (scaleInput.OrderBy(x => x).SequenceEqual(Bulb == 0 ? bulb1Notes : bulb2Notes))
        {
            Module.Log("Inputted the correct scale {0} for the {1} bulb. Permanently turning it off.", LogScale(scaleInput), Bulb == 0 ? "left" : "right");
            bulbSolved[Bulb] = true;
            lastPress = -1;
            if (bulbSolved.All(b => b))
            {
                Module.SolveModule("Inputted the correct scale for both bulbs. Module solved.");
            }
        }
        else
        {
            Module.Strike("Strike! Inputted the incorrect scale {0} for the {1} bulb.", LogScale(scaleInput), Bulb == 0 ? "left" : "right");
        }
        scaleInput = new List<int>();
    }

    public override void OnBulbButtonPress(int Button)
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

        if (uniquePresses.Count != 12)
        {
            Module.Strike("Strike! Tried to reset the module before pressing every key at least once.");
            return;
        }
        if (new List<int>() { 0, 2, 4, 5, 7, 9, 11 }.Contains(lastPress))
        {
            if (Info.BulbOLeft == (Button == 0))
            {
                Module.Strike("Strike! Incorrectly pressed the O key for resetting after the white key {0}.", PianoKeyNames[(PianoKeys)lastPress]);
                return;
            }
            else
            {
                Module.Log("Correctly pressed the I key for resetting after the white key {0}.", PianoKeyNames[(PianoKeys)lastPress]);
                ChangeBulb(0, false);
                ChangeBulb(1, false);
                lastPress = -1;
            }
        }
        else
        {
            if (Info.BulbOLeft == (Button == 0))
            {
                Module.Log("Correctly pressed the O key for resetting after the black key {0}.", PianoKeyNames[(PianoKeys)lastPress]);
                ChangeBulb(0, false);
                ChangeBulb(1, false);
                lastPress = -1; 
            }
            else
            {
                Module.Strike("Strike! Incorrectly pressed the I key for resetting after the black key {0}.", PianoKeyNames[(PianoKeys)lastPress]);
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
        return Scale.Select(x => PianoKeyNames[(PianoKeys)x]).Join(", ");
    }
}
