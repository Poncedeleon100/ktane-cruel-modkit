using KModkit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;
using Random = UnityEngine.Random;

public class ReverseCruelLEDPattern : Puzzle
{

    readonly MainColors[,] LEDGrid =
    {
        {MainColors.Cyan, MainColors.Orange, MainColors.Orange, MainColors.Purple, MainColors.Purple, MainColors.Pink, MainColors.Blue, MainColors.Yellow, MainColors.Orange, MainColors.Cyan, MainColors.Black, MainColors.Black, MainColors.Yellow, MainColors.Pink, MainColors.Yellow, MainColors.Blue},
        {MainColors.Purple, MainColors.Yellow, MainColors.Cyan, MainColors.Blue, MainColors.Blue, MainColors.Orange, MainColors.Purple, MainColors.Black, MainColors.Purple, MainColors.Orange, MainColors.Black, MainColors.Pink, MainColors.Pink, MainColors.Green, MainColors.Pink, MainColors.Black},
        {MainColors.Green, MainColors.Green, MainColors.Cyan, MainColors.Cyan, MainColors.Purple, MainColors.Black, MainColors.Pink, MainColors.Orange, MainColors.Green, MainColors.Purple, MainColors.Green, MainColors.Orange, MainColors.Green, MainColors.Green, MainColors.Yellow, MainColors.Pink},
        {MainColors.Blue, MainColors.Blue, MainColors.Lime, MainColors.Pink, MainColors.Red, MainColors.Lime, MainColors.Red, MainColors.Pink, MainColors.Pink, MainColors.Black, MainColors.Pink, MainColors.Green, MainColors.Black, MainColors.Yellow, MainColors.Black, MainColors.Green},
        {MainColors.Cyan, MainColors.Lime, MainColors.White, MainColors.Red, MainColors.Blue, MainColors.Red, MainColors.White, MainColors.Lime, MainColors.Orange, MainColors.Blue, MainColors.Red, MainColors.Black, MainColors.White, MainColors.Red, MainColors.Yellow, MainColors.Black},
        {MainColors.Yellow, MainColors.Cyan, MainColors.Green, MainColors.Blue, MainColors.White, MainColors.Cyan, MainColors.Blue, MainColors.White, MainColors.Purple, MainColors.Cyan, MainColors.Yellow, MainColors.Green, MainColors.Purple, MainColors.Orange, MainColors.Green, MainColors.White},
        {MainColors.Blue, MainColors.Black, MainColors.Black, MainColors.Cyan, MainColors.Red, MainColors.Red, MainColors.Orange, MainColors.Blue, MainColors.Lime, MainColors.White, MainColors.Red, MainColors.Black, MainColors.Blue, MainColors.Cyan, MainColors.Red, MainColors.White},
        {MainColors.Lime, MainColors.Cyan, MainColors.Purple, MainColors.Lime, MainColors.Lime, MainColors.Lime, MainColors.Black, MainColors.Lime, MainColors.Black, MainColors.Purple, MainColors.Black, MainColors.Purple, MainColors.Black, MainColors.Yellow, MainColors.Pink, MainColors.Cyan},
        {MainColors.Purple, MainColors.Blue, MainColors.Orange, MainColors.Lime, MainColors.Green, MainColors.White, MainColors.Cyan, MainColors.Black, MainColors.Orange, MainColors.Cyan, MainColors.Green, MainColors.Blue, MainColors.Orange, MainColors.Black, MainColors.Black, MainColors.Green},
        {MainColors.Yellow, MainColors.Red, MainColors.Black, MainColors.Purple, MainColors.Green, MainColors.Red, MainColors.White, MainColors.Cyan, MainColors.Blue, MainColors.Cyan, MainColors.White, MainColors.Lime, MainColors.Green, MainColors.Orange, MainColors.Green, MainColors.Cyan},
        {MainColors.Black, MainColors.Green, MainColors.Lime, MainColors.Black, MainColors.Purple, MainColors.White, MainColors.Green, MainColors.Red, MainColors.Purple, MainColors.Lime, MainColors.Pink, MainColors.Green, MainColors.Black, MainColors.Lime, MainColors.Red, MainColors.Green},
        {MainColors.Red, MainColors.Blue, MainColors.Green, MainColors.Green, MainColors.Cyan, MainColors.Orange, MainColors.Lime, MainColors.Green, MainColors.Red, MainColors.Orange, MainColors.Red, MainColors.Cyan, MainColors.White, MainColors.Cyan, MainColors.White, MainColors.Orange},
        {MainColors.Green, MainColors.Blue, MainColors.Orange, MainColors.Purple, MainColors.Yellow, MainColors.Blue, MainColors.Pink, MainColors.Blue, MainColors.Blue, MainColors.Lime, MainColors.White, MainColors.Cyan, MainColors.White, MainColors.Red, MainColors.Lime, MainColors.Orange},
        {MainColors.Cyan, MainColors.Purple, MainColors.Orange, MainColors.White, MainColors.White, MainColors.Red, MainColors.White, MainColors.Orange, MainColors.Orange, MainColors.Pink, MainColors.Blue, MainColors.Cyan, MainColors.Red, MainColors.Blue, MainColors.Purple, MainColors.Black},
        {MainColors.Blue, MainColors.Purple, MainColors.Lime, MainColors.Pink, MainColors.Red, MainColors.Blue, MainColors.White, MainColors.Orange, MainColors.Black, MainColors.Blue, MainColors.Black, MainColors.Blue, MainColors.White, MainColors.Lime, MainColors.Black, MainColors.Yellow},
        {MainColors.Red, MainColors.White, MainColors.Red, MainColors.Pink, MainColors.Blue, MainColors.Blue, MainColors.Black, MainColors.Purple, MainColors.Black, MainColors.Red, MainColors.Pink, MainColors.Blue, MainColors.Yellow, MainColors.Black, MainColors.Yellow, MainColors.White},
        {MainColors.Orange, MainColors.Purple, MainColors.Green, MainColors.Pink, MainColors.Black, MainColors.Green, MainColors.Lime, MainColors.Black, MainColors.Red, MainColors.Cyan, MainColors.Green, MainColors.Red, MainColors.Lime, MainColors.White, MainColors.Yellow, MainColors.Yellow},
        {MainColors.Purple, MainColors.White, MainColors.Black, MainColors.Black, MainColors.Yellow, MainColors.Lime, MainColors.Green, MainColors.Red, MainColors.Cyan, MainColors.Green, MainColors.Red, MainColors.Cyan, MainColors.Black, MainColors.Black, MainColors.Orange, MainColors.Red},
        {MainColors.White, MainColors.Black, MainColors.Purple, MainColors.Pink, MainColors.Cyan, MainColors.Red, MainColors.Red, MainColors.Purple, MainColors.Lime, MainColors.Yellow, MainColors.Yellow, MainColors.Pink, MainColors.Pink, MainColors.Green, MainColors.Cyan, MainColors.Green},
        {MainColors.Lime, MainColors.Black, MainColors.Blue, MainColors.Blue, MainColors.Purple, MainColors.Pink, MainColors.Cyan, MainColors.Red, MainColors.Blue, MainColors.White, MainColors.Red, MainColors.Yellow, MainColors.Cyan, MainColors.Yellow, MainColors.Green, MainColors.Yellow},
        {MainColors.Green, MainColors.White, MainColors.White, MainColors.Green, MainColors.Orange, MainColors.Pink, MainColors.Purple, MainColors.Blue, MainColors.Blue, MainColors.Purple, MainColors.Purple, MainColors.Blue, MainColors.Green, MainColors.Purple, MainColors.Green, MainColors.Lime}
    };

