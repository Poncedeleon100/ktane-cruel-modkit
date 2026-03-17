using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using static ComponentInfo;
using static CruelModkitScript;
using Random = UnityEngine.Random;
using KModkit;

public class EdgeworkEncoding : Puzzle
{

    public EdgeworkEncoding(CruelModkitScript module, int moduleID, ComponentInfo info, byte components) : base(module, moduleID, info, components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Edgework Encoding.", moduleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Widgets: Timer display is {1}. Number display is {2}.", moduleID, info.TimerDisplay.ToString().PadLeft(5, '0'), info.NumberDisplay);
        GetActiveComponents();
        CalculateStartingPoints();
        LogActiveComponents();
        CalculateEdgeworkAnswers();
        ValidateQuestion();
    }

    readonly List<string> edgeworkQuestions = new List<string>()
    {
        "Does the serial number contain letters from any of the present indicators?",
        "Is there a vowel in the serial number?",
        "How many D batteries are present?",
        "Does the serial number contain the total number of ports?",
        "Is there a CLR, FRQ, SIG, or NSA indicator present?",
        "What is the alphanumeric position of the second letter in the serial number?",
        "Does the serial number contain the number of D batteries?",
        "How many PS/2 and Serial ports are present?",
        "How many unlit indicators are present?",
        "Does the serial number contain any digits present in the calculated puzzle ID?",
        "How many AA batteries are present?",
        "How many Parallel and RJ-45 ports are present?",
        "Does the serial number contain the total number of indicators?",
        "Does the serial number contain any digits present in the total number of modules?",
        "How many battery holders are present?",
        "Is there an empty port plate present?",
        "How many lit indicators are present?",
        "What is the sum of the digits present in the serial number?",
        "Does the serial number contain the number of AA batteries?",
        "How many Stereo RCA and DVI-D ports are present?"
    };
    readonly List<int> edgeworkAnswers = new List<int>();

    readonly ComponentsEnum[] componentsArray = new ComponentsEnum[] 
        { ComponentsEnum.Wires, ComponentsEnum.Arrows, ComponentsEnum.Button, ComponentsEnum.Piano,
            ComponentsEnum.LED, ComponentsEnum.Symbols, ComponentsEnum.Bulbs, ComponentsEnum.Alphabet };
    readonly bool[] componentsActive = 
        { false, false, false, false, 
            false, false, false, false };
    int componentsActiveCount;
    int solvedComponents;

    // Moving down either list means a positive increment
    int edgeworkQuestionPosition;
    bool edgeworkQuestionPositiveIncrement;
    int componentPosition;
    bool componentPositionPositiveIncrement;

    readonly int[] arrowNumbers = new int[9];
    readonly bool[] arrowNumbersActivated = { false, false, false, false, false, false, false, false, false };
    int buttonPressCount = 0;
    bool ledsStartSolve = false;
    int bulbButtonPressCount = 0;

    public override void OnWireCut(int wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);
        Module.CutWire(wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the component selection was [{2}] instead of [{3}].", ModuleID, wire + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                ResetWires();

                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Wires)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the correct component was {1}.", ModuleID, wire + 1, componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            ResetWires();

            return;
        }

        ValidateWires(wire);
    }

    public override void OnButtonPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
        Module.Button.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Button)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the correct component was {1}.", ModuleID, componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        buttonPressCount++;
    }

    public override void OnSymbolPress(int symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Symbols[symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, symbol + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Module.StartCoroutine(Module.ButtonStrike(true, symbol));
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Symbols)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed when the correct component was {2}.", ModuleID, symbol + 1, componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(true, symbol));
            return;
        }

        ValidateSymbols(symbol);
    }

    public override void OnAlphabetPress(int alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Alphabet[alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, alphabet + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Alphabet)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the correct component was {2}.", ModuleID, alphabet + 1, componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
            return;
        }

        ValidateAlphabet(alphabet);
    }

    public override void OnPianoPress(int piano)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlaySoundAtTransform(Module.PianoSounds[piano + (Info.Piano * 12)].name, Module.transform);
        Module.Piano[piano].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, PianoKeyNames[(PianoKeys)piano], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Piano)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the correct component was {1}.", ModuleID, PianoKeyNames[(PianoKeys)piano], componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        ValidatePiano(piano);
    }

    public override void OnArrowPress(int arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Arrows[arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, ArrowDirectionNames[(ArrowDirections)arrow], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        Module.Audio.PlaySoundAtTransform(Module.ArrowSounds[arrow].name, Module.transform);
        Module.StartCoroutine(HandleArrowFlash(arrow));

        if (componentsArray[componentPosition] != ComponentsEnum.Arrows)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the correct component was {1}.", ModuleID, ArrowDirectionNames[(ArrowDirections)arrow], componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        Info.NumberDisplay = arrowNumbers[arrow];
        Module.SetNumber();

        if (arrowNumbersActivated[arrow] == true)
        {
            ValidateArrows(arrow);
        }
        else
        {
            arrowNumbersActivated[arrow] = true;
        }
    }

    public override void OnBulbButtonPress(int button)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.BulbButtons[button].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, (button == 0) == Info.BulbOLeft ? "O" : "I", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Bulbs)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button was pressed when the correct component was {1}.", ModuleID, (button == 0) == Info.BulbOLeft ? "O" : "I", componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        ValidateBulbs(button);
    }

    public override void OnBulbInteract(int bulb)
    {
        if (Module.IsAnimating())
            return;

        Module.HandleBulbScrew(bulb, BulbScrewedIn[bulb], Info.BulbOn[bulb]);

        BulbScrewedIn[bulb] = !BulbScrewedIn[bulb];

        Module.Audio.PlaySoundAtTransform(Module.BulbSounds[BulbScrewedIn[bulb] ? 0 : 1].name, Module.transform);
        Module.Bulbs[bulb].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents() && !BulbScrewedIn[bulb])
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the component selection was [{2}] instead of [{3}].", ModuleID, (bulb + 1) == 1 ? "first" : "second", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Bulbs)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the correct component was {1}.", ModuleID, (bulb + 1) == 1 ? "first" : "second", componentsArray[componentPosition].ToString());
            Module.CauseStrike();
            return;
        }
    }

    public override void OnUtilityPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.UtilityButton.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] == ComponentsEnum.Button)
        {
            ValidateButton();
        }
        if (componentsArray[componentPosition] == ComponentsEnum.LED)
        {
            if (!ledsStartSolve)
            {
                for (int i = 0; i < Info.LED.Length; i++)
                {
                    Info.LED[i] = (int)MainColors.Black;
                }
                Module.SetLEDs();
                ledsStartSolve = true;
                return;
            }
            ValidateLEDs();
        }
    }
    
    void GetActiveComponents()
    {
        foreach (ComponentsEnum Component in Enum.GetValues(typeof(ComponentsEnum)))
        {
            if (Component != ComponentsEnum.None && (base.Components & (byte)Component) == (byte)Component)
            {
                componentsActive[Array.IndexOf(componentsArray, Component)] = true;
            }
        }

        foreach (var ComponentActive in componentsActive)
        {
            if (ComponentActive)
                componentsActiveCount++;
        }
    }

    void CalculateStartingPoints()
    {
        // Last two digits of the timer display modulo 20; both positions are zero indexed here for convenience
        edgeworkQuestionPosition = (Info.TimerDisplay % 20) - 1;
        // Zeroes at this step become the max number for the specified value (20 here, 8 for ComponentPosition)
        if (edgeworkQuestionPosition < 0)
            edgeworkQuestionPosition = 19;
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting position for the \"Edgework Questions\" list is {1}: \"{2}\"", ModuleID, edgeworkQuestionPosition + 1, edgeworkQuestions[edgeworkQuestionPosition]);

        // First digit of timer display is even
        edgeworkQuestionPositiveIncrement = ((Convert.ToInt32(Info.TimerDisplay.ToString().PadLeft(5, '0').Substring(0, 1))) % 2) == 0;
        Debug.LogFormat("[The Cruel Modkit #{0}] The first digit on the timer display is {1}, so move {2} in the \"Edgework Questions\" list.", ModuleID, edgeworkQuestionPositiveIncrement ? "even" : "odd", edgeworkQuestionPositiveIncrement ? "down" : "up");


        // Number display modulo 8
        componentPosition = (Info.NumberDisplay % 8) - 1;
        if (componentPosition < 0)
            componentPosition = 7;
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting position for the \"Components\" list is {1}: \"{2}.\"", ModuleID, componentPosition + 1, componentsArray[componentPosition].ToString());

        // Number of components is even
        componentPositionPositiveIncrement = (componentsActiveCount % 2) == 0;
        Debug.LogFormat("[The Cruel Modkit #{0}] The number of active components is {1} which is {2}, so move {3} in the \"Components\" list.", ModuleID, componentsActiveCount, componentPositionPositiveIncrement ? "even" : "odd", componentPositionPositiveIncrement ? "down" : "up");
        // Second digit of timer display is even (used once Edgework Encoding becomes a normal module)
        //ComponentPositionPositiveIncrement = ((Convert.ToInt32(Info.TimerDisplay.ToString().Substring(1))) % 2) == 0;
        //Debug.LogFormat("[The Cruel Modkit #{0}] The second digit on the timer display is {1}, so move {2} in the \"Components\" list.", ModuleID, ComponentPositionPositiveIncrement ? "even" : "odd", ComponentPositionPositiveIncrement ? "down" : "up");
    }

    void LogActiveComponents()
    {
        foreach (ComponentsEnum component in componentsArray)
        {
            if (componentsActive[Array.IndexOf(componentsArray, component)])
            {
                switch (component)
                {
                    case ComponentsEnum.Wires:
                        Debug.LogFormat("[The Cruel Modkit #{0}] Wires present: {1}.", ModuleID, Info.GetWireInfo());
                        Debug.LogFormat("[The Cruel Modkit #{0}] Wire LEDs present: {1}.", ModuleID, Info.GetWireLEDInfo());
                        break;
                    case ComponentsEnum.Button:
                        Debug.LogFormat("[The Cruel Modkit #{0}] Button is {1}.", ModuleID, Info.GetButtonInfo());
                        break;
                    case ComponentsEnum.LED:
                        Debug.LogFormat("[The Cruel Modkit #{0}] LEDs present: {1}.", ModuleID, Info.GetLEDInfo());
                        break;
                    case ComponentsEnum.Symbols:
                        Debug.LogFormat("[The Cruel Modkit #{0}] Symbols present: {1}.", ModuleID, Info.GetSymbolInfo());
                        break;
                    case ComponentsEnum.Alphabet:
                        Debug.LogFormat("[The Cruel Modkit #{0}] Alphanumeric keys present: {1}.", ModuleID, Info.GetAlphabetInfo());
                        break;
                    case ComponentsEnum.Arrows:
                        Debug.LogFormat("[The Cruel Modkit #{0}] Arrows present: {1}.", ModuleID, Info.GetArrowsInfo());
                        break;
                    case ComponentsEnum.Bulbs:
                        Debug.LogFormat("[The Cruel Modkit #{0}] Bulb 1 is {1}, {2}, and {3}. Bulb 2 is {4}, {5}, and {6}. The O button is on the {7}.", ModuleID, Enum.GetName(typeof(BulbColorNames), Info.BulbColors[0]), Info.BulbOpaque[0] ? "opaque" : "see-through", Info.BulbOn[0] ? "on" : "off", Enum.GetName(typeof(BulbColorNames), Info.BulbColors[1]), Info.BulbOpaque[1] ? "opaque" : "see-through", Info.BulbOn[1] ? "on" : "off", Info.BulbOLeft ? "left" : "right");
                        break;
                }
            }
        }
    }

    void CalculateEdgeworkAnswers()
    {
        IEnumerable<char> serialNumberLetters = Module.Bomb.GetSerialNumberLetters();
        IEnumerable<int> serialNumberNumbers = Module.Bomb.GetSerialNumberNumbers();
        int dBatteryCount = Module.Bomb.GetBatteryCount(Battery.D);

        // Does the serial number contain letters from any of the present indicators?
        edgeworkAnswers.Add(Module.Bomb.GetIndicators().Join().Intersect(serialNumberLetters).Any() ? 1 : 0);
        
        // Is there a vowel in the serial number?
        edgeworkAnswers.Add(serialNumberLetters.Join().Intersect("AEIOU").Any() ? 1 : 0);
        
        // How many D batteries are present?
        edgeworkAnswers.Add(dBatteryCount);

        // Does the serial number contain any digits present in the total number of ports?
        edgeworkAnswers.Add(serialNumberNumbers.Join().Intersect((Module.Bomb.GetPortCount()).ToString()).Any() ? 1 : 0);
        
        // Is there a CLR, FRQ, SIG, or NSA indicator present?
        bool clr = Module.Bomb.IsIndicatorPresent(Indicator.CLR);
        bool frq = Module.Bomb.IsIndicatorPresent(Indicator.FRQ);
        bool sig = Module.Bomb.IsIndicatorPresent(Indicator.SIG);
        bool nsa = Module.Bomb.IsIndicatorPresent(Indicator.NSA);

        edgeworkAnswers.Add((clr || frq || sig || nsa) ? 1 : 0);

        // What is the alphanumeric position of the second letter in the serial number?
        edgeworkAnswers.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList().IndexOf(serialNumberLetters.ToList()[1]) + 1);

        // Does the serial number contain any digits present in the number of D batteries?
        edgeworkAnswers.Add(serialNumberNumbers.Join().Intersect(dBatteryCount.ToString()).Any() ? 1 : 0);

        // How many PS/2 and Serial ports are present?
        edgeworkAnswers.Add(Module.Bomb.GetPortCount(Port.PS2) + Module.Bomb.GetPortCount(Port.Serial));

        // How many unlit indicators are present?
        edgeworkAnswers.Add(Module.Bomb.GetOffIndicators().Count());

        // Does the serial number contain any digits present in the calculated puzzle ID?
        edgeworkAnswers.Add(serialNumberNumbers.Join().Intersect(componentsArray.ToString()).Any() ? 1 : 0);

        // How many AA batteries are present?
        edgeworkAnswers.Add(Module.Bomb.GetBatteryCount(Battery.AA));

        // How many Parallel and RJ - 45 ports are present?
        edgeworkAnswers.Add(Module.Bomb.GetPortCount(Port.Parallel) + Module.Bomb.GetPortCount(Port.RJ45));
        
        // Does the serial number contain any digits present in the total number of indicators?
        edgeworkAnswers.Add(serialNumberNumbers.Join().Intersect(Module.Bomb.GetIndicators().Count().ToString()).Any() ? 1 : 0);

        // Does the serial number contain any digits present in the total number of modules?
        edgeworkAnswers.Add(serialNumberNumbers.Join().Intersect(Module.Bomb.GetModuleNames().Count().ToString()).Any() ? 1 : 0);

        // How many battery holders are present?
        edgeworkAnswers.Add(Module.Bomb.GetBatteryHolderCount());

        // Is there an empty port plate present?
        edgeworkAnswers.Add(Module.Bomb.GetPortPlates().Any(plate => plate.Length == 0) ? 1 : 0);

        // How many lit indicators are present?
        edgeworkAnswers.Add(Module.Bomb.GetOnIndicators().Count());

        // What is the sum of the digits present in the serial number?
        edgeworkAnswers.Add(serialNumberNumbers.Sum());

        // Does the serial number contain any digits present in the number of AA batteries?
        edgeworkAnswers.Add(Module.Bomb.GetSerialNumberNumbers().Join().Intersect(Module.Bomb.GetBatteryCount(Battery.AA).ToString()).Any() ? 1 : 0);

        // How many Stereo RCA and DVI - D ports are present?
        edgeworkAnswers.Add(Module.Bomb.GetPortCount(Port.StereoRCA) + Module.Bomb.GetPortCount(Port.DVI));
    }

    void LogCurrentQuestion()
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] The current question is: \"{1}\" The answer is {2} and it must be submitted on the {3} component.", ModuleID, edgeworkQuestions[edgeworkQuestionPosition], edgeworkAnswers[edgeworkQuestionPosition], componentsArray[componentPosition].ToString());
        LogCurrentSolution();
    }

    void LogCurrentSolution()
    {
        switch (componentsArray[componentPosition])
        {
            case ComponentsEnum.Wires:
                Debug.LogFormat("[The Cruel Modkit #{0}] Cut wire {1} when the last digit of the timer is {2}.", ModuleID, (edgeworkAnswers[edgeworkQuestionPosition] % 7), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.Arrows:
                SetArrowNumbers();
                Debug.LogFormat("[The Cruel Modkit #{0}] The correct arrow direction is the {1} arrow.", ModuleID, ArrowDirectionNames[((ArrowDirections)Array.IndexOf(arrowNumbers, edgeworkAnswers[edgeworkQuestionPosition] % 10))]);
                break;
            case ComponentsEnum.Button:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press the button {1} time(s), then press the “❖” button.", ModuleID, edgeworkAnswers[edgeworkQuestionPosition]);
                break;
            case ComponentsEnum.Piano:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press key {1} when the last digit of the timer is {2}.", ModuleID, (edgeworkAnswers[edgeworkQuestionPosition] % 12), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.LED:
                Debug.LogFormat("[The Cruel Modkit #{0}] Submit {1} using the LEDs.", ModuleID, Convert.ToString(edgeworkAnswers[edgeworkQuestionPosition], 2).PadLeft(8, '0'));
                break;
            case ComponentsEnum.Symbols:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press symbol {1} when the last digit of the timer is {2}.", ModuleID, (edgeworkAnswers[edgeworkQuestionPosition] % 6), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.Bulbs:
                Debug.LogFormat("[The Cruel Modkit #{0}] Submit {1} using the buttons on Bulbs.", ModuleID, Convert.ToString(edgeworkAnswers[edgeworkQuestionPosition], 2));
                break;
            case ComponentsEnum.Alphabet:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press key {1} when the last digit of the timer is {2}.", ModuleID, (edgeworkAnswers[edgeworkQuestionPosition] % 6), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
        }
    }

    void SetArrowNumbers()
    {
        var tempArrowNumbers = "1234567890".ToCharArray().Select(x => Convert.ToInt32(x) - '0').OrderBy(x => Random.Range(0, 1000)).ToList();
        tempArrowNumbers.Remove(edgeworkAnswers[edgeworkQuestionPosition] % 10);
        int correctArrow = Random.Range(0, 9);

        for (int i = 0; i < arrowNumbers.Length; i++)
        {
            if (correctArrow == i)
            {
                arrowNumbers[i] = edgeworkAnswers[edgeworkQuestionPosition] % 10;
            }
            else
            {
                arrowNumbers[i] = tempArrowNumbers[i];
            }
        }
    }

    void ValidateQuestion()
    {
        if (!componentsActive[componentPosition])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The {1} component is not active on the module; skipping to the next question.", ModuleID, componentsArray[componentPosition].ToString());
            IncrementQuestion();
        }
        else
        {
            LogCurrentQuestion();
        }
    }

    void IncrementQuestion()
    {
        solvedComponents++;
        if (solvedComponents == 8)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] All answers submitted successfully. Module solved.", ModuleID);
            Module.Solve();
            return;
        }

        int totalComponents = componentsArray.Length - 1;
        int totalQuestions = edgeworkQuestions.Count() - 1;

        componentPosition += componentPositionPositiveIncrement ? 1 : -1;
        if (componentPosition < 0)
            componentPosition = totalComponents;
        if (componentPosition > totalComponents)
            componentPosition = 0;

        edgeworkQuestionPosition += edgeworkQuestionPositiveIncrement ? 1 : -1;
        if (edgeworkQuestionPosition < 0)
            edgeworkQuestionPosition = totalQuestions;
        if (edgeworkQuestionPosition > totalQuestions)
            edgeworkQuestionPosition = 0;

        ValidateQuestion();
    }

    void ValidateWires(int wire)
    {
        int correctWire = edgeworkAnswers[edgeworkQuestionPosition] % 7;
        int correctTimeLastDigit = edgeworkAnswers[edgeworkQuestionPosition] % 10;
        string currentTime = Module.Bomb.GetFormattedTime();
        int currentTimeLastDigit = Convert.ToInt32(currentTime.Substring(currentTime.Length - 1));

        if (wire != correctWire)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut instead of wire {2}.", ModuleID, wire, correctWire);
            Module.CauseStrike();
            ResetWires();
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The wire was cut when the timer's last digit was {1} instead of {2}.", ModuleID, currentTimeLastDigit, correctTimeLastDigit);
            Module.CauseStrike();
            ResetWires();
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Wires was successfully submitted.", ModuleID);
        IncrementQuestion();
    }

    void ResetWires()
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Resetting wires...", ModuleID);

        Info.GenerateWireInfo();
        Info.GenerateWireLEDInfo();
        Module.RegenWires();
    }

    void ValidateButton()
    {
        if (buttonPressCount != edgeworkAnswers[edgeworkQuestionPosition])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The “❖” button was pressed after the button was pressed {1} time(s) instead of {2} time(s).", ModuleID, buttonPressCount, edgeworkAnswers[edgeworkQuestionPosition]);
            Module.CauseStrike();
            buttonPressCount = 0;
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Button was successfully submitted.", ModuleID);
            IncrementQuestion();
        }
    }

    void ValidateLEDs()
    {
        string currentTime = Module.Bomb.GetFormattedTime();
        int currentTimeLastDigit = Convert.ToInt32(currentTime.Substring(currentTime.Length - 1));

        if (currentTimeLastDigit == 0 || currentTimeLastDigit == 9)
        {
            string submittedAnswer = GetCurrentLEDsSubmission();
            string correctAnswer = Convert.ToString(edgeworkAnswers[edgeworkQuestionPosition], 2).PadLeft(8, '0');
            if (submittedAnswer != correctAnswer)
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The number submitted was {1} instead of {2}.", ModuleID, submittedAnswer, correctAnswer);
                Module.CauseStrike();
            }
            else
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] The answer for LEDs was successfully submitted.", ModuleID);
                IncrementQuestion();
            }
            return;
        }
        else
        {
            currentTimeLastDigit--;
            if (Info.LED[currentTimeLastDigit] == (int)MainColors.Black)
            {
                Info.LED[currentTimeLastDigit] = (int)MainColors.White;
            }
            else
            {
                Info.LED[currentTimeLastDigit] = (int)MainColors.Black;
            }
            Module.SetLEDs();
        }
    }

    string GetCurrentLEDsSubmission()
    {
        string answer = String.Empty;

        foreach (var LED in Info.LED)
        {
            if (LED == (int)MainColors.Black)
            {
                answer += "0";
            }
            if (LED == (int)MainColors.White)
            {
                answer += "1";
            }
        }

        return answer;
    }

    void ValidateSymbols(int symbol)
    {
        int correctSymbol = edgeworkAnswers[edgeworkQuestionPosition] % 6;
        int correctTimeLastDigit = edgeworkAnswers[edgeworkQuestionPosition] % 10;
        string currentTime = Module.Bomb.GetFormattedTime();
        int currentTimeLastDigit = Convert.ToInt32(currentTime.Substring(currentTime.Length - 1));

        if (symbol != correctSymbol)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed instead of symbol {2}.", ModuleID, symbol, correctSymbol);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(true, symbol));
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The symbol was pressed when the timer's last digit was {1} instead of {2}.", ModuleID, currentTimeLastDigit, correctTimeLastDigit);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(true, symbol));
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Symbols was successfully submitted.", ModuleID);
        Module.Symbols[symbol].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[(int)KeyColors.Green];
        IncrementQuestion();
    }

    void ValidateAlphabet(int alphabet)
    {
        int correctAlphabet = edgeworkAnswers[edgeworkQuestionPosition] % 6;
        int correctTimeLastDigit = edgeworkAnswers[edgeworkQuestionPosition] % 10;
        string currentTime = Module.Bomb.GetFormattedTime();
        int currentTimeLastDigit = Convert.ToInt32(currentTime.Substring(currentTime.Length - 1));

        if (alphabet != correctAlphabet)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed instead of symbol {2}.", ModuleID, alphabet, correctAlphabet);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The alphanumeric key was pressed when the timer's last digit was {1} instead of {2}.", ModuleID, currentTimeLastDigit, correctTimeLastDigit);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Alphabet was successfully submitted.", ModuleID);
        Module.Alphabet[alphabet].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[(int)KeyColors.Green];
        IncrementQuestion();
    }

    void ValidatePiano(int piano)
    {
        int correctKey = edgeworkAnswers[edgeworkQuestionPosition] % 12;
        int correctTimeLastDigit = edgeworkAnswers[edgeworkQuestionPosition] % 10;
        string currentTime = Module.Bomb.GetFormattedTime();
        int currentTimeLastDigit = Convert.ToInt32(currentTime.Substring(currentTime.Length - 1));

        if (piano != correctKey)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Key {1} was pressed instead of key {2}.", ModuleID, piano, correctKey);
            Module.CauseStrike();
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The key was pressed when the timer's last digit was {1} instead of {2}.", ModuleID, currentTimeLastDigit, correctTimeLastDigit);
            Module.CauseStrike();
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Piano was successfully submitted.", ModuleID);
        IncrementQuestion();
    }

    void ValidateArrows(int arrow)
    {
        int correctAnswer = edgeworkAnswers[edgeworkQuestionPosition] % 10;
        int submittedAnswer = arrowNumbers[arrow];

        if (submittedAnswer != correctAnswer)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow was pressed for a second time, which submitted {2} instead of {3}.", ModuleID, ArrowDirectionNames[(ArrowDirections)arrow], submittedAnswer, correctAnswer);
            Module.CauseStrike();
            Debug.LogFormat("[The Cruel Modkit #{0}] Resetting arrow values...", ModuleID);

            SetArrowNumbers();
            for (int i = 0; i < arrowNumbersActivated.Length; i++)
            {
                arrowNumbersActivated[i] = false;
            }

            Debug.LogFormat("[The Cruel Modkit #{0}] The correct arrow direction is {1}.", ModuleID, ArrowDirectionNames[((ArrowDirections)Array.IndexOf(arrowNumbers, edgeworkAnswers[edgeworkQuestionPosition] % 10))]);
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Arrows was successfully submitted.", ModuleID);
            IncrementQuestion();
        }
    }

    void ValidateBulbs(int button)
    {
        string correctAnswer = Convert.ToString(edgeworkAnswers[edgeworkQuestionPosition], 2);

        if (Info.BulbOLeft)
        {
            if (button != Convert.ToInt32(correctAnswer.Substring(bulbButtonPressCount, 1)))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! You pressed the {1} button for digit {2}. Submission has been reset.", ModuleID, (button == 0) == Info.BulbOLeft ? "O" : "I", bulbButtonPressCount + 1);
                Module.CauseStrike();
                bulbButtonPressCount = 0;
                return;
            }
        }
        else
        {
            if ((1 - button) != Convert.ToInt32(correctAnswer.Substring(bulbButtonPressCount, 1)))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! You pressed the {1} button for digit {2}. Submission has been reset.", ModuleID, (button == 0) == Info.BulbOLeft ? "O" : "I", bulbButtonPressCount + 1);
                Module.CauseStrike();
                bulbButtonPressCount = 0;
                return;
            }
        }

        bulbButtonPressCount++;

        if (bulbButtonPressCount == correctAnswer.Length)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Bulbs was successfully submitted.", ModuleID);
            IncrementQuestion();
        }
    }
}
