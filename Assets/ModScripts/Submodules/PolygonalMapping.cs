using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wawa.Modules;
using Random = UnityEngine.Random;

public class PolygonalMapping : Puzzle
{
    private readonly string Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly string IgnoreString;
    private readonly List<string> FinalOrder = new List<string>();
    private readonly Vector2[] Coordinates = new Vector2[6];
    private readonly bool RanOutOfAttempts = false;
    private int RooAMashCount = 0;
    private readonly bool[] IsDepressed = new bool[12];

    private readonly int[,] BigTable = new int[26, 10] {
        {37, 18, 19, 17, 30, 8,  17, 26, 32, 26},
        {42, 3,  10, 19, 17, 47, 20, 26, 29, 37},
        {27, 3,  5,  26, 7,  28, 40, 14, 36, 16},
        {31, 26, 37, 0,  1,  39, 34, 5,  42, 43},
        {27, 36, 40, 19, 32, 35, 0,  12, 48, 43},
        {8,  27, 5,  30, 14, 9,  29, 36, 48, 24},
        {46, 22, 31, 2,  20, 12, 18, 23, 38, 10},
        {5,  38, 48, 9,  27, 29, 9,  13, 46, 35},
        {0,  22, 14, 48, 37, 10, 38, 3,  48, 23},
        {38, 14, 46, 24, 47, 45, 30, 2,  29, 13},
        {44, 40, 17, 35, 46, 42, 35, 28, 31, 13},
        {32, 40, 11, 25, 21, 45, 45, 22, 16, 10},
        {19, 14, 30, 1,  41, 12, 5,  47, 4,  39},
        {32, 44, 32, 23, 11, 3,  0,  0,  27, 7 },
        {5,  20, 6,  34, 19, 30, 25, 31, 43, 35},
        {0,  21, 2,  10, 19, 8,  11, 23, 34, 8 },
        {43, 41, 9,  42, 15, 4,  42, 3,  16, 13},
        {10, 36, 14, 36, 1,  40, 1,  43, 37, 22},
        {6,  24, 25, 2,  38, 39, 22, 7,  24, 24},
        {7,  33, 47, 2,  33, 25, 21, 41, 12, 17},
        {14, 15, 6,  42, 23, 31, 4,  12, 46, 18},
        {40, 2,  46, 32, 5,  34, 41, 21, 18, 15},
        {15, 25, 0,  13, 1,  37, 31, 27, 29, 33},
        {10, 34, 20, 15, 9,  18, 10, 4,  3,  5 },
        {6,  8,  13, 44, 21, 12, 45, 39, 6,  7 },
        {27, 4,  27, 6,  4,  44, 11, 8,  16, 44}
    };

