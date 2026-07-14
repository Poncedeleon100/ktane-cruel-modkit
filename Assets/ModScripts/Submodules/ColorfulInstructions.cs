using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;
using static UnityEditor.Graphs.Styles;
using Random = UnityEngine.Random;

public class ColorfulInstructions : Puzzle
{
    readonly bool _isBatteryCountEven;
    List<int> _validWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    List<int> _uncutWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
    readonly List<int> _usedLEDs = new List<int>();
    readonly bool _allBlackLEDs = false;
    int _ruleLoop;

    public ColorfulInstructions(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Colorful Instructions.");
        Module.Log("Wires present: {0}.", Info.GetWireInfo());
        RemoveBlackLEDs(_validWires);
        if (_validWires.Count == 0)
        {
            _allBlackLEDs = true;
            Module.Log("Wire LEDs present: {0}.", Info.GetWireLEDInfo());
            Module.Log("All LEDs are black. Press the ❖ button to solve the module.");
        }
        else
        {
            _isBatteryCountEven = (Module.Bomb.GetBatteryCount() % 2) == 0;
            GeneratePuzzle();
            _validWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            RemoveBlackLEDs(_validWires);
            Module.Log("Wire LEDs present: {0}.", Info.GetWireLEDInfo());
            if (_validWires.Count < 7)
            {
                Module.Log("There is at least one black LED present. Do not cut these wire(s) under any circumstances.");
            }
            Module.Log("The battery count is {0}, so blank LEDs will have a {1} star by default.", _isBatteryCountEven ? "even" : "odd", _isBatteryCountEven ? "white" : "black");
            Module.Log("Start by cutting any wire.");
        }
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

                Module.Log("Resetting wires...");
                ResetModule();

                return;
            }

