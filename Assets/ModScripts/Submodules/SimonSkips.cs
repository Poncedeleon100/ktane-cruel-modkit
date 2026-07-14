using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;
using Random = UnityEngine.Random;

public class SimonSkips : Puzzle
{
    int[] arrowColours;
    readonly int[] orderedArrows;
    readonly List<int> finalSequence = new List<int>(); // List of indexes to press
    readonly List<int> inputtedSequence = new List<int>();
    readonly bool submitEmpty = false;

    public SimonSkips(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Simon Skips. Press the ❖ button to initiate the module.");
        int[] newArrows = new int[] { 4, 6, 7, 8, 9 }.Shuffle().Take(4).ToArray();

        for (int i = 4; i < 8; i++)
        {
            Info.Arrows[i] = newArrows[i-4];
        }
        for (int i = 4; i < 8; i++)
        {
            Module.Arrows[i].GetComponentInChildren<Renderer>().material = Module.ArrowMats[Info.Arrows[i]];
            Module.Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().color = ArrowLightColors[Info.Arrows[i]];
            Module.Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().intensity += (Info.Arrows[i] == 8) ? 10 : 0;
        }

        // Arrow colors in ordered clockwise starting with up
        orderedArrows = new int[] { Info.Arrows[(int)ArrowDirections.Up], Info.Arrows[(int)ArrowDirections.UpRight], Info.Arrows[(int)ArrowDirections.Right], Info.Arrows[(int)ArrowDirections.DownRight], Info.Arrows[(int)ArrowDirections.Down], Info.Arrows[(int)ArrowDirections.DownLeft], Info.Arrows[(int)ArrowDirections.Left], Info.Arrows[(int)ArrowDirections.UpLeft] };
        finalSequence.Add(FindStartingColor());
        if (finalSequence[0] != 8)
            Module.Log("The starting colour is {0}.", ArrowColorNames[(ArrowColors)orderedArrows[finalSequence[0]]]);
        else
        {
            Module.Log("The starting color is white or black. Press the center button to submit an empty sequence.");
            submitEmpty = true;
        }
    }

    int FindStartingColor()
    {
        int product = Module.Bomb.GetSerialNumberNumbers().Where(x => x != 0).Aggregate(1, (a, b) => a * b);
        product += Info.NumberDisplay;
        product %= 8;
        if (orderedArrows[product] > 7)
            return 8;
        return product;
    }

    void FindFullSequence()
    {
        for (int i = 0; i < 8; i++)
        {
            int currentLEDNum = LEDNumToArrowNum(Info.LED[i]);
            int currentLEDIndex = Array.IndexOf(orderedArrows, currentLEDNum);
            int moveNum;
            if (currentLEDIndex > finalSequence[i])
            {
                moveNum = currentLEDIndex - finalSequence[i];
            }
            else
            {
                moveNum = 8 - (finalSequence[i] - currentLEDIndex);
            }
            int newPos = finalSequence[i] - moveNum;    
            if (newPos < 0) newPos += 8;
            if (orderedArrows[newPos] > 7)
            {
                finalSequence.Add(8);
                return;
            }
            else
            {
                finalSequence.Add(newPos);
            }
        }
        finalSequence.Add(8);
        return;
    }

    int[] ConvertArrowNumstoLEDNums()
    {
        int[] converter = {0, 3, 8, 10, 2, 10, 7, 5, 0, 9};
        return Info.Arrows.Where(x => Array.IndexOf(Info.Arrows, x) != 8).Select(x => converter[x]).ToArray();
    }

    int LEDNumToArrowNum(int ledColour)
    {
        int[] converter = { 8, 0, 4, 1, 999, 7, 999, 6, 2, 9, 3 };
        return converter[ledColour];
    }

    void NewLEDs()
    {
        int[] newLEDs = new int[8];
        for (int i = 0; i < 8; i++)
            newLEDs[i] = arrowColours[Random.Range(0, 8)];

        Info.LED = newLEDs;
        Module.SetLEDs();
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

        arrowColours = ConvertArrowNumstoLEDNums();
        NewLEDs();
        if (!submitEmpty)
        {
            FindFullSequence();
            string[] pressColours = finalSequence.Where(x => x != 8).Select(x => ArrowColorNames[(ArrowColors)orderedArrows[x]]).ToArray();
            Module.Log("The sequence of colours to press is {0}, followed by the center button.", string.Join(", ", pressColours));
        }

        return;
    }

    public override void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);
        Module.Play(Module.transform, Module.ArrowSounds[Arrow].name);

        if (Module.IsModuleSolved())
            return;

        Module.StartCoroutine(HandleArrowFlash(Arrow));

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} arrow button was pressed when the component selection was [{1}] instead of [{2}].", ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetEnabledComponents(), Module.GetTargetComponents());
            }
            else
                Module.Strike("Strike! Module not initialized.");
            return;
        }

        if (Arrow == 8)
        {
            inputtedSequence.Add(8);
            if (inputtedSequence.SequenceEqual(finalSequence))
            {
                Module.SolveModule("The correct sequence has been entered. Module solved.");
            }
            else
            {
                string[] inputtedColours = inputtedSequence.Where(x => x != 8).Select(x => ArrowColorNames[(ArrowColors)orderedArrows[x]]).ToArray();
                Module.Strike("Strike! An incorrect sequence of {0} has been submitted.", string.Join(", ", inputtedColours));
                inputtedSequence.Clear();
            }
        }
        else
        {
            int[] converter = { 0, 2, 4, 6, 1, 3, 5, 7 };
            inputtedSequence.Add(converter[Arrow]);
        }
    }
}