    public PolygonalMapping(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Polygonal Mapping. Submodule is preparing itself.");

        //Setup polygon; put inital alphabet buttons into coordinates array
        for (int i = 0; i < 6; i++)
        {
            string AlphabetKey = Info.Alphabet[i];
            Coordinates[i] = new Vector2(Base36.IndexOf(AlphabetKey[0]) - 10, Base36.IndexOf(AlphabetKey[AlphabetKey.Length - 1]));
        }

        int attempts = 1;
        int attemptCap = 300;
        bool isDupeReal = CoordinatesContainDupe();

        //generate polygons until a good one exists or ~300 attempts
        while ((isDupeReal || ReturnPolygonVertexes(Coordinates).Length != 6) && attempts < attemptCap)
        {
            if (isDupeReal)
                CoordinatesReplaceDupe();
            else
                for (int i = 0; i < 6; i++) RegenAlphabetLabel(i);

            isDupeReal = CoordinatesContainDupe();
            attempts++;
        }

        Module.SetAlphabet();

        //polygon couldnt generate
        if (attempts == attemptCap)
        {
            Module.Log("Failed making a polygon after {0} attempts, mash ❖ to solve.", attemptCap);
            Module.Log("Man I love Unicorns. (Also please let Possessed know this happened)");
            Module.WidgetText[1].text = "ERROR";
            RanOutOfAttempts = true;
            return;
        }

        //polygon generated well
        Module.Log("Alphabet buttons are: [{0}]", Info.GetAlphabetInfo());
        Module.Log("Symbols are: [{0}]", Info.GetSymbolInfo());

        string wordDisplayFixed = Info.WordDisplay.Where(n => char.IsLetterOrDigit(n)).Join(string.Empty);
        IgnoreString = Info.Morse + Info.TimerDisplay + Info.ResistorText[0] + Info.ResistorText[1] + Info.ResistorText[2] + Info.ResistorText[3] + Info.NumberDisplay + wordDisplayFixed + "543210";
        Module.Log("The string obtained from the widgets is \"{0}\".", IgnoreString);

        Module.Log("Calculating which buttons should be pressed, 0-indexed.");

        Vector2 testCoordinate = new Vector2();

        int[] symbolCounter = new int[6];

        for (int z = 0; z < 4; z++)
        {

            for (int i = 0; i < 6; i++)
            {
                symbolCounter[i] = 0;
            }

            bool isCurrentlyTied = false;

            //counting symbols
            for (int x = 0; x < 26; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    testCoordinate = new Vector2(x, y);
                    if (!Info.Symbols.Contains(BigTable[(int)testCoordinate.x, (int)testCoordinate.y])) continue;
                    if (!IsInPolygon(testCoordinate)) continue;
                    int i = Array.IndexOf(Info.Symbols, BigTable[(int)testCoordinate.x, (int)testCoordinate.y]);
                    if (FinalOrder.Contains("S" + i)) continue;
                    symbolCounter[i]++;
                    Module.Log("Found symbol {0} ({3}) at {1}{2}).", i, Base36[(int)testCoordinate.x + 10], (int)testCoordinate.y, Info.Symbols[i]);
                }
            }

            while (!isCurrentlyTied)
            {

                int highestCount = -1;
                int highestIndex = -1;

                for (int i = 0; i < 6; i++)
                {
                    if (FinalOrder.Contains("S" + i)) continue;
                    if (symbolCounter[i] == highestCount) isCurrentlyTied = true;
                    if (symbolCounter[i] > highestCount)
                    {
                        highestCount = symbolCounter[i];
                        highestIndex = i;
                        isCurrentlyTied = false;
                    }
                }

                if (isCurrentlyTied)
                {
                    Module.Log("There is now a tie in symbol counts, skip to pressing an alphabet button.");
                    break;
                }

                Module.Log("Current highest is index {0} at {1} entries.", highestIndex, highestCount);

                //symbol press
                if (!FinalOrder.Contains("S" + highestIndex.ToString()))
                {
                    FinalOrder.Add("S" + highestIndex.ToString());
                    Module.Log("You should press Symbol {0}.", highestIndex);
                }

                symbolCounter[highestIndex] = -1;
            }

            //alphabet press
            int j = (Base36.IndexOf(IgnoreString[0]) % 6);
            IgnoreString = IgnoreString.Substring(1, IgnoreString.Length - 1);
            while (FinalOrder.Contains("A" + j.ToString()))
            {
                //always set j & roll it off
                j = (Base36.IndexOf(IgnoreString[0]) % 6);
                IgnoreString = IgnoreString.Substring(1, IgnoreString.Length - 1);
            }

            FinalOrder.Add("A" + j.ToString());
            Module.Log("You should press Alphabet {0}.", j);
            Module.Log("Ignore string is currently \"{0}\".", IgnoreString, j.ToString());

            //alter coords array to ignore (another) alphabet button
            Coordinates[j] = new Vector2(777, 777);

        }

        //2 alpha buns left
        Module.Log("Two alphabet buttons remain, moving on to next section.");

        Vector2 finalCoord1 = new Vector2(-1, -1);
        Vector2 finalCoord2 = new Vector2(-1, -1);
        for (int i = 0; i < 6; i++)
        {
            if (!(FinalOrder.Contains("A" + i.ToString())))
            {
                if (finalCoord1.x == -1)
                {
                    finalCoord1 = Coordinates[i];
                }
                else
                {
                    finalCoord2 = Coordinates[i];
                }
            }
        }

