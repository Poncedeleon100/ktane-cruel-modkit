using KModkit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using wawa.Modules;
using static ComponentInfo;
using Random = UnityEngine.Random;

public class WhosWho : Puzzle
{
    readonly int LEDtoUse;
    readonly MainColors LEDcolor;

    readonly Dictionary<MainColors, int> offsets = new Dictionary<MainColors, int>()
    {
        {MainColors.Red, 1},
        {MainColors.Orange, 2},
        {MainColors.Yellow, 3},
        {MainColors.Green, -3},
        {MainColors.Lime, -1},
        {MainColors.Cyan, -2},
        {MainColors.Blue, 4},
        {MainColors.Purple, 5},
        {MainColors.Pink, -5},
        {MainColors.Black, -4},
        {MainColors.White, 6},
    };
    readonly Dictionary<MainColors, int> rows = new Dictionary<MainColors, int>()
    {
        {MainColors.Red, 2},
        {MainColors.Orange, 1},
        {MainColors.Yellow, 1},
        {MainColors.Green, 3},
        {MainColors.Lime, 4},
        {MainColors.Cyan, 0},
        {MainColors.Blue, 5},
        {MainColors.Purple, 6},
        {MainColors.Pink, 2},
        {MainColors.Black, 0},
        {MainColors.White, 7},
    };

    readonly Stopwatch PressTime = new Stopwatch();

    bool submissionMode = false;

    readonly int finalNumber = 0;

    readonly string[,] words = new string[,]
    {
        { "YES", "FIRST", "DISPLAY", "A DISPLAY", "OKAY", "OK", "SAYS", "SEZ" },
        { "NOTHING", "", "BLANK", "IT’S BLANK", "NO", "KNOW", "NOSE", "KNOWS" },
        { "LED", "LEAD", "LEED", "READ", "RED", "REED", "HOLD ON", "YOU" },
        { "U", "YOU ARE", "UR", "YOUR", "YOU’RE", "THERE", "THEY’RE", "THEIR" },
        { "THEY ARE", "SEE", "C", "SEA", "CEE", "READY", "WHAT", "WHAT?" },
        { "UH", "UHHH", "UH UH", "UH HUH", "LEFT", "RIGHT", "WRITE", "MIDDLE" },
        { "WAIT", "WAIT!", "WEIGHT", "PRESS", "DONE", "DUMB", "NEXT", "HOLD" },
        { "SURE", "LIKE", "LICK", "LEEK", "LEAK", "I", "INDIA", "EYE" }
    };

    readonly MainColors[,] colors = new MainColors[,]
    {
        { MainColors.Red, MainColors.Orange, MainColors.Lime, MainColors.Blue, MainColors.Cyan, MainColors.Purple, MainColors.White, MainColors.Pink },
        { MainColors.Orange, MainColors.White, MainColors.Purple, MainColors.Green, MainColors.Red, MainColors.Cyan, MainColors.Yellow, MainColors.Green },
        { MainColors.Lime, MainColors.Black, MainColors.Blue, MainColors.Purple, MainColors.Blue, MainColors.Green, MainColors.Cyan, MainColors.Orange },
        { MainColors.Cyan, MainColors.Lime, MainColors.Cyan, MainColors.Pink, MainColors.Purple, MainColors.Pink, MainColors.Blue, MainColors.Cyan },
        { MainColors.Black, MainColors.Pink, MainColors.Yellow, MainColors.Orange, MainColors.Lime, MainColors.Yellow, MainColors.Lime, MainColors.Blue },
        { MainColors.Blue, MainColors.Cyan, MainColors.White, MainColors.Black, MainColors.Pink, MainColors.Cyan, MainColors.Pink, MainColors.Purple },
        { MainColors.Purple, MainColors.Yellow, MainColors.Cyan, MainColors.White, MainColors.Orange, MainColors.Yellow, MainColors.Blue, MainColors.Lime },
        { MainColors.Yellow, MainColors.Purple, MainColors.Orange, MainColors.Black, MainColors.White, MainColors.Purple, MainColors.Black, MainColors.Pink },
    };

    readonly List<string> listA = new List<string>();
    readonly List<string> listB = new List<string>();

    int listB_Index = 0;

    public WhosWho(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Who's Who.");
        LEDtoUse = DetermineLED();
        LEDcolor = (MainColors)Info.LED[LEDtoUse];
        Module.Log("The chosen LED is {0}; the offset is {1} and the row to begin with is {2}.", LEDcolor.ToString().ToLowerInvariant(), offsets[LEDcolor] > 0 ? "+" + offsets[LEDcolor] : "" + offsets[LEDcolor], rows[LEDcolor] + 1);
        int row = rows[LEDcolor];
        int column = LEDtoUse;
        string cellWord;
        MainColors cellColor;
        for (int i = 0; i < 10; i++)
        {
            cellWord = words[row, column];
            cellColor = colors[row, column];
            if (listA.Contains(cellWord))
                break;
            else
                listA.Add(cellWord);
            row += 8;
            row += offsets[LEDcolor];
            row %= 8;
            column += 8;
            column += offsets[cellColor];
            column %= 8;
        }
        string word;
        int listBLength = Random.Range(10, 16);
        do
        {
            word = WordList.PickRandom();
            if (!listB.Contains(word))
                listB.Add(word);
        } while (listB.Count < listBLength);
        Module.Log("List A is as follows: [{0}].", string.Join(", ", listA.ToArray()));
        Module.Log("List B is as follows: [{0}].", string.Join(", ", listB.ToArray()));
        Module.WidgetText[1].text = listB.First();
        string commonWord = listB.FirstOrDefault(w => listA.Contains(w));
        if (commonWord == null)
        {
            Module.Log("Neither list has anything in common; the number to submit is 0.");
            finalNumber = 0;
        }
        else
        {
            Module.Log("The first word that each list has in common is {0}.", commonWord);
            finalNumber = CalculateFinalNumber(commonWord);
            Module.Log("The number to submit is {0}.", finalNumber);
        }
    }

