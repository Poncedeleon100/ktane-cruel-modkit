using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor.PackageManager;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;
using static CruelModkitScript;
using Random = UnityEngine.Random;

public class EdgeworkEncoding : Puzzle
{

    public EdgeworkEncoding(CruelModkitScript module, ComponentInfo info, byte components) : base(module, info, components)
    {
        module.Log("Solving Edgework Encoding.");
        Module.Log("Widgets: Timer display is {0}. Number display is {1}.", info.TimerDisplay.ToString().PadLeft(5, '0'), info.NumberDisplay);
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

        Module.Play(Module.transform, Sound.WireSnip);
        Module.CutWire(wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Wire {0} was cut when the component selection was [{1}] instead of [{2}].", wire + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                ResetWires();

                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Wires)
        {
            Module.Strike("Strike! Wire {0} was cut when the correct component was {0}.", wire + 1, componentsArray[componentPosition].ToString());
            ResetWires();

            return;
        }

        ValidateWires(wire);
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

        if (componentsArray[componentPosition] != ComponentsEnum.Button)
        {
            Module.Log("Strike! The button was pressed when the correct component was {0}.", componentsArray[componentPosition].ToString());
            return;
        }

        buttonPressCount++;
    }

    public override void OnSymbolPress(int symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Symbols[symbol].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Symbol {0} was pressed when the component selection was [{1}] instead of [{2}].", symbol + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                Module.StartCoroutine(Module.ButtonStrike(true, symbol));
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Symbols)
        {
            Module.Strike("Strike! Symbol {0} was pressed when the correct component was {1}.", symbol + 1, componentsArray[componentPosition].ToString());
            Module.StartCoroutine(Module.ButtonStrike(true, symbol));
            return;
        }

