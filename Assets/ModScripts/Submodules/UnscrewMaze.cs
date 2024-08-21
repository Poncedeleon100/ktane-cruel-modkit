using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class UnscrewMaze : Puzzle
{

    string[] maze = { "1", "13", "23", "12", "13", "23", "12", "3", "02", "0", "2", "02", "012", "3", "012", "13", "013", "03", "01", "123", "013", "13", "23", "2", "23", "02", "1", "123", "03", "02", "01", "03", "1", "013", "13", "03" };
    int[] positions;
    int curPos;
    bool[] bulbsSolved = { false, false };

    public UnscrewMaze(CruelModkitScript Module, int ModuleID, ComponentInfo Info, bool Vanilla, byte Components) : base(Module, ModuleID, Info, Vanilla, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Unscrew Maze.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Morse characters are {1}. ", ModuleID, Info.Morse);

        positions = Base36ToDec(Info.Morse);
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting position is ({1}, {2}).", ModuleID, Math.Floor(positions[0] / 6f)+1, (positions[0] % 6) + 1);
        Debug.LogFormat("[The Cruel Modkit #{0}] Bulb 1's coordinate is ({1}, {2}) and Bulb 2's coordinate is ({3}, {4}).", ModuleID, Math.Floor(positions[1] / 6f) + 1, (positions[1] % 6) + 1, Math.Floor(positions[2] / 6f) + 1, (positions[2] % 6) + 1);

        curPos = positions[0];
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

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Info.ArrowDirections[Arrow], Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        if (Arrow > 3)
            return;

        Module.StartSolve();

        int movementNum;
        if (Info.Arrows[8] == 9)
            movementNum = Arrow;
        else
        {
            int[] movementIndices = { 2, 3, 1, 0 };
            movementNum = Array.IndexOf(movementIndices, Info.Arrows[Arrow]);
        }
        if (!maze[curPos].Contains(movementNum.ToString()))
        {
            Module.CauseStrike();
            return;
        }
        switch (movementNum)
        {
            case 0:
                curPos -= 6;
                break;
            case 1:
                curPos += 1;
                break;
            case 2:
                curPos += 6;
                break;
            case 3:
                curPos -= 1;
                break;
        }
        UpdateMorse();
    }

    public override void OnBulbInteract(int Bulb)
    {
        if (Module.IsAnimating())
            return;

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbInfo[Bulb + 2]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Audio.PlaySoundAtTransform(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1].name, Module.transform);
        Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents() && !BulbScrewedIn[Bulb])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the component selection was [{2}] instead of [{3}].", ModuleID, (Bulb + 1) == 1 ? "first" : "second", Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        if (bulbsSolved[Bulb])
            return;

        if (positions[Bulb + 1] != curPos)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Bulb {1} unscrewed at wrong position. Resetting position!", ModuleID, Bulb);
            curPos = positions[0];
            Module.CauseStrike();
        }
        bulbsSolved[Bulb] = true;

        if (bulbsSolved[0] == true && bulbsSolved[1] == true)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Both bulbs have been unscrewed at the correct places, module solved.", ModuleID);
            Module.Solve();
        }

        return;
    }

    int[] Base36ToDec(string input)
    {
        string alpha = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return input.Select(x => Array.IndexOf(alpha.ToArray(), x)).ToArray();
    }

    void UpdateMorse()
    {
        string alpha = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Module.StopCoroutine(Module.MorseRoutine);
        string newMorse = alpha[curPos] + Info.Morse.Substring(1, 2);
        Info.Morse = newMorse;
        Module.MorseRoutine = Module.StartCoroutine(Module.PlayWord(Info.Morse));
    }

}