            Module.StartSolve();
        }

        _uncutWires.Remove(Wire);

        if (!_validWires.Contains(Wire))
        {
            Module.Strike("Strike! Wire {0} was cut which was not a valid wire.", Wire + 1);

            Module.Log("Resetting wires...");
            ResetModule();
            return;
        }

        List<int> uncutWires = new List<int>();
        uncutWires.AddRange(_uncutWires);
        RemoveBlackLEDs(uncutWires);
        if (uncutWires.Count == 0)
        {
            Module.SolveModule("All wires were cut. Module solved.");
            return;
        }

        _validWires.Clear();
        Module.Log("Wire {0} was cut. The current LED color is {1}.", Wire + 1, (MainColors)(Info.WireLED[Wire] % 11));
        _validWires.AddRange(FindValidWires(Wire));
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

        if (_allBlackLEDs)
        {
            Module.SolveModule("The ❖ button was pressed. Module solved.");
            return;
        }

        Module.Log("The ❖ button was pressed, resetting wires.");
        ResetModule();
    }

    void GeneratePuzzle()
    {
        Debug.LogFormat("Right at the start: Wire LEDs present: {0}.", Info.GetWireLEDInfo());
        // Keeps track of all of the wires we cut (and haven't cut)
        List<int> cutWireOrder = new List<int>();
        List<int> uncutWires = new List<int>();
        // Keeps track of how many LEDs we use in each step so we can easily undo any step
        List<int> usedLEDOffsets = new List<int>();
        // Select a random wire to start with
        List<int> startingValidWires = new List<int>();
        List<int> randomWire = _validWires.OrderBy(x => Random.Range(0, 1000)).ToList();
        int startingWire = 0;
        foreach (int wire in randomWire)
        {
            startingWire = wire;
            _uncutWires.Remove(startingWire);
            startingValidWires = FindValidWires(startingWire, false);
            if (startingValidWires.Count > 0)
            {
                break;
            }
            _usedLEDs.Clear();
            _uncutWires.Add(startingWire);
        }
        Debug.LogFormat($"Starting with wire {startingWire + 1}");
        cutWireOrder.Add(startingWire);
        usedLEDOffsets.Add(_usedLEDs.Count);

        // Break out early if we have six black LEDs
        uncutWires.AddRange(_uncutWires);
        RemoveBlackLEDs(uncutWires);
        if (uncutWires.Count == 0)
        {
            Debug.LogFormat("The first valid wire was the ONLY valid wire. We're done here.");
            _uncutWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            _usedLEDs.Clear();
            return;
        }
        uncutWires.Clear();

        // Choose a random valid wire from the first wire
        int currentWire = startingValidWires[Random.Range(0, startingValidWires.Count())];
        int currentWireStar = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[currentWire] / 11))));
        
        // For use in the loop below
        // Starting with the second wire
        int i = 1;
        bool onlyValidWire = false;
        // 0 = Not applicable for the current iteration
        // 1 = A white/green/black LED has been selected
        // 2 = We reached a deadend after being unable to change the current LED color
        // 3 = We have returned to the first green or black LED color
        int whiteLEDError = 0;
        int greenBlackLEDError = 0;

        // If this LED color only has one valid wire, then we may select the black LED as the next color
        onlyValidWire = startingValidWires.Count == 0;

        while (i < 7)
        {
            Debug.LogFormat("Current setup: Wire LEDs present: {0}.", Info.GetWireLEDInfo());
            Debug.LogFormat($"Current wire {currentWire + 1}");

            // Break out early if the rest of the wires are black LEDs
            cutWireOrder.Add(currentWire);
            _uncutWires.Remove(currentWire);
            uncutWires.AddRange(_uncutWires);
            RemoveBlackLEDs(uncutWires);
            if (uncutWires.Count == 0)
            {
                Debug.LogFormat("The last wire we cut was the last valid wire.");
                break;
            }
            Debug.LogFormat($"Valid wires remaining: {uncutWires.Select(x => x + 1).Join(", ")}");
            uncutWires.Clear();

            // Create a list of lists of all possible valid wires with each LED color
            List<List<int>> allValidWires = new List<List<int>>();
            List<int> colorIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            // Store the usedLEDs so we can change them later
            List<List<int>> allUsedLEDs = new List<List<int>>();
            List<int> backupUsedLEDs = new List<int>();
            backupUsedLEDs.AddRange(_usedLEDs);

            // We can only use the black LED as a valid LED color if there is no other possible choice
            if (!onlyValidWire)
            {
                colorIndices.Remove((int)MainColors.Black);
            }

            // If the last LED color was white, we cannot change the current LED color or a softlock occurs
            // We will only check the valid wires for the current LED color
            if (Info.WireLED[cutWireOrder[i - 1]] % 11 == (int)MainColors.White)
            {
                Debug.LogFormat($"The last LED color was white, so we cannot change the current LED color.");
                whiteLEDError = 1;
                colorIndices.Clear();
                colorIndices.Add(Info.WireLED[currentWire]);
            }
            // We only want to do this if the previous LED was not white
            else if (whiteLEDError == 2)
            {
                Debug.LogFormat($"We can now change the white LED.");
                colorIndices.Remove((int)MainColors.White);
                whiteLEDError = 0;
            }

            // If we reached a deadend ahead, we need to keep backtracking until we get back to the green/black LED
            if (greenBlackLEDError == 2)
            {
                if (Info.WireLED[currentWire] % 11 == (int)MainColors.Green || Info.WireLED[currentWire] % 11 == (int)MainColors.Black)
                {
                    Debug.LogFormat($"We will keep backtracking to the first green/black LED.");
                }
                else
                {
                    Debug.LogFormat($"We can now change the green/black LED.");
                    colorIndices.Remove((int)MainColors.Green);
                    greenBlackLEDError = 0;
                }
            }
            // If the current LED was used in a rule previously, we cannot change it or a softlock occurs
            // We will only check the valid wires for the current LED color
            else if (_usedLEDs.Contains(currentWire))
            {
                Debug.LogFormat($"We've reached a previously used LED, so we cannot change the current LED color.");
                colorIndices.Clear();
                colorIndices.Add(Info.WireLED[currentWire]);
            }

            // Iterate through every valid color and collect every potential valid wire
            int y = 0;
            foreach (int ledColor in colorIndices)
            {
                _usedLEDs.Clear();
                _usedLEDs.AddRange(backupUsedLEDs);
                Info.WireLED[currentWire] = ledColor + (currentWireStar * 11);
                allValidWires.Add(FindValidWires(currentWire, false));
                allUsedLEDs.Add(new List<int>(_usedLEDs));
                y++;
            }

            // Remove empty lists since there are no valid wires for these colors - unless it's the last wire
            if (i != 6)
            {
                for (int j = allValidWires.Count - 1; j > -1; j--)
                {
                    if (allValidWires[j].Count == 0)
                    {
                        colorIndices.RemoveAt(j);
                        allUsedLEDs.RemoveAt(j);
                    }
                }
            }
            allValidWires.RemoveAll(x => x.Count == 0);

            // If we have no valid wires here, we need to go back
            // Also if we are currently backtracking for a green/black LED
            if (colorIndices.Count == 0 || greenBlackLEDError == 2)
            {
                if (whiteLEDError == 1)
                {
                    Debug.LogFormat($"The current LED color sucks, I am deleting this LED color. (But I'm white)");
                    // When we go back through to the next iteration, we don't want to select the white LED again
                    whiteLEDError = 2;
                }
                if (greenBlackLEDError == 1)
                {
                    Debug.LogFormat($"The current LED color sucks, I am deleting this LED color. (But I'm green/black)");
                    // When we go back through to the next iteration, we don't want to select the green/black LED again
                    greenBlackLEDError = 2;
                }

                _uncutWires.Add(currentWire);
                currentWire = cutWireOrder[i - 1];
                cutWireOrder.Remove(currentWire);

                _usedLEDs.Clear();
                _usedLEDs.AddRange(backupUsedLEDs);
                _usedLEDs.RemoveRange(_usedLEDs.Count - (usedLEDOffsets[i - 1] + 1), usedLEDOffsets[i - 1]);
                usedLEDOffsets.RemoveAt(i - 1);

                // There's no way to verify this anymore, so it will be false
                onlyValidWire = false;

                i--;
                continue;
            }

            // This is just for logging
            List<string> colorNames = new List<string>();
            foreach (int j in colorIndices)
            {
                colorNames.Add(Enum.GetName(typeof(MainColors), j));
            }
            Debug.LogFormat($"Here's what we have: {colorNames.Join(", ")}");

            // Select a random list and set its index as currentWire's new LED color
            int validColorIndex = Random.Range(0, colorIndices.Count);
            int convertedColorIndex = colorIndices[validColorIndex];
            Debug.LogFormat($"Let's go with {Enum.GetName(typeof(MainColors), colorIndices[validColorIndex])}");
            Info.WireLED[currentWire] = convertedColorIndex + (currentWireStar * 11);
            // If this LED color only has one valid wire, then we may select the black LED as the next color
            onlyValidWire = allValidWires[validColorIndex].Count == 1;
            // If this LED color is green/black, then we cannot change the LED it refers to
            if (convertedColorIndex == (int)MainColors.Green || convertedColorIndex == (int)MainColors.Black)
            {
                greenBlackLEDError = 1;
            }
            // Select a random wire from the chosen list and set it as our new currentWire - unless it's the last wire
            if (i != 6)
            {
                currentWire = allValidWires[validColorIndex][Random.Range(0, allValidWires[validColorIndex].Count)];
                currentWireStar = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[currentWire] / 11))));
                _usedLEDs.Clear();
                _usedLEDs.AddRange(allUsedLEDs[validColorIndex]);
                usedLEDOffsets.Add(_usedLEDs.Count - backupUsedLEDs.Count);
            }
            Debug.LogFormat($"Choosing wire {currentWire + 1}");
            i++;
        }

        Debug.LogFormat($"Cut the wires in this order please: {cutWireOrder.Select(x => x + 1).Join(", ")}");

        Module.SetWireLEDs();
        _uncutWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        _usedLEDs.Clear();
    }

    List<int> FindValidWires(int wire, bool logWires=true)
    {
        _ruleLoop++;
        if (_ruleLoop > 5)
        {
            if (logWires)
            {
                _ruleLoop = 0;
                Module.Log("A recursive rule was used, resetting the module.");
                ResetModule();
                return new List<int>();
            }
            else
            {
                _ruleLoop = 0;
                return new List<int>();
            }
        }
        List<int> currentValidWires = new List<int>();
        int color = Info.WireLED[wire] % 11;
        int star = Convert.ToInt32(Math.Floor(Convert.ToDecimal((Info.WireLED[wire] / 11))));
        if (star == 0)
        {
            if (_isBatteryCountEven)
            {
                star = 2;
            }
            else
            {
                star = 1;
            }
        }
        bool isWhiteStar = star == 2;
        _usedLEDs.Add(wire);

        switch (Enum.GetName(typeof(MainColors), color))
        {
            case "Red":
                int validRedWire = wire + (isWhiteStar ? 1 : -1);
                if (validRedWire < 0)
                {
                    validRedWire = 6;
                }
                if (validRedWire > 6)
                {
                    validRedWire = 0;
                }
                if (logWires)
                {
                    Module.Log("Current Rule: Cut the wire to the {0} of this wire.", isWhiteStar ? "right" : "left");
                }
                if (_uncutWires.Contains(validRedWire))
                {
                    currentValidWires.Add(validRedWire);
                    if (logWires)
                    {
                        Module.Log("The valid wire to cut is wire {0}.", validRedWire + 1);
                    }
                }
                else if (logWires)
                {
                    Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                }
                break;
            case "Orange":
                int keyOrangeLED = wire + (isWhiteStar ? -1 : 1);
                if (keyOrangeLED < 0)
                {
                    keyOrangeLED = 6;
                }
                if (keyOrangeLED > 6)
                {
                    keyOrangeLED = 0;
                }
                keyOrangeLED = Info.WireLED[keyOrangeLED] % 11;

                foreach (int i in _uncutWires)
                {
                    if (keyOrangeLED == Info.WireLED[i] % 11)
                    {
                        currentValidWires.Add(i);
                    }
                }

                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire with an LED matching the LED to the {0}.", isWhiteStar ? "left" : "right");
                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("Cut any wire with a {0} LED. The valid wire(s) to cut are: {1}.", Enum.GetName(typeof(MainColors), keyOrangeLED), currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }
                break;
            case "Yellow":
                int yellowStar = isWhiteStar ? 1 : 2;
                foreach (int i in _uncutWires)
                {
                    int yellowStarConvert = Info.WireLED[i] / 11;
                    if (yellowStarConvert == 0)
                    {
                        if (_isBatteryCountEven)
                        {
                            yellowStarConvert = 2;
                        }
                        else
                        {
                            yellowStarConvert = 1;
                        }
                    }
                    if (yellowStar == yellowStarConvert)
                    {
                        currentValidWires.Add(i);
                    }
                }
                currentValidWires.Remove(wire);

                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire with a {0} star.", isWhiteStar ? "black" : "white");

                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }
                break;
            case "Lime":
                if (wire == 6 || wire == 0)
                {
                    currentValidWires.AddRange(_uncutWires.Where(x => x != wire));
                }
                else if (isWhiteStar)
                {
                    currentValidWires.AddRange(_uncutWires.Where(x => x < wire));
                }
                else
                {
                    currentValidWires.AddRange(_uncutWires.Where(x => x > wire));
                }

                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire to the {0} of this wire.", isWhiteStar ? "left" : "right");

                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }
                break;
            case "Green":
                if (!_uncutWires.Contains(wire))
                {
                    int greenLED = CalculateGreenLED(wire, isWhiteStar);

                    if (logWires)
                    {
                        Module.Log("Current Rule: Cut this wire and refer to the LED to the {0}.", isWhiteStar ? "right" : "left");
                        Module.Log("The current LED color is {0}.", (MainColors)(Info.WireLED[greenLED] % 11));
                    }

                    currentValidWires.Clear();
                    currentValidWires.AddRange(FindValidWires(greenLED, logWires));
                }
                else
                {
                    currentValidWires.Add(wire);
                }
                break;
            case "Cyan":
                foreach (int i in _uncutWires)
                {
                    // The positions are zero-indexed, so the even/odd calculation should be reversed.
                    if (i % 2 == (isWhiteStar ? 1 : 0))
                    {
                        currentValidWires.Add(i);
                    }
                }
                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire at an {0} position.", isWhiteStar ? "even" : "odd");

                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }
                break;
            case "Blue":
                foreach (int i in _uncutWires)
                {
                    bool ledUsed = _usedLEDs.Contains(i);

                    if (isWhiteStar == ledUsed)
                    {
                        currentValidWires.Add(i);
                    }
                }

                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire where you have {0} its LED.", isWhiteStar ? "used" : "not used");

                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }
                break;
            case "Purple":
                List<int> serialNumbers = Module.Bomb.GetSerialNumberNumbers().ToList();
                int purpleValidWire = 0;

                if (isWhiteStar)
                {
                    purpleValidWire = serialNumbers.Where(x => x < 8 && x > 0).FirstOrDefault();
                }
                else
                {
                    purpleValidWire = serialNumbers.Where(x => x < 8 && x > 0).LastOrDefault();
                }

                if (logWires)
                {
                    Module.Log("Current Rule: Cut the wire with the same number as the {0} applicable digit in the SN.", isWhiteStar ? "first" : "last");
                }

                if (_uncutWires.Contains(purpleValidWire - 1))
                {
                    currentValidWires.Add(purpleValidWire - 1);
                    if (logWires)
                    {
                        Module.Log("The valid wire to cut is wire {0}.", purpleValidWire);
                    }
                    break;
                }

                currentValidWires.AddRange(_uncutWires);

                if (logWires)
                {
                    if (purpleValidWire == 0)
                    {
                        Module.Log("There are no applicable digits in the SN. Cut any wire.");
                    }
                    else
                    {
                        Module.Log("Wire {0} was already cut. Cut any wire.", purpleValidWire);
                    }
                    Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                }

                break;
            case "Pink":
                if (isWhiteStar)
                {
                    currentValidWires.AddRange(_uncutWires.Where(x => x < 3));
                }
                else
                {
                    currentValidWires.AddRange(_uncutWires.Where(x => x > 3));
                }

                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire with the number {0}.", isWhiteStar ? "1-3" : "5-7");

                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }
                break;
            case "Black":
                int blackLED = CalculateBlackLED(wire, isWhiteStar);

                if (logWires)
                {
                    Module.Log("Current Rule: Do not cut this wire and refer to the LED to the {0}.", isWhiteStar ? "right" : "left");
                    Module.Log("The current LED color is {0}.", (MainColors)(Info.WireLED[blackLED] % 11));
                }

                currentValidWires.Clear();
                currentValidWires.AddRange(FindValidWires(blackLED, logWires));

                break;
            case "White":
                int whiteLED;

                if (logWires)
                {
                    Module.Log("Current Rule: Cut any wire with the color of the {0} LED used.", isWhiteStar ? "last" : "left-most");
                }

                if (isWhiteStar)
                {
                    whiteLED = Info.WireLED[_usedLEDs.Last()] % 11;
                    
                    if (logWires)
                    {
                        Module.Log("The last LED color used was {0}.", (MainColors)whiteLED);
                    }
                }
                else
                {
                    whiteLED = Info.WireLED[_usedLEDs.OrderBy(x => x).First()] % 11;

                    if (logWires)
                    {
                        Module.Log("The leftmost LED color used was {0}.", (MainColors)whiteLED);
                    }
                }

                foreach (int i in _uncutWires)
                {
                    if (whiteLED == Info.WireLED[i] % 11)
                    {
                        currentValidWires.Add(i);
                    }
                }

                if (logWires)
                {
                    if (currentValidWires.Count == 0)
                    {
                        Module.Log("There are no valid wires to cut. Press the ❖ button to reset the module.");
                    }
                    else
                    {
                        Module.Log("The valid wire(s) to cut are: {0}.", currentValidWires.Select(x => x + 1).Join(", "));
                    }
                }

                break;
        }

        List<int> updatedValidWires = new List<int>();
        updatedValidWires.AddRange(currentValidWires);
        RemoveBlackLEDs(updatedValidWires);
        if (currentValidWires.Count != 0 && updatedValidWires.Count == 0)
        {
            int validBlackWire = currentValidWires.First();
            if (logWires)
            {
                Module.Log("The only remaining wire(s) have black LEDs. Use the leftmost wire with a black LED, which is wire {0}.", validBlackWire + 1);
            }
            currentValidWires.Clear();
            currentValidWires.AddRange(FindValidWires(validBlackWire, logWires));
        }
        else
        {
            RemoveBlackLEDs(currentValidWires);
        }
        _ruleLoop = 0;
        return currentValidWires;
    }

    int CalculateGreenLED(int wire, bool isWhiteStar)
    {
        int greenLED = wire + (isWhiteStar ? 1 : -1);
        if (greenLED < 0)
        {
            greenLED = 6;
        }
        if (greenLED > 6)
        {
            greenLED = 0;
        }

        return greenLED;
    }

    int CalculateBlackLED(int wire, bool isWhiteStar)
    {
        int blackLED = wire + (isWhiteStar ? 1 : -1);
        if (blackLED < 0)
        {
            blackLED = 6;
        }
        if (blackLED > 6)
        {
            blackLED = 0;
        }

        return blackLED;
    }

    void RemoveBlackLEDs(List<int> input)
    {
        input.RemoveAll(wire => (Info.WireLED[wire] % 11) == (int)MainColors.Black);
    }

    void ResetModule()
    {
        Module.RegenWires();
        _uncutWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        _usedLEDs.Clear();
        _validWires = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        RemoveBlackLEDs(_validWires);
    }
}
