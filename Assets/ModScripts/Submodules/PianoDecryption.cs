using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using static ComponentInfo;

public class PianoDecryption : Puzzle
{
    readonly string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly Dictionary<string, PianoKeys[]> noteSequences = new Dictionary<string, PianoKeys[]>
    {
        { "ANSWER", new PianoKeys[] { PianoKeys.E, PianoKeys.Gb, PianoKeys.Gb, PianoKeys.Gb, PianoKeys.Gb, PianoKeys.E, PianoKeys.E, PianoKeys.E } }, 
        { "BANTER", new PianoKeys[] { PianoKeys.A, PianoKeys.E, PianoKeys.F, PianoKeys.G, PianoKeys.F, PianoKeys.E, PianoKeys.D, PianoKeys.D, PianoKeys.F, PianoKeys.A } }, 
        { "CREOLE", new PianoKeys[] { PianoKeys.Bb, PianoKeys.Bb, PianoKeys.Bb, PianoKeys.Bb, PianoKeys.Gb, PianoKeys.Ab, PianoKeys.Bb, PianoKeys.Ab, PianoKeys.Bb } }, 
        { "DRIVEN", new PianoKeys[] { PianoKeys.B, PianoKeys.D, PianoKeys.A, PianoKeys.G, PianoKeys.A, PianoKeys.B, PianoKeys.D, PianoKeys.A } }, 
        { "ELEVEN", new PianoKeys[] { PianoKeys.E, PianoKeys.E, PianoKeys.E, PianoKeys.C, PianoKeys.E, PianoKeys.G, PianoKeys.G } }, 
        { "FAKERS", new PianoKeys[] { PianoKeys.Eb, PianoKeys.Eb, PianoKeys.D, PianoKeys.D, PianoKeys.Eb, PianoKeys.Eb, PianoKeys.D, PianoKeys.Eb, PianoKeys.Eb, PianoKeys.D, PianoKeys.D, PianoKeys.Eb } }, 
        { "GARAGE", new PianoKeys[] { PianoKeys.G, PianoKeys.G, PianoKeys.C, PianoKeys.G, PianoKeys.G, PianoKeys.C, PianoKeys.G, PianoKeys.C } }, 
        { "HORROR", new PianoKeys[] { PianoKeys.Db, PianoKeys.D, PianoKeys.E, PianoKeys.F, PianoKeys.Db, PianoKeys.D, PianoKeys.E, PianoKeys.F, PianoKeys.Bb, PianoKeys.A } }, 
        { "JULIET", new PianoKeys[] { PianoKeys.Bb, PianoKeys.A, PianoKeys.Bb, PianoKeys.F, PianoKeys.Eb, PianoKeys.Bb, PianoKeys.A, PianoKeys.Bb, PianoKeys.F, PianoKeys.Eb } }, 
        { "KEVLAR", new PianoKeys[] { PianoKeys.G, PianoKeys.G, PianoKeys.G, PianoKeys.Eb, PianoKeys.Bb, PianoKeys.G, PianoKeys.Eb, PianoKeys.Bb, PianoKeys.G } }, 
        { "NETHER", new PianoKeys[] { PianoKeys.Db, PianoKeys.B, PianoKeys.A, PianoKeys.Gb, PianoKeys.Ab, PianoKeys.A, PianoKeys.Ab, PianoKeys.Gb } }, 
        { "OPIOID", new PianoKeys[] { PianoKeys.G, PianoKeys.A, PianoKeys.G, PianoKeys.E, PianoKeys.G, PianoKeys.A, PianoKeys.G, PianoKeys.E } }, 
        { "PARENT", new PianoKeys[] { PianoKeys.G, PianoKeys.G, PianoKeys.G, PianoKeys.G, PianoKeys.G, PianoKeys.G, PianoKeys.G, PianoKeys.Bb, PianoKeys.Eb, PianoKeys.F, PianoKeys.G } }, 
        { "QUESTS", new PianoKeys[] { PianoKeys.Gb, PianoKeys.G, PianoKeys.A, PianoKeys.A, PianoKeys.D, PianoKeys.B, PianoKeys.A, PianoKeys.G, PianoKeys.E, PianoKeys.D } }, 
        { "UMPIRE", new PianoKeys[] { PianoKeys.Eb, PianoKeys.Eb, PianoKeys.Db, PianoKeys.Ab, PianoKeys.Eb, PianoKeys.Eb, PianoKeys.F, PianoKeys.Db } }, 
        { "VICTOR", new PianoKeys[] { PianoKeys.B, PianoKeys.A, PianoKeys.G, PianoKeys.Eb, PianoKeys.D, PianoKeys.A, PianoKeys.B, PianoKeys.A, PianoKeys.G } }, 
        { "WIRING", new PianoKeys[] { PianoKeys.Eb, PianoKeys.F, PianoKeys.Eb, PianoKeys.C, PianoKeys.Ab, PianoKeys.F, PianoKeys.Eb } }, 
        { "XENONS", new PianoKeys[] { PianoKeys.D, PianoKeys.D, PianoKeys.D, PianoKeys.Db, PianoKeys.Db, PianoKeys.Db, PianoKeys.B, PianoKeys.Db, PianoKeys.B, PianoKeys.Gb } }, 
        { "YIELDS", new PianoKeys[] { PianoKeys.G, PianoKeys.E, PianoKeys.F, PianoKeys.G, PianoKeys.C, PianoKeys.B, PianoKeys.C, PianoKeys.D, PianoKeys.C, PianoKeys.B, PianoKeys.A, PianoKeys.G } }, 
        { "ZAMBIA", new PianoKeys[] { PianoKeys.Bb, PianoKeys.A, PianoKeys.Bb, PianoKeys.G } }
    };
    readonly string[] WordList = { "ANSWER", "BANTER", "CREOLE", "DRIVEN", "ELEVEN", "FAKERS", "GARAGE", "HORROR", "JULIET", "KEVLAR", "NETHER", "OPIOID", "PARENT", "QUESTS", "UMPIRE", "VICTOR", "WIRING", "XENONS", "YIELDS", "ZAMBIA" };
    string encryptedWord, decryptedWord = "";
    readonly PianoKeys[] solutionSequence;
    readonly int repeats = 1;
    int currentNote = 0;

