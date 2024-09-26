using KModkit;
using System;
using System.Collections;
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

    private void UpdateAlphAndShould()
    {
        alph = Modify();
        Debug.LogFormat("[The Cruel Modkit #{0}] The resulting alphabet is {1}.", ModuleID, alph);
        shouldBePressed = DeterminePress();
    }

    public DerangedKeypad(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Deranged Keypad.", ModuleID);
        Debug.LogFormat("[The Cruel Modkit #{0}] Alphanumeric keys present: {1}.", ModuleID, Info.GetAlphabetInfo());
        Debug.LogFormat("[The Cruel Modkit #{0}] Button is {1}.", ModuleID, Info.GetButtonInfo());
        alph = startingAlphabets[Info.Button];
        Debug.LogFormat("[The Cruel Modkit #{0}] The starting alphabet is {1}.", ModuleID, alph);
        UpdateAlphAndShould();
    }

    private int DeterminePress()
    {
        foreach (char c in alph)
        {
            for (int i = 0; i < Info.Alphabet.Length; i++)
            {
                if (Info.Alphabet[i].Contains(c) && !pressedKeys.Contains(i))
                {
                    Debug.LogFormat("[The Cruel Modkit #{0}] The first character that appears in a non-pressed key is {1}, which is in key {2}.", ModuleID, c, i + 1);
                    return i;
                }
            }
        }
        throw new InvalidOperationException("erm what the sigma");
    }

    private IEnumerator ChangeButton()
    {
        yield return Module.StartCoroutine(Module.HideComponent(CruelModkitScript.ComponentsEnum.Button));
        Info.ButtonText = ComponentInfo.ButtonList[UnityEngine.Random.Range(0, 14)];
        Module.Button.transform.Find("ButtonText").GetComponentInChildren<TextMesh>().text = Info.ButtonText;
        yield return new WaitForSeconds(.5f);
        yield return Module.StartCoroutine(Module.ShowComponent(CruelModkitScript.ComponentsEnum.Button));
        UpdateAlphAndShould();
        buttonShouldBePressed = false;
    }

    public override void OnButtonPress()
    {
        if (Module.IsAnimating())
            return;

        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
        Module.Button.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.25f);

        if (Module.IsModuleSolved())
            return;

        if (!Module.IsSolving() && !Module.CheckValidComponents())
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The button was pressed when the component selection was [{1}] instead of [{2}].", ModuleID, Module.GetOnComponents(), Module.GetTargetComponents());
            Module.CauseStrike();
            return;
        }

        if (buttonShouldBePressed)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Pressed the button after 2 alphabet key presses.", ModuleID);
            Module.StartCoroutine(ChangeButton());
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! Pressed the button when a key was supposed to be pressed.", ModuleID);
            Module.CauseStrike();
            return;
        }
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
                Module.ButtonStrike(false, Alphabet);
                return;
            }
            Module.StartSolve();
        }

        if (buttonShouldBePressed)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! A key was pressed when the button was supposed to be pressed.", ModuleID);
            Module.ButtonStrike(false, Alphabet);
            Module.CauseStrike();
        }
        else if (Alphabet == shouldBePressed)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Correctly pressed the key labeled {1}.", ModuleID, Info.Alphabet[Alphabet].Replace('\n', ' '));
            Module.Alphabet[Alphabet].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            pressedKeys.Add(shouldBePressed);
            if (pressedKeys.Count == 6)
            {
                Debug.LogFormat("[The Cruel Modkit #{0}] All alphabet keys have been pressed! Solved!", ModuleID);
                Module.Solve();
            }
            else if (pressedKeys.Count % 2 == 0)
            {
                buttonShouldBePressed = true;
            }
            else
            {
                shouldBePressed = DeterminePress();
            }
        }
        else if (pressedKeys.Contains(Alphabet))
        {
            return;
        }
        else
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The key labeled {1} was pressed when the correct key was {2}.", ModuleID, Info.Alphabet[Alphabet].Replace('\n', ' '), Info.Alphabet[shouldBePressed].Replace('\n', ' '));
            Module.ButtonStrike(false, Alphabet);
            Module.CauseStrike();
        }
    }

    private void MoveToBeginningOrEnd(int index)
    {
        if (index == 25)
        {
            alph = alph[index] + alph.Substring(0, index);
        }
        else
        {
            alph = alph[index] + alph.Substring(0, index) + alph.Substring(index + 1);
        }
    }

    private string Modify()
    {
        switch (Info.ButtonText)
        {
            case "":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button label has no text, so the alphabet string is unchanged.", ModuleID);
                break;
            case "PRESS":
                if (alph[0] == Module.Bomb.GetSerialNumberLetters().First())
                {
                    Debug.LogFormat("[The Cruel Modkit #{0}] The button reads PRESS and the first character of the serial number is already at the beginning, so it will be moved to the end.", ModuleID);
                    alph = alph.Substring(1) + alph[0];
                }
                else
                {
                    Debug.LogFormat("[The Cruel Modkit #{0}] The button reads PRESS and the first character of the serial number is not already at the beginning, so it will be moved there.", ModuleID);
                    int firstLetterIndex = alph.IndexOf(Module.Bomb.GetSerialNumberLetters().First());
                    MoveToBeginningOrEnd(firstLetterIndex);
                }
                break;
            case "HOLD":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads HOLD, so both halves of the alphabet string will be swapped.", ModuleID);
                alph = alph.Substring(13) + alph.Substring(0, 13);
                break;
            case "DETONATE":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads DETONATE, so the alphabet string will be encrypted via the Atbash cipher.", ModuleID);
                alph = GetAtbash(alph);
                break;
            case "MASH":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads MASH, so the first consonant will be swapped with the last vowel.", ModuleID);
                char[] consonants = "BCDFGHJKLMNPQRSTVWXYZ".ToCharArray();
                char[] vowels = "AEIOU".ToCharArray();
                int firstConsonant = alph.IndexOfAny(consonants);
                int lastVowel = alph.LastIndexOfAny(vowels);
                alph = SwapChars(alph, firstConsonant, lastVowel);
                break;
            case "TAP":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads TAP, so the alphabet string will be Caesar-shifted forward by the sum of the digits in the Alphabet section.", ModuleID);
                int caesarOffset = 0;
                foreach (string button in Info.Alphabet)
                {
                    foreach (char key in button)
                    {
                        if (char.IsDigit(key))
                        {
                            caesarOffset += int.Parse(key.ToString());
                        }
                    }
                }
                Debug.LogFormat("[The Cruel Modkit #{0}] The sum of all alphabet digits is {1}.", ModuleID, caesarOffset);
                alph = Caesar(alph, caesarOffset);
                break;
            case "PUSH":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads PUSH, so the letters A, B, C, D and E will be moved immediately after Q.", ModuleID);
                string[] ABCDE = new string[] { "A", "B", "C", "D", "E" };
                foreach (string n in ABCDE)
                {
                    alph = alph.Replace(n, "");
                }
                int Q = alph.IndexOf('Q');
                alph = alph.Insert(Q + 1, "ABCDE");
                break;
            case "ABORT":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads ABORT, so the first letter in the alphabet string with an odd-numbered alphabetic position will be swapped with the last letter in the string with an even-numbered alphabetic position", ModuleID);
                string oddLetters = "ACEGIKMOQSUWY";
                string evenLetters = "BDFHJLNPRTVXZ";
                int firstOdd = alph.IndexOfAny(oddLetters.ToCharArray());
                int lastEven = alph.LastIndexOfAny(evenLetters.ToCharArray());
                alph = SwapChars(alph, firstOdd, lastEven);
                break;
            case "BUTTON":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads BUTTON, so the last character's alphabetic position will be multiplied by 5, moduloed by 26, have 1 added to it, and be moved to the beginning of the string.", ModuleID);
                string letterIndices = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int lastCharPosition = alph.IndexOf(letterIndices[((letterIndices.IndexOf(alph[25]) + 1) * 5) % 26 + 1]);
                MoveToBeginningOrEnd(lastCharPosition);
                break;
            case "CLICK":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads CLICK, so the alphabet string will be encrypted into ROT13, or Caesar-shifted by 13.", ModuleID);
                alph = Caesar(alph, 13);
                break;
            case "NOTHING":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads NOTHING, so the letter that comes after the first letter alphabetically will be moved to the end of the alphabet string.", ModuleID);
                string letter = Caesar(alph[0].ToString(), 1);
                int indexOfLetter = alph.IndexOf(letter);
                alph = alph.Remove(indexOfLetter, 1);
                alph += letter;
                break;
            case "NO":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads NO, so the first half will be reversed.", ModuleID);
                alph = new string(alph.Substring(0, 13).Reverse().ToArray()) + alph.Substring(13);
                break;
            case "I DON'T KNOW":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads I DON'T KNOW, so the entire alphabet string will be reversed.", ModuleID);
                alph = new string(alph.Reverse().ToArray());
                break;
            case "YES":
                Debug.LogFormat("[The Cruel Modkit #{0}] The button reads YES, so the second half will be reversed.", ModuleID);
                alph = alph.Substring(0, 13) + new string(alph.Substring(13).Reverse().ToArray());
                break;
        }
        return alph;
    }

    private string GetAtbash(string s)
    {
        string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string e = "";
        for (int i = 0; i < s.Length; i++) e += alpha[25 - alpha.IndexOf(s[i])];
        return e;
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
        string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string e = "";
        for (int i = 0; i < input.Length; i++) e += alpha[RealModulo(alpha.IndexOf(input[i]) + key, 26)];
        return e;
    }

    int RealModulo(int n, int m)
    {
        if (n > -1) return n % m;
        while (n < m)
        {
            n += m;
            if (n > m)
            {
                n -= m;
                break;
            }
        }
        return n;
    }
}
