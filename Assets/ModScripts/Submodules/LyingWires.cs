using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class LyingWires : Puzzle
{
    private readonly Dictionary<int, bool> colorConditions = new Dictionary<int, bool>();
    private readonly Dictionary<int, string> colorStatements = new Dictionary<int, string>();
    private readonly int[] trueColors;
    private readonly bool[] initialStatements = new bool[7];
    private readonly bool[] liars = new bool[7];
    private readonly bool[] firstValues = new bool[7];
    private readonly bool[] secondValues = new bool[7];
    private readonly bool[] finalCuts = new bool[7];
    private int numberOfLiars;
    private int targetLastDigit;
    private bool tap;
    private bool incorrectHold = false;
    private List<int> wiresToBeCut = new List<int>();
    private readonly string[] cluedoCharacters = new string[] { "Miss Scarlett", "Colonel Mustard", "Reverend Green", "Mrs Peacock", "Professor Plum", "Mrs White", "Dr Orchid" };
    private readonly string[] monsplodeCharacters = new string[] { "Percy", "Lanaluff", "Nibs", "Clondar", "Melbor", "Magmy", "Pouse" };
    private readonly string[] ktaneDiscordServerMembers = new string[] { "Yoshi Dojo", "VFlyer", "Sameone", "Cyanix", "Konoko", "Red Penguin", "GhostSalt", "Yabbaguy" };
    private readonly int[] cluedoColors = new int[] { 8, 5, 7, 10 };
    private readonly int[] monsplodeColors = new int[] { 3, 4, 6 };
    private readonly int[] ktaneDiscordColors = new int[] { 1, 2, 9, 0 };
    private readonly int[] refersToColorName = new int[] { 1, 3, 4, 5, 6 };
    private readonly int[] refersToColorLabel = new int[] { 8, 11 };
    private readonly Stopwatch buttonHoldDetection = new Stopwatch();

    public LyingWires(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Solving Lying Wires.", ModuleID);
        string buttonColorName = Info.MainColors[Info.Button].ToUpper();
        colorConditions.Add(0, Info.ButtonText[0] >= 78 && Info.ButtonText[0] <= 90);
        colorConditions.Add(1, Info.Button == 1);
        colorConditions.Add(2, buttonColorName[buttonColorName.Length - 1] >= 65 && buttonColorName[buttonColorName.Length - 1] <= 77);
        colorConditions.Add(3, Info.Button == 8 || Info.Button == 3 || Info.Button == 1);
        colorConditions.Add(4, Info.Button == 9 || Info.Button == 0);
        colorConditions.Add(5, Info.Button == 1 || Info.Button == 7 || Info.Button == 6 || Info.Button == 0 || Info.Button == 9);
        colorConditions.Add(6, Info.Button == 8 || Info.Button == 5 || Info.Button == 10);
        colorConditions.Add(7, true);
        colorConditions.Add(8, Info.ButtonText == "YES" || Info.ButtonText == "NO" || Info.ButtonText == "I DON'T KNOW");
        colorConditions.Add(9, Info.ButtonText.Length < 5);
        colorConditions.Add(10, Info.MainColors[Info.Button].ToUpper().Contains("R"));
        colorConditions.Add(11, Info.ButtonText == "PRESS" || Info.ButtonText == "TAP" || Info.ButtonText == "PUSH" || Info.ButtonText == "CLICK");

        colorStatements.Add(0, "The button’s label begins with a letter N-Z");
        colorStatements.Add(1, "The button is Blue");
        colorStatements.Add(2, "The name of the button’s color ends with a Letter A-M");
        colorStatements.Add(3, "The button is Red, Green, or Blue");
        colorStatements.Add(4, "The button is White or Black");
        colorStatements.Add(5, "The button’s color can be found in the right half of the manual's first table");
        colorStatements.Add(6, "The button is Red, Orange, or Yellow");
        colorStatements.Add(7, "The button is a button");
        colorStatements.Add(8, "The button’s label is “YES”, “NO”, or “I DON'T KNOW”");
        colorStatements.Add(9, "The button’s label contains fewer than 5 letters");
        colorStatements.Add(10, "The button’s color name contains the letter “R”");
        colorStatements.Add(11, "The button’s label is “Press”, “Tap”, “Push”, or “Click”");

        trueColors = colorConditions.Where(x => x.Value).Select(x => x.Key).ToArray();
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The button is {1}.", ModuleID, Info.GetButtonInfo());
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The wires are as follows: {1}.", ModuleID, Info.GetWireInfo());
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The wire LEDs are as follows: {1}.", ModuleID, Info.GetWireLEDInfo());
        DetermineWires();
    }

    public override void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the component selection was [{2}] instead of [{3}].", ModuleID, Wire + 1, Module.GetOnComponents(), Module.GetTargetComponents());
            RegenWires();
            return;
        }

        if (finalCuts[Wire])
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was incorrectly cut.", ModuleID, Wire + 1);
            RegenWires();
            return;
        }

        Module.StartSolve();
    }

    public override void OnButtonPress()
    {
        buttonHoldDetection.Start();
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
        Module.Button.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        int lastDigitOfTimer = ((int)Module.Bomb.GetTime()) % 10;
        if (wiresToBeCut.OrderBy(x => x).SequenceEqual(Module.WiresCut.OrderBy(x => x)))
        {
            if (tap && lastDigitOfTimer != (numberOfLiars + Info.NumberDisplay) % 10)
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was tapped at the wrong time (last digit of the timer was {1}, was supposed to be {2}).", ModuleID, lastDigitOfTimer, (numberOfLiars + Info.NumberDisplay) % 10);
                RegenWires();
                incorrectHold = true;
            }
            else if (!tap && lastDigitOfTimer != numberOfLiars)
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was held at the wrong time (last digit of the timer was {1}, was supposed to be {2}).", ModuleID, lastDigitOfTimer, numberOfLiars);
                RegenWires();
                incorrectHold = true;
            }
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed/held when not all wires had been correctly cut.", ModuleID);
            RegenWires();
            incorrectHold = true;
        }

        Module.StartSolve();
    }

    public override void OnButtonRelease()
    {
        buttonHoldDetection.Stop();
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Module.transform);
        if (incorrectHold)
        {
            buttonHoldDetection.Reset();
            incorrectHold = false;
            return;
        }
        int lastDigitOfTimer = ((int)Module.Bomb.GetTime()) % 10;
        if (wiresToBeCut.OrderBy(x => x).SequenceEqual(Module.WiresCut.OrderBy(x => x)))
        {
            if (tap && buttonHoldDetection.ElapsedMilliseconds >= 500)
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was held when it was supposed to be tapped.", ModuleID);
                RegenWires();
            }
            else if (!tap && buttonHoldDetection.ElapsedMilliseconds <= 500)
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was tapped when it was supposed to be held.", ModuleID);
                RegenWires();
            }

            else if (lastDigitOfTimer != targetLastDigit)
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was released at the wrong time (last digit of the timer was {1}, was supposed to be {2}).", ModuleID, lastDigitOfTimer, targetLastDigit);
                RegenWires();
            }
            else
            {
                UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Solved! The button was released at the right time.", ModuleID);
                Module.Solve();
            }
        }

        buttonHoldDetection.Reset();
    }

    private void DetermineWires()
    {
        wiresToBeCut.Clear();
        for (int i = 0; i < 7; i++)
        {
            if ((refersToColorName.Contains(Info.Wires[0][i]) && refersToColorName.Contains(Info.Wires[1][i])) || (refersToColorLabel.Contains(Info.Wires[0][i]) && refersToColorLabel.Contains(Info.Wires[1][i])))
            {
                initialStatements[i] = trueColors.Contains(Info.Wires[0][i]) || trueColors.Contains(Info.Wires[1][i]);
            }
            else
            {
                initialStatements[i] = trueColors.Contains(Info.Wires[0][i]) && trueColors.Contains(Info.Wires[1][i]);
            }
        }
        foreach (int color in trueColors)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] {1}, so any {2} wires may be telling true statements.", ModuleID, colorStatements[color], ComponentInfo.WireColors[color].ToLower());
        }
        List<string> initiallyTrueWireIndices = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            if (initialStatements[i])
            {
                initiallyTrueWireIndices.Add((i + 1).ToString());
            }
        }

        if (initiallyTrueWireIndices.Count == 0)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] None of the wires are telling true statements.", ModuleID, string.Join(", ", initiallyTrueWireIndices.ToArray()));
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The following wires are telling true statements: {1}.", ModuleID, string.Join(", ", initiallyTrueWireIndices.ToArray()));
        }

        if (Info.NumberDisplay % 4 == 0)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The number display's number ({1}) is divisible by 4, so any wire with a white star will have a true first value.", ModuleID, Info.NumberDisplay);
            for (int i = 0; i < 7; i++)
            {
                int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[i] / 11))));
                firstValues[i] = star == 2;
            }
        }
        else if (Info.NumberDisplay % 3 == 0)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The number display's number ({1}) is divisible by 3, so any wire with a black star will have a true first value.", ModuleID, Info.NumberDisplay);
            for (int i = 0; i < 7; i++)
            {
                int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[i] / 11))));
                firstValues[i] = star == 1;
            }
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The number display's number ({1}) is not divisible by 3 or 4, so any wire with no star will have a true first value.", ModuleID, Info.NumberDisplay);
            for (int i = 0; i < 7; i++)
            {
                int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[i] / 11))));
                firstValues[i] = star == 0;
            }
        }

        string identity = Info.IdentityNames[Info.Identity[0]];

        if (cluedoCharacters.Contains(identity))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The host, {1}, is a Cluedo character, so any wire with a red, orange, purple or yellow LED will have a true second value.", ModuleID, identity);
            for (int i = 0; i < 7; i++)
            {
                secondValues[i] = cluedoColors.Contains(Info.WireLED[i] % 11);
            }
        }
        else if (monsplodeCharacters.Contains(identity))
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The host, {1}, is a Monsplode™, so any wire with a green, lime or pink LED will have a true second value.", ModuleID, identity);
            for (int i = 0; i < 7; i++)
            {
                secondValues[i] = monsplodeColors.Contains(Info.WireLED[i] % 11);
            }
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The host, {1}, is a KTaNE Discord server member, so any wire with a blue, cyan, white or black LED will have a true second value.", ModuleID, identity);
            for (int i = 0; i < 7; i++)
            {
                secondValues[i] = ktaneDiscordColors.Contains(Info.WireLED[i] % 11);
            }
        }

        string[] operators = new string[] { "AND", "OR", "XOR", "NAND", "NOR", "XNOR" };
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The meter is {1}, so the {2} operation will be applied to the two booleans.", ModuleID, Info.MeterColors[Info.MeterColor].ToLower(), operators[Info.MeterColor]);
        switch (Info.MeterColor)
        {
            case 0:
                for (int i = 0; i < 7; i++)
                {
                    liars[i] = firstValues[i] && secondValues[i];
                }
                break;
            case 1:
                for (int i = 0; i < 7; i++)
                {
                    liars[i] = firstValues[i] || secondValues[i];
                }
                break;
            case 2:
                for (int i = 0; i < 7; i++)
                {
                    liars[i] = (firstValues[i] && !secondValues[i]) || (!firstValues[i] && secondValues[i]);
                }
                break;
            case 3:
                for (int i = 0; i < 7; i++)
                {
                    liars[i] = !firstValues[i] || !secondValues[i];
                }
                break;
            case 4:
                for (int i = 0; i < 7; i++)
                {
                    liars[i] = !firstValues[i] && !secondValues[i];
                }
                break;
            case 5:
            default:
                for (int i = 0; i < 7; i++)
                {
                    liars[i] = (firstValues[i] || !secondValues[i]) && (!firstValues[i] || secondValues[i]);
                }
                break;
        }
        List<string> lyingWireIndices = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            if (!liars[i])
            {
                lyingWireIndices.Add((i + 1).ToString());
            }
        }
        if (lyingWireIndices.Count == 0)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] None of the wires are lying.", ModuleID);
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The following wires are lying: {1}.", ModuleID, string.Join(", ", lyingWireIndices.ToArray()));
        }
        List<string> wiresToBeCutIndices = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            if (liars[i])
            {
                finalCuts[i] = initialStatements[i];
            }
            else
            {
                finalCuts[i] = !initialStatements[i];
            }
            if (!finalCuts[i])
            {
                wiresToBeCut.Add(i);
                wiresToBeCutIndices.Add((i + 1).ToString());
            }
        }
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The following wires should be cut: {1}.", ModuleID, string.Join(", ", wiresToBeCutIndices.ToArray()));
        numberOfLiars = finalCuts.Count(x => !x);
        if (numberOfLiars % 2 == 0)
        {
            targetLastDigit = (numberOfLiars + Info.NumberDisplay) % 10;
            tap = true;
        }
        else
        {
            targetLastDigit = Info.NumberDisplay % 10;
            tap = false;
        }

        if (numberOfLiars == 1)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] There is 1 wire to cut.", ModuleID);
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] There are {1} wires to cut.", ModuleID, numberOfLiars);
        }
        if (numberOfLiars % 2 == 0)
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] There is an even number of wires to be cut, so the button should be tapped when the last digit of the timer is {1}.", ModuleID, (numberOfLiars + Info.NumberDisplay) % 10);
        }
        else
        {
            UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] There is an odd number of wires to be cut, so the button should be held when the last digit of the timer is {1}, and released when it is {2}.", ModuleID, numberOfLiars, Info.NumberDisplay);
        }
    }

    private void RegenWires()
    {
        Module.CauseStrike();
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] Resetting wires...", ModuleID);
        Module.RegenWires();
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The wires are as follows: {1}", ModuleID, Info.GetWireInfo());
        UnityEngine.Debug.LogFormat("[The Cruel Modkit #{0}] The wire LEDs are as follows: {1}", ModuleID, Info.GetWireLEDInfo());
        DetermineWires();
    }

}
