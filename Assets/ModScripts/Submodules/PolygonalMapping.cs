using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//i really fucking hope im doing this right

public class PolygonalMapping : Puzzle
{

    readonly string Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string IgnoreString;
    List<string> FinalOrder = new List<string>();
    Vector2[] Coordinates = new Vector2[6];
    bool RanOutOfAttempts = false;
    int RooAMashCount = 0;
    bool[] IsDepressed = new bool[12];

    //cols/rows swapped here, but nowhere else :P | updated as of 2024/08/28
    //"Good lord, what is happening in there?" -Chalmers
    readonly int[,] BigTable = new int[26, 10] {
        {38, 18, 19, 17, 30, 8,  17, 26, 32, 26},
        {43, 3,  10, 19, 17, 47, 20, 26, 29, 37},
        {28, 3,  5,  26, 7,  28, 40, 14, 36, 16},
        {32, 26, 37, 0,  1,  39, 34, 5,  42, 43},
        {28, 36, 40, 19, 32, 35, 0,  12, 48, 43},
        {9,  27, 5,  30, 14, 9,  29, 36, 48, 24},
        {47, 22, 31, 2,  20, 12, 18, 23, 38, 10},
        {6,  38, 48, 9,  27, 29, 9,  13, 46, 35},
        {0,  22, 14, 48, 37, 10, 38, 3,  48, 23},
        {39, 14, 46, 24, 47, 45, 30, 2,  29, 13},
        {45, 40, 17, 35, 46, 42, 35, 28, 31, 13},
        {33, 40, 11, 25, 21, 45, 45, 22, 16, 10},
        {20, 14, 30, 1,  41, 12, 5,  47, 4,  39},
        {33, 44, 32, 23, 11, 3,  0,  0,  27, 7 },
        {6,  20, 6,  34, 19, 30, 25, 31, 43, 35},
        {1,  21, 2,  10, 19, 8,  11, 23, 34, 8 },
        {44, 41, 9,  42, 15, 4,  42, 3,  16, 13},
        {11, 36, 14, 36, 1,  40, 1,  43, 37, 22},
        {7,  24, 25, 2,  38, 39, 22, 7,  24, 24},
        {8,  33, 47, 2,  33, 25, 21, 41, 12, 17},
        {15, 15, 6,  42, 23, 31, 4,  12, 46, 18},
        {41, 2,  46, 32, 5,  34, 41, 21, 18, 15},
        {16, 25, 0,  13, 1,  37, 31, 27, 29, 33},
        {11, 34, 20, 15, 9,  18, 10, 4,  3,  5 },
        {7,  8,  13, 44, 21, 12, 45, 39, 6,  7 },
        {28, 4,  27, 6,  4,  44, 11, 8,  16, 44}
    };
    //ty chatgpt for doing all the tedious work of changing BigTable to fit different formats o7
    //fuck chatgpt for giving me a faulty table wtffffffffffffffffffffffff

