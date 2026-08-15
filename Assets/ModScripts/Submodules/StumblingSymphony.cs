using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wawa.Modules;

public class StumblingSymphony : Puzzle
{
    readonly string[] melodyNames = { "Für Elise", "All I Want for Christmas is You", "It’s the Most Wonderful Time of the Year", "We Three Kings", "Empire Strikes Back", "Shiny Smily Story", "Pink Panther", "Pirates of the Carribean", "Let It Be", "A Town with an Ocean’s View" };
    readonly string[] ruleLog = { "You are a piano maestro, you will not stumble.", "You will stumble on every sharp/flat note.", "You will stumble on every note from C to F.", "You will stumble on the first, last, and center notes.", "You will stumble on every note from F# to B.", "You will stumble on every third note." };
    readonly List<List<string>> noteSequences = new List<List<string>>() {
        new List<string> { "E", "D#", "E", "D#", "E", "B", "D", "C" },
        new List<string> { "B", "A", "G", "D#", "D", "A", "B", "A", "G" },
        new List<string> { "F#", "G", "A", "A", "D", "B", "A", "G", "E", "D" },
        new List<string> { "C#", "B", "A", "F#", "G#", "A", "G#", "F#" },
        new List<string> { "G", "G", "G", "D#", "A#", "G", "D#", "A#", "G" },
        new List<string> { "D#", "F", "G", "G#", "D#", "A#", "G#", "G", "G#", "A#" },
        new List<string> { "C#", "D", "E", "F", "C#", "D", "E", "F", "A#", "A" },
        new List<string> { "A", "C", "D", "D", "D", "E", "F", "F", "F", "G", "E", "E" },
        new List<string> { "E", "D", "C", "E", "G", "A", "G", "E", "D", "C", "A", "G", "E" },
        new List<string> { "B", "G", "B", "F#", "B", "E", "D", "C", "D", "G", "A" } };
    readonly string[] noteOrder = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    readonly string[] melodiesForRule = { "0123456789", "3456", "0259", "0123456789", "1234589", "12456789" };

    readonly List<List<int>> noteIndices = new List<List<int>>();

    int activeRule;

    List<int> finalInput = new List<int>();
    readonly List<int> buttonPresses = new List<int>(); // After which note input should you press the button
    int numInputs;
    bool buttonPressed;

    int semiShift;

    public StumblingSymphony(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Stumbling Symphony. Press the ❖ button to initiate the module.");

        for (int i = 0; i < noteSequences.Count(); i++)
            noteIndices.Add(noteSequences[i].Select(x => Array.IndexOf(noteOrder, x)).ToList());

        FindActiveRule();
    }

    void FindActiveRule()
    {
        if (Info.ButtonText == "") activeRule = 0;
        else if (Info.ButtonText.Count(x => "AEIOU".Contains(x.ToString().ToUpper())) >= 2) activeRule = 1;
        else if (Info.ButtonText.Length == 5) activeRule = 2;
        else if (Info.ButtonText == "YES" || Info.ButtonText == "NO") activeRule = 3;
        else if (Info.ButtonText.Contains("P")) activeRule = 4;
        else activeRule = 5;
    }

    void PickNumber()
    {
        int number = Int32.Parse(melodiesForRule[activeRule].PickRandom().ToString());
        Info.NumberDisplay = number;
        Module.WidgetText[2].text = Info.NumberDisplay.ToString();
    }

