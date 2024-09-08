using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DerangedKeypad : Puzzle
{
    private readonly string[] startingAlphabets = new string[] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ETIANMSURWDKGOHVFLPJBXCYZQ",
            "GORIYSHQBFLPZATNKVCUJMDEXW",
            "ABCDEGKNPSTXZFHIJLMOQRUVWY",
            "SEQUFNCGTHRVIODJWKXYLPMZAB",
            "WBSMEJTUCPFAHZOQLIKNYVGXRD",
            "ADGJMPSVYBEHKNQTWZCFILORUX",
            "BMVFQZYSXJGIWHAEPRLNTKUDCO",
            "XQUMFEPOWLTJDZHGBVYKCRIASN",
            "QWERTYUIOPASDFGHJKLZXCVBNM",
            "AELFHBRVOTCYDQUXPWGNIMSKZJ"
        };

    private readonly List<int> pressedKeys = new List<int>();

    int shouldBePressed;

    bool buttonShouldBePressed = false;

    string alph;

    public DerangedKeypad(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Deranged Keypad.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] The alphabet keys are as follows: {1}.", ModuleID, Info.GetAlphabetInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] The button is {1}.", ModuleID, Info.GetButtonInfo());
        alph = startingAlphabets[Info.Button];
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting alphabet is {1}.", ModuleID, alph);
        alph = Modify(Info.ButtonText, alph);
        Debug.LogFormat("[The Cruel Modkit #{0}] The resulting alphabet is {1}.", ModuleID, alph);
        shouldBePressed = determinePress(alph, Info.Alphabet, pressedKeys);
    }

    private int determinePress(string alphabet, string[] keys, List<int> alreadyPressed)
    {
        foreach (char c in alphabet)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].Contains(c) && !alreadyPressed.Contains(i))
                {
                    Debug.LogFormat("[The Cruel Modkit #{0}] The first character that appears in a non-pressed key is {1}, which is in key {2}.", ModuleID, c, i + 1);
                    return i;
                }
            }
        }
        throw new InvalidOperationException("erm what the sigma");
    }

    public override void OnButtonPress()
    {
        if (Module.IsAnimating())
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

        if (pressedKeys.Count == 0)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Pressed the button when no keys have been pressed.", ModuleID);
            Module.CauseStrike();
            return;
        }

        if (buttonShouldBePressed)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Pressed the button after 2 alphabet key presses.", ModuleID);
            Info.ButtonText = ComponentInfo.ButtonList[UnityEngine.Random.Range(0, 14)];
            Module.Button.transform.Find("ButtonText").GetComponentInChildren<TextMesh>().text = Info.ButtonText;
            alph = Modify(Info.ButtonText, alph);
            Debug.LogFormat("[The Cruel Modkit #{0}] The resulting alphabet is {1}.", ModuleID, alph);
            shouldBePressed = determinePress(alph, Info.Alphabet, pressedKeys);
            buttonShouldBePressed = false;
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Pressed the button when a key was supposed to be pressed.", ModuleID);
            Module.CauseStrike();
            return;
        }

        if (!Module.IsSolving())
            Module.StartSolve();
    }

    public override void OnAlphabetPress(int Alphabet)
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        Module.Alphabet[Alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, Alphabet + 1, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            Module.ButtonStrike(false, Alphabet);
            return;
        }

        if (Module.Alphabet[Alphabet].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material == Module.KeyLightMats[2])
        {
            return;
        }

        if (Alphabet == shouldBePressed)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Correctly pressed the key labeled {1}.", ModuleID, Info.Alphabet[Alphabet].Replace('\n', ' '));
            Module.Alphabet[Alphabet].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            pressedKeys.Add(shouldBePressed);
            if (pressedKeys.Count % 2 == 0 && pressedKeys.Count < 6)
            {
                buttonShouldBePressed = true;
                return;
            }
            else if (pressedKeys.Count < 6)
            {
                shouldBePressed = determinePress(alph, Info.Alphabet, pressedKeys);
            }
            if (pressedKeys.Count == 6)
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] All alphabet keys have been pressed! Solved!", ModuleID);
                Module.Solve();
            }
        }
        else if (buttonShouldBePressed)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A key was pressed when the button was supposed to be pressed.", ModuleID);
            Module.CauseStrike();
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The key labeled {1} was pressed when the correct key was {2}.", ModuleID, Info.Alphabet[Alphabet].Replace('\n', ' '), Info.Alphabet[shouldBePressed].Replace('\n', ' '));
            Module.CauseStrike();
        }

        if (!Module.IsSolving())
            Module.StartSolve();
    }

    private string Modify(string buttonLabel, string alphabet)
    {
        switch (buttonLabel)
        {
            case "":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button label has no text, so the alphabet string is unchanged.", ModuleID);
                break;
            case "PRESS":
                if (alphabet[0] == Module.Bomb.GetSerialNumberLetters().ToArray()[0])
                {
                    Debug.LogFormat("[The Cruel Modkit #{0}] The button reads PRESS and the first character of the serial number is already at the beginning, so it will be moved to the end.", ModuleID);
                    alphabet = alphabet.Substring(1) + alphabet[0];
                }
                else
                {
                    Debug.LogFormat("[The Cruel Modkit #{0}] The button reads PRESS and the first character of the serial number is not already at the beginning, so it will be moved there.", ModuleID);
                    int firstLetterIndex = alphabet.IndexOf(Module.Bomb.GetSerialNumberLetters().ToArray()[0]);
                    if (firstLetterIndex == 25)
                    {
                        alphabet = alphabet[firstLetterIndex] + alphabet.Substring(0, firstLetterIndex);
                    }
                    else
                    {
                        alphabet = alphabet[firstLetterIndex] + alphabet.Substring(0, firstLetterIndex) + alphabet.Substring(firstLetterIndex + 1);
                    }
                }
                break;
            case "HOLD":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads HOLD, so both halves of the alphabet string will be swapped.", ModuleID);
                alphabet = alphabet.Substring(13) + alphabet.Substring(0, 13);
                break;
            case "DETONATE":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads DETONATE, so the alphabet string will be encrypted via the Atbash cipher.", ModuleID);
                alphabet = GetAtbash(alphabet);
                break;
            case "MASH":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads MASH, so the first consonant will be swapped with the last vowel.", ModuleID);
                char[] consonants = "BCDFGHJKLMNPQRSTVWXYZ".ToCharArray();
                char[] vowels = "AEIOU".ToCharArray();
                int firstConsonant = alphabet.IndexOfAny(consonants);
                int lastVowel = alphabet.LastIndexOfAny(vowels);
                alphabet = SwapChars(alphabet, firstConsonant, lastVowel);
                break;
            case "TAP":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads TAP, so the alphabet string will be Caesar-shifted forward by the sum of the digits in the Alphabet section.", ModuleID);
                string digits = "0123456789";
                int caesarOffset = 0;
                foreach (char key in Info.GetAlphabetInfo())
                {
                    if (digits.Contains(key))
                    {
                        caesarOffset += key - '0';
                    }
                }
                Debug.LogFormat("[The Cruel Modkit #{0}] The sum of all alphabet digits is {1}.", ModuleID, caesarOffset);
                alphabet = Caesar(alphabet, caesarOffset);
                break;
            case "PUSH":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads PUSH, so the letters A, B, C, D and E will be moved immediately after Q.", ModuleID);
                int[] ABCDE = new int[] { alphabet.IndexOf('A'), alphabet.IndexOf('B'), alphabet.IndexOf('C'), alphabet.IndexOf('D'), alphabet.IndexOf('E') };
                ABCDE = ABCDE.OrderByDescending(a => a).ToArray();
                foreach (int n in ABCDE)
                {
                    alphabet.Remove(n);
                }
                int Q = alphabet.IndexOf('Q');
                alphabet = alphabet.Insert(Q, "ABCDE");
                break;
            case "ABORT":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads ABORT, so the first letter in the alphabet string with an odd-numbered alphabetic position will be swapped with the last letter in the string with an even-numbered alphabetic position", ModuleID);
                string oddLetters = "ACEGIKMOQSUWY";
                string evenLetters = "BDFHJLNPRTVXZ";
                int firstOdd = alphabet.IndexOfAny(oddLetters.ToCharArray());
                int lastEven = alphabet.LastIndexOfAny(evenLetters.ToCharArray());
                alphabet = SwapChars(alphabet, firstOdd, lastEven);
                break;
            case "BUTTON":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads BUTTON, so the last character's alphabetic position will be multiplied by 5, moduloed by 26, have 1 added to it, and be moved to the beginning of the string.", ModuleID);
                string letterIndices = " ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // space at the beginning so A is 1
                int lastCharPosition = alphabet.IndexOf(letterIndices[(letterIndices.IndexOf(alphabet[25]) * 5) % 26 + 1]);
                if (lastCharPosition == 25)
                {
                    alphabet = alphabet[lastCharPosition] + alphabet.Substring(0, lastCharPosition);
                }
                else
                {
                    alphabet = alphabet[lastCharPosition] + alphabet.Substring(0, lastCharPosition) + alphabet.Substring(lastCharPosition + 1);
                }
                break;
            case "CLICK":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads CLICK, so the alphabet string will be encrypted into ROT13, or Caesar-shifted by 13.", ModuleID);
                alphabet = Caesar(alphabet, 13);
                break;
            case "NOTHING":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads NOTHING, so the letter that comes after the first letter alphabetically will be moved to the end of the alphabet string.", ModuleID);
                string nextLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZA"; // A at the end so i don't have to do wraparound shenanigans
                int indexOfLetter = alphabet.IndexOf(nextLetters[nextLetters.IndexOf(alphabet[0]) + 1]);
                string letter = alphabet[indexOfLetter].ToString();
                alphabet = alphabet.Remove(indexOfLetter, 1);
                alphabet = alphabet + letter;
                break;
            case "NO":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads NO, so the first half will be reversed.", ModuleID);
                alphabet = new string(alphabet.Substring(0, 13).Reverse().ToArray()) + alphabet.Substring(13);
                break;
            case "I DON'T KNOW":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads I DON'T KNOW, so the entire alphabet string will be reversed.", ModuleID);
                alphabet = new string(alphabet.Reverse().ToArray());
                break;
            case "YES":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads YES, so the second half will be reversed.", ModuleID);
                alphabet = alphabet.Substring(0, 13) + new string(alphabet.Substring(13).Reverse().ToArray());
                break;
        }
        return alphabet;
    }

    private string GetAtbash(string s)
    {
        var charArray = s.ToCharArray();

        for (int i = 0; i < charArray.Length; i++)
        {
            char c = charArray[i];

            if (c >= 'a' && c <= 'z')
            {
                charArray[i] = (char)(96 + (123 - c));
            }

            if (c >= 'A' && c <= 'Z')
            {
                charArray[i] = (char)(64 + (91 - c));
            }
        }

        return new String(charArray);
    }

    private string SwapChars(string str, int index1, int index2)
    {
        char[] strChar = str.ToCharArray();
        char temp = strChar[index1];
        strChar[index1] = strChar[index2];
        strChar[index2] = temp;

        return new String(strChar);
    }

    private string Caesar(string input, int key)
    {
        string output = "";

        foreach (char ch in input)
        {
            output += cipher(ch, key);
        }

        return output;
    }

    private char cipher(char ch, int key)
    {
        if (!char.IsLetter(ch))
        {
            return ch;
        }

        char d = char.IsUpper(ch) ? 'A' : 'a';
        return (char)((((ch + key) - d) % 26) + d);


    }

}
