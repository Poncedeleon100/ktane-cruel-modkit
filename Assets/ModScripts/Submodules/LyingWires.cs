using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using wawa.Modules;
using static ComponentInfo;

public class LyingWires : Puzzle
{
    readonly Dictionary<int, bool> colorConditions = new Dictionary<int, bool>();
    readonly Dictionary<int, string> colorStatements = new Dictionary<int, string>{
       { 0, "The button’s label begins with a letter N-Z"},
       { 1, "The button is Blue" },
       { 2, "The name of the button’s color ends with a Letter A-M" },
       { 3, "The button is Red, Green, or Blue" },
       { 4, "The button is White or Black" },
       { 5, "The button’s color can be found in the right half of the manual's first table" },
       { 6, "The button is Red, Orange, or Yellow" },
       { 7, "The button is a button" },
       { 8, "The button’s label is \"YES\", \"NO\", or \"I DON'T KNOW\"" },
       { 9, "The button’s label contains fewer than 5 letters" },
       { 10, "The button’s color name contains the letter \"R\"" },
       { 11, "The button’s label is \"Press\", \"Tap\", \"Push\", or \"Click\"" },
    };
    readonly int[] trueColors;
    readonly bool[] initialStatements = new bool[7];
    readonly bool[] liars = new bool[7];
    readonly bool[] firstValues = new bool[7];
    readonly bool[] secondValues = new bool[7];
    readonly bool[] finalCuts = new bool[7];
    int numberOfLiars;
    int targetLastDigit;
    bool tap;
    bool incorrectHold = false;
    readonly List<int> wiresToBeCut = new List<int>();
    readonly string[] cluedoCharacters = new string[] { "Miss Scarlett", "Colonel Mustard", "Reverend Green", "Mrs Peacock", "Professor Plum", "Mrs White", "Dr Orchid" };
    readonly string[] monsplodeCharacters = new string[] { "Percy", "Lanaluff", "Nibs", "Clondar", "Melbor", "Magmy", "Pouse" };
    readonly int[] cluedoColors = new int[] { 8, 5, 7, 10 };
    readonly int[] monsplodeColors = new int[] { 3, 4, 6 };
    readonly int[] ktaneDiscordColors = new int[] { 1, 2, 9, 0 };
    readonly int[] refersToColorName = new int[] { 1, 3, 4, 5, 6 };
    readonly int[] refersToColorLabel = new int[] { 8, 11 };
    readonly Stopwatch buttonHoldDetection = new Stopwatch();

    public LyingWires(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Lying Wires.");
        string buttonColorName = Enum.GetName(typeof(MainColors), Info.Button).ToUpper();
        InitializeColorConditions(buttonColorName);
        trueColors = colorConditions.Where(x => x.Value).Select(x => x.Key).ToArray();
        Module.Log("Wires present: {0}.", Info.GetWireInfo());
        Module.Log("Wire LEDs present: {0}.", Info.GetWireLEDInfo());
        Module.Log("Button is {0}.", Info.GetButtonInfo());
        DetermineWires();
    }

    private void InitializeColorConditions(string buttonColorName){
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
        colorConditions.Add(10, buttonColorName.Contains("R"));
        colorConditions.Add(11, Info.ButtonText == "PRESS" || Info.ButtonText == "TAP" || Info.ButtonText == "PUSH" || Info.ButtonText == "CLICK");
    }

