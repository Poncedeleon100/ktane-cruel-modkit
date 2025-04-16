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

    public EdgeworkEncoding(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Edgework Encoding.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Widgets: Timer display is {1}. Number display is {2}.", ModuleID, Info.TimerDisplay.ToString().PadLeft(5, '0'), Info.NumberDisplay);
        GetActiveComponents();
        CalculateStartingPoints();
        LogActiveComponents();
        CalculateEdgeworkAnswers();
        ValidateQuestion();
    }

    readonly List<string> EdgeworkQuestions = new List<string>()
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
    readonly List<int> EdgeworkAnswers = new List<int>();

    readonly ComponentsEnum[] ComponentsArray = new ComponentsEnum[] 
        { ComponentsEnum.Wires, ComponentsEnum.Arrows, ComponentsEnum.Button, ComponentsEnum.Piano,
            ComponentsEnum.LED, ComponentsEnum.Symbols, ComponentsEnum.Bulbs, ComponentsEnum.Alphabet };
    readonly bool[] ComponentsActive = 
        { false, false, false, false, 
            false, false, false, false };
    int ComponentsActiveCount;
    int SolvedComponents;

    // Moving down either list means a positive increment
    int EdgeworkQuestionPosition;
    bool EdgeworkQuestionPositiveIncrement;
    int ComponentPosition;
    bool ComponentPositionPositiveIncrement;

    readonly int[] ArrowNumbers = new int[9];
    readonly bool[] ArrowNumbersActivated = { false, false, false, false, false, false, false, false, false };
    int ButtonPressCount = 0;
    bool LEDsStartSolve = false;
    int BulbButtonPressCount = 0;

    public override void OnWireCut(int Wire)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, Module.transform);
        Module.CutWire(Wire);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the component selection was [{2}] instead of [{3}].", ModuleID, Wire + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                ResetWires();

                return;
            }

            Module.StartSolve();
        }

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Wires)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut when the correct component was {1}.", ModuleID, Wire + 1, ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            ResetWires();

            return;
        }

        ValidateWires(Wire);
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

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Button)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the correct component was {1}.", ModuleID, ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        ButtonPressCount++;
    }

    public override void OnSymbolPress(int Symbol)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Symbols[Symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Symbol + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Module.StartCoroutine(Module.ButtonStrike(true, Symbol));
                return;
            }

            Module.StartSolve();
        }

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Symbols)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed when the correct component was {2}.", ModuleID, Symbol + 1, ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(true, Symbol));
            return;
        }

        ValidateSymbols(Symbol);
    }

    public override void OnAlphabetPress(int Alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Alphabet[Alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Alphabet + 1, Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
                return;
            }

            Module.StartSolve();
        }

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Alphabet)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the correct component was {2}.", ModuleID, Alphabet + 1, ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
            return;
        }

        ValidateAlphabet(Alphabet);
    }

    public override void OnPianoPress(int Piano)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlaySoundAtTransform(Module.PianoSounds[Piano + (Info.Piano * 12)].name, Module.transform);
        Module.Piano[Piano].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, PianoKeyNames[(PianoKeys)Piano], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Piano)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the correct component was {1}.", ModuleID, PianoKeyNames[(PianoKeys)Piano], ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        ValidatePiano(Piano);
    }

    public override void OnArrowPress(int Arrow)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Arrows[Arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, ArrowDirectionNames[(ArrowDirections)Arrow], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        Module.Audio.PlaySoundAtTransform(Module.ArrowSounds[Arrow].name, Module.transform);
        Module.StartCoroutine(HandleArrowFlash(Arrow));

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Arrows)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow button was pressed when the correct component was {1}.", ModuleID, ArrowDirectionNames[(ArrowDirections)Arrow], ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        Info.NumberDisplay = ArrowNumbers[Arrow];
        Module.SetNumber();

        if (ArrowNumbersActivated[Arrow] == true)
        {
            ValidateArrows(Arrow);
        }
        else
        {
            ArrowNumbersActivated[Arrow] = true;
        }
    }

    public override void OnBulbButtonPress(int Button)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.BulbButtons[Button].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents())
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, (Button == 0) == Info.BulbOLeft ? "O" : "I", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Bulbs)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} button was pressed when the correct component was {1}.", ModuleID, (Button == 0) == Info.BulbOLeft ? "O" : "I", ComponentsArray[ComponentPosition].ToString());
            Module.CauseStrike();
            return;
        }

        ValidateBulbs(Button);
    }

    public override void OnBulbInteract(int Bulb)
    {
        if (Module.IsAnimating())
            return;

        Module.HandleBulbScrew(Bulb, BulbScrewedIn[Bulb], Info.BulbOn[Bulb]);

        BulbScrewedIn[Bulb] = !BulbScrewedIn[Bulb];

        Module.Audio.PlaySoundAtTransform(Module.BulbSounds[BulbScrewedIn[Bulb] ? 0 : 1].name, Module.transform);
        Module.Bulbs[Bulb].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving())
        {
            if (!Module.CheckValidComponents() && !BulbScrewedIn[Bulb])
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the component selection was [{2}] instead of [{3}].", ModuleID, (Bulb + 1) == 1 ? "first" : "second", Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }

        if (ComponentsArray[ComponentPosition] != ComponentsEnum.Bulbs)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} bulb was removed when the correct component was {1}.", ModuleID, (Bulb + 1) == 1 ? "first" : "second", ComponentsArray[ComponentPosition].ToString());
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

        if (ComponentsArray[ComponentPosition] == ComponentsEnum.Button)
        {
            ValidateButton();
        }
        if (ComponentsArray[ComponentPosition] == ComponentsEnum.LED)
        {
            if (!LEDsStartSolve)
            {
                for (int i = 0; i < Info.LED.Length; i++)
                {
                    Info.LED[i] = Convert.ToInt32(MainColors.Black);
                }
                Module.SetLEDs();
                LEDsStartSolve = true;
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
                ComponentsActive[Array.IndexOf(ComponentsArray, Component)] = true;
            }
        }

        foreach (var ComponentActive in ComponentsActive)
        {
            if (ComponentActive)
                ComponentsActiveCount++;
        }
    }

    void CalculateStartingPoints()
    {
        // Last two digits of the timer display modulo 20; both positions are zero indexed here for convenience
        EdgeworkQuestionPosition = ((Info.TimerDisplay % 100) % 20) - 1;
        // Zeroes at this step become the max number for the specified value (20 here, 8 for ComponentPosition)
        if (EdgeworkQuestionPosition < 0)
            EdgeworkQuestionPosition = 19;
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting position for the \"Edgework Questions\" list is {1}: \"{2}\"", ModuleID, EdgeworkQuestionPosition + 1, EdgeworkQuestions[EdgeworkQuestionPosition]);

        // First digit of timer display is even
        EdgeworkQuestionPositiveIncrement = ((Convert.ToInt32(Info.TimerDisplay.ToString().Substring(0))) % 2) == 0;
        Debug.LogFormat("[The Cruel Modkit #{0}] The first digit on the timer display is {1}, so move {2} in the \"Edgework Questions\" list.", ModuleID, EdgeworkQuestionPositiveIncrement ? "even" : "odd", EdgeworkQuestionPositiveIncrement ? "down" : "up");


        // Number display modulo 8
        ComponentPosition = (Info.NumberDisplay % 8) - 1;
        if (ComponentPosition < 0)
            ComponentPosition = 7;
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting position for the \"Components\" list is {1}: \"{2}.\"", ModuleID, ComponentPosition + 1, ComponentsArray[ComponentPosition].ToString());

        // Number of components is even
        ComponentPositionPositiveIncrement = (ComponentsActiveCount % 2) == 0;
        Debug.LogFormat("[The Cruel Modkit #{0}] The number of active components is {1} which is {2}, so move {3} in the \"Components\" list.", ModuleID, ComponentsActiveCount, ComponentPositionPositiveIncrement ? "even" : "odd", ComponentPositionPositiveIncrement ? "down" : "up");
        // Second digit of timer display is even (used once Edgework Encoding becomes a normal module)
        //ComponentPositionPositiveIncrement = ((Convert.ToInt32(Info.TimerDisplay.ToString().Substring(1))) % 2) == 0;
        //Debug.LogFormat("[The Cruel Modkit #{0}] The second digit on the timer display is {1}, so move {2} in the \"Components\" list.", ModuleID, ComponentPositionPositiveIncrement ? "even" : "odd", ComponentPositionPositiveIncrement ? "down" : "up");
    }

    void LogActiveComponents()
    {
        foreach (ComponentsEnum Component in ComponentsArray)
        {
            if (ComponentsActive[Array.IndexOf(ComponentsArray, Component)])
            {
                switch (Component)
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
        IEnumerable<char> SerialNumberLetters = Module.Bomb.GetSerialNumberLetters();
        IEnumerable<int> SerialNumberNumbers = Module.Bomb.GetSerialNumberNumbers();
        int DBatteryCount = Module.Bomb.GetBatteryCount(Battery.D);

        // Does the serial number contain letters from any of the present indicators?
        EdgeworkAnswers.Add(Module.Bomb.GetIndicators().Join().Intersect(SerialNumberLetters).Any() ? 1 : 0);
        
        // Is there a vowel in the serial number?
        EdgeworkAnswers.Add(SerialNumberLetters.Join().Intersect("AEIOU").Any() ? 1 : 0);
        
        // How many D batteries are present?
        EdgeworkAnswers.Add(DBatteryCount);

        // Does the serial number contain any digits present in the total number of ports?
        EdgeworkAnswers.Add(SerialNumberNumbers.Join().Intersect((Module.Bomb.GetPortCount()).ToString()).Any() ? 1 : 0);
        
        // Is there a CLR, FRQ, SIG, or NSA indicator present?
        bool CLR = Module.Bomb.IsIndicatorPresent(Indicator.CLR);
        bool FRQ = Module.Bomb.IsIndicatorPresent(Indicator.FRQ);
        bool SIG = Module.Bomb.IsIndicatorPresent(Indicator.SIG);
        bool NSA = Module.Bomb.IsIndicatorPresent(Indicator.NSA);

        EdgeworkAnswers.Add((CLR || FRQ || SIG || NSA) ? 1 : 0);

        // What is the alphanumeric position of the second letter in the serial number?
        EdgeworkAnswers.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList().IndexOf(SerialNumberLetters.ToList()[1]) + 1);

        // Does the serial number contain any digits present in the number of D batteries?
        EdgeworkAnswers.Add(SerialNumberNumbers.Join().Intersect(DBatteryCount.ToString()).Any() ? 1 : 0);

        // How many PS/2 and Serial ports are present?
        EdgeworkAnswers.Add(Module.Bomb.GetPortCount(Port.PS2) + Module.Bomb.GetPortCount(Port.Serial));

        // How many unlit indicators are present?
        EdgeworkAnswers.Add(Module.Bomb.GetOffIndicators().Count());

        // Does the serial number contain any digits present in the calculated puzzle ID?
        EdgeworkAnswers.Add(SerialNumberNumbers.Join().Intersect(ComponentsArray.ToString()).Any() ? 1 : 0);

        // How many AA batteries are present?
        EdgeworkAnswers.Add(Module.Bomb.GetBatteryCount(Battery.AA));

        // How many Parallel and RJ - 45 ports are present?
        EdgeworkAnswers.Add(Module.Bomb.GetPortCount(Port.Parallel) + Module.Bomb.GetPortCount(Port.RJ45));
        
        // Does the serial number contain any digits present in the total number of indicators?
        EdgeworkAnswers.Add(SerialNumberNumbers.Join().Intersect(Module.Bomb.GetIndicators().Count().ToString()).Any() ? 1 : 0);

        // Does the serial number contain any digits present in the total number of modules?
        EdgeworkAnswers.Add(SerialNumberNumbers.Join().Intersect(Module.Bomb.GetModuleNames().Count().ToString()).Any() ? 1 : 0);

        // How many battery holders are present?
        EdgeworkAnswers.Add(Module.Bomb.GetBatteryHolderCount());

        // Is there an empty port plate present?
        EdgeworkAnswers.Add(Module.Bomb.GetPortPlates().Any(plate => plate.Length == 0) ? 1 : 0);

        // How many lit indicators are present?
        EdgeworkAnswers.Add(Module.Bomb.GetOnIndicators().Count());

        // What is the sum of the digits present in the serial number?
        EdgeworkAnswers.Add(SerialNumberNumbers.Sum());

        // Does the serial number contain any digits present in the number of AA batteries?
        EdgeworkAnswers.Add(Module.Bomb.GetSerialNumberNumbers().Join().Intersect(Module.Bomb.GetBatteryCount(Battery.AA).ToString()).Any() ? 1 : 0);

        // How many Stereo RCA and DVI - D ports are present?
        EdgeworkAnswers.Add(Module.Bomb.GetPortCount(Port.StereoRCA) + Module.Bomb.GetPortCount(Port.DVI));
    }

    void LogCurrentQuestion()
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] The current question is: \"{1}\" The answer is {2} and it must be submitted on the {3} component.", ModuleID, EdgeworkQuestions[EdgeworkQuestionPosition], EdgeworkAnswers[EdgeworkQuestionPosition], ComponentsArray[ComponentPosition].ToString());
        LogCurrentSolution();
    }

    void LogCurrentSolution()
    {
        switch (ComponentsArray[ComponentPosition])
        {
            case ComponentsEnum.Wires:
                Debug.LogFormat("[The Cruel Modkit #{0}] Cut wire {1} when the last digit of the timer is {2}.", ModuleID, (EdgeworkAnswers[EdgeworkQuestionPosition] % 7) + 1, EdgeworkAnswers[EdgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.Arrows:
                SetArrowNumbers();
                Debug.LogFormat("[The Cruel Modkit #{0}] The correct arrow direction is the {1} arrow.", ModuleID, ArrowDirectionNames[((ArrowDirections)Array.IndexOf(ArrowNumbers, EdgeworkAnswers[EdgeworkQuestionPosition] % 10))]);
                break;
            case ComponentsEnum.Button:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press the button {1} time(s), then press the “❖” button.", ModuleID, EdgeworkAnswers[EdgeworkQuestionPosition]);
                break;
            case ComponentsEnum.Piano:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press key {1} when the last digit of the timer is {2}.", ModuleID, (EdgeworkAnswers[EdgeworkQuestionPosition] % 12) + 1, EdgeworkAnswers[EdgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.LED:
                Debug.LogFormat("[The Cruel Modkit #{0}] Submit {1} using the LEDs.", ModuleID, Convert.ToString(EdgeworkAnswers[EdgeworkQuestionPosition], 2).PadLeft(8, '0'));
                break;
            case ComponentsEnum.Symbols:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press symbol {1} when the last digit of the timer is {2}.", ModuleID, (EdgeworkAnswers[EdgeworkQuestionPosition] % 6) + 1, EdgeworkAnswers[EdgeworkQuestionPosition] % 10);
                break;
            case ComponentsEnum.Bulbs:
                Debug.LogFormat("[The Cruel Modkit #{0}] Submit {1} using the buttons on Bulbs.", ModuleID, Convert.ToString(EdgeworkAnswers[EdgeworkQuestionPosition], 2));
                break;
            case ComponentsEnum.Alphabet:
                Debug.LogFormat("[The Cruel Modkit #{0}] Press key {1} when the last digit of the timer is {2}.", ModuleID, (EdgeworkAnswers[EdgeworkQuestionPosition] % 6) + 1, EdgeworkAnswers[EdgeworkQuestionPosition] % 10);
                break;
        }
    }

    void SetArrowNumbers()
    {
        var TempArrowNumbers = "1234567890".ToCharArray().Select(x => Convert.ToInt32(x) - '0').OrderBy(x => Random.Range(0, 1000)).ToList();
        TempArrowNumbers.Remove(EdgeworkAnswers[EdgeworkQuestionPosition] % 10);
        int CorrectArrow = Random.Range(0, 9);

        for (int i = 0; i < ArrowNumbers.Length; i++)
        {
            if (CorrectArrow == i)
            {
                ArrowNumbers[i] = EdgeworkAnswers[EdgeworkQuestionPosition] % 10;
            }
            else
            {
                ArrowNumbers[i] = TempArrowNumbers[i];
            }
        }
    }

    void ValidateQuestion()
    {
        if (!ComponentsActive[ComponentPosition])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The {1} component is not active on the module; skipping to the next question.", ModuleID, ComponentsArray[ComponentPosition].ToString());
            IncrementQuestion();
        }
        else
        {
            LogCurrentQuestion();
        }
    }

    void IncrementQuestion()
    {
        SolvedComponents++;
        if (SolvedComponents == 8)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] All answers submitted successfully. Module solved.", ModuleID);
            Module.Solve();
            return;
        }

        int TotalComponents = ComponentsArray.Length - 1;
        int TotalQuestions = EdgeworkQuestions.Count() - 1;

        ComponentPosition += ComponentPositionPositiveIncrement ? 1 : -1;
        if (ComponentPosition < 0)
            ComponentPosition = TotalComponents;
        if (ComponentPosition > TotalComponents)
            ComponentPosition = 0;

        EdgeworkQuestionPosition += EdgeworkQuestionPositiveIncrement ? 1 : -1;
        if (EdgeworkQuestionPosition < 0)
            EdgeworkQuestionPosition = TotalQuestions;
        if (EdgeworkQuestionPosition > TotalQuestions)
            EdgeworkQuestionPosition = 0;

        ValidateQuestion();
    }

    void ValidateWires(int Wire)
    {
        int CorrectWire = EdgeworkAnswers[EdgeworkQuestionPosition] % 7;
        int CorrectTimeLastDigit = EdgeworkAnswers[EdgeworkQuestionPosition] % 10;
        string CurrentTime = Module.Bomb.GetFormattedTime();
        int CurrentTimeLastDigit = Convert.ToInt32(CurrentTime.Substring(CurrentTime.Length - 1));

        if (Wire != CorrectWire)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Wire {1} was cut instead of wire {2}.", ModuleID, Wire + 1, CorrectWire + 1);
            Module.CauseStrike();
            ResetWires();
            return;
        }

        if (CurrentTimeLastDigit != CorrectTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The wire was cut when the timer's last digit was {1} instead of {2}.", ModuleID, CurrentTimeLastDigit, CorrectTimeLastDigit);
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

        if (ComponentsArray[ComponentPosition] == ComponentsEnum.Wires)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Cut wire {1} when the last digit of the timer is {2}.", ModuleID, (EdgeworkAnswers[EdgeworkQuestionPosition] % 7) + 1, EdgeworkAnswers[EdgeworkQuestionPosition] % 10);
        }
    }

    void ValidateButton()
    {
        if (ButtonPressCount != EdgeworkAnswers[EdgeworkQuestionPosition])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The “❖” button was pressed after the button was pressed {1} time(s) instead of {2} time(s).", ModuleID, ButtonPressCount, EdgeworkAnswers[EdgeworkQuestionPosition]);
            Module.CauseStrike();
            ButtonPressCount = 0;
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Button was successfully submitted.", ModuleID);
            IncrementQuestion();
        }
    }

    void ValidateLEDs()
    {
        string CurrentTime = Module.Bomb.GetFormattedTime();
        int CurrentTimeLastDigit = Convert.ToInt32(CurrentTime.Substring(CurrentTime.Length - 1));
        string SubmittedAnswer = GetCurrentLEDsSubmission();
        string CorrectAnswer = Convert.ToString(EdgeworkAnswers[EdgeworkQuestionPosition], 2).PadLeft(8, '0');

        if (CurrentTimeLastDigit == 0 || CurrentTimeLastDigit == 9)
        {
            if (SubmittedAnswer != CorrectAnswer)
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The number submitted was {1} instead of {2}.", ModuleID, SubmittedAnswer, CorrectAnswer);
                Module.CauseStrike();
                return;
            }
            else
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] The answer for LEDs was successfully submitted.", ModuleID);
                IncrementQuestion();
                return;
            }
        }
        else
        {
            CurrentTimeLastDigit--;
            if (Info.LED[CurrentTimeLastDigit] == Convert.ToInt32(MainColors.Black))
            {
                Info.LED[CurrentTimeLastDigit] = Convert.ToInt32(MainColors.White);
                Module.SetLEDs();
            }
            else
            {
                Info.LED[CurrentTimeLastDigit] = Convert.ToInt32(MainColors.Black);
                Module.SetLEDs();
            }
        }
    }

    string GetCurrentLEDsSubmission()
    {
        string Answer = String.Empty;

        foreach (var LED in Info.LED)
        {
            if (LED == Convert.ToInt32(MainColors.Black))
            {
                Answer += "0";
            }
            if (LED == Convert.ToInt32(MainColors.White))
            {
                Answer += "1";
            }
        }

        return Answer;
    }

    void ValidateSymbols(int Symbol)
    {
        int CorrectSymbol = EdgeworkAnswers[EdgeworkQuestionPosition] % 6;
        int CorrectTimeLastDigit = EdgeworkAnswers[EdgeworkQuestionPosition] % 10;
        string CurrentTime = Module.Bomb.GetFormattedTime();
        int CurrentTimeLastDigit = Convert.ToInt32(CurrentTime.Substring(CurrentTime.Length - 1));

        if (Symbol != CorrectSymbol)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Symbol {1} was pressed instead of symbol {2}.", ModuleID, Symbol + 1, CorrectSymbol + 1);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(true, Symbol));
            return;
        }

        if (CurrentTimeLastDigit != CorrectTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The symbol was pressed when the timer's last digit was {1} instead of {2}.", ModuleID, CurrentTimeLastDigit, CorrectTimeLastDigit);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(true, Symbol));
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Symbols was successfully submitted.", ModuleID);
        Module.Symbols[Symbol].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[(int)KeyColors.Green];
        IncrementQuestion();
    }

    void ValidateAlphabet(int Alphabet)
    {
        int CorrectAlphabet = EdgeworkAnswers[EdgeworkQuestionPosition] % 6;
        int CorrectTimeLastDigit = EdgeworkAnswers[EdgeworkQuestionPosition] % 10;
        string CurrentTime = Module.Bomb.GetFormattedTime();
        int CurrentTimeLastDigit = Convert.ToInt32(CurrentTime.Substring(CurrentTime.Length - 1));

        if (Alphabet != CorrectAlphabet)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed instead of symbol {2}.", ModuleID, Alphabet + 1, CorrectAlphabet + 1);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
            return;
        }

        if (CurrentTimeLastDigit != CorrectTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The alphanumeric key was pressed when the timer's last digit was {1} instead of {2}.", ModuleID, CurrentTimeLastDigit, CorrectTimeLastDigit);
            Module.CauseStrike();
            Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Alphabet was successfully submitted.", ModuleID);
        Module.Alphabet[Alphabet].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[(int)KeyColors.Green];
        IncrementQuestion();
    }

    void ValidatePiano(int Piano)
    {
        int CorrectKey = EdgeworkAnswers[EdgeworkQuestionPosition] % 12;
        int CorrectTimeLastDigit = EdgeworkAnswers[EdgeworkQuestionPosition] % 10;
        string CurrentTime = Module.Bomb.GetFormattedTime();
        int CurrentTimeLastDigit = Convert.ToInt32(CurrentTime.Substring(CurrentTime.Length - 1));

        if (Piano != CorrectKey)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Key {1} was pressed instead of key {2}.", ModuleID, Piano + 1, CorrectKey + 1);
            Module.CauseStrike();
            return;
        }

        if (CurrentTimeLastDigit != CorrectTimeLastDigit)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The key was pressed when the timer's last digit was {1} instead of {2}.", ModuleID, CurrentTimeLastDigit, CorrectTimeLastDigit);
            Module.CauseStrike();
            return;
        }

        Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Piano was successfully submitted.", ModuleID);
        IncrementQuestion();
    }

    void ValidateArrows(int Arrow)
    {
        int CorrectAnswer = EdgeworkAnswers[EdgeworkQuestionPosition] % 10;
        int SubmittedAnswer = ArrowNumbers[Arrow];

        if (SubmittedAnswer != CorrectAnswer)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} arrow was pressed for a second time, which submitted {2} instead of {3}.", ModuleID, ArrowDirectionNames[(ArrowDirections)Arrow], SubmittedAnswer, CorrectAnswer);
            Module.CauseStrike();
            Debug.LogFormat("[The Cruel Modkit #{0}] Resetting arrow values...", ModuleID);

            SetArrowNumbers();
            for (int i = 0; i < ArrowNumbersActivated.Length; i++)
            {
                ArrowNumbersActivated[i] = false;
            }

            Debug.LogFormat("[The Cruel Modkit #{0}] The correct arrow direction is {1}.", ModuleID, ArrowDirectionNames[((ArrowDirections)Array.IndexOf(ArrowNumbers, EdgeworkAnswers[EdgeworkQuestionPosition] % 10))]);
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Arrows was successfully submitted.", ModuleID);
            IncrementQuestion();
        }
    }

    void ValidateBulbs(int Button)
    {
        string CorrectAnswer = Convert.ToString(EdgeworkAnswers[EdgeworkQuestionPosition], 2);

        if (Info.BulbOLeft)
        {
            if (Button != Convert.ToInt32(CorrectAnswer.Substring(BulbButtonPressCount, 1)))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! You pressed the {1} button for digit {2}. Submission has been reset.", ModuleID, (Button == 0) == Info.BulbOLeft ? "O" : "I", BulbButtonPressCount + 1);
                Module.CauseStrike();
                BulbButtonPressCount = 0;
                return;
            }
        }
        else
        {
            if ((1 - Button) != Convert.ToInt32(CorrectAnswer.Substring(BulbButtonPressCount, 1)))
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! You pressed the {1} button for digit {2}. Submission has been reset.", ModuleID, (Button == 0) == Info.BulbOLeft ? "O" : "I", BulbButtonPressCount + 1);
                Module.CauseStrike();
                BulbButtonPressCount = 0;
                return;
            }
        }

        BulbButtonPressCount++;

        if (BulbButtonPressCount == CorrectAnswer.Length)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The answer for Bulbs was successfully submitted.", ModuleID);
            IncrementQuestion();
        }
    }
}
