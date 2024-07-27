using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class ComponentInfo
{
    public static readonly string[] WireColors = { "Black", "Blue", "Cyan", "Green", "Grey", "Lime", "Orange", "Pink", "Purple", "Red", "White", "Yellow" };
    public static readonly string[] ButtonList = { "Press", "Hold", "Detonate", "Mash", "Tap", "Push", "Abort", "Button", "Click", "_", "Nothing", "No", "I Don't Know", "Yes" };
    public static readonly string[] SymbolCharacters = { "©", "★", "☆", "ټ", "Җ", "Ω", "Ѭ", "Ѽ", "ϗ", "ϫ", "Ϭ", "Ϟ", "Ѧ", "æ", "Ԇ", "Ӭ", "҈", "Ҋ", "Ѯ", "¿", "¶", "Ͼ", "Ͽ", "ψ", "Ѫ", "Ҩ", "҂", "Ϙ", "ζ", "ƛ", "Ѣ", "ע", "⦖", "ኒ", "エ", "π", "Э", "⁋", "ᛤ", "Ƿ", "Щ", "ξ", "Ᵹ", "Ю", "௵", "ϑ", "Triquetra", "ꎵ", "よ" };
    //                                                                blue,                  green,                red,             yellow,               cyan,            gold-yellow,            magenta,                 orange,              black,           white
    public static readonly Color[] ArrowLightColors = { new Color(0, 0, 1), new Color(0, .737f, 0), new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 1), new Color(1, .753f, 0), new Color(1, 0, 1), new Color(1, .647f, 0), new Color(0, 0, 0), new Color(1, 1, 1) };
    public readonly string[] IdentityNames = { "Clondar", "Colonel Mustard", "Cyanix", "Dr Orchid", "GhostSalt", "Konoko", "Lanaluff", "Magmy", "Melbor", "Miss Scarlett", "Mrs Peacock", "Mrs White", "Nibs", "Percy", "Pouse", "Professor Plum", "Red Penguin", "Reverend Green", "Sameone", "VFlyer", "Yabbaguy", "Yoshi Dojo" };
    public readonly string[] IdentityItems = { "Candlestick", "Wrench", "Lead Pipe", "Rope", "Dagger", "Broom", "Revolver", "Water Gun", "Pearls", "Cane", "Bundle of Wires", "Giant Ring", "Specimen", "Fruit Basket", "Dozen Eggs", "Toolkit", "Hand Mirror", "Simon Says", "Manga", "Fishbowl", "Bomb" };
    public readonly string[] IdentityLocations = { "Ballroom", "Conservatory", "Study", "Lounge", "Library", "Dining Room", "Hall", "Dojo", "Barnyard", "Treehouse", "I.T. Centre", "vOld", "Laboratory", "Supermarket", "Island", "Factory", "Home Depot", "Office", "Anime Con", "Arctic Base", "Solitary" };
    public readonly string[] IdentityRarity = {"●", "♦", "★", "☆"};
    //                                                              black,                   blue,               cyan,                      green,               lime,                 orange,                       pink,                     purple,                red,              white,            yellow
    public static readonly Color[] BulbColorsArray = { new Color(0, 0, 0), new Color(0, .498f, 0), new Color(0, 1, 1), new Color(0, .557f, .078f), new Color(0, 1, 0), new Color(1, .502f, 0), new Color(1, .235f, .784f), new Color(.498f, 0, .498f), new Color(1, 0, 0), new Color(1, 1, 1), new Color(1, 1, 0) };
    public static readonly Color[] BulbColorHalosArray = { new Color(0, 0, 0), new Color(0, .498f, 0), new Color(0, 1, 1), new Color(0, .557f, .078f), new Color(0, 1, 0), new Color(1, .502f, 0), new Color(1, .235f, .784f), new Color(.498f, 0, .498f), new Color(1, 0, 0), new Color(1, 1, 1), new Color(1, 1, 0) };
    public static readonly string[] WordList = { "YES", "FIRST", "DISPLAY", "A DISPLAY", "OKAY", "OK", "SAYS", "SEZ", "NOTHING", "_", "BLANK", "IT’S BLANK", "NO", "KNOW", "NOSE", "KNOWS", "LED", "LEAD", "LEED", "READ", "RED", "REED", "HOLD ON", "YOU", "U", "YOU ARE", "UR", "YOUR", "YOU’RE", "THERE", "THEY’RE", "THEIR", "THEY ARE", "SEE", "C", "SEA", "CEE", "READY", "WHAT", "WHAT?", "UH", "UHHH", "UH UH", "UH HUH", "LEFT", "RIGHT", "WRITE", "MIDDLE", "WAIT", "WAIT!", "WEIGHT", "PRESS", "DONE", "DUMB", "NEXT", "HOLD", "SURE", "LIKE", "LICK", "LEEK", "LEAK", "I", "INDIA", "EYE" };
    public readonly string[] MorseList = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    //For converting adjacent colors into their associated slider colors. Colors within string pairs are ordered alphabetically: Blue, Green, Red, Yellow
    Dictionary<string, int> SliderColors = new Dictionary<string, int>
    {
        { "01", 4 }, //Cyan
        { "12", 5 }, //Golden Yellow
        { "02", 6 }, //Magenta
        { "23", 7 }, //Orange
        { "03", 8 }, //Black
        { "13", 9 }, //White
    };

    //Colors
    public static readonly Color ButtonTextWhite = new Color(1, 1, 1);

    //Logging
    public readonly string[] AdventureNames = { "Attack", "Defend", "Item", "Left", "Up", "Down", "Right" };
    public readonly string[] PianoKeyNames = { "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B" };
    public readonly string[] ArrowDirections = { "Up", "Right", "Down", "Left", "Up/Right", "Down/Right", "Down/Left", "Up/Left", "Center" };

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
    public Color[] ArrowLights = new Color[9];
    public int[][] Identity = new int[4][];
    public bool BulbOLeft;
    public bool[] BulbInfo = new bool[4];
    public Color[] BulbColors = new Color[4];
    public int[][] ResistorColors = new int[4][];
    public string[] ResistorText;
    public int TimerDisplay;
    public string WordDisplay;
    public int NumberDisplay;
    public string Morse;
    public int MeterColor;
    public float MeterValue;

    public ComponentInfo() {
        List<int> Temp = new List<int>();
        //Generate colors for Wires
        for(int i = 0; i < 7; i++)
        {
            int TempColor1 = Random.Range(0, 12);
            int TempColor2 = Random.Range(0, 12);
            if(TempColor1 > TempColor2)
            {
                int x = TempColor2;
                TempColor2 = TempColor1;
                TempColor1 = x;
            }
                Wires[0][i] = TempColor1;
                Wires[1][i] = TempColor2;
        }
        //Generate LEDs/Stars for Wire LEDs
        while(Temp.Count < 7) {
            int Star = Random.Range(0, 3);
            int Color = Random.Range(0, 11);
            int Coefficient = (Star * 11);
            Color += Coefficient;
            Temp.Add(Color);
        }
        WireLED = Temp.ToArray();
        //Generate color and text for Button
        ButtonText = ButtonList[Random.Range(0, 14)];
        Button = Random.Range(0, 11);
        //Generate colors for LEDs
        Temp.Clear();
        while(Temp.Count < 8)
        {
            int Color = Random.Range(0, 11);
            Temp.Add(Color);
        }
        LED = Temp.ToArray();
        //Generate symbols
        Temp.Clear();
        while(Temp.Count < 6)
        {
            int Symbol = Random.Range(0, 49);
            if(!Temp.Contains(Symbol))
                Temp.Add(Symbol);
        }
        Symbols = Temp.ToArray();
        //Generate Alphabet text
        for(int i = 0; i < 6; i++)
        {
            string AlphabetKey = String.Empty;
            string[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
            string[] Numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
            int LetterAmount = Random.Range(0, 2);
            int NumberAmount = Random.Range(0, 2);
            for(int x = 0; x <= LetterAmount; x++)
            {
                AlphabetKey += Letters[x];
            }
            if(LetterAmount == 1 && NumberAmount == 1)
            {
                AlphabetKey += Environment.NewLine;
            }
            for(int x = 0; x <= NumberAmount; x++)
            {
                AlphabetKey += Numbers[x];
            }
            Alphabet[i] = AlphabetKey;
        }
        //Generate Piano octave
        Piano = Random.Range(0, 3);
        //Generate arrow colors
        int[] ArrowColors = new int[] { 0, 1, 2, 3 }.OrderBy(x => Random.Range(0, 1000)).ToArray();
        for(int i = 0; i < 4; i++)
        {
            Arrows[i] = ArrowColors[i];
        }
        //Create slider colors based on generated arrow colors
        string[] TempSliders = new string[4];
        for(int i = 0; i < 4; i++)
        {
            if(i == 3) {
                if(Arrows[0] > Arrows[i])
                {
                    TempSliders[i] = Arrows[i].ToString() + Arrows[0].ToString();
                }
                else
                {
                    TempSliders[i] = Arrows[0].ToString() + Arrows[i].ToString();
                }
            }
            else if(Arrows[i] > Arrows[i + 1])
            {
                TempSliders[i] = Arrows[i + 1].ToString() + Arrows[i].ToString();
            }
            else
            {
                TempSliders[i] = Arrows[i].ToString() + Arrows[i + 1].ToString();
            }
        }
        for(int i = 0; i < 4; i++)
        {
            Arrows[i + 4] = SliderColors[TempSliders[i]];
        }
        Arrows[8] = Random.Range(8, 10);
        for(int i = 0; i < 9; i++)
        {
            ArrowLights[i] = ArrowLightColors[Arrows[i]];
        }
        //Generate Identity information
        List<int> IdentityTemp = new List<int>();
        while(IdentityTemp.Count < 3)
        {
            int Name = Random.Range(0, 22);
            if(!IdentityTemp.Contains(Name))
                IdentityTemp.Add(Name);
        }
        Identity[0] = IdentityTemp.ToArray();
        IdentityTemp.Clear();
        while(IdentityTemp.Count < 3)
        {
            int Item = Random.Range(0, 21);
            if(!IdentityTemp.Contains(Item))
                IdentityTemp.Add(Item);
        }
        Identity[1] = IdentityTemp.ToArray();
        IdentityTemp.Clear();
        while(IdentityTemp.Count < 3)
        {
            int Location = Random.Range(0, 21);
            if(!IdentityTemp.Contains(Location))
                IdentityTemp.Add(Location);
        }
        Identity[2] = IdentityTemp.ToArray();
        IdentityTemp.Clear();
        IdentityTemp.Add(Random.Range(0, 4));
        Identity[3] = IdentityTemp.ToArray();
        //Generate Bulb colors and button labels
        BulbOLeft = Random.Range(0, 2) == 0;
        for(int i = 0; i < 2; i++)
        {
            //Opacity of the bulb
            BulbInfo[i] = Random.Range(0, 2) == 0;
            //Whether it starts on or not
            BulbInfo[i + 2] = Random.Range(0, 2) == 0;
            //Color of the bulb (Bulb material is set to the first color, bulb light is set to the second color or the halo color)
            int ColorIndex = Random.Range(0, BulbColorsArray.Length);
            Color TempBulbColor1 = BulbColorsArray[ColorIndex];
            TempBulbColor1[3] = BulbInfo[i] ? 1f : .55f;
            Color TempBulbColor2 = BulbColorHalosArray[ColorIndex];
            TempBulbColor2[3] = BulbInfo[i] ? 1f : .55f;
            BulbColors[i] = TempBulbColor1;
            BulbColors[i + 2] = TempBulbColor2;
        }
        //Generate text and colors for Resistor
        Temp.Clear();
        for(int i = 0; i < 4; i++)
        {
            while(Temp.Count < 3)
            {
                int Color = Random.Range(0, 13);
                if(!Temp.Contains(Color))
                    Temp.Add(Color);
            }
            ResistorColors[i] = Temp.ToArray();
            Temp.Clear();
        }
        string[] ResistorLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        ResistorText = new string[2];
        ResistorText[0] = ResistorLetters[0];
        ResistorText[1] = ResistorLetters[1];
        //Generate timer text
        TimerDisplay = Random.Range(0, 100);
        //Generate word display text
        WordDisplay = WordList[Random.Range(0, WordList.Length)];
        //Generate number display text
        NumberDisplay = Random.Range(0, 10);
        //Generate morse code display (Random string of 3 characters; may be changed by modules)
        Morse = String.Empty;
        for (int i = 0; i < 3; i++)
        {
            Morse += MorseList[Random.Range(0, MorseList.Length)];
        }
        //Generate meter value and color
        MeterColor = Random.Range(0, 6);
        MeterValue = Random.value;
        //Rounds the value if it's close enough to one of the lines on the meter (0, 1/4, 1/3, 1/2, 2/3, 3/4, 1)
        if (MeterValue < 0.02f)
        {
            MeterValue = 0;
        }
        else if (0.23f < MeterValue && MeterValue < 0.27f)
        {
            MeterValue = 0.25f;
        }
        else if (0.32f < MeterValue && MeterValue < 0.345f)
        {
            MeterValue = 0.333f;
        }
        else if (0.48f < MeterValue && MeterValue < 0.52f)
        {
            MeterValue = 0.5f;
        }
        else if (0.65f < MeterValue && MeterValue < 0.68f)
        {
            MeterValue = 0.667f;
        }
        else if (0.765f < MeterValue && MeterValue < 0.735f)
        {
            MeterValue = 0.75f;
        }
        else if (0.98f < MeterValue)
        {
            MeterValue = 1;
        }
    }

    public void RegenWires()
    {
        List<int> Temp = new List<int>();
        //Generate colors for Wires
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
        //Generate LEDs/Stars for Wire LEDs
        while (Temp.Count < 7)
        {
            int Star = Random.Range(0, 3);
            int Color = Random.Range(0, 11);
            int Coefficient = (Star * 11);
            Color += Coefficient;
            Temp.Add(Color);
        }
        WireLED = Temp.ToArray();
    }
}