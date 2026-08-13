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

public class CruelLEDPattern : Puzzle
{

    readonly MainColors[,] LEDGrid =
    {
        {MainColors.White, MainColors.Blue, MainColors.Black, MainColors.Lime, MainColors.Green, MainColors.Green, MainColors.Green, MainColors.Blue, MainColors.Yellow, MainColors.Purple, MainColors.Purple, MainColors.Blue, MainColors.Pink, MainColors.Green, MainColors.Blue, MainColors.White},
        {MainColors.Lime, MainColors.Cyan, MainColors.White, MainColors.Green, MainColors.Green, MainColors.Orange, MainColors.Orange, MainColors.White, MainColors.Cyan, MainColors.Green, MainColors.Red, MainColors.Black, MainColors.White, MainColors.Purple, MainColors.Green, MainColors.Pink},
        {MainColors.Green, MainColors.Black, MainColors.Yellow, MainColors.Blue, MainColors.Orange, MainColors.Green, MainColors.Purple, MainColors.Blue, MainColors.Orange, MainColors.Lime, MainColors.Red, MainColors.Red, MainColors.Purple, MainColors.Pink, MainColors.Black, MainColors.Blue},
        {MainColors.Black, MainColors.Black, MainColors.Red, MainColors.Red, MainColors.White, MainColors.Lime, MainColors.Red, MainColors.Pink, MainColors.Pink, MainColors.Purple, MainColors.Orange, MainColors.Yellow, MainColors.Pink, MainColors.Lime, MainColors.Lime, MainColors.White},
        {MainColors.Lime, MainColors.Cyan, MainColors.Red, MainColors.Black, MainColors.Cyan, MainColors.Green, MainColors.Lime, MainColors.Lime, MainColors.Red, MainColors.Yellow, MainColors.Orange, MainColors.Cyan, MainColors.Pink, MainColors.Orange, MainColors.Orange, MainColors.Purple},
        {MainColors.White, MainColors.Green, MainColors.Green, MainColors.Cyan, MainColors.White, MainColors.Purple, MainColors.Purple, MainColors.Orange, MainColors.Red, MainColors.Lime, MainColors.Black, MainColors.Yellow, MainColors.Red, MainColors.Yellow, MainColors.Purple, MainColors.Green},
        {MainColors.Lime, MainColors.Orange, MainColors.White, MainColors.Pink, MainColors.Purple, MainColors.Purple, MainColors.Red, MainColors.Lime, MainColors.Yellow, MainColors.Green, MainColors.Yellow, MainColors.Purple, MainColors.Red, MainColors.Yellow, MainColors.Blue, MainColors.Red},
        {MainColors.Black, MainColors.Cyan, MainColors.Yellow, MainColors.Green, MainColors.Black, MainColors.Blue, MainColors.Yellow, MainColors.Orange, MainColors.Pink, MainColors.White, MainColors.Lime, MainColors.Lime, MainColors.Lime, MainColors.White, MainColors.Cyan, MainColors.Lime},
        {MainColors.Green, MainColors.Pink, MainColors.Pink, MainColors.Blue, MainColors.Blue, MainColors.White, MainColors.Yellow, MainColors.Yellow, MainColors.Blue, MainColors.Orange, MainColors.Cyan, MainColors.Blue, MainColors.Orange, MainColors.Blue, MainColors.Yellow, MainColors.Pink},
        {MainColors.Purple, MainColors.Black, MainColors.Orange, MainColors.White, MainColors.Lime, MainColors.Lime, MainColors.Blue, MainColors.Lime, MainColors.Orange, MainColors.White, MainColors.Orange, MainColors.Orange, MainColors.Lime, MainColors.Yellow, MainColors.Red, MainColors.Cyan},
        {MainColors.Cyan, MainColors.Red, MainColors.Red, MainColors.Lime, MainColors.Yellow, MainColors.Pink, MainColors.Orange, MainColors.Red, MainColors.Black, MainColors.Blue, MainColors.Pink, MainColors.Orange, MainColors.Yellow, MainColors.Purple, MainColors.Yellow, MainColors.Lime},
        {MainColors.Purple, MainColors.Red, MainColors.White, MainColors.Lime, MainColors.Lime, MainColors.Black, MainColors.White, MainColors.Yellow, MainColors.Yellow, MainColors.Cyan, MainColors.Purple, MainColors.Green, MainColors.White, MainColors.Purple, MainColors.Purple, MainColors.Lime},
        {MainColors.Blue, MainColors.Orange, MainColors.Green, MainColors.Blue, MainColors.Orange, MainColors.Purple, MainColors.Green, MainColors.Pink, MainColors.Green, MainColors.Orange, MainColors.Purple, MainColors.Red, MainColors.Cyan, MainColors.Red, MainColors.Purple, MainColors.Yellow},
        {MainColors.Yellow, MainColors.Purple, MainColors.Blue, MainColors.Green, MainColors.Lime, MainColors.Orange, MainColors.Black, MainColors.Orange, MainColors.Orange, MainColors.Blue, MainColors.Cyan, MainColors.Black, MainColors.Pink, MainColors.Lime, MainColors.Blue, MainColors.White}
    };

    int[][] PatternCoordinates =
    {
        new int[] { 0, 0, 0, 0, 0, 0, 0, 0 },
        new int[] { 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    int CorrectTimerDigit;

    public CruelLEDPattern(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Cruel LED Pattern.");
        GenerateValidPattern();
        Module.Log("LEDs present: {0}.", Info.GetLEDInfo());
        IdentifySections();
        Module.Log($"The correct timer digit is {CorrectTimerDigit}.");
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

        int lastTimerDigit = (int)Module.Bomb.GetTime() % 10;

        if (lastTimerDigit == CorrectTimerDigit)
        {
            Module.SolveModule($"Correctly pressed the ❖ button when the last digit on the countdown timer was {lastTimerDigit}. Module solved.");
        }
        else
        {
            Module.Strike($"Strike! Incorrectly pressed the ❖ button when the last seconds digit on the countdown timer was {lastTimerDigit}.");
        }
    }

    void GenerateValidPattern()
    {
        string[] directionNames = { "up", "down", "right", "left" };

        int[][] directions =
        {
            new int[] { -1, 0 }, // up
            new int[] { 1, 0 }, // down
            new int[] { 0, 1 }, // right
            new int[] { 0, -1 }, // left
        };

        int x = Random.Range(0, 16);
        int y = Random.Range(0, 14);
        int d = Random.Range(0, 4);
        int[] direction = directions[d];

        List<MainColors> sequence = new List<MainColors>();

        for (int i = 0; i < 8; i++)
        {
            int xx = (x + direction[1] * i) % 16;
            int yy = (y + direction[0] * i) % 14;

            if (xx < 0) xx += 16;
            if (yy < 0) yy += 14;

            sequence.Add(LEDGrid[yy, xx]);
            PatternCoordinates[0][i] = xx;
            PatternCoordinates[1][i] = yy;
        }

        Info.LED = sequence.Select(j => (int)j).ToArray();
        Module.SetLEDs();
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
            int verticalSection = (y - (y % 2)) / 2;

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

        CorrectTimerDigit = leastHorizontalSection + leastVerticalSection;
    }
}
