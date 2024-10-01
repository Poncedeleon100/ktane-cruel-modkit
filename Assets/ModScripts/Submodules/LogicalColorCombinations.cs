using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LogicalColorCombinations : Puzzle
{
    readonly string[,] pairTable = new string[11, 8] {
        { "0", "H", "W", "M", "Z", "G", "Y", "9" },
        { "S", "4", "Z", "I", "U", "7", "2", "H" },
        { "C", "J", "V", "A", "6", "B", "M", "5" },
        { "P", "2", "F", "7", "W", "L", "N", "D" },
        { "E", "9", "G", "O", "A", "P", "1", "J" },
        { "5", "T", "L", "3", "O", "E", "I", "C" },
        { "K", "6", "D", "R", "F", "0", "X", "Q" },
        { "Q", "1", "8", "X", "R", "3", "T", "K" },
        { "U", "Y", "B", "N", "8", "K", "4", "S" },
        { "Z", "Z", "Z", "Z", "0", "0", "0", "0" },
        { "0", "0", "0", "0", "Z", "Z", "Z", "Z" },
    };
    readonly int[,] logicOperatorTable = new int[11, 14]
    {
        { 1, 4, 0, 2, 4, 5, 3, 3, 4, 0, 2, 0, 5, 1 },
        { 3, 5, 2, 1, 3, 0, 2, 0, 4, 3, 5, 1, 2, 3 },
        { 2, 0, 4, 3, 3, 4, 1, 2, 1, 5, 2, 3, 0, 5 },
        { 5, 3, 1, 0, 4, 2, 5, 3, 2, 4, 1, 5, 3, 0 },
        { 4, 0, 5, 0, 0, 1, 3, 0, 4, 0, 1, 2, 3, 4 },
        { 0, 5, 2, 3, 1, 4, 5, 5, 1, 2, 4, 2, 0, 5 },
        { 1, 0, 3, 5, 2, 2, 4, 4, 0, 5, 3, 1, 4, 2 },
        { 2, 5, 1, 5, 2, 3, 3, 5, 3, 1, 4, 0, 5, 2 },
        { 3, 1, 4, 2, 5, 0, 4, 1, 5, 5, 0, 2, 1, 4 },
        { 4, 2, 3, 0, 5, 2, 1, 2, 5, 1, 4, 3, 4, 0 },
        { 0, 5, 4, 1, 1, 1, 0, 2, 5, 1, 4, 3, 4, 0 }
    };
    readonly string[,] pairOrderTable = new string[11, 14]
    {
        { "0123", "0213", "0123", "0123", "1302", "1302", "1302", "0123", "1302", "0213", "1203", "1302", "1203", "0312" },
        { "0312", "0123", "1302", "0312", "0213", "0213", "0312", "1203", "0213", "1203", "2301", "1302", "0123", "2301" },
        { "1203", "0312", "2301", "0123", "1302", "0123", "0213", "2301", "0213", "0123", "0213", "0312", "0123", "0123" },
        { "1302", "0213", "0123", "1302", "0213", "0213", "1203", "0213", "0312", "1203", "1203", "0213", "1302", "2301" },
        { "0312", "0213", "0312", "0123", "0312", "1203", "2301", "0123", "2301", "0312", "0312", "1302", "0312", "0213" },
        { "2301", "2301", "0312", "0312", "1302", "1203", "0213", "0312", "1302", "0123", "0312", "0123", "1203", "1302" },
        { "1203", "1203", "2301", "0123", "0213", "1203", "0312", "0123", "1302", "1203", "1203", "0213", "1302", "0213" },
        { "2301", "0213", "1302", "1203", "0123", "1203", "0213", "0123", "1302", "1302", "1302", "2301", "0312", "0312" },
        { "1203", "0312", "1203", "2301", "0312", "1203", "2301", "1203", "2301", "0213", "0312", "2301", "0123", "0123" },
        { "0123", "0213", "0123", "1302", "2301", "1302", "2301", "1203", "1302", "0123", "2301", "0123", "0312", "0213" },
        { "0123", "0213", "1302", "1302", "0213", "1203", "2301", "1203", "1302", "0123", "2301", "0123", "0312", "0213" }
    };
    readonly string[] operatorNames = { "AND", "OR", "XOR", "NAND", "NOR", "XNOR" };
    readonly string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly string[] ArrowDirections = { "Up", "Down", "Left", "Right" };
    readonly int[] flashes = new int[8];
    readonly string[] LogABCD = new string[4];
    readonly int[] ABCD = new int[4];
    readonly int logicOperator;
    readonly string pairOrder = "";
    readonly string[] operationPairs = new string[2];
    readonly string[] finalPairs = new string[2];
    bool customAnimating = false;
    bool submissionMode = false;
    bool struckThisStage;
    int curLEDPos = -1;
    int curLEDColor = Random.Range(0, 9);
    char inputChar;
    int targetColor, targetDir = 0;
    readonly List<int> solvedLEDs = new List<int>();
    readonly List<int> solvedColors = new List<int>();

    public LogicalColorCombinations(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Logical Color Combinations.", ModuleID);
        for (int i = 0; i < 8; i++)
        {
            flashes[i] = Random.Range(0, 4);
        }
        Debug.LogFormat("[The Cruel Modkit #{0}] Arrow's flashing sequence is {1}. Press the ❖ button to play this sequence.", ModuleID, flashes.Select(x => ComponentInfo.ArrowColors[Info.Arrows[x]]).Join(", "));
        Debug.LogFormat("[The Cruel Modkit #{0}] The LEDs are {1}.", ModuleID, Info.GetLEDInfo());
        for (int i = 0; i < 4; i++)
        {
            string pair = "";pair += pairTable[MainColorConvert(Info.LED[i * 2], false), ArrowColorConvert(i * 2)];
            pair += pairTable[MainColorConvert(Info.LED[i * 2 + 1], false), ArrowColorConvert(i * 2 + 1)];
            LogABCD[i] = pair;
            ABCD[i] = Base36Convert(pair);
        }
        Debug.LogFormat("[The Cruel Modkit #{0}] A, B, C and D respectively are {1}.", ModuleID, LogABCD.Join(", "));
        logicOperator = logicOperatorTable[MainColorConvert(Info.Button, true), Array.IndexOf(ComponentInfo.ButtonList, Info.ButtonText)];
        pairOrder = pairOrderTable[MainColorConvert(Info.Button, true), Array.IndexOf(ComponentInfo.ButtonList, Info.ButtonText)];
        Debug.LogFormat("[The Cruel Modkit #{0}] The correct operator to use is {1}. The two letter pairs are {2} and {3}.", ModuleID, operatorNames[logicOperator], "ABCD"[pairOrder[0] - 48].ToString() + "ABCD"[pairOrder[1] - 48].ToString(), "ABCD"[pairOrder[2] - 48].ToString() + "ABCD"[pairOrder[3] - 48].ToString());
        for (int i = 0; i < 2; i++)
        {
            operationPairs[i] = ApplyOperator(logicOperator, pairOrder.Substring(i * 2, 2));
            finalPairs[i] = ToBase36(Convert.ToInt32(operationPairs[i], 2));
        }
        Debug.LogFormat("[The Cruel Modkit #{0}] The two binary numbers after applying the operator are {1} and {2}.", ModuleID, operationPairs[0], operationPairs[1]);
        Debug.LogFormat("[The Cruel Modkit #{0}] The final Base-36 numbers are {1} and {2}.", ModuleID, finalPairs[0], finalPairs[1]);
    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating() || customAnimating)
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
        Module.StartCoroutine(FlashSequence());
    }

    public override void OnButtonPress()
    {
        if (Module.IsAnimating() || customAnimating)
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
        Module.Button.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();
        if (!submissionMode)
        {
            submissionMode = true;
            if (!struckThisStage)
            {
                curLEDPos = Random.Range(0, 8);
                while (solvedLEDs.Contains(curLEDPos)) curLEDPos = Random.Range(0, 8);
                Debug.LogFormat("[The Cruel Modkit #{0}] Button pressed. Entering submission mode.", ModuleID);
                CalculateInput();
            }
            for (int i = 0; i < Module.LED.Length; i++)
            {
                if (solvedLEDs.Contains(i)) Module.LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[ToMainColor(solvedColors[solvedLEDs.IndexOf(i)])];
                else if (i == curLEDPos) Module.LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[ToMainColor(curLEDColor)];
                else Module.LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[0];
            }
        }
        else
        {
            curLEDColor += 1;
            curLEDColor %= 9;
            Module.LED[curLEDPos].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[ToMainColor(curLEDColor)];
        }
    }

    public override void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating() || customAnimating)
            return;

        Module.Audio.PlaySoundAtTransform(Module.ArrowSounds[Arrow].name, Module.transform);
        Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Info.ArrowDirections[Arrow], Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        Module.StartSolve();

        if (!submissionMode || Arrow > 3) return;
        if (curLEDColor != targetColor)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} color was submitted instead of {2}.", ModuleID, Enum.GetName(typeof(ComponentInfo.MainColors), ToMainColor(curLEDColor)), Enum.GetName(typeof(ComponentInfo.MainColors), ToMainColor(targetColor)));
            Module.CauseStrike();
            struckThisStage = true;
            ExitSubmission();
            return;
        }
        if (ArrowDirConvert(Arrow) != targetDir)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} direction was submitted instead of {2}.", ModuleID, ArrowDirections[ArrowDirConvert(Arrow)], ArrowDirections[targetDir]);
            Module.CauseStrike();
            struckThisStage = true;
            ExitSubmission();
            return;
        }

        solvedLEDs.Add(curLEDPos);
        solvedColors.Add(curLEDColor);
        curLEDPos = Random.Range(0, 8);
        while (solvedLEDs.Contains(curLEDPos)) curLEDPos = Random.Range(0, 8);
        curLEDColor = Random.Range(0, 9);
        Debug.LogFormat("[The Cruel Modkit #{0}] Correctly submitted the character {1}.", ModuleID, inputChar);
        if (solvedLEDs.Count == 6)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Correctly submitted all characters. Module solved.", ModuleID);
            Module.Solve();
            return;
        }

        CalculateInput();
        Info.LED[curLEDPos] = ToMainColor(curLEDColor);
        Module.LED[curLEDPos].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[Info.LED[curLEDPos]];
        struckThisStage = false;
    }

    void ExitSubmission()
    {
        for (int i = 0; i < Module.LED.Length; i++)
        {
            Module.LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = Module.LEDMats[Info.LED[i]];
        }
        submissionMode = false;
    }

    void CalculateInput()
    {
        inputChar = finalPairs[(int)solvedLEDs.Count / 3][solvedLEDs.Count % 3];
        int inputNum = base36.IndexOf(finalPairs[(int)solvedLEDs.Count / 3][solvedLEDs.Count % 3]);
        if (Info.Arrows[8] == 8) inputNum = 35 - inputNum;
        targetColor = (int)inputNum / 4;
        targetDir = inputNum % 4;
        Debug.LogFormat("[The Cruel Modkit #{0}] Character {1}: target color - {2}, target direction - {3}.", ModuleID, inputChar, Enum.GetName(typeof(ComponentInfo.MainColors), ToMainColor(targetColor)), ArrowDirections[targetDir]);
    }

    string ApplyOperator(int o, string pair)
    {
        string b1 = Convert.ToString(ABCD[pair[0] - 48], 2);
        string b2 = Convert.ToString(ABCD[pair[1] - 48], 2);
        string bf = "";
        b1 = new string('0', 11 - b1.Length) + b1;
        b2 = new string('0', 11 - b2.Length) + b2;
        for (int i = 0; i < 11; i++)
        {
            switch (o)
            {
                case 0: bf += b1[i] == '1' & b2[i] == '1' ? "1" : "0"; break;
                case 1: bf += b1[i] == '1' || b2[i] == '1' ? "1" : "0"; break;
                case 2: bf += b1[i] == '1' ^ b2[i] == '1' ? "1" : "0"; break;
                case 3: bf += !(b1[i] == '1' & b2[i] == '1') ? "1" : "0"; break;
                case 4: bf += !(b1[i] == '1' || b2[i] == '1') ? "1" : "0"; break;
                case 5: bf += !(b1[i] == '1' ^ b2[i] == '1') ? "1" : "0"; break;
            }
        }
        return bf;
    }

    int Base36Convert(string n)
    {
        return base36.IndexOf(n[0]) * 36 + base36.IndexOf(n[1]);
    }

    string ToBase36(int n)
    {
        string c = "";
        for (int i = 0; i < 3; i++)
        {
            c += base36[n % 36];
            n = (int)n / 36;
        }
        return new string(c.Reverse().ToArray());
    }

    int ArrowColorConvert(int Arrow)
    {
        return new int[] { 2, 3, 0, 1 }[Info.Arrows[flashes[Arrow]]] + 4 * (Info.Arrows[8] == 8 ? 1 : 0);
    }

    int ArrowDirConvert(int Arrow)
    {
        return new int[] { 0, 3, 1, 2 }[Arrow];
    }

    int MainColorConvert(int Color, bool Button)
    {
        return new int[] { Button ? 9 : 10, 6, 5, 4, 3, 1, 8, 7, 0, Button ? 10 : 9, 2 }[Color];
    }

    int ToMainColor(int Color)
    {
        return new int[] { 8, 5, 10, 4, 3, 2, 1, 7, 6, 0, 9 }[Color];
    }

    IEnumerator FlashSequence()
    {
        customAnimating = true;
        for (int i = 0; i < 8; i++)
        {
            Module.Arrows[flashes[i]].transform.Find("ArrowLight").GetComponentInChildren<Light>().range = 0.018966577f;
            Module.Arrows[flashes[i]].transform.Find("ArrowLight").gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            Module.Arrows[flashes[i]].transform.Find("ArrowLight").gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }
        customAnimating = false;
    }
}