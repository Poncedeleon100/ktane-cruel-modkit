using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PolygonalMapping : Puzzle
{
    readonly string Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string IgnoreString;
    readonly List<string> FinalOrder = new List<string>();
    readonly Vector2[] Coordinates = new Vector2[6];
    readonly bool RanOutOfAttempts = false;
    int RooAMashCount = 0;
    readonly bool[] IsDepressed = new bool[12];

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

    public PolygonalMapping(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components){
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Polygonal Mapping. Submodule is preparing itself.", ModuleID);

        //Setup polygon; put inital alphabet buttons into coordinates array
        for(int i = 0; i < 6; i++){
            string AlphabetKey = Info.Alphabet[i];
            Coordinates[i] = new Vector2(Base36.IndexOf(AlphabetKey[0]) - 10, Base36.IndexOf(AlphabetKey[AlphabetKey.Length-1]));
        }

        int Attempts = 1;
        int AttemptCap = 300;
        bool IsDupeReal = CoordinatesContainDupe();

        //generate polygons until a good one exists or ~300 attempts
        while( (IsDupeReal || ReturnPolygonVertexes(Coordinates).Length != 6) && Attempts < AttemptCap){
            if(IsDupeReal)
                CoordinatesReplaceDupe();
            else
                for(int i = 0; i < 6; i++) RegenAlphabetLabel(i);

            IsDupeReal = CoordinatesContainDupe();
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
        Debug.LogFormat("[The Cruel Modkit #{0}] Alphabet buttons are: [{1}]", ModuleID, Info.GetAlphabetInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Symbols are: [{1}]", ModuleID, Info.GetSymbolInfo());

        string WordDisplayFixed = Info.WordDisplay.Where(n => char.IsLetterOrDigit(n)).Join(string.Empty);
        IgnoreString = Info.Morse + Info.TimerDisplay + Info.ResistorText[0] + Info.ResistorText[1] + Info.ResistorText[2] + Info.ResistorText[3] + Info.NumberDisplay + WordDisplayFixed + "543210";
        Debug.LogFormat("[The Cruel Modkit #{0}] The string obtained from the widgets is \"{1}\".", ModuleID, IgnoreString);

        Debug.LogFormat("[The Cruel Modkit #{0}] Calculating which buttons should be pressed, 0-indexed.", ModuleID);

        Vector2 TestCoordinate = new Vector2();

        int[] SymbolCounter = new int[6];

        for(int z = 0; z < 4; z++){

            for(int i = 0; i < 6; i++){
                SymbolCounter[i] = 0;
            }

            bool isCurrentlyTied = false;

            //counting symbols
            for(int x = 0; x < 26; x++){
                for(int y = 0; y < 10; y++){
                    TestCoordinate = new Vector2(x,y);
                    if( ! Info.Symbols.Contains( BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y])) continue;
                    if( ! IsInPolygon(TestCoordinate)) continue;
                    int i = Array.IndexOf(Info.Symbols, BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y] );
                    if(FinalOrder.Contains("S"+i)) continue;
                    SymbolCounter[i]++;
                    Debug.LogFormat("[The Cruel Modkit #{0}] Found symbol {1} ({4}) at {2}{3}).", ModuleID, i, Base36[(int)TestCoordinate.x+10], (int)TestCoordinate.y, Info.Symbols[i]);                    
                }
            }

            while(!isCurrentlyTied){

                int HighestCount = -1;
                int HighestIndex = -1;

                for(int i = 0; i < 6; i++){
                    if(FinalOrder.Contains("S"+i)) continue;
                    if(SymbolCounter[i] == HighestCount) isCurrentlyTied = true;
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

                Debug.LogFormat("[The Cruel Modkit #{0}] Current highest is index {1} at {2} entries.", ModuleID, HighestIndex, HighestCount);

                //symbol press
                if(!FinalOrder.Contains("S" + HighestIndex.ToString())){
                    FinalOrder.Add("S" + HighestIndex.ToString());
                    Debug.LogFormat("[The Cruel Modkit #{0}] You should press Symbol {1}.", ModuleID, HighestIndex);
                }

                SymbolCounter[HighestIndex] = -1;
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
            Debug.LogFormat("[The Cruel Modkit #{0}] You should press Alphabet {1}.", ModuleID, j);
            Debug.LogFormat("[The Cruel Modkit #{0}] Ignore string is currently \"{1}\".", ModuleID, IgnoreString, j.ToString());

            //alter coords array to ignore (another) alphabet button
            Coordinates[j] = new Vector2(777,777);

        }

        //2 alpha buns left
        Debug.LogFormat("[The Cruel Modkit #{0}] Two alphabet buttons remain, moving on to next section.", ModuleID);

        Vector2 FinalCoord1 = new Vector2(-1,-1);
        Vector2 FinalCoord2 = new Vector2(-1,-1);
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
            if( ! (FinalOrder.Contains("S" + i.ToString())) ){

                for(int j = 0; j < 6; j++){
                    if( ! (FinalOrder.Contains("A" + j.ToString())) ){
                        int ValueFromCoord = BigTable[(int)Coordinates[j].x, (int)Coordinates[j].y];

                        if(ValueFromCoord == Info.Symbols[i]){
                            FinalOrder.Add("S" + i.ToString());
                            FinalOrder.Add("A" + j.ToString());
                            Debug.LogFormat("[The Cruel Modkit #{0}] You should press Symbol {1}, then Alphabet {2}.", ModuleID, i, j);                            
                        }
                    }
                }
            }
        }

        for(int i = 0; i < 6; i++){
            SymbolCounter[i] = 0;
        }

        //check if symbols lie on the line
        for(int x = 0; x < 26; x++){
            for(int y = 0; y < 10; y++){
                TestCoordinate = new Vector2(x,y);

                if(! CoordinateLiesOnLine(FinalCoord1, FinalCoord2, TestCoordinate)) continue;
                if( ! Info.Symbols.Contains( BigTable[(int)TestCoordinate.x, (int)TestCoordinate.y]) ) continue;

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

    //this function makes the largest strictly convex polygon with the given vertex coords, then returns the vertexes used to make that polygon
    //strictly convex means that the polygon is easier for the expert to construct and doesnt cause weird jank when vertexes are removed
    string ReturnPolygonVertexes(Vector2[] vertexes){
        string vertexList = "";

        //find first vertex in reverse chinese order bc of how atan2 works
        int firstIndex = 0;
        int firstPosition = 261;
        for(int i = 0; i < vertexes.Length; i++){
            if(vertexes[i].x*10 + (10- vertexes[i].y) < firstPosition){
                firstPosition = (int)(vertexes[i].x*10 + (10- vertexes[i].y));
                firstIndex = i;
            }
        }

        int currentIndex = firstIndex;
        int nextIndex = 999;
        double largestAtan2 = -4;
        double atan2StepCap = 4;

        //itteratively goes around the vertexes to form polygon, starting at firstIndex vertex going south,counter
        while(nextIndex != firstIndex && vertexList.Length != 8){
            for(int i = 0; i < vertexes.Length; i++){
                
                //dont compare a coord to itself; ignore coords with a dummy value
                if(i == currentIndex) continue; 
                if((int)vertexes[i].x == 777) continue;
                
                double currentAtan2 = ReturnAtan2(vertexes[currentIndex], vertexes[i]);
                
                if(currentAtan2 == largestAtan2) return "6";
                //this only occurs when there is a straight line of 3 or more symbols
                //returning "6" is just the easiest way to do what i want whenever this function is called

                //currentAtan2 < atan2StepCap ensures that the method goes counter only
                if(currentAtan2 > largestAtan2 && currentAtan2 < atan2StepCap){
                    nextIndex = i;
                    largestAtan2 = currentAtan2;
                }
            }

            //setup for next itteration
            vertexList+= nextIndex;
            atan2StepCap = largestAtan2;
            currentIndex = nextIndex;
            largestAtan2 = -4;
        }
        //feel free to ping Possessed on stuff that still doesn't make sense
        return vertexList;
    }

    bool IsInPolygon(Vector2 SymbCoord){
        Vector2[] coordinatesAndTest = new Vector2[7];
        Array.Copy(Coordinates, coordinatesAndTest, 6);
        coordinatesAndTest[6] = new Vector2(SymbCoord.x, SymbCoord.y);
        if(ReturnPolygonVertexes(coordinatesAndTest).IndexOf("6") == -1)
            return true;
        else
            return false;

    }

    bool CoordinatesContainDupe(){
        if(Coordinates.Distinct().ToArray().Length == Coordinates.Length) return false;
        return true;
    }

    void CoordinatesReplaceDupe(){
        if(!CoordinatesContainDupe()) return;

        HashSet<Vector2> distinctCoordinates = new HashSet<Vector2>();
        for(int i = 0; i < 6; i++){
            if(!distinctCoordinates.Add(Coordinates[i])){
                RegenAlphabetLabel(i);
                i--;
            }
        }
        return;
    }

    double ReturnAtan2(Vector2 From, Vector2 To){
        return Math.Atan2(To.x - From.x, To.y - From.y);
    }

    void RegenAlphabetLabel(int i){
        string alphabetKey = String.Empty;
        string[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        string[] numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        int letterAmount = Random.Range(0, 2);
        int numberAmount = Random.Range(0, 2);
        for (int x = 0; x <= letterAmount; x++) alphabetKey += letters[x];
        if (letterAmount == 1 && numberAmount == 1) alphabetKey += Environment.NewLine;
        for (int x = 0; x <= numberAmount; x++) alphabetKey += numbers[x];
        Info.Alphabet[i] = alphabetKey;
        Module.Alphabet[i].transform.Find("AlphabetText").GetComponentInChildren<TextMesh>().text = alphabetKey;
        Coordinates[i] = new Vector2(Base36.IndexOf(alphabetKey[0]) - 10, Base36.IndexOf(alphabetKey[alphabetKey.Length-1]));
    }
    
    bool CoordinateLiesOnLine (Vector2 alphaCoord1, Vector2 alphaCoord2, Vector2 testCoord){
        if(testCoord == alphaCoord1 || testCoord == alphaCoord2) return false;
        if(ReturnAtan2(alphaCoord1, alphaCoord2) == ReturnAtan2(alphaCoord1, testCoord) && ReturnAtan2(alphaCoord1, alphaCoord2) == ReturnAtan2(testCoord, alphaCoord2))
            return true;

        return false;
    }
}