    void CalcInputs()
    {
        List<int> noteSeq = noteIndices[Info.NumberDisplay];
        if (activeRule == 0)
            finalInput = noteSeq.ToList();
        switch (activeRule)
        {
            case 1:
                int[] sharpNotes = { 1, 3, 6, 8, 10 };
                FindNotesForNotesInSet(sharpNotes, noteSeq);
                break;
            case 2:
                int[] CtoFNotes = { 0, 1, 2, 3, 4, 5 };
                FindNotesForNotesInSet(CtoFNotes, noteSeq);
                break;
            case 3:
                int[] centerNotesI;
                if (noteSeq.Count() % 2 == 0)
                    centerNotesI = new int[] { noteSeq.Count() / 2 - 1, noteSeq.Count() / 2 };
                else
                    centerNotesI = new int[] { (int)Math.Ceiling(noteSeq.Count() / 2.0) - 1 };
                for (int i = 0; i < noteSeq.Count(); i++)
                {
                    if (i == 0 || i == noteSeq.Count() - 1 || centerNotesI.Contains(i))
                    {
                        finalInput.Add(FindStumbleNote(noteSeq[i]));
                        buttonPresses.Add(finalInput.Count());
                    }
                    finalInput.Add(noteSeq[i]);
                }
                break;
            case 4:
                int[] FSharpToBNotes = { 6, 7, 8, 9, 10, 11 };
                FindNotesForNotesInSet(FSharpToBNotes, noteSeq);
                break;
            case 5:
                for (int i = 0; i < noteSeq.Count(); i++)
                {
                    if ((i + 1) % 3 == 0)
                    {
                        finalInput.Add(FindStumbleNote(noteSeq[i]));
                        buttonPresses.Add(finalInput.Count());
                    }
                    finalInput.Add(noteSeq[i]);
                }
                break;
        }
    }

    void FindNotesForNotesInSet(int[] set, List<int> noteSeq)
    {
        for (int i = 0; i < noteSeq.Count(); i++)
        {
            if (set.Contains(noteSeq[i]))
            {
                finalInput.Add(FindStumbleNote(noteSeq[i]));
                buttonPresses.Add(finalInput.Count());
            }
            finalInput.Add(noteSeq[i]);
        }
    } 

    int FindStumbleNote(int initialNote)
    {
        int[] shifter = { 1, 8, 7, 5, 6, 3, 10, 9, 2, 11, 4 };
        semiShift = shifter[Info.Button];
        initialNote = (initialNote + semiShift) % 12;
        return initialNote;
    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.UtilityButton.GetComponentInChildren<KMSelectable>(), 0.5f, Sound.ButtonPress);

        if (Module.IsModuleSolved() || Module.IsSolving())
            return;

        if (!Module.CheckValidComponents())
        {
            Module.Strike("Strike! The ❖ button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
            return;
        }

        Module.StartSolve();

        PickNumber();
        Module.Log("The melody is {0}.", melodyNames[Info.NumberDisplay]);
        Module.Log("The rule used is rule {0}. {1}", activeRule + 1, ruleLog[activeRule]);
        CalcInputs();
        Module.Log("The full sequence of notes to play (including stumbles) is {0}.", string.Join(", ", finalInput.Select(x => noteOrder[x]).ToArray()));
        Module.Log("The button is {0}. Your stumbles will be shifted {1} semitones forward", Enum.GetName(typeof(ComponentInfo.MainColors), Info.Button), semiShift);
        return;
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
                Module.Strike("Strike! The {0} key on the piano was pressed when the component selection was [{1}] instead of [{2}].", ComponentInfo.PianoKeyNames[(ComponentInfo.PianoKeys)Piano], Module.GetEnabledComponents(), Module.GetTargetComponents());

            }
            else
            {
                Module.Strike("Strike! Module not initialized.");
            }
            return;
        }
        
        if (Piano == finalInput[numInputs])
            numInputs++;
        else
        {
            Module.Strike("Strike! You played {0} when {1} was needed.", noteOrder[Piano], noteOrder[finalInput[numInputs]]);
            numInputs = 0;
            return;
        }

        if (buttonPresses.Contains(numInputs - 1) && !buttonPressed)
        {
            Module.Strike("Strike! You have forgotten to press the button after a stumble (note {0})!", noteOrder[finalInput[numInputs-2]]);
            numInputs = 0;
            return;
        }
        else if (buttonPresses.Contains(numInputs - 1))
            buttonPressed = false;

        if (numInputs == finalInput.Count())
        {
            Module.SolveModule("You have impressed the button. Module solved.");
        }
    }

    public override void OnButtonPress()
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
            }
            else
                Module.Strike("Strike! Module not initialized.");
            return;
        }

        if (buttonPresses.Contains(numInputs))
            buttonPressed = true;
        else
        {
            Module.Strike("Strike! You have pressed the button at an incorect time!");
            numInputs = 0;
        }
    }
}