    private int DetermineLED()
    {
        MainColors[] warmColors = new MainColors[] { MainColors.Red, MainColors.Orange, MainColors.Yellow };
        MainColors[] LEDcolors = Info.LED.Select(l => (MainColors)l).ToArray();
        if (Module.Bomb.GetSerialNumberNumbers().All(x => x % 2 == 0) || Module.Bomb.GetSerialNumberNumbers().All(x => x % 2 == 1))
        {
            Module.Log("All serial number digits have matching parity, using the third LED.");
            return 2;
        }
        else if (Module.Bomb.GetSerialNumberNumbers().Contains(Module.Bomb.GetBatteryCount()))
        {
            Module.Log("The amount of batteries matches a number in the serial number, using the eighth LED.");
            return 7;
        }
        else if (Module.Bomb.GetOnIndicators().Count() == Module.Bomb.GetOffIndicators().Count())
        {
            Module.Log("The amount of lit and unlit indicators are equal, using the first LED.");
            return 0;
        }
        else if (LEDcolors.Count(x => warmColors.Contains(x)) >= 3)
        {
            Module.Log("Three or more LEDs are warm colors (red, orange, or yellow), using the seventh LED.");
            return 6;
        }
        else if (Module.Bomb.GetPortPlateCount() <= Module.Bomb.GetPortCount())
        {
            Module.Log("The port plate count is less than or equal to the port count, using the fifth LED.");
            return 4;
        }
        else if (SameColorTwiceInARow(LEDcolors))
        {
            Module.Log("Two matching LEDs are next to each other, using the second LED.");
            return 1;
        }
        else if (Module.Bomb.GetPortPlates().Any(x => x.Contains("DVI") && (x.Contains("StereoRCA") || x.Contains("PS2"))))
        {
            Module.Log("A DVI-D port is on the same port plate as a Stereo RCA port or a PS/2 port, using the sixth LED.");
            return 5;
        }
        else if ((Module.Bomb.GetBatteryCount(1) > 0 && Module.Bomb.GetBatteryCount(2) == 0) || (Module.Bomb.GetBatteryCount(2) > 0 && Module.Bomb.GetBatteryCount(1) == 0))
        {
            Module.Log("There are only D batteries or AA batteries, using the fourth LED.");
            return 3;
        }
        else
        {
            string[] ordinals = new string[] { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth" };
            int value = Module.Bomb.GetSerialNumberNumbers().Sum() % 8;
            Module.Log("None of the conditions applied, using the {0} LED.", ordinals[value]);
            return value;
        }
    }

    private bool SameColorTwiceInARow(MainColors[] colors)
    {
        for (int i = 0; i < colors.Length - 1; i++)
            if (colors[i] == colors[i + 1])
                return true;
        return false;
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

        PressTime.Start();
    }

    public override void OnBulbButtonRelease(int Button)
    {
        PressTime.Stop();

        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.ButtonRelease);

        if (PressTime.Elapsed.TotalSeconds >= 1)
        {
            if (!submissionMode)
            {
                submissionMode = true;
                Module.Log("Entering submission mode.");
            }
            else if (Info.NumberDisplay == finalNumber)
            {
                Module.SolveModule("Correctly submitted {0}. Module solved.", finalNumber);
            }
            else
            {
                Module.Strike("Strike! Tried to submit {0} when the answer was {1}. Exiting submission mode.", Module.WidgetText[2].text, finalNumber);
                submissionMode = false;
            }
        }
        else
        {
            if (!submissionMode)
            {
                if (Button == 1)
                    listB_Index++;
                else
                {
                    listB_Index--;
                    listB_Index += listB.Count;
                }

                listB_Index %= listB.Count;
                Module.WidgetText[1].text = listB[listB_Index];
            }
            else
            {
                if (Button == 1)
                    Info.NumberDisplay++;
                else
                    Info.NumberDisplay += 9; // add 10 and subtract 1 so that the number is never negative
                Info.NumberDisplay %= 10;
                Module.WidgetText[2].text = Info.NumberDisplay.ToString();
            }
        }
        PressTime.Reset();
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

        if (!submissionMode)
        {
            listB_Index = 0;
            Module.WidgetText[1].text = listB[listB_Index];
        }
    }

    private int CalculateFinalNumber(string Word)
    {
        int FinalNumber = 0;

        for (int i = 0; i < words.GetLength(0); i++)
        {
            for (int j = 0;  j < words.GetLength(1); j++)
            {
                if ((words[i, j] == Word))
                    FinalNumber = i + j + 2;
            }
        }

        return FinalNumber;
    }
}
