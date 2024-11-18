using System;
using System.Linq;
using UnityEngine;
using static ComponentInfo;

public class UnscrewMaze : Puzzle
{
    readonly ArrowDirections[][] maze = new ArrowDirections[][]
    {
        //Row 1
        new ArrowDirections[] { ArrowDirections.Right },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Down, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Down, ArrowDirections.Left },
        //Row 2
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Up },
        new ArrowDirections[] { ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Down },
        //Row 3
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right, ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right, ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Left },
        //Row 4
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Down, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Down, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Down },
        //Row 5
        new ArrowDirections[] { ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Down },
        new ArrowDirections[] { ArrowDirections.Right },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Down, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Down },
        //Row 6
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Right },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Right, ArrowDirections.Left },
        new ArrowDirections[] { ArrowDirections.Up, ArrowDirections.Left },
    };
    readonly int[] positions;
    int curPos;
    readonly bool[] bulbsSolved = { false, false };

    public UnscrewMaze(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Unscrew Maze.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Morse characters are {1}. ", ModuleID, Info.Morse);

        positions = Base36ToDec(Info.Morse);
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting position is ({1}, {2}).", ModuleID, Math.Floor(positions[0] / 6f)+1, (positions[0] % 6) + 1);
        Debug.LogFormat("[The Cruel Modkit #{0}] Bulb 1's coordinate is ({1}, {2}) and Bulb 2's coordinate is ({3}, {4}).", ModuleID, Math.Floor(positions[1] / 6f) + 1, (positions[1] % 6) + 1, Math.Floor(positions[2] / 6f) + 1, (positions[2] % 6) + 1);
        Debug.LogFormat("[The Cruel Modkit #{0}] The center button is {1}.", ModuleID, Info.Arrows[(int)ArrowDirections.Center] == (int)ArrowColors.White ? "white. Use the arrow directions to navigate" : "grey. Use the arrow colors to navigate");
        
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

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (Arrow > (int)ArrowDirections.Left)
            return;

        Module.StartSolve();

        int movementNum;
        if (Info.Arrows[(int)ArrowDirections.Center] == (int)ArrowColors.White)
            movementNum = Arrow;
        else
        {
            int[] movementIndices = { (int)ArrowColors.Red, (int)ArrowColors.Yellow, (int)ArrowColors.Green, (int)ArrowColors.Blue };
            movementNum = Array.IndexOf(movementIndices, Info.Arrows[Arrow]);
        }
        if (!ConvertEnum(maze[curPos]).Contains(movementNum.ToString()))
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! You hit a wall by moving {1} at the coordinates ({2}, {3}). Resetting maze position.", ModuleID, ArrowDirectionNames[(ArrowDirections)movementNum].ToLower(), Math.Floor(curPos / 6f) + 1, (curPos % 6) + 1);
            curPos = positions[0];
            UpdateMorse();
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

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the component selection was [{2}] instead of [{3}].", ModuleID, (Bulb + 1) == 1 ? "first" : "second", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbOn[Bulb]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];
        bulbsSolved[Bulb] = !bulbsSolved[Bulb];

        Module.Audio.PlaySoundAtTransform(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1].name, Module.transform);
        Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved() || BulbScrewedIn[Bulb])
            return;

        if (positions[Bulb + 1] != curPos)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Bulb {1} incorrectly unscrewed at ({2}, {3}). Resetting maze position.", ModuleID, Bulb + 1, Math.Floor(curPos / 6f) + 1, (curPos % 6) + 1);
            curPos = positions[0];
            UpdateMorse();
            Module.CauseStrike();
        }

        if (bulbsSolved[0] && bulbsSolved[1])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Both bulbs have been unscrewed. Module solved.", ModuleID);
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
        Info.Morse = alpha[curPos] + Info.Morse.Substring(1, 2);
        Module.SetMorse();
    }

    // Makes the maze array initialization a little bit cleaner
    private string ConvertEnum(ArrowDirections[] arrowDirections)
    {
        string stringDirections = String.Empty;

        foreach (var direction in arrowDirections)
        {
            stringDirections += ((int)direction).ToString();
        }
        return stringDirections;
    }
}
