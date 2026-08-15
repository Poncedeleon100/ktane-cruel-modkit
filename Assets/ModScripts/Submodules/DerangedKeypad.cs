using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wawa.Modules;
using static ComponentInfo;

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
        Module.Log("The resulting alphabet is {0}.", alph);
        shouldBePressed = DeterminePress();
    }

    public DerangedKeypad(CruelModkitScript Module, ComponentInfo Info, byte Components) : base(Module, Info, Components)
    {
        Module.Log("Solving Deranged Keypad.");
        Module.Log("Alphanumeric keys present: {0}.", Info.GetAlphabetInfo());
        Module.Log("Button is {0}.", Info.GetButtonInfo());
        alph = startingAlphabets[Info.Button];
        Module.Log("The starting alphabet is {0}.", alph);
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
                    Module.Log("The first character that appears in a non-pressed key is {0}, which is in key {1}.", c, i + 1);
                    return i;
                }
            }
        }
        throw new InvalidOperationException("erm what the sigma");
    }

    private IEnumerator ChangeButton()
    {
        yield return Module.StartCoroutine(Module.HideComponent(CruelModkitScript.ComponentsEnum.Button));
        Info.ButtonText = ButtonList[UnityEngine.Random.Range(0, 14)];
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

        if (buttonShouldBePressed)
        {
            Module.Log("Pressed the button after 2 alphabet key presses.");
            Module.StartCoroutine(ChangeButton());
        }
        else
        {
            Module.Strike("Strike! Pressed the button when a key was supposed to be pressed.");
            return;
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

        if (buttonShouldBePressed)
        {
            Module.Strike("Strike! A key was pressed when the button was supposed to be pressed.");
            Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
        }
        else if (Alphabet == shouldBePressed)
        {
            Module.Log("Correctly pressed the key labeled {0}.", Info.Alphabet[Alphabet]);
            Module.Alphabet[Alphabet].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = Module.KeyLightMats[2];
            pressedKeys.Add(shouldBePressed);
            if (pressedKeys.Count == 6)
            {
                Module.SolveModule("All alphabet keys have been pressed! Solved!");
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
            Module.Strike("Strike! The key labeled {0} was pressed when the correct key was {1}.", Info.Alphabet[Alphabet], Info.Alphabet[shouldBePressed]);
            Module.StartCoroutine(Module.ButtonStrike(false, Alphabet));
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
                Module.Log("The button label has no text, so the alphabet string is unchanged.");
                break;
            case "PRESS":
                if (alph[0] == Module.Bomb.GetSerialNumberLetters().First())
                {
                    Module.Log("The button reads PRESS and the first character of the serial number is already at the beginning, so it will be moved to the end.");
                    alph = alph.Substring(1) + alph[0];
                }
                else
                {
                    Module.Log("The button reads PRESS and the first character of the serial number is not already at the beginning, so it will be moved there.");
                    int firstLetterIndex = alph.IndexOf(Module.Bomb.GetSerialNumberLetters().First());
                    MoveToBeginningOrEnd(firstLetterIndex);
                }
                break;
            case "HOLD":
                Module.Log("The button reads HOLD, so both halves of the alphabet string will be swapped.");
                alph = alph.Substring(13) + alph.Substring(0, 13);
                break;
            case "DETONATE":
                Module.Log("The button reads DETONATE, so the alphabet string will be encrypted via the Atbash cipher.");
                alph = GetAtbash(alph);
                break;
            case "MASH":
                Module.Log("The button reads MASH, so the first consonant will be swapped with the last vowel.");
                char[] consonants = "BCDFGHJKLMNPQRSTVWXYZ".ToCharArray();
                char[] vowels = "AEIOU".ToCharArray();
                int firstConsonant = alph.IndexOfAny(consonants);
                int lastVowel = alph.LastIndexOfAny(vowels);
                alph = SwapChars(alph, firstConsonant, lastVowel);
                break;
            case "TAP":
                Module.Log("The button reads TAP, so the alphabet string will be Caesar-shifted forward by the sum of the digits in the Alphabet section.");
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
                Module.Log("The sum of all alphabet digits is {0}.", caesarOffset);
                alph = Caesar(alph, caesarOffset);
                break;
            case "PUSH":
                Module.Log("The button reads PUSH, so the letters A, B, C, D and E will be moved immediately after Q.");
                string[] ABCDE = new string[] { "A", "B", "C", "D", "E" };
                foreach (string n in ABCDE)
                {
                    alph = alph.Replace(n, "");
                }
                int Q = alph.IndexOf('Q');
                alph = alph.Insert(Q + 1, "ABCDE");
                break;
            case "ABORT":
                Module.Log("The button reads ABORT, so the first letter in the alphabet string with an odd-numbered alphabetic position will be swapped with the last letter in the string with an even-numbered alphabetic position");
                string oddLetters = "ACEGIKMOQSUWY";
                string evenLetters = "BDFHJLNPRTVXZ";
                int firstOdd = alph.IndexOfAny(oddLetters.ToCharArray());
                int lastEven = alph.LastIndexOfAny(evenLetters.ToCharArray());
                alph = SwapChars(alph, firstOdd, lastEven);
                break;
            case "BUTTON":
                Module.Log("The button reads BUTTON, so the last character's alphabetic position will be multiplied by 5, moduloed by 26, have 1 added to it, and be moved to the beginning of the string.");
                string letterIndices = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int lastCharPosition = alph.IndexOf(letterIndices[((letterIndices.IndexOf(alph[25]) + 1) * 5) % 26 + 1]);
                MoveToBeginningOrEnd(lastCharPosition);
                break;
            case "CLICK":
                Module.Log("The button reads CLICK, so the alphabet string will be encrypted into ROT13, or Caesar-shifted by 13.");
                alph = Caesar(alph, 13);
                break;
            case "NOTHING":
                Module.Log("The button reads NOTHING, so the letter that comes after the first letter alphabetically will be moved to the end of the alphabet string.");
                string letter = Caesar(alph[0].ToString(), 1);
                int indexOfLetter = alph.IndexOf(letter);
                alph = alph.Remove(indexOfLetter, 1);
                alph += letter;
                break;
            case "NO":
                Module.Log("The button reads NO, so the first half will be reversed.");
                alph = new string(alph.Substring(0, 13).Reverse().ToArray()) + alph.Substring(13);
                break;
            case "I DON'T KNOW":
                Module.Log("The button reads I DON'T KNOW, so the entire alphabet string will be reversed.");
                alph = new string(alph.Reverse().ToArray());
                break;
            case "YES":
                Module.Log("The button reads YES, so the second half will be reversed.");
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
