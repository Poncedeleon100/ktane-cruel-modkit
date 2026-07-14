using KModkit;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using wawa.Modules;

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

    public TimerTimings(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Timer Timings. Press the ❖ button to activate the timer.");
        Module.Log("Number display is {0}. Correct rule to follow is: \"{1}.\"", Info.NumberDisplay, RuleList[Info.NumberDisplay]);
        switch (Info.NumberDisplay)
        {
            case 3:
                int ModuleCount = Module.Bomb.GetSolvableModuleIDs().Count();
                if (ModuleCount > 100)
                {
                    int SerialNumberParity = Module.Bomb.GetSerialNumberNumbers().Last() % 2;
                    Module.Log("The number of solvable modules ({0}) is greater than 100. The new rule to follow is: \"A and B share parity with last digit of S#.\"", ModuleCount);
                    Module.Log("The last digit of the serial number is {0} and has {1} parity.", Module.Bomb.GetSerialNumberNumbers().Last(), SerialNumberParity == 0 ? "even" : "odd");
                }
                else
                    Module.Log("The number of solvable modules is {0}.", ModuleCount);
                break;
            case 7:
                Module.Log("The number of distinct ports modulo 10 is {0}.", Module.Bomb.CountUniquePorts() % 10);
                break;
            case 8:
                Module.Log("The sum of the serial number digits modulo 18 is {0}.", Module.Bomb.GetSerialNumberNumbers().Sum() % 18);
                break;
            case 1:
            case 9:
                Module.Log("The number of lit indicators is {0}. The number of unlit indicators is {1}.", Module.Bomb.GetOnIndicators().Count(), Module.Bomb.GetOffIndicators().Count());
                break;
        }
    }

    bool IsTimerChanging;

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

        if (!IsTimerChanging)
        {
            Module.Log("Pressed the ❖ button with the correct components. Activating the timer...");
            IsTimerChanging = true;
            Module.StartCoroutine(CycleTimerDisplay());
        }
        else
        {
            // A = leftmost digit, B = rightmost digit
            int A = Convert.ToInt32(Info.TimerDisplay.ToString().PadLeft(5, '0').Substring(0, 1));
            int B = Convert.ToInt32(Info.TimerDisplay.ToString().PadLeft(5, '0').Substring(4, 1));
            Module.Log("The ❖ button was pressed when the timer display was {0}. A = {1} and B = {2}.", Info.TimerDisplay.ToString().PadLeft(5, '0'), A, B);
            switch (Info.NumberDisplay)
            {
                // A + B is a prime number
                case 0:
                    if (IsPrime(A + B))
                    {
                        Module.SolveModule("A + B = {0}, which is a prime number. Module solved.", A + B);
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! A + B = {0}, which is not a prime number.", A + B);
                    }
                    break;
                // A > amount of lit indicators, B ≤ amount of unlit indicators
                case 1:
                    bool AIndicators = A > Module.Bomb.GetOnIndicators().Count();
                    bool BIndicators = B <= Module.Bomb.GetOffIndicators().Count();
                    if (AIndicators && BIndicators)
                    {
                        Module.SolveModule("A is greater than the amount of lit indicators and B is less than or equal to the amount of unlit indicators. Module solved.");
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! A is not greater than the amount of lit indicators and/or B is not less than or equal to the amount of unlit indicators.");
                    }
                    break;
                // A / B = a whole number
                case 2:
                    if (((A / B) % 1) == 0)
                    {
                        Module.SolveModule("A / B = {0}, which is a whole number. Module solved.", A / B);
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! A / B = {0}, which is not a whole number.", Math.Round(Convert.ToDouble(A / B), 2));
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
                            Module.SolveModule("Both A and B have {0} parity. Module solved.", SerialNumberParity == 0 ? "even" : "odd");
                            IsTimerChanging = false;
                        }
                        else
                        {
                            Module.Strike("Strike! A and/or B does not have {0} parity.", SerialNumberParity == 0 ? "even" : "odd");
                        }
                    }
                    else
                    {
                        int ConcatenatedValue = int.Parse(A.ToString() + B.ToString());
                        bool Rule3 = ConcatenatedValue == 0 || (ModuleCount % ConcatenatedValue) == 0;
                        if (Rule3)
                        {
                            Module.SolveModule("A and B concatenated is a multiple of the number of solvable modules. Module solved.");
                            IsTimerChanging = false;
                        }
                        else
                        {
                            Module.Strike("Strike! A and B concatenated is not a multiple of the number of solvable modules.");
                        }
                    }
                    break;
                // The digital root of A + B is odd
                case 4:
                    if ((DigitalRoot(A + B) % 2) == 1)
                    {
                        Module.SolveModule("A + B = {0}. The digital root is {1}, which is odd. Module solved.", A + B, DigitalRoot(A + B));
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! A + B = {0}. The digital root is {1}, which is even.", A + B, DigitalRoot(A + B));
                    }
                    break;
                // A or B matches a digit on the bomb timer
                case 5:
                    Module.Log("The ❖ button was pressed when the bomb timer display was {0}.", Module.Bomb.GetFormattedTime());
                    if (Module.Bomb.GetFormattedTime().Contains(A.ToString()) || Module.Bomb.GetFormattedTime().Contains(B.ToString()))
                    {
                        Module.SolveModule("The bomb timer display contains A or B. Module solved.");
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! The bomb timer display contains neither A nor B.");
                    }
                    break;
                // B - Last digit of S# ≤ A
                case 6:
                    int Rule6Value = B - Module.Bomb.GetSerialNumberNumbers().Last();
                    if (Rule6Value <= A)
                    {
                        Module.SolveModule("B - Last digit of serial number = {0}, which is less than or equal to A. Module solved.", Rule6Value);
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! B - Last digit of serial number = {0}, which is not less than or equal to A.", Rule6Value);
                    }
                    break;
                // B - A > the number of distinct ports modulo 10
                case 7:
                    if ((B - A) > (Module.Bomb.CountUniquePorts() % 10))
                    {
                        Module.SolveModule("B - A = {0}, which is greater than the number of distinct ports modulo 10. Module solved.", B - A);
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! B - A = {0}, is not greater than the number of distinct ports modulo 10.", B - A);
                    }
                    break;
                // A + B ≥ sum of S# digits modulo 18
                case 8:
                    if ((A + B) >= (Module.Bomb.GetSerialNumberNumbers().Sum() % 18))
                    {
                        Module.SolveModule("A + B = {0}, which is greater than or equal to the sum of the serial number digits modulo 18. Module solved.", A + B);
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! A + B = {0}, which is not greater than or equal to the sum of the serial number digits modulo 18.", A + B);
                    }
                    break;
                // A and B = the amount of lit or unlit indicators
                case 9:
                    AIndicators = (A == (Module.Bomb.GetOffIndicators().Count()) || (A == (Module.Bomb.GetOnIndicators().Count())));
                    BIndicators = (B == (Module.Bomb.GetOffIndicators().Count()) || (B == (Module.Bomb.GetOnIndicators().Count())));
                    if (AIndicators && BIndicators)
                    {
                        Module.SolveModule("Both A and B is equal to the amount of lit or unlit indicators. Module solved.");
                        IsTimerChanging = false;
                    }
                    else
                    {
                        Module.Strike("Strike! A and/or B are not equal to the number of lit or unlit indicators.");
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
            Info.GenerateTimerInfo();
            Module.SetTimer();
            yield return new WaitForSeconds(1f);
        }
    }
}