    int[][] PatternCoordinates =
    {
        new int[] { 0, 0, 0, 0, 0, 0, 0, 0 },
        new int[] { 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    int RootPianoKey;
    int CorrectTimerDigit;
    int SubmittedTimerDigit;
    bool ValidPattern;

    public ReverseCruelLEDPattern(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Reverse Cruel LED Pattern.");
        Module.Log("Timer display is {0}.", Info.TimerDisplay.ToString().PadLeft(5, '0'));
        CorrectTimerDigit = Info.TimerDisplay % 10;
        Module.Log($"The correct timer digit is {CorrectTimerDigit}.");
        RootPianoKey = Random.Range(0, 12);
        Module.Log($"The root piano key is {PianoKeyNames[(PianoKeys)RootPianoKey]}.");
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

        int currentLED = Piano - RootPianoKey;
        if (currentLED < 0) currentLED += 12;
        if (currentLED > 7) return;

        int currentLEDValue = Info.LED[currentLED];
        currentLEDValue++;
        if (currentLEDValue > 10) currentLEDValue -= 11;
        Info.LED[currentLED] = currentLEDValue;
        Module.SetLEDs();
    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.UtilityButton.GetComponentInChildren<KMSelectable>(), 0.5f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The ❖ button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        Module.Log("Submitted LED pattern: {0}.", Info.GetLEDInfo());

        LocatePattern();

        if (!ValidPattern)
        {
            Module.Strike($"Strike! The submitted pattern was invalid.");
            return;
        }

        IdentifySections();

        if (SubmittedTimerDigit == CorrectTimerDigit)
        {
            Module.SolveModule($"The submitted pattern corresponds to the correct timer digit, which was {SubmittedTimerDigit}. Module solved.");
        }
        else
        {
            Module.Strike($"Strike! The submitted pattern corresponds to an incorrect timer digit, which was {SubmittedTimerDigit}.");
        }
    }

    void LocatePattern()
    {
        ValidPattern = false;

        int[][] directions =
        {
            new int[] { -1, 0 }, // up
            new int[] { 1, 0 }, // down
            new int[] { 0, 1 }, // right
            new int[] { 0, -1 }, // left
        };

        int width = LEDGrid.GetLength(1);
        int height = LEDGrid.GetLength(0);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (LEDGrid[y, x] != (MainColors)Info.LED[0])
                {
                    continue;
                }

                foreach (int[] d in directions)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        int xx = (x + d[1] * i) % width;
                        int yy = (y + d[0] * i) % height;

                        if (xx < 0) xx += width;
                        if (yy < 0) yy += height;

                        if (LEDGrid[yy, xx] != (MainColors)Info.LED[i])
                        {
                            break;
                        }

                        PatternCoordinates[0][i] = xx;
                        PatternCoordinates[1][i] = yy;

                        if (i == 7)
                        {
                            ValidPattern = true;
                        }
                    }
                    
                    if (ValidPattern)
                    {
                        break;
                    }
                }

                if (ValidPattern)
                {
                    break;
                }
            }

            if (ValidPattern)
            {
                break;
            }
        }
    }

    void IdentifySections()
    {
        List<int> horizontalSections = new List<int>();
        List<int> verticalSections = new List<int>();

        for (int i = 0; i < 8; i++)
        {
            int x = PatternCoordinates[0][i];
            int y = PatternCoordinates[1][i];

            int horizontalSection = (x - (x % 4)) / 4;
            int verticalSection = (y - (y % 3)) / 3;

            horizontalSections.Add(horizontalSection);
            verticalSections.Add(verticalSection);
        }

        int leastHorizontalSection = horizontalSections
            .GroupBy(x => x)
            .OrderBy(g => g.Count())
            .ThenBy(g => g.Key)
            .First()
            .Key;

        int leastVerticalSection = verticalSections
            .GroupBy(x => x)
            .OrderBy(g => g.Count())
            .ThenBy(g => g.Key)
            .First()
            .Key;

        SubmittedTimerDigit = leastHorizontalSection + leastVerticalSection;
    }
}
