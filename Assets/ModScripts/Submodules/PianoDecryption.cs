using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class PianoDecryption : Puzzle
{
    readonly string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly Dictionary<string, int[]> noteSequences = new Dictionary<string, int[]>
    {
        { "ANSWER", new int[] { 4, 6, 6, 6, 6, 4, 4, 4 } }, 
        { "BANTER", new int[] { 9, 4, 5, 7, 5, 4, 2, 2, 5, 9 } }, 
        { "CREOLE", new int[] { 10, 10, 10, 10, 6, 8, 10, 8, 10 } }, 
        { "DRIVEN", new int[] { 11, 2, 9, 7, 9, 11, 2, 9 } }, 
        { "ELEVEN", new int[] { 4, 4, 4, 0, 4, 7, 7 } }, 
        { "FAKERS", new int[] { 3, 3, 2, 2, 3, 3, 2, 3, 3, 2, 2, 3 } }, 
        { "GARAGE", new int[] { 7, 7, 0, 7, 7, 0, 7, 0 } }, 
        { "HORROR", new int[] { 1, 2, 4, 5, 1, 2, 4, 5, 10, 9 } }, 
        { "JULIET", new int[] { 10, 9, 10, 5, 3, 10, 9, 10, 5, 3 } }, 
        { "KEVLAR", new int[] { 7, 7, 7, 3, 10, 7, 3, 10, 7 } }, 
        { "NETHER", new int[] { 1, 11, 9, 6, 8, 9, 8, 6 } }, 
        { "OPIOID", new int[] { 7, 9, 7, 4, 7, 9, 7, 4 } }, 
        { "PARENT", new int[] { 7, 7, 7, 7, 7, 7, 7, 10, 3, 5, 7 } }, 
        { "QUESTS", new int[] { 6, 7, 9, 9, 2, 11, 9, 7, 4, 2 } }, 
        { "UMPIRE", new int[] { 3, 3, 1, 8, 3, 3, 5, 1 } }, 
        { "VICTOR", new int[] { 11, 9, 7, 3, 2, 9, 11, 9, 7 } }, 
        { "WIRING", new int[] { 3, 5, 3, 0, 8, 5, 3 } }, 
        { "XENONS", new int[] { 2, 2, 2, 1, 1, 1, 11, 1, 11, 6 } }, 
        { "YIELDS", new int[] { 7, 4, 5, 7, 0, 11, 0, 2, 0, 11, 9, 7 } }, 
        { "ZAMBIA", new int[] { 10, 9, 10, 7 } }
    };
    readonly string[] WordList = { "ANSWER", "BANTER", "CREOLE", "DRIVEN", "ELEVEN", "FAKERS", "GARAGE", "HORROR", "JULIET", "KEVLAR", "NETHER", "OPIOID", "PARENT", "QUESTS", "UMPIRE", "VICTOR", "WIRING", "XENONS", "YIELDS", "ZAMBIA" };
    string encryptedWord, decryptedWord = "";
    readonly int[] solutionSequence = new int[] { };
    readonly int repeats = 1;
    int currentNote = 0;

    public PianoDecryption(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Piano Decryption.", ModuleID);
        WordList = WordList.OrderBy(x => Random.Range(1, 1000)).ToArray();
        EncryptWord();
        solutionSequence = noteSequences[decryptedWord];
        if (decryptedWord == "ZAMBIA") repeats = Random.Range(3, 7);
        Debug.LogFormat("[The Cruel Modkit #{0}] Solution piano sequence: {1}{2}.", ModuleID, solutionSequence.Select(x => ComponentInfo.PianoKeyNames[(ComponentInfo.PianoKeys)x]).Join(", "),
            repeats > 1 ? (" (played " + repeats.ToString() + " times)") : "");
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
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, ComponentInfo.PianoKeyNames[(ComponentInfo.PianoKeys)Piano], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
        if (Piano != solutionSequence[repeats > 1 ? currentNote % solutionSequence.Length : currentNote])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key was pressed instead of {2}. Resetting input.", ModuleID, ComponentInfo.PianoKeyNames[(ComponentInfo.PianoKeys)Piano], ComponentInfo.PianoKeyNames[(ComponentInfo.PianoKeys)solutionSequence[currentNote]]);
            Module.CauseStrike();
            currentNote = 0;
            return;
        }
        currentNote++;
        if (currentNote == solutionSequence.Length * repeats)
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] The entire piano sequence was submitted. Module solved.", ModuleID);
            Module.Solve();
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
        currentNote = 0;
    }

    void EncryptWord()
    {
        for (int i = 0; i < WordList.Length; i++)
        {
            List<string> Encryption = new List<string>() { };
            encryptedWord = WordList[i];
            decryptedWord = encryptedWord;
            Encryption.Add(encryptedWord);

            encryptedWord = AtbashCipher(encryptedWord);
            Encryption.Add(encryptedWord);

            string rot13 = Rot13Cipher(encryptedWord);
            char[] serialLetters = Module.GetComponent<KMBombInfo>().GetSerialNumberLetters().ToArray();
            /* Contradicting case for this cipher: encrypted word contains a letter in S#, but ROT13 doesn't:
             * Encrypting with ROT13 would cause a contradiction during decryption - the encrypted word won't have a letter in S#, so ROT13 won't be needed, but we said the otherwise during encryption.
             * Not encrypting with ROT13 would also cause a contradiction during decryption - the encrypted word will have a letter in S#, so ROT13 will be needed, but we said the otherwise during encryption.
             * If this case is reached, the word will be impossible to encrypt. */
            if (encryptedWord.Any(x => serialLetters.Contains(x)) && !rot13.Any(x => serialLetters.Contains(x))) continue; // Check for the contradicting case
            Debug.LogFormat("[The Cruel Modkit #{0}] Decrypted word: {1}. Beginning encryption.", ModuleID, Encryption[0]);
            Debug.LogFormat("[The Cruel Modkit #{0}] Atbash Cipher: {1} -> {2}", ModuleID, Encryption[0], Encryption[1]);
            if (rot13.Any(x => serialLetters.Contains(x))) // Check for the need of ROT13
            {
                encryptedWord = rot13;
                Encryption.Add(encryptedWord);
                Debug.LogFormat("[The Cruel Modkit #{0}] ROT13 Cipher: {1} -> {2}", ModuleID, Encryption[1], Encryption[2]);
            }

            int X = Module.GetComponent<KMBombInfo>().GetIndicators().Count() * Module.GetComponent<KMBombInfo>().GetSerialNumberNumbers().First() + 1;
            encryptedWord = AffineTimesXCipher(X, encryptedWord);
            Debug.LogFormat("[The Cruel Modkit #{0}] Affine *X Cipher, X = {1}: {2} -> {3}", ModuleID, X, Encryption.Last(), encryptedWord);
            Encryption.Add(encryptedWord);

            if (Module.GetComponent<KMBombInfo>().GetPortCount() <= Module.GetComponent<KMBombInfo>().GetPortPlateCount())
            {
                encryptedWord = Key3RailFenceCipher(encryptedWord);
                Debug.LogFormat("[The Cruel Modkit #{0}] Rail Fence Cipher, Key = 3: {1} -> {2}", ModuleID, Encryption.Last(), encryptedWord);
                Encryption.Add(encryptedWord);
            }

            bool forward = Module.GetComponent<KMBombInfo>().GetIndicators().Count() == 0;
            int offset = forward ? Module.GetComponent<KMBombInfo>().GetSerialNumberNumbers().Last() : Module.GetComponent<KMBombInfo>().GetIndicators().Count();
            encryptedWord = CaesarShift(encryptedWord, offset, forward);
            Debug.LogFormat("[The Cruel Modkit #{0}] Caesar Shift {1} time{2} {3}: {4} -> {5}", ModuleID, offset,
                offset != 1 ? "s" : "", forward ? "forwards" : "backwards", Encryption.Last(), encryptedWord);

            Debug.LogFormat("[The Cruel Modkit #{0}] Final encrypted word: {1}.", ModuleID, encryptedWord);
            break;
        }
        Info.Morse = encryptedWord;
        Module.StopCoroutine(Module.MorseCodeAnimation);
        IEnumerator MorseCodeAnimation = Module.PlayWord(Info.Morse);
        Module.StartCoroutine(MorseCodeAnimation);
    }

    string AtbashCipher(string w)
    {
        string e = "";
        for (int i = 0; i < w.Length; i++) e += alpha[25 - alpha.IndexOf(w[i])];
        return e;
    }

    string Rot13Cipher(string w)
    {
        string e = "";
        for (int i = 0; i < w.Length; i++) e += alpha[(alpha.IndexOf(w[i]) + 13) % 26];
        return e;
    }

    string AffineTimesXCipher(int X, string w)
    {
        string e = "";
        for (int i = 0; i < w.Length; i++) e += alpha[(alpha.IndexOf(w[i]) * X) % 26];
        return e;
    }

    string Key3RailFenceCipher(string w)
    {
        return w[0].ToString() + w[4].ToString() + w[1].ToString() + w[3].ToString() + w[5].ToString() + w[2].ToString();
    }

    string CaesarShift(string w, int offset, bool forward)
    {
        string e = "";
        for (int i = 0; i < w.Length; i++) e += alpha[RealModulo(alpha.IndexOf(w[i]) + offset * (forward ? 1 : -1), 26)];
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
