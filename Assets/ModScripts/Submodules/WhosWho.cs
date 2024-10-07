using System.Collections.Generic;
using System.Diagnostics; // to track how long each button has been held
using System.Linq;
using KModkit;
using Random = UnityEngine.Random;
using static ComponentInfo;

public class WhosWho : Puzzle
{
    readonly int LEDtoUse;
    readonly Color LEDcolor;
    // public readonly string[] MainColors = { "Black", "Blue", "Cyan", "Green", "Lime", "Orange", "Pink", "Purple", "Red", "White", "Yellow" , "Gold" , "Silver" };
    enum Color
    {
        BLACK,
        BLUE,
        CYAN,
        GREEN,
        LIME,
        ORANGE,
        PINK,
        PURPLE,
        RED,
        WHITE,
        YELLOW
    }

    readonly Dictionary<Color, int> offsets = new Dictionary<Color, int>()
    {
        {Color.RED, 1},
        {Color.ORANGE, 2},
        {Color.YELLOW, 3},
        {Color.GREEN, -3},
        {Color.LIME, -1},
        {Color.CYAN, -2},
        {Color.BLUE, 4},
        {Color.PURPLE, 5},
        {Color.PINK, -5},
        {Color.BLACK, -4},
        {Color.WHITE, 6},
    };
    readonly Dictionary<Color, int> rows = new Dictionary<Color, int>()
    {
        {Color.RED, 2},
        {Color.ORANGE, 1},
        {Color.YELLOW, 1},
        {Color.GREEN, 3},
        {Color.LIME, 4},
        {Color.CYAN, 0},
        {Color.BLUE, 5},
        {Color.PURPLE, 6},
        {Color.PINK, 2},
        {Color.BLACK, 0},
        {Color.WHITE, 7},
    };

    readonly Stopwatch PressTime = new Stopwatch();

    bool submissionMode = false;

    readonly int finalNumber = 0;

    readonly string[][] words = new string[][]
    {
        new string[]{"YES", "FIRST", "DISPLAY", "A DISPLAY", "OKAY", "OK", "SAYS", "SEZ"},
        new string[]{"NOTHING", "", "BLANK", "IT’S BLANK", "NO", "KNOW", "NOSE", "KNOWS"},
        new string[]{"LED", "LEAD", "LEED", "READ", "RED", "REED", "HOLD ON", "YOU"},
        new string[]{"U", "YOU ARE", "UR", "YOUR", "YOU’RE", "THERE", "THEY’RE", "THEIR"},
        new string[]{"THEY ARE", "SEE", "C", "SEA", "CEE", "READY", "WHAT", "WHAT?"},
        new string[]{"UH", "UHHH", "UH UH", "UH HUH", "LEFT", "RIGHT", "WRITE", "MIDDLE"},
        new string[]{"WAIT", "WAIT!", "WEIGHT", "PRESS", "DONE", "DUMB", "NEXT", "HOLD"},
        new string[]{"SURE", "LIKE", "LICK", "LEEK", "LEAK", "I", "INDIA", "EYE"}
    };

    readonly Color[][] colors = new Color[][]
    {
        new Color[]{Color.RED, Color.ORANGE, Color.LIME, Color.BLUE, Color.CYAN, Color.PURPLE, Color.WHITE, Color.PINK},
        new Color[]{Color.ORANGE, Color.WHITE, Color.PURPLE, Color.GREEN, Color.RED, Color.CYAN, Color.YELLOW, Color.GREEN},
        new Color[]{Color.LIME, Color.BLACK, Color.BLUE, Color.PURPLE, Color.BLUE, Color.GREEN, Color.CYAN, Color.ORANGE},
        new Color[]{Color.CYAN, Color.LIME, Color.CYAN, Color.PINK, Color.PURPLE, Color.PINK, Color.BLUE, Color.CYAN},
        new Color[]{Color.BLACK, Color.PINK, Color.YELLOW, Color.ORANGE, Color.LIME, Color.YELLOW, Color.LIME, Color.BLUE},
        new Color[]{Color.BLUE, Color.CYAN, Color.WHITE, Color.BLACK, Color.PINK, Color.CYAN, Color.PINK, Color.PURPLE},
        new Color[]{Color.PURPLE, Color.YELLOW, Color.CYAN, Color.WHITE, Color.ORANGE, Color.YELLOW, Color.BLUE, Color.LIME},
        new Color[]{Color.YELLOW, Color.PURPLE, Color.ORANGE, Color.BLACK, Color.WHITE, Color.PURPLE, Color.BLACK, Color.PINK},
    };

    readonly List<string> listA = new List<string>();
    readonly List<string> listB = new List<string>();

    int listB_Index = 0;

    public WhosWho(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Solving Who's Who.", ModuleID);
        LEDtoUse = DetermineLED();
        LEDcolor = (Color)Info.LED[LEDtoUse];
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The chosen LED is {1}; the offset is {2} and the row to begin with is {3}.", ModuleID, LEDcolor.ToString().ToLowerInvariant(), offsets[LEDcolor] > 0 ? "+" + offsets[LEDcolor] : "" + offsets[LEDcolor], rows[LEDcolor] + 1);
        int row = rows[LEDcolor];
        int column = LEDtoUse;
        string cellWord;
        Color cellColor;
        for (int i = 0; i < 10; i++)
        {
            cellWord = words[row][column];
            cellColor = colors[row][column];
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
            int index = listB.IndexOf(x => x == commonWord);
            index = (((index / 8) + 1) + ((index % 8) + 1)) % 10;
            finalNumber = index;
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The number to submit is {1}.", ModuleID, finalNumber);
        }
    }

    private int DetermineLED()
    {
        Color[] warmColors = new Color[] { Color.RED, Color.ORANGE, Color.YELLOW };
        Color[] LEDcolors = Info.LED.Select(l => (Color)l).ToArray();
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

    private bool SameColorTwiceInARow(Color[] colors)
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
            if (!Module.IsSolving() && !Module.CheckValidComponents())
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
}
