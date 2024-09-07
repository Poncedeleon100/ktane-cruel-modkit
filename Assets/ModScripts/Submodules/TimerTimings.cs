using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;



public class TimerTimings : Puzzle
{

    readonly string[] RuleList =
    {
        "A + B = a prime number",
        "A > amount of lit indicators, B ≤ amount of unlit indicators",
        "A / B = a whole number",
        "A and B concatenated = a multiple of the module count, excluding needies",
        "The digital root of A + B is odd",
        "A or B matches a digit on the bomb timer",
        "B - Last digit of S# ≤ A",
        "B - A > the number of distinct ports modulo 10",
        "A + B ≥ sum of S# digits modulo 18",
        "A and B = the amount of lit or unlit indicators"
    };

    public TimerTimings(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Timer Timings. Press the ❖ button to activate the timer.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Number display is {1}. Correct rule to follow is: \"{2}.\"", ModuleID, Info.NumberDisplay, RuleList[Info.NumberDisplay]);
        switch (Info.NumberDisplay)
        {
            case 3:
                int ModuleCount = Module.Bomb.GetSolvableModuleIDs().Count();
                if (ModuleCount > 100)
                {
                    int SerialNumberParity = Module.Bomb.GetSerialNumberNumbers().Last() % 2;
                    Debug.LogFormat("[The Cruel Modkit #{0}] The number of solvable modules ({1}) is greater than 100. The new rule to follow is: \"A and B share parity with last digit of S#.\"", ModuleID, ModuleCount);
                    Debug.LogFormat("[The Cruel Modkit #{0}] The last digit of the serial number is {1} and has {2} parity.", ModuleID, Module.Bomb.GetSerialNumberNumbers().Last(), SerialNumberParity == 0 ? "even" : "odd");
                }
                else
                    Debug.LogFormat("[The Cruel Modkit #{0}] The number of solvable modules is {1}.", ModuleID, ModuleCount);
                break;
            case 7:
                Debug.LogFormat("[The Cruel Modkit #{0}] The number of distinct ports modulo 10 is {1}.", ModuleID, Module.Bomb.CountUniquePorts() % 10);
                break;
            case 8:
                Debug.LogFormat("[The Cruel Modkit #{0}] The sum of the serial number digits modulo 18 is {1}.", ModuleID, Module.Bomb.GetSerialNumberNumbers().Sum() % 18);
                break;
            case 1:
            case 9:
                Debug.LogFormat("[The Cruel Modkit #{0}] The number of lit indicators is {1}. The number of unlit indicators is {2}.", ModuleID, Module.Bomb.GetOnIndicators().Count(), Module.Bomb.GetOffIndicators().Count());
                break;
        }
    }

    bool IsTimerChanging;

    public override void OnUtilityPress()
    {
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

        if (!IsTimerChanging)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Pressed the ❖ button with the correct components. Activating the timer...", ModuleID);
            IsTimerChanging = true;
            Module.StartCoroutine(CycleTimerDisplay());
        }
        else
        {
            // A = leftmost digit, B = rightmost digit
            int A = Convert.ToInt32(Info.TimerDisplay.ToString().PadLeft(5, '0').Substring(0, 1));
            int B = Convert.ToInt32(Info.TimerDisplay.ToString().PadLeft(5, '0').Substring(4, 1));
            Debug.LogFormat("[The Cruel Modkit #{0}] The ❖ button was pressed when the timer display was {1}. A = {2} and B = {3}.", ModuleID, Info.TimerDisplay.ToString().PadLeft(5, '0'), A, B);
            switch (Info.NumberDisplay)
            {
                // A + B is a prime number
                case 0:
                    if (IsPrime(A + B))
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] A + B = {1}, which is a prime number. Module solved.", ModuleID, A + B);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A + B = {1}, which is not a prime number.", ModuleID, A + B);
                        Module.CauseStrike();
                    }
                    break;
                // A > amount of lit indicators, B ≤ amount of unlit indicators
                case 1:
                    bool AIndicators = A > Module.Bomb.GetOnIndicators().Count();
                    bool BIndicators = B <= Module.Bomb.GetOffIndicators().Count();
                    if (AIndicators && BIndicators)
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] A is greater than the amount of lit indicators and B is less than or equal to the amount of unlit indicators. Module solved.", ModuleID);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A is not greater than the amount of lit indicators and/or B is not less than or equal to the amount of unlit indicators.", ModuleID);
                        Module.CauseStrike();
                    }
                    break;
                // A / B = a whole number
                case 2:
                    if (((A / B) % 1) == 0)
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] A / B = {1}, which is a whole number. Module solved.", ModuleID, A / B);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A / B = {1}, which is not a whole number.", ModuleID, Math.Round(Convert.ToDouble(A / B), 2));
                        Module.CauseStrike();
                    }
                    break;
                // A and B = a multiple of the module count, excluding needies
                case 3:
                    int ModuleCount = Module.Bomb.GetSolvableModuleIDs().Count();
                    // 100+ modules exception: A and B share parity with last digit of S#
                    if (ModuleCount > 100)
                    {
                        int SerialNumberParity = Module.Bomb.GetSerialNumberNumbers().Last() % 2;
                        if ((SerialNumberParity == (A % 2)) && (SerialNumberParity == (B % 2)))
                        {
                            Debug.LogFormat("[The Cruel Modkit #{0}] Both A and B have {1} parity. Module solved.", ModuleID, SerialNumberParity == 0 ? "even" : "odd");
                            Module.Solve();
                            IsTimerChanging = false;
                        }
                        else
                        {
                            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A and/or B does not have {1} parity.", ModuleID, SerialNumberParity == 0 ? "even" : "odd");
                            Module.CauseStrike();
                        }
                    }
                    else
                    {
                        int ConcatenatedValue = int.Parse(A.ToString() + B.ToString());
                        bool Rule3 = ConcatenatedValue == 0 ? true : (ModuleCount % ConcatenatedValue) == 0;
                        if (Rule3)
                        {
                            Debug.LogFormat("[The Cruel Modkit #{0}] A and B concatenated is a multiple of the number of solvable modules. Module solved.", ModuleID);
                            Module.Solve();
                            IsTimerChanging = false;
                        }
                        else
                        {
                            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A and B concatenated is not a multiple of the number of solvable modules.", ModuleID);
                            Module.CauseStrike();
                        }
                    }
                    break;
                // The digital root of A + B is odd
                case 4:
                    if ((DigitalRoot(A + B) % 2) == 1)
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] A + B = {1}. The digital root is {2}, which is odd. Module solved.", ModuleID, A + B, DigitalRoot(A + B));
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A + B = {1}. The digital root is {2}, which is even.", ModuleID, A + B, DigitalRoot(A + B));
                        Module.CauseStrike();
                    }
                    break;
                // A or B matches a digit on the bomb timer
                case 5:
                    Debug.LogFormat("[The Cruel Modkit #{0}] The ❖ button was pressed when the bomb timer display was {1}.", ModuleID, Module.Bomb.GetFormattedTime());
                    if (Module.Bomb.GetFormattedTime().Contains(A.ToString()) || Module.Bomb.GetFormattedTime().Contains(B.ToString()))
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] The bomb timer display contains A or B. Module solved.", ModuleID);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The bomb timer display contains neither A nor B.", ModuleID);
                        Module.CauseStrike();
                    }
                    break;
                // B - Last digit of S# ≤ A
                case 6:
                    int Rule6Value = B - Module.Bomb.GetSerialNumberNumbers().Last();
                    if (Rule6Value <= A)
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] B - Last digit of serial number = {1}, which is less than or equal to A. Module solved.", ModuleID, Rule6Value);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! B - Last digit of serial number = {1}, which is not less than or equal to A.", ModuleID, Rule6Value);
                        Module.CauseStrike();
                    }
                    break;
                // B - A > the number of distinct ports modulo 10
                case 7:
                    if ((B - A) > (Module.Bomb.CountUniquePorts() % 10))
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] B - A = {1}, which is greater than the number of distinct ports modulo 10. Module solved.", ModuleID, B - A);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! B - A = {1}, is not greater than the number of distinct ports modulo 10.", ModuleID, B - A);
                        Module.CauseStrike();
                    }
                    break;
                // A + B ≥ sum of S# digits modulo 18
                case 8:
                    if ((A + B) >= (Module.Bomb.GetSerialNumberNumbers().Sum() % 18))
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] A + B = {1}, which is greater than or equal to the sum of the serial number digits modulo 18. Module solved.", ModuleID, A + B);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A + B = {1}, which is not greater than or equal to the sum of the serial number digits modulo 18.", ModuleID, A + B);
                        Module.CauseStrike();
                    }
                    break;
                // A and B = the amount of lit or unlit indicators
                case 9:
                    AIndicators = (A == (Module.Bomb.GetOffIndicators().Count()) || (A == (Module.Bomb.GetOnIndicators().Count())));
                    BIndicators = (B == (Module.Bomb.GetOffIndicators().Count()) || (B == (Module.Bomb.GetOnIndicators().Count())));
                    if (AIndicators && BIndicators)
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Both A and B is equal to the amount of lit or unlit indicators. Module solved.", ModuleID);
                        Module.Solve();
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A and/or B are not equal to the number of lit or unlit indicators.", ModuleID);
                        Module.CauseStrike();
                    }
                    break;
            }
        }

        return;
    }

    bool IsPrime(int Number)
    {
        if (Number <= 1)
            return false;

        for (int i = 2; i <= Math.Sqrt(Number); i++)
            if (Number % i == 0) return false;

        return true;

    }

    int DigitalRoot(int Number)
    {
        while (Number > 9)
        {
            Number = Number.ToString().ToCharArray().Sum(x => x - '0');
        }
        return Number;
    }

    public IEnumerator CycleTimerDisplay()
    {
        while (IsTimerChanging)
        {
            Info.TimerDisplay = Random.Range(0, 100000);
            Module.WidgetText[0].text = Info.TimerDisplay.ToString().PadLeft(5, '0');
            yield return new WaitForSeconds(1f);
        }
    }
}