    public override void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.WireSnip);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Wire {0} was cut when the component selection was [{1}] instead of [{2}].", Wire + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                RegenWires();
                return;
            }
            Module.StartSolve();
        }

        if (finalCuts[Wire])
        {
            Module.Strike("Strike! Wire {0} was incorrectly cut.", Wire + 1);
            RegenWires();
            return;
        }

        Module.Log("Wire {0} was correctly cut.", Wire + 1);
        WiresCut.Add(Wire);

        Module.StartSolve();
    }

    public override void OnButtonPress()
    {
        buttonHoldDetection.Start();
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Button.GetComponentInChildren<KMSelectable>(), 0.25f, Sound.BigButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }
            Module.StartSolve();
        }

        int lastDigitOfTimer = ((int)Module.Bomb.GetTime()) % 10;
        if (wiresToBeCut.OrderBy(x => x).SequenceEqual(WiresCut.OrderBy(x => x)))
        {
            if (tap && lastDigitOfTimer != (numberOfLiars + Info.NumberDisplay) % 10)
            {
                Module.Strike("Strike! The button was tapped at the wrong time (last digit of the timer was {0}, was supposed to be {1}).", lastDigitOfTimer, (numberOfLiars + Info.NumberDisplay) % 10);
                RegenWires();
                incorrectHold = true;
            }
            else if (!tap && lastDigitOfTimer != numberOfLiars)
            {
                Module.Strike("Strike! The button was held at the wrong time (last digit of the timer was {0}, was supposed to be {1}).", lastDigitOfTimer, numberOfLiars);
                RegenWires();
                incorrectHold = true;
            }
        }
        else
        {
            Module.Strike("Strike! The button was pressed/held when not all wires had been correctly cut.");
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

        Module.Play(Module.transform, Sound.BigButtonRelease);

        if (incorrectHold)
        {
            buttonHoldDetection.Reset();
            incorrectHold = false;
            return;
        }
        int lastDigitOfTimer = ((int)Module.Bomb.GetTime()) % 10;
        if (wiresToBeCut.OrderBy(x => x).SequenceEqual(WiresCut.OrderBy(x => x)))
        {
            if (tap && buttonHoldDetection.ElapsedMilliseconds >= 500)
            {
                Module.Strike("Strike! The button was held when it was supposed to be tapped.");
                RegenWires();
            }
            else if (!tap && buttonHoldDetection.ElapsedMilliseconds <= 500)
            {
                Module.Strike("Strike! The button was tapped when it was supposed to be held.");
                RegenWires();
            }

            else if (lastDigitOfTimer != targetLastDigit)
            {
                Module.Strike("Strike! The button was released at the wrong time (last digit of the timer was {0}, was supposed to be {1}).", lastDigitOfTimer, targetLastDigit);
                RegenWires();
            }
            else
            {
                Module.SolveModule("Solved! The button was released at the right time.");
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
            Module.Log("{0}, so any {1} wires may be telling true statements.", colorStatements[color], Enum.GetName(typeof(WireColors), color).ToLower());
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
            Module.Log("None of the wires are telling true statements.", string.Join(", ", initiallyTrueWireIndices.ToArray()));
        }
        else
        {
            Module.Log("The following wires are telling true statements: {0}.", string.Join(", ", initiallyTrueWireIndices.ToArray()));
        }

        if (Info.NumberDisplay % 4 == 0)
        {
            Module.Log("The number display's number ({0}) is divisible by 4, so any wire with a white star will have a true first value.", Info.NumberDisplay);
            for (int i = 0; i < 7; i++)
            {
                int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[i] / 11))));
                firstValues[i] = star == 2;
            }
        }
        else if (Info.NumberDisplay % 3 == 0)
        {
            Module.Log("The number display's number ({0}) is divisible by 3, so any wire with a black star will have a true first value.", Info.NumberDisplay);
            for (int i = 0; i < 7; i++)
            {
                int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[i] / 11))));
                firstValues[i] = star == 1;
            }
        }
        else
        {
            Module.Log("The number display's number ({0}) is not divisible by 3 or 4, so any wire with no star will have a true first value.", Info.NumberDisplay);
            for (int i = 0; i < 7; i++)
            {
                int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[i] / 11))));
                firstValues[i] = star == 0;
            }
        }

        string identity = IdentityNames[Info.Identity[0]];

        if (cluedoCharacters.Contains(identity))
        {
            Module.Log("The host, {0}, is a Cluedo character, so any wire with a red, orange, purple or yellow LED will have a true second value.", identity);
            for (int i = 0; i < 7; i++)
            {
                secondValues[i] = cluedoColors.Contains(Info.WireLED[i] % 11);
            }
        }
        else if (monsplodeCharacters.Contains(identity))
        {
            Module.Log("The host, {0}, is a Monsplode™, so any wire with a green, lime or pink LED will have a true second value.", identity);
            for (int i = 0; i < 7; i++)
            {
                secondValues[i] = monsplodeColors.Contains(Info.WireLED[i] % 11);
            }
        }
        else
        {
            Module.Log("The host, {0}, is a KTaNE Discord server member, so any wire with a blue, cyan, white or black LED will have a true second value.", identity);
            for (int i = 0; i < 7; i++)
            {
                secondValues[i] = ktaneDiscordColors.Contains(Info.WireLED[i] % 11);
            }
        }

        string[] operators = new string[] { "AND", "OR", "XOR", "NAND", "NOR", "XNOR" };
        Module.Log("The meter is {0}, so the {1} operation will be applied to the two booleans.", Enum.GetName(typeof(MeterColors), Info.MeterColor).ToLower(), operators[Info.MeterColor]);
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
            Module.Log("None of the wires are lying.");
        }
        else
        {
            Module.Log("The following wires are lying: {0}.", string.Join(", ", lyingWireIndices.ToArray()));
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
        Module.Log("The following wires should be cut: {0}.", string.Join(", ", wiresToBeCutIndices.ToArray()));
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
            Module.Log("There is 1 wire to cut.");
        }
        else
        {
            Module.Log("There are {0} wires to cut.", numberOfLiars);
        }
        if (numberOfLiars % 2 == 0)
        {
            Module.Log("There is an even number of wires to be cut, so the button should be tapped when the last digit of the timer is {0}.", (numberOfLiars + Info.NumberDisplay) % 10);
        }
        else
        {
            Module.Log("There is an odd number of wires to be cut, so the button should be held when the last digit of the timer is {0}, and released when it is {1}.", numberOfLiars, Info.NumberDisplay);
        }
    }

    private void RegenWires()
    {
        Module.Log("Resetting wires...");
        Module.RegenWires();
        Module.Log("The wires are as follows: {0}", Info.GetWireInfo());
        Module.Log("The wire LEDs are as follows: {0}", Info.GetWireLEDInfo());
        DetermineWires();
    }

}
