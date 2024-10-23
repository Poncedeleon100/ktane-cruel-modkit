using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KModkit;
using Random = UnityEngine.Random;
using static ComponentInfo;

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

    public WhosWho(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Solving Who's Who.", ModuleID);
        LEDtoUse = DetermineLED();
        LEDcolor = (MainColors)Info.LED[LEDtoUse];
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The chosen LED is {1}; the offset is {2} and the row to begin with is {3}.", ModuleID, LEDcolor.ToString().ToLowerInvariant(), offsets[LEDcolor] > 0 ? "+" + offsets[LEDcolor] : "" + offsets[LEDcolor], rows[LEDcolor] + 1);
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
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] List A is as follows: [{1}].", ModuleID, string.Join(", ", listA.ToArray()));
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] List B is as follows: [{1}].", ModuleID, string.Join(", ", listB.ToArray()));
        Module.WidgetText[1].text = listB.First();
        string commonWord = listB.FirstOrDefault(w => listA.Contains(w));
        if (commonWord == null)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Neither list has anything in common; the number to submit is 0.", ModuleID);
            finalNumber = 0;
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The first word that each list has in common is {1}.", ModuleID, commonWord);
            finalNumber = CalculateFinalNumber(commonWord);
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The number to submit is {1}.", ModuleID, finalNumber);
        }
    }

    private int DetermineLED()
    {
        MainColors[] warmColors = new MainColors[] { MainColors.Red, MainColors.Orange, MainColors.Yellow };
        MainColors[] LEDcolors = Info.LED.Select(l => (MainColors)l).ToArray();
        if (Module.Bomb.GetSerialNumberNumbers().All(x => x % 2 == 0) || Module.Bomb.GetSerialNumberNumbers().All(x => x % 2 == 1))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] All serial number digits have matching parity, using the third LED.", ModuleID);
            return 2;
        }
        else if (Module.Bomb.GetSerialNumberNumbers().Contains(Module.Bomb.GetBatteryCount()))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The amount of batteries matches a number in the serial number, using the eighth LED.", ModuleID);
            return 7;
        }
        else if (Module.Bomb.GetOnIndicators().Count() == Module.Bomb.GetOffIndicators().Count())
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The amount of lit and unlit indicators are equal, using the first LED.", ModuleID);
            return 0;
        }
        else if (LEDcolors.Count(x => warmColors.Contains(x)) >= 3)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Three or more LEDs are warm colors (red, orange, or yellow), using the seventh LED.", ModuleID);
            return 6;
        }
        else if (Module.Bomb.GetPortPlateCount() <= Module.Bomb.GetPortCount())
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The port plate count is less than or equal to the port count, using the fifth LED.", ModuleID);
            return 4;
        }
        else if (SameColorTwiceInARow(LEDcolors))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Two matching LEDs are next to each other, using the second LED.", ModuleID);
            return 1;
        }
        else if (Module.Bomb.GetPortPlates().Any(x => x.Contains("DVI") && (x.Contains("StereoRCA") || x.Contains("PS2"))))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] A DVI-D port is on the same port plate as a Stereo RCA port or a PS/2 port, using the sixth LED.", ModuleID);
            return 5;
        }
        else if ((Module.Bomb.GetBatteryCount(1) > 0 && Module.Bomb.GetBatteryCount(2) == 0) || (Module.Bomb.GetBatteryCount(2) > 0 && Module.Bomb.GetBatteryCount(1) == 0))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] There are only D batteries or AA batteries, using the fourth LED.", ModuleID);
            return 3;
        }
        else
        {
            string[] ordinals = new string[] { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth" };
            int value = Module.Bomb.GetSerialNumberNumbers().Sum() % 8;
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] None of the conditions applied, using the {1} LED.", ModuleID, ordinals[value]);
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

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Bulbs[Button].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, (Button == 2) == Info.BulbInfo[4] ? "O" : "I", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
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

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Module.transform);

        if (PressTime.Elapsed.TotalSeconds >= 1)
        {
            if (!submissionMode)
            {
                submissionMode = true;
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Entering submission mode.", ModuleID);
            }
            else if (Info.NumberDisplay == finalNumber)
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Correctly submitted {1}. Module solved.", ModuleID, finalNumber);
                Module.StartSolve();
                Module.Solve();
            }
            else
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Tried to submit {1} when the answer was {2}. Exiting submission mode.", ModuleID, Module.WidgetText[2].text, finalNumber);
                Module.CauseStrike();
                submissionMode = false;
            }
        }
        else
        {
            if (!submissionMode)
            {
                if (Button == 3)
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
                if (Button == 3)
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

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.UtilityButton.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
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