        //check if symbols lie on a coordinate
        for (int i = 0; i < 6; i++)
        {
            if (!(FinalOrder.Contains("S" + i.ToString())))
            {

                for (int j = 0; j < 6; j++)
                {
                    if (!(FinalOrder.Contains("A" + j.ToString())))
                    {
                        int ValueFromCoord = BigTable[(int)Coordinates[j].x, (int)Coordinates[j].y];

                        if (ValueFromCoord == Info.Symbols[i])
                        {
                            FinalOrder.Add("S" + i.ToString());
                            FinalOrder.Add("A" + j.ToString());
                            Module.Log("You should press Symbol {0}, then Alphabet {1}.", i, j);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < 6; i++)
        {
            symbolCounter[i] = 0;
        }

        //check if symbols lie on the line
        for (int x = 0; x < 26; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                testCoordinate = new Vector2(x, y);

                if (!CoordinateLiesOnLine(finalCoord1, finalCoord2, testCoordinate)) continue;
                if (!Info.Symbols.Contains(BigTable[(int)testCoordinate.x, (int)testCoordinate.y])) continue;

                int i = Array.IndexOf(Info.Symbols, BigTable[(int)testCoordinate.x, (int)testCoordinate.y]);

                symbolCounter[i] = 1;
            }
        }

        //R->L
        for (int i = 5; i > -1; i--)
        {
            //check if symbol is unpressed
            if (!(FinalOrder.Contains("S" + i.ToString())))
            {
                if (symbolCounter[i] == 1)
                {
                    FinalOrder.Add("S" + i.ToString());
                    Module.Log("You should press Symbol {0}.", i);
                }
            }
        }

        //Press everything else L->R
        for (int i = 0; i < 6; i++)
        {
            if (!(FinalOrder.Contains("S" + i.ToString())))
            {
                FinalOrder.Add("S" + i.ToString());
                Module.Log("You should press Symbol {0}.", i);
            }
        }
        for (int i = 0; i < 6; i++)
        {
            if (!(FinalOrder.Contains("A" + i.ToString())))
            {
                FinalOrder.Add("A" + i.ToString());
                Module.Log("You should press Alphabet {0}.", i);
            }
        }

        Module.Log("And thus concludes the calculations, get to pressing!");

    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.UtilityButton.GetComponentInChildren<KMSelectable>(), 0.5f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Module.Strike("Strike! The ❖ button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
            return;
        }

        Module.StartSolve();

        if (RanOutOfAttempts)
        {
            RooAMashCount++;
            if (RooAMashCount > 47)
            {
                Module.SolveModule("Module solved.");
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

        Module.Shake(Module.Symbols[y].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Module.Strike("Strike! A symbol was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
            return;
        }

        if (FinalOrder[0] == "S" + y.ToString())
        {
            //good press
            Module.Log("You pressed Symbol {0}, good.", y);
            Module.StartCoroutine(AnimateButtonPress(Module.Symbols[y].transform, Vector3.down * 0.003f, 1));
            Module.Symbols[y].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            IsDepressed[y] = true;
            FinalOrder.RemoveAt(0);
        }
        else
        {
            //bad press
            Module.StartCoroutine(Module.ButtonStrike(true, y));
            Module.Strike("You pressed Symbol {0}, wrong.", y);
        }

        if (FinalOrder.Count == 0)
        {
            Module.SolveModule("Module solved, good job.");
        }

    }

    public override void OnAlphabetPress(int y)
    {

        if (Module.IsAnimating())
            return;

        if (IsDepressed[y + 6] == true)
            return;

        Module.Shake(Module.Alphabet[y].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Module.Strike("Strike! An alphabet button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
            return;
        }

        if (FinalOrder[0] == "A" + y.ToString())
        {
            //good press
            Module.Log("You pressed Alphabet {0}, good.", y);
            Module.StartCoroutine(AnimateButtonPress(Module.Alphabet[y].transform, Vector3.down * 0.003f, 1));
            Module.Alphabet[y].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            IsDepressed[y + 6] = true;
            FinalOrder.RemoveAt(0);
        }
        else
        {
            //bad press
            Module.StartCoroutine(Module.ButtonStrike(false, y));
            Module.Strike("You pressed Alphabet {0}, wrong.", y);
        }

        if (FinalOrder.Count == 0)
        {
            Module.SolveModule("Module solved, good job.");
        }

    }

    //this function makes the largest strictly convex polygon with the given vertex coords, then returns the vertexes used to make that polygon
    //strictly convex means that the polygon is easier for the expert to construct and doesnt cause weird jank when vertexes are removed
    private string ReturnPolygonVertexes(Vector2[] vertexes)
    {
        string vertexList = "";

        //find first vertex in reverse chinese order bc of how atan2 works
        int firstIndex = 0;
        int firstPosition = 261;
        for (int i = 0; i < vertexes.Length; i++)
        {
            if (vertexes[i].x * 10 + (10 - vertexes[i].y) < firstPosition)
            {
                firstPosition = (int)(vertexes[i].x * 10 + (10 - vertexes[i].y));
                firstIndex = i;
            }
        }

        int currentIndex = firstIndex;
        int nextIndex = 999;
        double largestAtan2 = -4;
        double atan2StepCap = 4;

        //itteratively goes around the vertexes to form polygon, starting at firstIndex vertex going south,counter
        while (nextIndex != firstIndex && vertexList.Length != 8)
        {
            for (int i = 0; i < vertexes.Length; i++)
            {

                //dont compare a coord to itself; ignore coords with a dummy value
                if (i == currentIndex) continue;
                if ((int)vertexes[i].x == 777) continue;

                double currentAtan2 = ReturnAtan2(vertexes[currentIndex], vertexes[i]);

                if (currentAtan2 == largestAtan2) return "6";
                //this only occurs when there is a straight line of 3 or more symbols
                //returning "6" is just the easiest way to do what i want whenever this function is called

                //currentAtan2 < atan2StepCap ensures that the method goes counter only
                if (currentAtan2 > largestAtan2 && currentAtan2 < atan2StepCap)
                {
                    nextIndex = i;
                    largestAtan2 = currentAtan2;
                }
            }

            //setup for next itteration
            vertexList += nextIndex;
            atan2StepCap = largestAtan2;
            currentIndex = nextIndex;
            largestAtan2 = -4;
        }
        //feel free to ping Possessed on stuff that still doesn't make sense
        return vertexList;
    }

    private bool IsInPolygon(Vector2 SymbCoord)
    {
        Vector2[] coordinatesAndTest = new Vector2[7];
        Array.Copy(Coordinates, coordinatesAndTest, 6);
        coordinatesAndTest[6] = new Vector2(SymbCoord.x, SymbCoord.y);
        return ReturnPolygonVertexes(coordinatesAndTest).IndexOf("6") == -1;
    }

    private bool CoordinatesContainDupe()
    {
        return Coordinates.Distinct().ToArray().Length != Coordinates.Length;
    }

    private void CoordinatesReplaceDupe()
    {
        if (!CoordinatesContainDupe()) return;

        HashSet<Vector2> distinctCoordinates = new HashSet<Vector2>();
        for (int i = 0; i < 6; i++)
        {
            if (!distinctCoordinates.Add(Coordinates[i]))
            {
                RegenAlphabetLabel(i);
                i--;
            }
        }
    }

    private double ReturnAtan2(Vector2 From, Vector2 To)
    {
        return Math.Atan2(To.x - From.x, To.y - From.y);
    }

    private void RegenAlphabetLabel(int i)
    {
        string alphabetKey = String.Empty;
        string[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        string[] numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => Random.Range(0, 1000)).ToArray();
        int letterAmount = Random.Range(0, 2);
        int numberAmount = Random.Range(0, 2);
        for (int x = 0; x <= letterAmount; x++)
            alphabetKey += letters[x];
        for (int x = 0; x <= numberAmount; x++)
            alphabetKey += numbers[x];
        Info.Alphabet[i] = alphabetKey;
        Coordinates[i] = new Vector2(Base36.IndexOf(alphabetKey[0]) - 10, Base36.IndexOf(alphabetKey[alphabetKey.Length - 1]));
    }

    private bool CoordinateLiesOnLine(Vector2 alphaCoord1, Vector2 alphaCoord2, Vector2 testCoord)
    {
        if (testCoord == alphaCoord1 || testCoord == alphaCoord2) return false;
        return ReturnAtan2(alphaCoord1, alphaCoord2) == ReturnAtan2(alphaCoord1, testCoord) && ReturnAtan2(alphaCoord1, alphaCoord2) == ReturnAtan2(testCoord, alphaCoord2);
    }
}