    public PianoDecryption(CruelModkitScript Module, int ModuleID, ComponentInfo Info, byte Components) : base(Module, ModuleID, Info, Components)
    {
        Debug.LogFormat("[The Cruel Modkit #{0}] Solving Piano Decryption.", ModuleID);
        WordList = WordList.OrderBy(x => Random.Range(1, 1000)).ToArray();
        EncryptWord();
        solutionSequence = noteSequences[decryptedWord];
        if (decryptedWord == "ZAMBIA") repeats = Random.Range(3, 7);
        Debug.LogFormat("[The Cruel Modkit #{0}] Solution piano sequence: {1}{2}.", ModuleID, solutionSequence.Select(x => PianoKeyNames[(PianoKeys)x]).Join(", "),
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
                Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key on the piano was pressed when the component selection was [{2}] instead of [{3}].", ModuleID, PianoKeyNames[(PianoKeys)Piano], Module.GetOnComponents(), Module.GetTargetComponents());
                Module.CauseStrike();
                return;
            }

            Module.StartSolve();
        }
        if (Piano != (int)solutionSequence[repeats > 1 ? currentNote % solutionSequence.Length : currentNote])
        {
            Debug.LogFormat("[The Cruel Modkit #{0}] Strike! The {1} key was pressed instead of {2}. Resetting input.", ModuleID, PianoKeyNames[(PianoKeys)Piano], PianoKeyNames[solutionSequence[currentNote]]);
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
