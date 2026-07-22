using System;
using System.Linq;
using wawa.Modules;
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

    public UnscrewMaze(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Unscrew Maze.");
        Module.Log("Morse characters are {0}. ", Info.Morse);

        positions = Base36ToDec(Info.Morse);
        Module.Log("The starting position is ({0}, {1}).", Math.Floor(positions[0] / 6f)+1, (positions[0] % 6) + 1);
        Module.Log("Bulb 1's coordinate is ({0}, {1}) and Bulb 2's coordinate is ({2}, {3}).", Math.Floor(positions[1] / 6f) + 1, (positions[1] % 6) + 1, Math.Floor(positions[2] / 6f) + 1, (positions[2] % 6) + 1);
        Module.Log("The center button is {0}.", Info.Arrows[(int)ArrowDirections.Center] == (int)ArrowColors.White ? "white. Use the arrow directions to navigate" : "grey. Use the arrow colors to navigate");
        
        curPos = positions[0];
    }


    public override void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);
        Module.Play(Module.transform, Module.ArrowSounds[Arrow].name);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} arrow button was pressed when the component selection was [{1}] instead of [{2}].", ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetEnabledComponents(), Module.GetTargetComponents());
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
            Module.Strike("Strike! You hit a wall by moving {0} at the coordinates ({1}, {2}). Resetting maze position.", ArrowDirectionNames[(ArrowDirections)movementNum].ToLower(), Math.Floor(curPos / 6f) + 1, (curPos % 6) + 1);
            curPos = positions[0];
            UpdateMorse();
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
                Module.Strike("Strike! The {0} bulb was removed when the component selection was [{1}] instead of [{2}].", (Bulb + 1) == 1 ? "first" : "second", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbOn[Bulb]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];
        bulbsSolved[Bulb] = !bulbsSolved[Bulb];

        Module.Shake(Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1]));

        if (Module.IsModuleSolved() || BulbScrewedIn[Bulb])
            return;

        if (positions[Bulb + 1] != curPos)
        {
            Module.Strike("Bulb {0} incorrectly unscrewed at ({1}, {2}). Resetting maze position.", Bulb + 1, Math.Floor(curPos / 6f) + 1, (curPos % 6) + 1);
            curPos = positions[0];
            UpdateMorse();

            Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbOn[Bulb]);

            BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];
            bulbsSolved[Bulb] = !bulbsSolved[Bulb];
        }

        if (bulbsSolved[0] && bulbsSolved[1])
        {
            Module.SolveModule("Both bulbs have been unscrewed. Module solved.");
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
