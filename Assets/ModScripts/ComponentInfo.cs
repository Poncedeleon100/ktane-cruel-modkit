using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class ComponentInfo {

    //Strings
    public static readonly string[] WireColors = {"Black", "Blue", "Cyan", "Green", "Grey", "Lime", "Orange", "Pink", "Purple", "Red", "White", "Yellow"};
    public static readonly string[] ButtonList = {"Press", "Hold", "Detonate", "Mash", "Tap", "Push", "Abort", "Button", "Click", "_", "Nothing", "No", "I Don't Know", "Yes"};
    public static readonly string[] SymbolCharacters = {"©", "★", "☆", "ټ", "Җ", "Ω", "Ѭ", "Ѽ", "ϗ", "ϫ", "Ϭ", "Ϟ", "Ѧ", "æ", "Ԇ", "Ӭ", "҈", "Ҋ", "Ѯ", "¿", "¶", "Ͼ", "Ͽ", "ψ", "Ѫ", "Ҩ", "҂", "Ϙ", "ζ", "ƛ", "Ѣ", "ע", "⦖", "ኒ", "エ", "π", "Э", "⁋", "ᛤ", "Ƿ", "Щ", "ξ", "Ᵹ", "Ю", "௵", "ϑ", "Triquetra", "ꎵ", "よ"};
    public static readonly string[] IdentityNames = {"Clondar", "Colonel Mustard", "Cyanix", "Dr Orchid", "GhostSalt", "Konoko", "Lanaluff", "Magmy", "Melbor", "Miss Scarlett", "Mrs Peacock", "Mrs White", "Nibs", "Percy", "Pouse", "Professor Plum", "Red Penguin", "Reverend Green", "Sameone", "VFlyer", "Yabbaguy", "Yoshi Dojo"};
    public static readonly string[] IdentityItems = {"Candlestick", "Wrench", "Lead Pipe", "Rope", "Dagger", "Broom", "Revolver", "Water Gun", "Pearls", "Cane", "Bundle of Wires", "Giant Ring", "Specimen", "Fruit Basket", "Dozen Eggs", "Toolkit", "Hand Mirror", "Simon Says", "Manga", "Fishbowl", "Bomb"};
    public static readonly string[] IdentityLocations = {"Ballroom", "Conservatory", "Study", "Lounge", "Library", "Dining Room", "Hall", "Dojo", "Barnyard", "Treehouse", "I.T. Centre", "vOld", "Laboratory", "Supermarket", "Island", "Factory", "Home Depot", "Office", "Anime Con", "Arctic Base", "Solitary"};
    public static readonly string[] IdentityRarity = {"●", "♦", "★", "☆"};
    //                                                      black,                 blue,             cyan,                    green,             lime,               orange,                     pink,                   purple,              red,            white,           yellow
    public static readonly Color[] BulbColorsArray = {new Color(0,0,0), new Color(0,.498f,0), new Color(0,1,1), new Color(0,.557f,.078f), new Color(0,1,0), new Color(1,.502f,0), new Color(1,.235f,.784f), new Color(.498f,0,.498f), new Color(1,0,0), new Color(1,1,1), new Color(1,1,0)};
    public static readonly Color[] BulbColorHalosArray = {new Color(0,0,0), new Color(0,.498f,0), new Color(0,1,1), new Color(0,.557f,.078f), new Color(0,1,0), new Color(1,.502f,0), new Color(1,.235f,.784f), new Color(.498f,0,.498f), new Color(1,0,0), new Color(1,1,1), new Color(1,1,0)};
    public static readonly string[] WordList = {"YES", "FIRST", "DISPLAY", "A DISPLAY", "OKAY", "OK", "SAYS", "SEZ", "NOTHING", "_", "BLANK", "IT’S BLANK", "NO", "KNOW", "NOSE", "KNOWS", "LED", "LEAD", "LEED", "READ", "RED", "REED", "HOLD ON", "YOU", "U", "YOU ARE", "UR", "YOUR", "YOU’RE", "THERE", "THEY’RE", "THEIR", "THEY ARE", "SEE", "C", "SEA", "CEE", "READY", "WHAT", "WHAT?", "UH", "UHHH", "UH UH", "UH HUH", "LEFT", "RIGHT", "WRITE", "MIDDLE", "WAIT", "WAIT!", "WEIGHT", "PRESS", "DONE", "DUMB", "NEXT", "HOLD", "SURE", "LIKE", "LICK", "LEEK", "LEAK", "I", "INDIA", "EYE"};

    //For converting adjacent colors into their associated slider colors. Colors within string pairs are ordered alphabetically: Blue, Green, Red, Yellow
    Dictionary<string, int> SliderColors = new Dictionary<string, int> {
        {"01", 4}, //Cyan
        {"12", 5}, //Golden Yellow
        {"02", 6}, //Magenta
        {"23", 7}, //Orange
        {"03", 8}, //Black
        {"13", 9}, //White
    };

    //Colors
    public static readonly Color ButtonTextWhite = new Color(1,1,1);
    //public static readonly Color LightColors = {new Color()};

    //Variables to be accessed in the main script
    public int[][] Wires = new int[][] {
        new int[] {0, 0, 0, 0, 0, 0, 0},
        new int[] {0, 0, 0, 0, 0, 0, 0},
    };
    public int[] WireLED;
    public string ButtonText;
    public int Button;
    public int[] LED;
    public int[] Symbols;
    public string[] Alphabet = new string[6];
    public int[][] Arrows = new int[3][];
    public string[][] Identity = new string[4][];
    public bool BulbOLeft;
    public bool[] BulbInfo = new bool[4];
    public Color[] BulbColors = new Color[4];
    public int[][] ResistorColors = new int[4][];
    public string[] ResistorText;
    public int Timer;
    public string Word;
    public int Number;
    public string[] WidgetText = new string[3];
    public float Meter;

    public ComponentInfo() {
        List<int> Temp = new List<int>();
        //Generate colors for Wires
        for(int i = 0; i < 7; i++) {
            int TempColor1 = rnd.Range(0,12);
            int TempColor2 = rnd.Range(0,12);
            if(TempColor1 > TempColor2) {
                int x = TempColor2;
                TempColor2 = TempColor1;
                TempColor1 = x;
            }
                Wires[0][i] = TempColor1;
                Wires[1][i] = TempColor2;
        }
        //Generate LEDs/Stars for Wire LEDs
        while(Temp.Count < 7) {
            int Star = rnd.Range(0,3);
            int Color = rnd.Range(0,11);
            int Coefficient = (Star * 11);
            Color += Coefficient;
            Temp.Add(Color);
        }
        WireLED = Temp.ToArray();
        //Generate color and text for Button
        ButtonText = ButtonList[rnd.Range(0,14)];
        Button = rnd.Range(0,11);
        //Generate colors for LEDs
        Temp.Clear();
        while(Temp.Count < 8) {
            int Color = rnd.Range(0,11);
            Temp.Add(Color);
        }
        LED = Temp.ToArray();
        //Generate symbols
        Temp.Clear();
        while(Temp.Count < 6) {
            int Symbol = rnd.Range(0,49);
            if(!Temp.Contains(Symbol))
                Temp.Add(Symbol);
        }
        Symbols = Temp.ToArray();
        //Generate Alphabet text
        for(int i = 0; i < 6; i++) {
            string AlphabetKey = System.String.Empty;
            string[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
            string[] Numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
            int LetterAmount = rnd.Range(0,2);
            int NumberAmount = rnd.Range(0,2);
            for(int x = 0; x <= LetterAmount; x++) {
                AlphabetKey += Letters[x];
            }
            if(LetterAmount == 1 && NumberAmount == 1) {
                AlphabetKey += Environment.NewLine;
            }
            for(int x = 0; x <= NumberAmount; x++) {
                AlphabetKey += Numbers[x];
            }
            Alphabet[i] = AlphabetKey;
        }
        //Generate arrow colors
        Arrows[0] = new int[] {0, 1, 2, 3}.OrderBy(x => rnd.Range(0, 1000)).ToArray();
        //Create slider colors based on generated arrow colors
        string[] TempSliders = new string[4];
        for(int i = 0; i < 4; i++) {
            if(i == 3) {
                if(Arrows[0][0] > Arrows[0][i]) {
                    TempSliders[i] = Arrows[0][i].ToString() + Arrows[0][0].ToString();
                }
                else {
                TempSliders[i] = Arrows[0][0].ToString() + Arrows[0][i].ToString();
                }
            }
            else if(Arrows[0][i] > Arrows[0][i + 1]) {
                TempSliders[i] = Arrows[0][i + 1].ToString() + Arrows[0][i].ToString();
            }
            else {
                TempSliders[i] = Arrows[0][i].ToString() + Arrows[0][i + 1].ToString();
            }
        }
        Arrows[1] = new int[4];
        for(int i = 0; i < 4; i++) {
            Arrows[1][i] = SliderColors[TempSliders[i]];
        }
        Arrows[2] = new int[] {rnd.Range(8,10)};
        //Generate Identity information
        List<string> IdentityTemp = new List<string>();
        while(IdentityTemp.Count < 3) {
            string Name = IdentityNames[rnd.Range(0,22)];
            if(!IdentityTemp.Contains(Name))
                IdentityTemp.Add(Name);
        }
        Identity[0] = IdentityTemp.ToArray();
        IdentityTemp.Clear();
        while(IdentityTemp.Count < 3) {
            string Item = IdentityItems[rnd.Range(0,21)];
            if(!IdentityTemp.Contains(Item))
                IdentityTemp.Add(Item);
        }
        Identity[1] = IdentityTemp.ToArray();
        IdentityTemp.Clear();
        while(IdentityTemp.Count < 3) {
            string Location = IdentityLocations[rnd.Range(0,21)];
            if(!IdentityTemp.Contains(Location))
                IdentityTemp.Add(Location);
        }
        Identity[2] = IdentityTemp.ToArray();
        IdentityTemp.Clear();
        IdentityTemp.Add(IdentityRarity[rnd.Range(0,4)]);
        Identity[3] = IdentityTemp.ToArray();
        //Generate Bulb colors and button labels
        BulbOLeft = rnd.Range(0,2) == 0;
        for(int i = 0; i < 2; i++) {
            //Opacity of the bulb
            BulbInfo[i] = rnd.Range(0,2) == 0;
            //Whether it starts on or not
            BulbInfo[i + 2] = rnd.Range(0,2) == 0;
            //Color of the bulb (Bulb material is set to the first color, bulb light is set to the second color or the halo color)
            int ColorIndex = rnd.Range(0,BulbColorsArray.Length);
            Color TempBulbColor1 = BulbColorsArray[ColorIndex];
            TempBulbColor1[3] = BulbInfo[i] ? 1f : .55f;
            Color TempBulbColor2 = BulbColorHalosArray[ColorIndex];
            TempBulbColor2[3] = BulbInfo[i] ? 1f : .55f;
            BulbColors[i] = TempBulbColor1;
            BulbColors[i + 2] = TempBulbColor2;
        }
        //Generate text and colors for Resistor
        Temp.Clear();
        for(int i = 0; i < 4; i++) {
            while(Temp.Count < 3) {
                int Color = rnd.Range(0,13);
                if(!Temp.Contains(Color))
                    Temp.Add(Color);
            }
            ResistorColors[i] = Temp.ToArray();
            Temp.Clear();
        }
        string[] ResistorLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
        ResistorText = new string[2];
        ResistorText[0] = ResistorLetters[0];
        ResistorText[1] = ResistorLetters[1];
        //Generate timer text
        string TimerText = System.String.Empty;
        Timer = rnd.Range(0, 100);
        if(Timer < 10) {
            TimerText += "0";
        }
        TimerText += Timer.ToString();
        WidgetText[0] = TimerText;
        //Generate word display text
        Word = WidgetText[1] = WordList[rnd.Range(0, WordList.Length)];
        //Generate number display text
        Number = rnd.Range(0, 10);
        WidgetText[2] = Number.ToString();
        //Generate morse code display (Can't be bothered right now to be honest)

        //Generate meter value and color
        Meter = rnd.value;
        //Probably have to round the value to a reasonable number of decimal places (3), and then round it further if it's within a certain distance of a specific number

    }
}
