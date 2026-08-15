using System;
using wawa.Modules;
using static ComponentInfo;

public class TemplatePuzzle : Puzzle
{
    public TemplatePuzzle(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Template Puzzle. Initialize your module and log the required components in this constructor.");
        // Remove any logs that aren't necessary for solving the module
        Module.Log("Wires present: {0}.", Info.GetWireInfo());
        Module.Log("Wire LEDs present: {0}.", Info.GetWireLEDInfo());
        Module.Log("Button is {0}.", Info.GetButtonInfo());
        Module.Log("LEDs present: {0}.", Info.GetLEDInfo());
        Module.Log("Symbols present: {0}.", Info.GetSymbolInfo());
        Module.Log("Alphanumeric keys present: {0}.", Info.GetAlphabetInfo());
        Module.Log("Arrows present: {0}.", Info.GetArrowsInfo());
        Module.Log("Bulb 1 is {0}, {1}, and {2}. Bulb 2 is {3}, {4}, and {5}. The O button is on the {6}.", Enum.GetName(typeof(BulbColorNames), Info.BulbColors[0]), Info.BulbOpaque[0] ? "opaque" : "see-through", Info.BulbOn[0] ? "on" : "off", Enum.GetName(typeof(BulbColorNames), Info.BulbColors[1]), Info.BulbOpaque[1] ? "opaque" : "see-through", Info.BulbOn[1] ? "on" : "off", Info.BulbOLeft ? "left" : "right");
        Module.Log($"Timer display is {Info.TimerDisplay.ToString().PadLeft(5, '0')}.");
        Module.Log($"Word display is {Info.WordDisplay}.");
        Module.Log($"Number display is {Info.NumberDisplay}.");
        Module.Log($"Morse LED is {Info.Morse}.");
        Module.Log($"The meter is {(MeterColors)Info.MeterColor} and at {Math.Round(Info.MeterValue * 100, 1)}%.");
        Module.Log($"Resistor 1 has ports {Info.ResistorText[0]} and {Info.ResistorText[1]} with the colors {Info.GetResistorInfo(0)}.");
        Module.Log($"Resistor 2 has ports {Info.ResistorText[2]} and {Info.ResistorText[3]} with the colors {Info.GetResistorInfo(1)}.");
        Module.Log($"The identity card displays {IdentityNames[Info.Identity[0]]} with {IdentityItems[Info.Identity[1]]} in the {IdentityLocations[Info.Identity[2]]} with a rarity of {IdentityRarity[Info.Identity[3]]}");
    }

    // Make sure to only use the methods that your module will actually need to use and delete the rest
    // Don't modify any of the logic already present in these methods unless you absolutely need to,
    //  put any module logic at the end once Module.StartSolve() is called
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
                Module.Log("Resetting wires...");

                Info.GenerateWireInfo();
                Info.GenerateWireLEDInfo();
                Module.RegenWires();

                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnButtonPress()
    {
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
    }

    public override void OnButtonRelease()
    {
        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.BigButtonRelease);
    }

    public override void OnSymbolPress(int Symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Symbols[Symbol].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Symbol {0} was pressed when the component selection was [{1}] instead of [{2}].", Symbol + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                Module.StartCoroutine(Module.ButtonStrike(true, Symbol));
                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnAlphabetPress(int Alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Alphabet[Alphabet].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Alphanumeric key {0} was pressed when the component selection was [{1}] instead of [{2}].", Alphabet + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnPianoPress(int Piano)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Piano[Piano].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.PianoSounds[Piano + (Info.Piano * 12)]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} key on the piano was pressed when the component selection was [{1}] instead of [{2}].", PianoKeyNames[(PianoKeys)Piano], Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} arrow button was pressed when the component selection was [{1}] instead of [{2}].", ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnBulbButtonPress(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.BulbButtons[Button].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} button was pressed when the component selection was [{1}] instead of [{2}].", (Button == 0) == Info.BulbOLeft ? "O" : "I", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnBulbButtonRelease(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Play(Module.transform, Sound.ButtonRelease);
    }

    public override void OnBulbInteract(int Bulb)
    {
        if (Module.IsAnimating())
            return;

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbOn[Bulb]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Shake(Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents() && !BulbScrewedIn[Bulb])
            {
                Module.Strike("Strike! The {0} bulb was removed when the component selection was [{1}] instead of [{2}].", (Bulb + 1) == 1 ? "first" : "second", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.UtilityButton.GetComponentInChildren<KMSelectable>(), 0.5f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The ❖ button was pressed when the component selection was [{0}] instead of [{1}].", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }
    }
}
