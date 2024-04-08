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

    //Variables to be accessed in main script
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
        //Generate Bulb colors and button labels
        //Generate text and colors for Resistor
        //Generate timer text
        //Generate word display text
        //Generate number display text
        //Generate morse code display
    }
}