        ValidateSymbols(symbol);
    }

    public override void OnAlphabetPress(int alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Alphabet[alphabet].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! Alphanumeric key {0} was pressed when the component selection was [{1}] instead of [{2}].", alphabet + 1, Module.GetEnabledComponents(), Module.GetTargetComponents());
                Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Alphabet)
        {
            Module.Strike("Strike! Alphanumeric key {0} was pressed when the correct component was {1}.", alphabet + 1, componentsArray[componentPosition].ToString());
            Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
            return;
        }

        ValidateAlphabet(alphabet);
    }

    public override void OnPianoPress(int piano)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Piano[piano].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.PianoSounds[piano + (Info.Piano * 12)]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} key on the piano was pressed when the component selection was [{1}] instead of [{2}].", PianoKeyNames[(PianoKeys)piano], Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Piano)
        {
            Module.Strike("Strike! The {0} key on the piano was pressed when the correct component was {0}.", PianoKeyNames[(PianoKeys)piano], componentsArray[componentPosition].ToString());
            return;
        }

        ValidatePiano(piano);
    }

    public override void OnArrowPress(int arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Shake(Module.Arrows[arrow].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} arrow button was pressed when the component selection was [{1}] instead of [{2}].", ArrowDirectionNames[(ArrowDirections)arrow], Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        Module.Play(Module.transform, Module.ArrowSounds[arrow].name);
        Module.StartCoroutine(HandleArrowFlash(arrow));

        if (componentsArray[componentPosition] != ComponentsEnum.Arrows)
        {
            Module.Strike("Strike! The {0} arrow button was pressed when the correct component was {0}.", ArrowDirectionNames[(ArrowDirections)arrow], componentsArray[componentPosition].ToString());
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

        Module.Shake(Module.BulbButtons[button].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.ButtonPress);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Module.Strike("Strike! The {0} button was pressed when the component selection was [{1}] instead of [{2}].", (button == 0) == Info.BulbOLeft ? "O" : "I", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Bulbs)
        {
            Module.Strike("Strike! The {0} button was pressed when the correct component was {0}.", (button == 0) == Info.BulbOLeft ? "O" : "I", componentsArray[componentPosition].ToString());
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

        Module.Shake(Module.Bulbs[bulb].GetComponentInChildren<KMSelectable>(), 0.25f, Sound.FromObject(Module.BulbSounds[BulbScrewedIn[bulb] ? 0 : 1]));

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents() && !BulbScrewedIn[bulb])
            {
                Module.Strike("Strike! The {0} bulb was removed when the component selection was [{1}] instead of [{2}].", (bulb + 1) == 1 ? "first" : "second", Module.GetEnabledComponents(), Module.GetTargetComponents());
                return;
            }

            Module.StartSolve();
        }

        if (componentsArray[componentPosition] != ComponentsEnum.Bulbs)
        {
            Module.Strike("Strike! The {0} bulb was removed when the correct component was {0}.", (bulb + 1) == 1 ? "first" : "second", componentsArray[componentPosition].ToString());
            return;
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
        Module.Log("The starting position for the \"Edgework Questions\" list is {0}: \"{1}\"", edgeworkQuestionPosition + 1, edgeworkQuestions[edgeworkQuestionPosition]);

        // First digit of timer display is even
        edgeworkQuestionPositiveIncrement = ((Convert.ToInt32(Info.TimerDisplay.ToString().PadLeft(5, '0').Substring(0, 1))) % 2) == 0;
        Module.Log("The first digit on the timer display is {0}, so move {1} in the \"Edgework Questions\" list.", edgeworkQuestionPositiveIncrement ? "even" : "odd", edgeworkQuestionPositiveIncrement ? "down" : "up");


        // Number display modulo 8
        componentPosition = (Info.NumberDisplay % 8) - 1;
        if (componentPosition < 0)
            componentPosition = 7;
        Module.Log("The starting position for the \"Components\" list is {0}: \"{1}.\"", componentPosition + 1, componentsArray[componentPosition].ToString());

        // Number of components is even
        componentPositionPositiveIncrement = (componentsActiveCount % 2) == 0;
        Module.Log("The number of active components is {0} which is {1}, so move {2} in the \"Components\" list.", componentsActiveCount, componentPositionPositiveIncrement ? "even" : "odd", componentPositionPositiveIncrement ? "down" : "up");
        // Second digit of timer display is even (used once Edgework Encoding becomes a normal module)
        //ComponentPositionPositiveIncrement = ((Convert.ToInt32(Info.TimerDisplay.ToString().Substring(1))) % 2) == 0;
        //Module.Log("The second digit on the timer display is {0}, so move {1} in the \"Components\" list.", ComponentPositionPositiveIncrement ? "even" : "odd", ComponentPositionPositiveIncrement ? "down" : "up");
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
                        Module.Log("Wires present: {0}.", Info.GetWireInfo());
                        Module.Log("Wire LEDs present: {0}.", Info.GetWireLEDInfo());
                        break;
                    case ComponentsEnum.Button:
                        Module.Log("Button is {0}.", Info.GetButtonInfo());
                        break;
                    case ComponentsEnum.LED:
                        Module.Log("LEDs present: {0}.", Info.GetLEDInfo());
                        break;
                    case ComponentsEnum.Symbols:
                        Module.Log("Symbols present: {0}.", Info.GetSymbolInfo());
                        break;
                    case ComponentsEnum.Alphabet:
                        Module.Log("Alphanumeric keys present: {0}.", Info.GetAlphabetInfo());
                        break;
                    case ComponentsEnum.Arrows:
                        Module.Log("Arrows present: {0}.", Info.GetArrowsInfo());
                        break;
                    case ComponentsEnum.Bulbs:
                        Module.Log("Bulb 1 is {0}, {1}, and {2}. Bulb 2 is {3}, {4}, and {5}. The O button is on the {6}.", Enum.GetName(typeof(BulbColorNames), Info.BulbColors[0]), Info.BulbOpaque[0] ? "opaque" : "see-through", Info.BulbOn[0] ? "on" : "off", Enum.GetName(typeof(BulbColorNames), Info.BulbColors[1]), Info.BulbOpaque[1] ? "opaque" : "see-through", Info.BulbOn[1] ? "on" : "off", Info.BulbOLeft ? "left" : "right");
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
        Module.Log("The current question is: \"{0}\" The answer is {1} and it must be submitted on the {2} component.", edgeworkQuestions[edgeworkQuestionPosition], edgeworkAnswers[edgeworkQuestionPosition], componentsArray[componentPosition].ToString());
        LogCurrentSolution();
    }

    void LogCurrentSolution()
    {
        switch (componentsArray[componentPosition])
        {
            case ComponentsEnum.Wires:
                Module.Log("Cut wire {0} when the last digit of the timer is {1}.", (edgeworkAnswers[edgeworkQuestionPosition] % 7), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.Arrows:
                SetArrowNumbers();
                Module.Log("The correct arrow direction is the {0} arrow.", ArrowDirectionNames[((ArrowDirections)Array.IndexOf(arrowNumbers, edgeworkAnswers[edgeworkQuestionPosition] % 10))]);
                break;
            case ComponentsEnum.Button:
                Module.Log("Press the button {0} time(s), then press the “❖” button.", edgeworkAnswers[edgeworkQuestionPosition]);
                break;
            case ComponentsEnum.Piano:
                Module.Log("Press key {0} when the last digit of the timer is {1}.", (edgeworkAnswers[edgeworkQuestionPosition] % 12), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.LED:
                Module.Log("Submit {0} using the LEDs.", Convert.ToString(edgeworkAnswers[edgeworkQuestionPosition], 2).PadLeft(8, '0'));
                break;
            case ComponentsEnum.Symbols:
                Module.Log("Press symbol {0} when the last digit of the timer is {1}.", (edgeworkAnswers[edgeworkQuestionPosition] % 6), edgeworkAnswers[edgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.Bulbs:
                Module.Log("Submit {0} using the buttons on Bulbs.", Convert.ToString(edgeworkAnswers[edgeworkQuestionPosition], 2));
                break;
            case ComponentsEnum.Alphabet:
                Module.Log("Press key {0} when the last digit of the timer is {1}.", (edgeworkAnswers[edgeworkQuestionPosition] % 6), edgeworkAnswers[edgeworkQuestionPosition] % 10);
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
            Module.Log("The {0} component is not active on the module; skipping to the next question.", componentsArray[componentPosition].ToString());
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
            Module.SolveModule("All answers submitted successfully. Module solved.");
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
            Module.Strike("Strike! Wire {0} was cut instead of wire {1}.", wire, correctWire);
            ResetWires();
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Module.Strike("Strike! The wire was cut when the timer's last digit was {0} instead of {1}.", currentTimeLastDigit, correctTimeLastDigit);
            ResetWires();
            return;
        }

        Module.Log("The answer for Wires was successfully submitted.");
        IncrementQuestion();
    }

    void ResetWires()
    {
        Module.Log("Resetting wires...");

        Info.GenerateWireInfo();
        Info.GenerateWireLEDInfo();
        Module.RegenWires();
    }

    void ValidateButton()
    {
        if (buttonPressCount != edgeworkAnswers[edgeworkQuestionPosition])
        {
            Module.Strike("Strike! The “❖” button was pressed after the button was pressed {0} time(s) instead of {1} time(s).", buttonPressCount, edgeworkAnswers[edgeworkQuestionPosition]);
            buttonPressCount = 0;
        }
        else
        {
            Module.Log("The answer for Button was successfully submitted.");
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
                Module.Strike("Strike! The number submitted was {0} instead of {1}.", submittedAnswer, correctAnswer);
            }
            else
            {
                Module.Log("The answer for LEDs was successfully submitted.");
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
            Module.Strike("Strike! Symbol {0} was pressed instead of symbol {1}.", symbol, correctSymbol);
            Module.StartCoroutine(Module.ButtonStrike(true, symbol));
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Module.Strike("Strike! The symbol was pressed when the timer's last digit was {0} instead of {1}.", currentTimeLastDigit, correctTimeLastDigit);
            Module.StartCoroutine(Module.ButtonStrike(true, symbol));
            return;
        }

        Module.Log("The answer for Symbols was successfully submitted.");
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
            Module.Strike("Strike! Alphanumeric key {0} was pressed instead of symbol {1}.", alphabet, correctAlphabet);
            Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Module.Strike("Strike! The alphanumeric key was pressed when the timer's last digit was {0} instead of {1}.", currentTimeLastDigit, correctTimeLastDigit);
            Module.StartCoroutine(Module.ButtonStrike(false, alphabet));
            return;
        }

        Module.Log("The answer for Alphabet was successfully submitted.");
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
            Module.Strike("Strike! Key {0} was pressed instead of key {1}.", piano, correctKey);
            return;
        }

        if (currentTimeLastDigit != correctTimeLastDigit)
        {
            Module.Strike("Strike! The key was pressed when the timer's last digit was {0} instead of {1}.", currentTimeLastDigit, correctTimeLastDigit);
            return;
        }

        Module.Log("The answer for Piano was successfully submitted.");
        IncrementQuestion();
    }

    void ValidateArrows(int arrow)
    {
        int correctAnswer = edgeworkAnswers[edgeworkQuestionPosition] % 10;
        int submittedAnswer = arrowNumbers[arrow];

        if (submittedAnswer != correctAnswer)
        {
            Module.Strike("Strike! The {0} arrow was pressed for a second time, which submitted {1} instead of {2}.", ArrowDirectionNames[(ArrowDirections)arrow], submittedAnswer, correctAnswer);
            Module.Log("Resetting arrow values...");

            SetArrowNumbers();
            for (int i = 0; i < arrowNumbersActivated.Length; i++)
            {
                arrowNumbersActivated[i] = false;
            }

            Module.Log("The correct arrow direction is {0}.", ArrowDirectionNames[((ArrowDirections)Array.IndexOf(arrowNumbers, edgeworkAnswers[edgeworkQuestionPosition] % 10))]);
        }
        else
        {
            Module.Log("The answer for Arrows was successfully submitted.");
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
                Module.Strike("Strike! You pressed the {0} button for digit {1}. Submission has been reset.", (button == 0) == Info.BulbOLeft ? "O" : "I", bulbButtonPressCount + 1);
                bulbButtonPressCount = 0;
                return;
            }
        }
        else
        {
            if ((1 - button) != Convert.ToInt32(correctAnswer.Substring(bulbButtonPressCount, 1)))
            {
                Module.Strike("Strike! You pressed the {0} button for digit {1}. Submission has been reset.", (button == 0) == Info.BulbOLeft ? "O" : "I", bulbButtonPressCount + 1);
                bulbButtonPressCount = 0;
                return;
            }
        }

        bulbButtonPressCount++;

        if (bulbButtonPressCount == correctAnswer.Length)
        {
            Module.Log("The answer for Bulbs was successfully submitted.");
            IncrementQuestion();
        }
    }
}