    public PolygonalMapping(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Polygonal Mapping. Submodule is preparing itself.", ModuleID);

        //Setup polygon
        for(int i = 0; i < 6; i++) RegenAlphabetLabel(i); //also puts them in Coordinates array
        int Attempts = 1;
        int AttemptCap = 300;

        while( ! (ReturnPolygonVertexes(Coordinates).Length == 6 && CoordinatesFindDupe() == -1) && Attempts < AttemptCap){
            if(CoordinatesFindDupe() != -1){
                RegenAlphabetLabel(CoordinatesFindDupe());
            } else {
                if(Attempts %20 == 0){ //make sure the first 3 arent shite to work with
                    RegenAlphabetLabel(0);
                    RegenAlphabetLabel(1);
                    RegenAlphabetLabel(2);
                }
                RegenAlphabetLabel(3);
                RegenAlphabetLabel(4);
                RegenAlphabetLabel(5);
            }
            Attempts++;
        }

        //polygon couldnt generate
        if(Attempts == AttemptCap){
            Debug.LogFormat("[The Cruel Modkit #{0}] Failed making a polygon after {1} attempts, mash ❖ to solve.", ModuleID, AttemptCap);            
            Debug.LogFormat("[The Cruel Modkit #{0}] Man I love Unicorns. (Also please let Possessed know this happened)", ModuleID);            
            Module.WidgetText[1].text = "ERROR";
            RanOutOfAttempts = true;
            return;
        }

        //polygon generated well
        Debug.LogFormat("[The Cruel Modkit #{0}] Success on attempt {1}.", ModuleID, Attempts);
        Debug.LogFormat("[The Cruel Modkit #{0}] Alphabet buttons are: [{1}]", ModuleID, Info.GetAlphabetInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Symbols are: [{1}]", ModuleID, Info.GetSymbolInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Info of Symbols: 0:{1}, 1:{2}.", ModuleID, Info.Symbols[0], Info.Symbols[1]);                    


        //setup ignore string
        string WordDisplayFixed = "";

        //c++ brainrot
        for(int i = 0; i < Info.WordDisplay.Length; i++){
            if(Base36.IndexOf(Info.WordDisplay[i]) != -1){
                WordDisplayFixed += Info.WordDisplay[i];
            }
        }
        IgnoreString = Info.Morse + Info.TimerDisplay + Info.ResistorText[0] + Info.ResistorText[1] +
                            Info.ResistorText[2] + Info.ResistorText[3] + Info.NumberDisplay + WordDisplayFixed + "543210";
        Debug.LogFormat("[The Cruel Modkit #{0}] The string obtained from the widgets is \"{1}\".", ModuleID, IgnoreString);

        Debug.LogFormat("[The Cruel Modkit #{0}] Calculating which buttons should be pressed, 0-indexed.", ModuleID);

        Vector2 TestCoordinate = new Vector2();


        //no more brainf :(
        int[] SymbolCounter = new int[6];

        for(int z = 0; z < 4; z++){

            //clear symbol counter
            for(int i = 0; i < 6; i++){
                SymbolCounter[i] = 0;
            }

            bool isCurrentlyTied = false;

            //counting loop
            for(int x = 0; x < 26; x++){
                for(int y = 0; y < 10; y++){
                    
                    TestCoordinate = new Vector2(x,y);

                    //never-nesting type programming
                    if( ! Info.Symbols.Contains( BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y]) ){
                        continue;
                    }

                    if( ! IsInPolygon(TestCoordinate)){
                        continue;
                    }

                    int i = Array.IndexOf(Info.Symbols, BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y] ); 

                    SymbolCounter[i]++;
                    Debug.LogFormat("[The Cruel Modkit #{0}] Found symbol {1} ({4}) at {2}{3}).", ModuleID, i, Base36[(int)TestCoordinate.x+10], (int)TestCoordinate.y, Info.Symbols[i]);                    
                    
                }
            }



            //start of symbol loop
            while(!isCurrentlyTied){

                int HighestCount = -1;
                int HighestIndex = -1;

                //highest symbol check loop
                for(int i = 0; i < 6; i++){

                    if(FinalOrder.Contains("S"+i)){
                        continue;
                    }

                    if(SymbolCounter[i] == HighestCount){
                        isCurrentlyTied = true;
                    }

                    if(SymbolCounter[i] > HighestCount){
                        HighestCount = SymbolCounter[i];
                        HighestIndex = i;
                        isCurrentlyTied = false;
                    }
                }

                if(isCurrentlyTied){
                    Debug.LogFormat("[The Cruel Modkit #{0}] There is now a tie in symbol counts, skip to pressing an alphabet button.", ModuleID);
                    break;
                }

                Debug.LogFormat("[The Cruel Modkit #{0}] Current highest is index {1} at {2} entries", ModuleID, HighestIndex, HighestCount);

                //symbol press

                if(!FinalOrder.Contains("S" + HighestIndex.ToString())){
                    FinalOrder.Add("S" + HighestIndex.ToString());
                    Debug.LogFormat("[The Cruel Modkit #{0}] You should press Symbol {0}", ModuleID, HighestIndex);
                }


                SymbolCounter[HighestIndex] = -1;

                //end of symbol loop
            }

            //alphabet press
            
            int j = (Base36.IndexOf(IgnoreString[0]) % 6);
            IgnoreString = IgnoreString.Substring(1, IgnoreString.Length-1);
            while(FinalOrder.Contains("A" + j.ToString())){            
                //always set j & roll it off
                j = (Base36.IndexOf(IgnoreString[0]) % 6);
                IgnoreString = IgnoreString.Substring(1, IgnoreString.Length-1);
            }

            FinalOrder.Add("A" + j.ToString());
            Debug.LogFormat("[The Cruel Modkit #{0}] You should press Alphabet {1}", ModuleID, j);
            Debug.LogFormat("[The Cruel Modkit #{0}] Ignore string is currently \"{1}\"", ModuleID, IgnoreString, j.ToString());

            //alter coords array to ignore (another) alphabet button
            Coordinates[j] = new Vector2(777,777);

