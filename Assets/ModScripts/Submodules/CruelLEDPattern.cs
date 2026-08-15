using System.Collections.Generic;
using System.Linq;
using wawa.Modules;
using static ComponentInfo;
using Random = UnityEngine.Random;

public class CruelLEDPattern : Puzzle
{

    readonly MainColors[,] LEDGrid =
    {
        {MainColors.Green, MainColors.Purple, MainColors.Lime, MainColors.Blue, MainColors.Red, MainColors.Black, MainColors.Blue, MainColors.Yellow, MainColors.Blue, MainColors.Blue, MainColors.Pink, MainColors.White, MainColors.Orange, MainColors.Black, MainColors.Blue, MainColors.Lime},
        {MainColors.Lime, MainColors.Lime, MainColors.Lime, MainColors.Blue, MainColors.Lime, MainColors.Yellow, MainColors.Pink, MainColors.Orange, MainColors.Lime, MainColors.Lime, MainColors.Orange, MainColors.Black, MainColors.Lime, MainColors.Red, MainColors.Purple, MainColors.Pink},
        {MainColors.Pink, MainColors.Yellow, MainColors.Green, MainColors.Blue, MainColors.Blue, MainColors.Cyan, MainColors.Purple, MainColors.Orange, MainColors.White, MainColors.Yellow, MainColors.Red, MainColors.Purple, MainColors.Lime, MainColors.Cyan, MainColors.Green, MainColors.Orange},
        {MainColors.White, MainColors.Green, MainColors.Blue, MainColors.Cyan, MainColors.Green, MainColors.Purple, MainColors.Red, MainColors.Lime, MainColors.Pink, MainColors.Yellow, MainColors.Purple, MainColors.Yellow, MainColors.Green, MainColors.White, MainColors.Green, MainColors.White},
        {MainColors.Lime, MainColors.White, MainColors.Blue, MainColors.Cyan, MainColors.Blue, MainColors.White, MainColors.White, MainColors.Green, MainColors.Black, MainColors.Blue, MainColors.Pink, MainColors.Cyan, MainColors.Orange, MainColors.Green, MainColors.Red, MainColors.Purple},
        {MainColors.Yellow, MainColors.Purple, MainColors.Orange, MainColors.Lime, MainColors.White, MainColors.Cyan, MainColors.Lime, MainColors.Yellow, MainColors.Blue, MainColors.Lime, MainColors.Cyan, MainColors.Lime, MainColors.Blue, MainColors.Pink, MainColors.White, MainColors.White},
        {MainColors.Lime, MainColors.Green, MainColors.Lime, MainColors.Pink, MainColors.Green, MainColors.Orange, MainColors.Black, MainColors.Yellow, MainColors.Blue, MainColors.Red, MainColors.Black, MainColors.Orange, MainColors.Pink, MainColors.Yellow, MainColors.Orange, MainColors.Lime},
        {MainColors.Cyan, MainColors.Green, MainColors.Cyan, MainColors.Orange, MainColors.White, MainColors.Blue, MainColors.Orange, MainColors.Black, MainColors.Lime, MainColors.Red, MainColors.Blue, MainColors.Pink, MainColors.Lime, MainColors.White, MainColors.Purple, MainColors.Orange},
        {MainColors.Purple, MainColors.Black, MainColors.Yellow, MainColors.Green, MainColors.Purple, MainColors.Cyan, MainColors.Lime, MainColors.Yellow, MainColors.Black, MainColors.White, MainColors.Red, MainColors.Green, MainColors.White, MainColors.Yellow, MainColors.Purple, MainColors.Red},
        {MainColors.Green, MainColors.Blue, MainColors.Orange, MainColors.Lime, MainColors.Purple, MainColors.Purple, MainColors.Orange, MainColors.Red, MainColors.Black, MainColors.Black, MainColors.Orange, MainColors.Pink, MainColors.Yellow, MainColors.Red, MainColors.Purple, MainColors.Green},
        {MainColors.Orange, MainColors.White, MainColors.Purple, MainColors.Red, MainColors.Red, MainColors.Orange, MainColors.Yellow, MainColors.Yellow, MainColors.Blue, MainColors.Purple, MainColors.Orange, MainColors.Blue, MainColors.Cyan, MainColors.Green, MainColors.Red, MainColors.Red},
        {MainColors.Red, MainColors.Orange, MainColors.Yellow, MainColors.White, MainColors.Lime, MainColors.Yellow, MainColors.White, MainColors.Purple, MainColors.White, MainColors.Green, MainColors.Pink, MainColors.Black, MainColors.Orange, MainColors.Blue, MainColors.Purple, MainColors.Pink},
        {MainColors.Lime, MainColors.Orange, MainColors.Red, MainColors.Green, MainColors.Green, MainColors.Pink, MainColors.Green, MainColors.Purple, MainColors.White, MainColors.Purple, MainColors.Yellow, MainColors.Purple, MainColors.Cyan, MainColors.Purple, MainColors.Black, MainColors.Blue},
        {MainColors.Pink, MainColors.Blue, MainColors.White, MainColors.Pink, MainColors.Cyan, MainColors.Pink, MainColors.Orange, MainColors.Orange, MainColors.Blue, MainColors.Lime, MainColors.Yellow, MainColors.Pink, MainColors.Lime, MainColors.Red, MainColors.Yellow, MainColors.Purple},
        {MainColors.Cyan, MainColors.Red, MainColors.Red, MainColors.Green, MainColors.Red, MainColors.Blue, MainColors.Lime, MainColors.Orange, MainColors.Lime, MainColors.Yellow, MainColors.Black, MainColors.Cyan, MainColors.Orange, MainColors.Pink, MainColors.Purple, MainColors.Pink},
        {MainColors.Orange, MainColors.Black, MainColors.Purple, MainColors.Yellow, MainColors.Yellow, MainColors.Purple, MainColors.Black, MainColors.Red, MainColors.Yellow, MainColors.Red, MainColors.Blue, MainColors.Purple, MainColors.Red, MainColors.Yellow, MainColors.Lime, MainColors.Blue},
        {MainColors.Orange, MainColors.Black, MainColors.Black, MainColors.Cyan, MainColors.Green, MainColors.Yellow, MainColors.Green, MainColors.Green, MainColors.Black, MainColors.Black, MainColors.Green, MainColors.Red, MainColors.White, MainColors.Orange, MainColors.Purple, MainColors.Orange},
        {MainColors.Yellow, MainColors.Pink, MainColors.White, MainColors.Purple, MainColors.Red, MainColors.Blue, MainColors.Green, MainColors.Blue, MainColors.Lime, MainColors.Yellow, MainColors.Red, MainColors.Cyan, MainColors.Pink, MainColors.Green, MainColors.Green, MainColors.Lime},
        {MainColors.Purple, MainColors.Yellow, MainColors.Orange, MainColors.White, MainColors.Cyan, MainColors.Black, MainColors.Red, MainColors.Yellow, MainColors.Black, MainColors.Yellow, MainColors.White, MainColors.Blue, MainColors.Red, MainColors.Pink, MainColors.Purple, MainColors.Green},
        {MainColors.Blue, MainColors.Lime, MainColors.Black, MainColors.Red, MainColors.Cyan, MainColors.Black, MainColors.Purple, MainColors.Lime, MainColors.Red, MainColors.Lime, MainColors.White, MainColors.Orange, MainColors.White, MainColors.Cyan, MainColors.White, MainColors.Orange},
        {MainColors.Green, MainColors.Orange, MainColors.Orange, MainColors.Cyan, MainColors.Black, MainColors.Pink, MainColors.Pink, MainColors.Lime, MainColors.Pink, MainColors.Red, MainColors.Green, MainColors.Cyan, MainColors.Cyan, MainColors.White, MainColors.Lime, MainColors.Cyan}
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
        int[][] directions =
        {
            new int[] { -1, 0 }, // up
            new int[] { 1, 0 }, // down
            new int[] { 0, 1 }, // right
            new int[] { 0, -1 }, // left
        };

        int width = LEDGrid.GetLength(1);
        int height = LEDGrid.GetLength(0);

        int x = Random.Range(0, width);
        int y = Random.Range(0, height);
        int d = Random.Range(0, 4);
        int[] direction = directions[d];

        List<MainColors> sequence = new List<MainColors>();

        for (int i = 0; i < 8; i++)
        {
            int xx = (x + direction[1] * i) % width;
            int yy = (y + direction[0] * i) % height;

            if (xx < 0) xx += width;
            if (yy < 0) yy += height;

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

        CorrectTimerDigit = leastHorizontalSection + leastVerticalSection;
    }
}
