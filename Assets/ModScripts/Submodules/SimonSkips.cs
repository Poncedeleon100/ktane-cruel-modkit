using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;



public class SimonSkips : Puzzle
{
    int[] arrowColours;
    readonly int[] orderedArrows;
    readonly List<int> finalSequence = new List<int>(); // List of indexes to press
    readonly List<int> inputtedSequence = new List<int>();
    readonly bool submitEmpty = false;
    readonly int inputNum = 0;

    public SimonSkips(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Simon Skips. Press the ❖ button to initiate the module.", ModuleID);
        int[] newArrows = new int[] { 4, 6, 7, 8, 9 }.Shuffle().Take(4).ToArray();

        for (int i = 4; i < 8; i++)
        {
            Info.Arrows[i] = newArrows[i-4];
        }
        for (int i = 4; i < 8; i++)
        {
            Module.Arrows[i].GetComponentInChildren<Renderer>().material = Module.ArrowMats[Info.Arrows[i]];
            Module.Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().color = Info.ArrowLightColors[Info.Arrows[i]];
            Module.Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().intensity += (Info.Arrows[i] == 8) ? 10 : 0;
        }

        orderedArrows = new int[] { Info.Arrows[0],  Info.Arrows[4], Info.Arrows[1], Info.Arrows[5], Info.Arrows[2], Info.Arrows[6], Info.Arrows[3], Info.Arrows[7] };
        finalSequence.Add(FindStartingColor());
        if (finalSequence[0] != 8)
            Debug.LogFormat("[The Cruel Modkit #{0}] The starting colour is {1}.", ModuleID, ComponentInfo.ArrowColors[orderedArrows[finalSequence[0]]]);
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The starting color is white or black. Press the center button to submit an empty sequence.", ModuleID);
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
        for (int i = 1; i < 9; i++)
        {
            int currentLEDNum = LEDNumToArrowNum(Info.LED[i-1]);
            int moveNum;
            if (Array.IndexOf(orderedArrows, currentLEDNum) > finalSequence[i-1])
            {
                moveNum = Array.IndexOf(orderedArrows, currentLEDNum) - finalSequence[i-1];
            }
            else
            {
                moveNum = 8 - (finalSequence[i-1] - Array.IndexOf(orderedArrows, currentLEDNum));
            }
            int newPos = finalSequence[i-1] - moveNum;
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
        int[] converter = {1, 3, 8, 10, 2, 10, 7, 5, 0, 9};
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
        for (int i = 0; i < 8; i++)
            Module.LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[Info.LED[i]];
    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.UtilityButton.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved() || Module.IsModuleSolving())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();

        arrowColours = ConvertArrowNumstoLEDNums();
        NewLEDs();
        if (!submitEmpty)
        {
            FindFullSequence();
            string[] pressColours = finalSequence.Where(x => x != 8).Select(x => ComponentInfo.ArrowColors[orderedArrows[x]]).ToArray();
            Debug.LogFormat("[The Cruel Modkit #{0}] The sequence of colours to press is {1}, followed by the center button.", ModuleID, string.Join(", ", pressColours));
        }

        return;
    }

    public override void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Audio.PlaySoundAtTransform(Module.ArrowSounds[Arrow].name, Module.transform);
        Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        Module.StartCoroutine(HandleArrowFlash(Arrow));

        if (!Module.IsModuleSolving())
            return;

        if (Arrow == 8)
        {
            inputtedSequence.Add(8);
            if (inputtedSequence.SequenceEqual(finalSequence))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] The correct sequence has been entered. Module solved.", ModuleID);
                Module.Solve();
            }
            else
            {
                string[] inputtedColours = inputtedSequence.Where(x => x != 8).Select(x => ComponentInfo.ArrowColors[orderedArrows[x]]).ToArray();
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! An incorrect sequence of {1} has been submitted.", ModuleID, string.Join(", ", inputtedColours));
                Module.CauseStrike();
                inputtedSequence.Clear();
            }
        }
        else
        {
            int[] converter = { 0, 2, 4, 6, 1, 3, 5, 7 };
            inputtedSequence.Add(converter[Arrow]);
        }
    }

    public IEnumerator HandleArrowFlash(int Arrow)
    {
        if (Arrow < 0 || Arrow >= 9) yield break;
        yield return null;
        Module.Arrows[Arrow].transform.Find("ArrowLight").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Module.Arrows[Arrow].transform.Find("ArrowLight").gameObject.SetActive(false);
    }

}