            //loop
        }

        //2 alpha buns left
        Debug.LogFormat("[The Cruel Modkit #{0}] Two alphabet buttons remain, moving on to next section.", ModuleID);

        Vector2 FinalCoord1 = new Vector2(-1,-1);
        Vector2 FinalCoord2 = new Vector2(-1,-1);
        //quickly pop the last 2 coords into those 2 vars above
        for(int i = 0; i < 6; i++){
            if( ! (FinalOrder.Contains("A" + i.ToString())) ){

                if(FinalCoord1.x == -1){
                    FinalCoord1 = Coordinates[i];
                } else {
                    FinalCoord2 = Coordinates[i];
                }

            }
        }

        //check if symbols lie on a coordinate
        for(int i = 0; i < 6; i++){
            //check if symbol is unpressed
            if( ! (FinalOrder.Contains("S" + i.ToString())) ){

                for(int j = 0; j < 6; j++){
                    //check if alpha is unpressed
                    if( ! (FinalOrder.Contains("A" + j.ToString())) ){
                        //ik i dont use FinalCoord1/2 here, but i coded this part first and im too lazy to optimize :)
                        int ValueFromCoord = BigTable[(int)Coordinates[j].x, (int)Coordinates[j].y];

                        if(ValueFromCoord == Info.Symbols[i]){
                            FinalOrder.Add("S" + i.ToString());
                            FinalOrder.Add("A" + j.ToString());
                            Debug.LogFormat("[The Cruel Modkit #{0}] You should press Symbol {1}, then Alphabet {2}.", ModuleID, i, j);                            
                        }
                    }
                }
            }
        } //i love nesting :)

        //check if symbols lie on the line

        //clear symbol counter
        for(int i = 0; i < 6; i++){
            SymbolCounter[i] = 0;
        }

        for(int x = 0; x < 26; x++){
            for(int y = 0; y < 10; y++){
                //I LOVE REUSING VARIABLES!!!
                TestCoordinate = new Vector2(x,y);

                if(! CoordinateLiesOnLine(FinalCoord1, FinalCoord2, TestCoordinate)){
                    continue;
                }

                if( ! Info.Symbols.Contains( BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y]) ){
                    continue;
                }

                int i = Array.IndexOf(Info.Symbols, BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y] );

                SymbolCounter[i] = 1; 


            }
        }

        //R->L
        for(int i = 5; i > -1; i--){
            //check if symbol is unpressed
            if( ! (FinalOrder.Contains("S" + i.ToString())) ){
                if(SymbolCounter[i] == 1){
                    FinalOrder.Add("S" + i.ToString());
                    Debug.LogFormat("[The Cruel Modkit #{0}] You should press Symbol {1}.", ModuleID, i);                            
                }
            }
        }

        //Press everything else L->R
        for(int i = 0; i < 6; i++){
            if( ! (FinalOrder.Contains("S" + i.ToString())) ){
                FinalOrder.Add("S" + i.ToString());
                Debug.LogFormat("[The Cruel Modkit #{0}] You should press Symbol {1}.", ModuleID, i);               
            }
        }
        for(int i = 0; i < 6; i++){
            if( ! (FinalOrder.Contains("A" + i.ToString())) ){
                FinalOrder.Add("A" + i.ToString());
                Debug.LogFormat("[The Cruel Modkit #{0}] You should press Alphabet {1}.", ModuleID, i);               
            }
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] And thus concludes the calculations, get to pressing!", ModuleID);

    }

    public override void OnUtilityPress(){
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.UtilityButton.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();

        if(RanOutOfAttempts){
            RooAMashCount++;
            if(RooAMashCount > 47){
                Module.Solve();
            }
        }
        
        return;
    }

    public override void OnSymbolPress(int y)
    {

        if (Module.IsAnimating())
            return;

        if (IsDepressed[y] == true)
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Symbols[y].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A symbol was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }


        if(FinalOrder[0] == "S" + y.ToString()){
            //good press
            Debug.LogFormat("[The Cruel Modkit #{0}] You pressed Symbol {1}, good.", ModuleID, y);
            Module.StartCoroutine(AnimateButtonPress(Module.Symbols[y].transform , Vector3.down * 0.003f, 1));
            Module.Symbols[y].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            IsDepressed[y] = true;
            FinalOrder.RemoveAt(0);
        } else {
            //bad press
            Module.StartCoroutine(Module.ButtonStrike(true, y));
            Debug.LogFormat("[The Cruel Modkit #{0}] You pressed Symbol {1}, wrong.", ModuleID, y);
            Module.CauseStrike();
        }

        if(FinalOrder.Count == 0){
            Module.Solve();
            Debug.LogFormat("[The Cruel Modkit #{0}] Module solved, good job.", ModuleID);
        }

    }

    public override void OnAlphabetPress(int y)
    {

        if (Module.IsAnimating())
            return;

        if (IsDepressed[y+6] == true)
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Alphabet[y].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! An alphabet button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        if(FinalOrder[0] == "A" + y.ToString()){
            //good press
            Debug.LogFormat("[The Cruel Modkit #{0}] You pressed Alphabet {1}, good.", ModuleID, y);
            Module.StartCoroutine(AnimateButtonPress(Module.Alphabet[y].transform , Vector3.down * 0.003f, 1));
            Module.Alphabet[y].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            IsDepressed[y+6] = true;
            FinalOrder.RemoveAt(0);
        } else {
            //bad press
            Module.StartCoroutine(Module.ButtonStrike(false, y));
            Debug.LogFormat("[The Cruel Modkit #{0}] You pressed Alphabet {1}, wrong.", ModuleID, y);
            Module.CauseStrike();
        }

        if(FinalOrder.Count == 0){
            Module.Solve();
            Debug.LogFormat("[The Cruel Modkit #{0}] Module solved, good job.", ModuleID);
        }

    }

    string ReturnPolygonVertexes(Vector2[] Vertexes){
        string VertexList = "";
        //find first label in reverse chinese order bc of how atan2 works :P (???)
        int FirstIndex = 0;
        int FirstPosition = 261; //beyond last
        for(int i = 0; i < Vertexes.Length; i++){
            if(Vertexes[i].x*10 + (10- Vertexes[i].y) < FirstPosition){
                FirstPosition = (int)(Vertexes[i].x*10 + (10- Vertexes[i].y));
                FirstIndex = i;
            }
        }

        int NextIndex = 999;
        int CurrentIndex = FirstIndex;
        double MaxAtan2 = -4; //impossibly low
        double Atan2StepCap = 4; //impossibly high

        while(NextIndex != FirstIndex && VertexList.Length != 8){ //failsafe in debugging
            for(int i = 0; i < Vertexes.Length; i++){
                if(i == CurrentIndex)
                    continue;

                //instead of making the array a list, im gonna use dummy values
                //because im too lazy to be bothered
                if((int)Vertexes[i].x == 777)
                    continue;

                double CurrentAtan2 = ReturnAtan2(Vertexes[CurrentIndex], Vertexes[i]);
                
                if(CurrentAtan2 == MaxAtan2)
                    return "6"; //what a fucking bodge

                if(CurrentAtan2 > MaxAtan2 && CurrentAtan2 < Atan2StepCap){
                    NextIndex = i;
                    MaxAtan2 = CurrentAtan2;
                }
            }

            VertexList+= NextIndex;
            Atan2StepCap = MaxAtan2;
            CurrentIndex = NextIndex;
            MaxAtan2 = -4;
        }
        return VertexList;
    }

    bool IsInPolygon(Vector2 SymbCoord){

        Vector2[] CoordinatesAndTest = new Vector2[7];

        Array.Copy(Coordinates, CoordinatesAndTest, 6);

        CoordinatesAndTest[6] = new Vector2(SymbCoord.x, SymbCoord.y);

        if(ReturnPolygonVertexes(CoordinatesAndTest).IndexOf("6") == -1)
            return true;
        else
            return false;

    }

    int CoordinatesFindDupe(){
        HashSet<Vector2> DistinctCoordinates = new HashSet<Vector2>();
        for(int i = 0; i < 6; i++)
            if(!DistinctCoordinates.Add(Coordinates[i]))
                return i;
        return -1;
    }

    double ReturnAtan2(Vector2 From, Vector2 To){
        return Math.Atan2(To.x - From.x, To.y - From.y);
    }

    void RegenAlphabetLabel(int i){
        string AlphabetKey = String.Empty;
        string[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        string[] Numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        int LetterAmount = Random.Range(0, 2);
        int NumberAmount = Random.Range(0, 2);
        for (int x = 0; x <= LetterAmount; x++) AlphabetKey += Letters[x];
        if (LetterAmount == 1 && NumberAmount == 1) AlphabetKey += Environment.NewLine;
        for (int x = 0; x <= NumberAmount; x++) AlphabetKey += Numbers[x];
        Info.Alphabet[i] = AlphabetKey;
        Module.Alphabet[i].transform.Find("AlphabetText").GetComponentInChildren<TextMesh>().text = AlphabetKey;
        Coordinates[i] = new Vector2(Base36.IndexOf(AlphabetKey[0]) - 10, Base36.IndexOf(AlphabetKey[AlphabetKey.Length-1]));
    }
    
    bool CoordinateLiesOnLine (Vector2 AlphaCoord1, Vector2 AlphaCoord2, Vector2 TestCoord){

        if(TestCoord == AlphaCoord1 || TestCoord == AlphaCoord2) return false;

        //nah i'd cache
        if(ReturnAtan2(AlphaCoord1, AlphaCoord2) == ReturnAtan2(AlphaCoord1, TestCoord) && ReturnAtan2(AlphaCoord1, AlphaCoord2) == ReturnAtan2(TestCoord, AlphaCoord2))
            return true;

        return false;

    }

}
