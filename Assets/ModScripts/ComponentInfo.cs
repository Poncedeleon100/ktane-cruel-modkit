using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComponentInfo
{
    // Strings
    public static readonly string[] ButtonList = { "PRESS", "HOLD", "DETONATE", "MASH", "TAP", "PUSH", "ABORT", "BUTTON", "CLICK", "", "NOTHING", "NO", "I DON'T KNOW", "YES" };
    public static readonly string[] SymbolCharacters = { "©", "★", "☆", "ټ", "Җ", "Ω", "Ѭ", "Ѽ", "ϗ", "ϫ", "Ϭ", "Ϟ", "Ѧ", "æ", "Ԇ", "Ӭ", "҈", "Ҋ", "Ѯ", "¿", "¶", "Ͼ", "Ͽ", "ψ", "Ѫ", "Ҩ", "҂", "Ϙ", "ζ", "ƛ", "Ѣ", "ע", "⦖", "ኒ", "エ", "π", "Э", "⁋", "ᛤ", "Ƿ", "Щ", "ξ", "Ᵹ", "Ю", "௵", "ϑ", "Triquetra", "ꎵ", "よ" };
    public static readonly string[] IdentityNames = { "Clondar", "Colonel Mustard", "Cyanix", "Dr. Orchid", "GhostSalt", "Konoko", "Lanaluff", "Magmy", "Melbor", "Miss Scarlett", "Mrs. Peacock", "Mrs. White", "Nibs", "Percy", "Pouse", "Professor Plum", "Red Penguin", "Reverend Green", "Sameone", "VFlyer", "Yabbaguy", "Yoshi Dojo" };
    public static readonly string[] IdentityItems = { "Candlestick", "Wrench", "Lead Pipe", "Rope", "Dagger", "Broom", "Revolver", "Water Gun", "Pearls", "Cane", "Bundle of Wires", "Giant Ring", "Specimen", "Fruit Basket", "Dozen Eggs", "Toolkit", "Hand Mirror", "Simon Says", "Manga", "Fishbowl", "Bomb", "Salt" };
    public static readonly string[] IdentityLocations = { "Ballroom", "Conservatory", "Study", "Lounge", "Library", "Dining Room", "Hall", "Dojo", "Barnyard", "Treehouse", "I.T. Centre", "vOld", "Laboratory", "Supermarket", "Island", "Factory", "Home Depot", "Office", "Anime Con", "Arctic Base", "Solitary", "Mansion" };
    public static readonly string[] IdentityRarity = { "●", "♦", "★", "☆" };
    public static readonly string[] WordList = { "YES", "FIRST", "DISPLAY", "A DISPLAY", "OKAY", "OK", "SAYS", "SEZ", "NOTHING", "", "BLANK", "IT’S BLANK", "NO", "KNOW", "NOSE", "KNOWS", "LED", "LEAD", "LEED", "READ", "RED", "REED", "HOLD ON", "YOU", "U", "YOU ARE", "UR", "YOUR", "YOU’RE", "THERE", "THEY’RE", "THEIR", "THEY ARE", "SEE", "C", "SEA", "CEE", "READY", "WHAT", "WHAT?", "UH", "UHHH", "UH UH", "UH HUH", "LEFT", "RIGHT", "WRITE", "MIDDLE", "WAIT", "WAIT!", "WEIGHT", "PRESS", "DONE", "DUMB", "NEXT", "HOLD", "SURE", "LIKE", "LICK", "LEEK", "LEAK", "I", "INDIA", "EYE" };
    public static readonly string[] MorseList = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    // Dictionaries
    // For converting adjacent arrow colors into their associated slider colors. Colors within string pairs must be ordered alphabetically: Blue, Green, Red, Yellow
    static readonly Dictionary<string, int> SliderColors = new Dictionary<string, int>
    {
        { ((int)ArrowColors.Blue).ToString() +   ((int)ArrowColors.Blue).ToString(),   (int)ArrowColors.Blue },
        { ((int)ArrowColors.Green).ToString() +  ((int)ArrowColors.Green).ToString(),  (int)ArrowColors.Green },
        { ((int)ArrowColors.Red).ToString() +    ((int)ArrowColors.Red).ToString(),    (int)ArrowColors.Red },
        { ((int)ArrowColors.Yellow).ToString() + ((int)ArrowColors.Yellow).ToString(), (int)ArrowColors.Yellow },
        { ((int)ArrowColors.Blue).ToString() +   ((int)ArrowColors.Green).ToString(),  (int)ArrowColors.Cyan },
        { ((int)ArrowColors.Green).ToString() +  ((int)ArrowColors.Red).ToString(),    (int)ArrowColors.GoldYellow },
        { ((int)ArrowColors.Blue).ToString() +   ((int)ArrowColors.Red).ToString(),    (int)ArrowColors.Magenta },
        { ((int)ArrowColors.Red).ToString() +    ((int)ArrowColors.Yellow).ToString(), (int)ArrowColors.Orange },
        { ((int)ArrowColors.Blue).ToString() +   ((int)ArrowColors.Yellow).ToString(), (int)ArrowColors.Grey },
        { ((int)ArrowColors.Green).ToString() +  ((int)ArrowColors.Yellow).ToString(), (int)ArrowColors.White },
    };
    // These dictionaries are intended to convert unfriendly enum names into friendly names for helpful logging
    public static readonly Dictionary<PianoKeys, string> PianoKeyNames = new Dictionary<PianoKeys, string>
    {
        { PianoKeys.C, "C" },
        { PianoKeys.Db, "C#/Db" },
        { PianoKeys.D, "D" },
        { PianoKeys.Eb, "D#/Eb" },
        { PianoKeys.E, "E" },
        { PianoKeys.F, "F" },
        { PianoKeys.Gb, "F#/Gb" },
        { PianoKeys.G, "G" },
        { PianoKeys.Ab, "G#/Ab" },
        { PianoKeys.A, "A" },
        { PianoKeys.Bb, "A#/Bb" },
        { PianoKeys.B, "B" },
    };
    public static readonly Dictionary<ArrowColors, string> ArrowColorNames = new Dictionary<ArrowColors, string>
    {
        { ArrowColors.Blue, "Blue" },
        { ArrowColors.Green, "Green" },
        { ArrowColors.Red, "Red" },
        { ArrowColors.Yellow, "Yellow" },
        { ArrowColors.Cyan, "Cyan" },
        { ArrowColors.GoldYellow, "Golden Yellow" },
        { ArrowColors.Magenta, "Magenta" },
        { ArrowColors.Orange, "Orange" },
        { ArrowColors.Grey, "Grey" },
        { ArrowColors.White, "White" }
    };
    public static readonly Dictionary<ArrowDirections, string> ArrowDirectionNames = new Dictionary<ArrowDirections, string>
    {
        { ArrowDirections.Up, "Up" },
        { ArrowDirections.Right, "Right" },
        { ArrowDirections.Down, "Down" },
        { ArrowDirections.Left, "Left" },
        { ArrowDirections.UpRight, "Up-Right" },
        { ArrowDirections.DownRight, "Down-Right" },
        { ArrowDirections.DownLeft, "Down-Left" },
        { ArrowDirections.UpLeft, "Up-Left" },
        { ArrowDirections.Center, "Center" },
    };

    // Enums
    public enum WireColors { Black, Blue, Cyan, Green, Grey, Lime, Orange, Pink, Purple, Red, White, Yellow }
    // Used for Wire LEDs, Button, and LEDs
    public enum MainColors { Black, Blue, Cyan, Green, Lime, Orange, Pink, Purple, Red, White, Yellow }
    public enum BulbColorNames { Blue, Cyan, Green, Grey, Lime, Orange, Pink, Purple, Red, White, Yellow }
    // This one deviates from alphabetical order because of how each strip needs a specific range of colors.
    // Every color is present on every strip except for black, white, gold, and silver.
    public enum ResistorColorNames { Black, White, Blue, Brown, Grey, Green, Orange, Purple, Red, Yellow, Gold, Silver }
    public enum MeterColors { Red, Orange, Yellow, Green, Blue, Purple }
    public enum KeyColors { Black, Blue, Green, Orange, Pink, Purple, Red, White, Yellow }
    // The following enums have unfriendly names, so use them as puzzle constants only and use the associated dictionaries for logging
    public enum PianoKeys { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B }
    public enum ArrowColors { Blue, Green, Red, Yellow, Cyan, GoldYellow, Magenta, Orange, Grey, White }
    public enum ArrowDirections { Up, Right, Down, Left, UpRight, DownRight, DownLeft, UpLeft, Center }

    //Colors
    public static readonly Color ButtonTextWhite = new Color(1, 1, 1);
    public static readonly Color ButtonTextBlack = new Color(0, 0, 0);
    //                                                                blue,                  green,                red,             yellow,               cyan,            gold-yellow,            magenta,                 orange,                          grey,               white
    public static readonly Color[] ArrowLightColors = { new Color(0, 0, 1), new Color(0, .737f, 0), new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 1), new Color(1, .753f, 0), new Color(1, 0, 1), new Color(1, .647f, 0), new Color(.326f, .326f, .326f), new Color(1, 1, 1) };
    //                                                                 blue,               cyan,                green,                           grey,               lime,                 orange,                       pink,                     purple,                red,              white,            yellow
    public static readonly Color[] BulbColorValues = { new Color(0, 0, .5f), new Color(0, 1, 1), new Color(0, .5f, 0), new Color(.326f, .326f, .326f), new Color(0, 1, 0), new Color(1, .502f, 0), new Color(1, .235f, .784f), new Color(.498f, 0, .498f), new Color(1, 0, 0), new Color(1, 1, 1), new Color(1, 1, 0) };

    //Variables to be accessed in the main script
    public int[][] Wires = new int[][]
    {
        new int[] { 0, 0, 0, 0, 0, 0, 0 },
        new int[] { 0, 0, 0, 0, 0, 0, 0 },
    };
    public int[] WireLED;
    public string ButtonText;
    public int Button;
    public int[] LED;
    public int[] Symbols;
    public string[] Alphabet = new string[6];
    public int Piano;
    public int[] Arrows = new int[9];
    public bool[] BulbInfo = new bool[5];
    public int[] BulbColors = new int[2];
    public int[] Identity = new int[4];
    public bool[] ResistorReversed = new bool[2];
    public int[] ResistorColors = new int[8];
    public string[] ResistorText = new string[4];
    public int TimerDisplay;
    public string WordDisplay;
    public int NumberDisplay;
    public string Morse;
    public int MeterColor;
    public double MeterValue;

    public ComponentInfo()
    {
        GenerateWireInfo();
        GenerateWireLEDInfo();
        GenerateButtonInfo();
        GenerateLEDInfo();
        GenerateSymbolInfo();
        GenerateAlphabetInfo();
        GeneratePianoInfo();
        GenerateArrowInfo();
        GenerateBulbInfo();
        GenerateResistorInfo();
        GenerateIdentityInfo();
        GenerateTimerInfo();
        GenerateWordInfo();
        GenerateNumberInfo();
        GenerateMorseInfo();
        GenerateMeterInfo();
    }

    public void GenerateWireInfo()
    {
        for (int i = 0; i < 7; i++)
        {
            int TempColor1 = Random.Range(0, 12);
            int TempColor2 = Random.Range(0, 12);
            if (TempColor1 > TempColor2)
            {
                int x = TempColor2;
                TempColor2 = TempColor1;
                TempColor1 = x;
            }
            Wires[0][i] = TempColor1;
            Wires[1][i] = TempColor2;
        }
    }

    public void GenerateWireLEDInfo()
    {
        List<int> Colors = new List<int>();
        while (Colors.Count < 7)
        {
            int Star = Random.Range(0, 3);
            int Color = Random.Range(0, 11);
            int Coefficient = (Star * 11);
            Color += Coefficient;
            Colors.Add(Color);
        }
        WireLED = Colors.ToArray();
    }

    public void GenerateButtonInfo()
    {
        ButtonText = ButtonList[Random.Range(0, ButtonList.Length)];
        Button = Random.Range(0, 11);
    }

    public void GenerateLEDInfo()
    {
        List<int> Colors = new List<int>();
        while (Colors.Count < 8)
        {
            int Color = Random.Range(0, 11);
            Colors.Add(Color);
        }
        LED = Colors.ToArray();
    }

    public void GenerateSymbolInfo()
    {
        List<int> GeneratedSymbols = new List<int>();
        while (GeneratedSymbols.Count < 6)
        {
            int Symbol = Random.Range(0, 49);
            if (!GeneratedSymbols.Contains(Symbol))
                GeneratedSymbols.Add(Symbol);
        }
        Symbols = GeneratedSymbols.ToArray();
    }

    public void GenerateAlphabetInfo()
    {
        List<string> GeneratedAlphabet = new List<string>();
        while (GeneratedAlphabet.Count < 6)
        {
            string AlphabetKey = String.Empty;
            string[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
            string[] Numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
            int LetterAmount = Random.Range(0, 2);
            int NumberAmount = Random.Range(0, 2);
            for (int x = 0; x <= LetterAmount; x++)
                AlphabetKey += Letters[x];
            if (LetterAmount == 1 && NumberAmount == 1)
                AlphabetKey += "\n";
            for (int x = 0; x <= NumberAmount; x++)
                AlphabetKey += Numbers[x];

            if(!GeneratedAlphabet.Contains(AlphabetKey))
                GeneratedAlphabet.Add(AlphabetKey);
        }
        Alphabet = GeneratedAlphabet.ToArray();
    }

    public void GeneratePianoInfo()
    {
        Piano = Random.Range(0, 3);
    }

    public void GenerateArrowInfo()
    {
        int[] ArrowColors = new int[] { 0, 1, 2, 3 }.OrderBy(x => Random.Range(0, 1000)).ToArray();
        for (int i = 0; i < 4; i++)
            Arrows[i] = ArrowColors[i];

        //Create slider colors based on generated arrow colors
        string[] GeneratedSliders = new string[4];
        for (int i = 0; i < 4; i++)
        {
            if (i == 3)
            {
                if (Arrows[0] > Arrows[i])
                    GeneratedSliders[i] = Arrows[i].ToString() + Arrows[0].ToString();
                else
                    GeneratedSliders[i] = Arrows[0].ToString() + Arrows[i].ToString();
            }
            else if (Arrows[i] > Arrows[i + 1])
                GeneratedSliders[i] = Arrows[i + 1].ToString() + Arrows[i].ToString();
            else
                GeneratedSliders[i] = Arrows[i].ToString() + Arrows[i + 1].ToString();
        }
        for (int i = 0; i < 4; i++)
            Arrows[i + 4] = SliderColors[GeneratedSliders[i]];

        Arrows[(int)ArrowDirections.Center] = Random.Range(8, 10);
    }

    public void GenerateBulbInfo()
    {
        // BulbInfo layout (It's weird so that this information can be iterated through if needed):
        // 0 = Is Bulb 1 opaque?
        // 1 = Is Bulb 2 opaque?
        // 2 = Does Bulb 1 start on?
        // 3 = Does Bulb 2 start on?
        // 4 = Is the O button on the left?
        BulbInfo[4] = Random.Range(0, 2) == 0;
        for (int i = 0; i < 2; i++)
        {
            //Color of the bulb
            BulbColors[i] = Random.Range(0, BulbColorValues.Length);
            //Opacity of the bulb
            BulbInfo[i] = Random.Range(0, 2) == 0;
            //Whether it starts on or not
            BulbInfo[i + 2] = Random.Range(0, 2) == 0;
        }
    }

    public void GenerateResistorInfo()
    {
        for (int i = 0; i < ResistorReversed.Length; i++)
            ResistorReversed[i] = Random.Range(0, 2) == 0;

        for (int i = 0; i < 2; i++)
        {
            if (ResistorReversed[i])
            {
                ResistorColors[i] = Random.Range(2, 12);
                ResistorColors[i + 2] = Random.Range(0, 12);
                ResistorColors[i + 4] = Random.Range(0, 10);
                ResistorColors[i + 6] = Random.Range(1, 10);
            }
            else
            {
                ResistorColors[i] = Random.Range(1, 10);
                ResistorColors[i + 2] = Random.Range(0, 10);
                ResistorColors[i + 4] = Random.Range(0, 12);
                ResistorColors[i + 6] = Random.Range(2, 12);
            }
        }

        string[] ResistorLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        for (int i = 0; i < ResistorText.Length; i++)
            ResistorText[i] = ResistorLetters[i];
    }
    public void GenerateIdentityInfo()
    {
        Identity[0] = Random.Range(0, IdentityNames.Length);
        Identity[1] = Random.Range(0, IdentityItems.Length);
        Identity[2] = Random.Range(0, IdentityLocations.Length);
        Identity[3] = Random.Range(0, IdentityRarity.Length);
    }

    public void GenerateTimerInfo()
    {
        TimerDisplay = Random.Range(0, 100000);
    }

    public void GenerateWordInfo()
    {
        WordDisplay = WordList[Random.Range(0, WordList.Length)];
    }

    public void GenerateNumberInfo()
    {
        NumberDisplay = Random.Range(0, 10);
    }

    public void GenerateMorseInfo()
    {
        Morse = String.Empty;
        for (int i = 0; i < 3; i++)
            Morse += MorseList[Random.Range(0, MorseList.Length)];
    }

    public void GenerateMeterInfo()
    {
        MeterColor = Random.Range(0, 6);
        MeterValue = Random.value;

        //Rounds the value if it's close enough to one of the lines on the meter (0, 1/4, 1/3, 1/2, 2/3, 3/4, 1)
        // in order to prevent ambiguous meter readings
        if (MeterValue < 0.02d)
            MeterValue = 0;
        else if (0.23f < MeterValue && MeterValue < 0.27f)
            MeterValue = 0.25d;
        else if (0.32f < MeterValue && MeterValue < 0.345f)
            MeterValue = 0.333d;
        else if (0.48f < MeterValue && MeterValue < 0.52f)
            MeterValue = 0.5d;
        else if (0.65f < MeterValue && MeterValue < 0.68f)
            MeterValue = 0.667d;
        else if (0.765f < MeterValue && MeterValue < 0.735f)
            MeterValue = 0.75d;
        else if (0.98f < MeterValue)
            MeterValue = 1;
    }

    public string GetWireInfo()
    {
        List<string> Names = new List<string>();

        for (int i = 0; i < Wires[0].Length; i++)
        {
            if (Wires[0][i] == Wires[1][i])
                Names.Add(Enum.GetName(typeof(WireColors), Wires[0][i]));
            else
                Names.Add(Enum.GetName(typeof(WireColors), Wires[0][i]) + "/" + Enum.GetName(typeof(WireColors), Wires[1][i]));
        }

        return Names.Join(", ");
    }

    public string GetWireLEDInfo()
    {
        List<string> Names = new List<string>();

        for (int i = 0; i < WireLED.Length; i++)
        {
            string Color = Enum.GetName(typeof(MainColors), WireLED[i] % 11);
            string StarName = String.Empty;

            int Star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((WireLED[i] / 11))));

            switch (Star)
            {
                case 0:
                    StarName = "";
                    break;
                case 1:
                    StarName = " w/ Black Star";
                    break;
                case 2:
                    StarName = " w/ White Star";
                    break;
            }

            Names.Add(Color + StarName);
        }

        return Names.Join(", ");
    }

    public string GetButtonInfo()
    {
        string Article;

        if (ButtonText == "")
            Article = "no";
        else
        {
            char FirstLetter = ButtonText.ToLower()[0];
            if ("aeiou".IndexOf(FirstLetter) >= 0)
                Article = "an ";
            else
                Article = "a ";
        }
        
        return Enum.GetName(typeof(MainColors), Button) + " with " + Article + ButtonText.ToUpper() + " label";
    }

    public string GetLEDInfo()
    {
        List<string> Names = new List<string>();

        for (int i = 0; i < LED.Length; i++)
            Names.Add(Enum.GetName(typeof(MainColors), LED[i]));

        return Names.Join(", ");
    }

    public string GetSymbolInfo()
    {
        List<string> Names = new List<string>();

        for (int i = 0; i < Symbols.Length; i++)
            Names.Add(SymbolCharacters[Symbols[i]]);

        return Names.Join(", ");
    }

    public string GetAlphabetInfo()
    {
        List<string> Names = new List<string>();

        for (int i = 0; i < Alphabet.Length; i++)
            Names.Add(Alphabet[i].Replace(Environment.NewLine, string.Empty));

        return Names.Join(", ");
    }

    public string GetArrowsInfo()
    {
        string[] Names = new string[Arrows.Length];

        foreach (int Arrow in Arrows)
            Names[Array.IndexOf(Arrows, Arrow)] = ArrowColorNames[(ArrowColors)Arrow];

        return "Up: " + Names[0] + ", Right: " + Names[1] + ", Down: " + Names[2] + ", Left: " + Names[3] + ", Up-Right: " + Names[4] + ", Down-Right: " + Names[5] + ", Down-Left: " + Names[6] + ", Up-Left: " + Names[7] + ", Center: " + Names[8];
    }

    public string GetResistorInfo(int ResistorNumber)
    {
        List<string> Names = new List<string>();

        if (ResistorReversed[ResistorNumber])
        {
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 6]));
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 4]));
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 2]));
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 0]));
        }
        else
        {
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 0]));
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 2]));
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 4]));
            Names.Add(Enum.GetName(typeof(ResistorColorNames), ResistorColors[ResistorNumber + 6]));
        }

        return Names.Join(", ");
    }
}