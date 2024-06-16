﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class CruelModkitScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    //Component Selector
    public GameObject[] Doors;
    public GameObject[] Components;
    public KMSelectable[] SelectorButtons;
    public KMSelectable[] UtilityButton;
    public TextMesh DisplayText;

    //Materials
    public Material[] WireMats;
    public Material[] WireLEDMats;
    public Material[] ButtonMats;
    public Material[] LEDMats;
    public Material[] KeyLightMats;
    public Material[] SymbolMats;
    public Material[] ArrowMats;
    public Material[] IdentityMats;
    public Material[] ResistorMats;
    public Material[] MorseMats;
    public Material[] MeterMats;

    //Module Objects
    public GameObject[] Wires;
    public GameObject[] WireLED;
    public TextMesh ButtonText;
    public Renderer Button;
    public GameObject[] Adventure;
    public GameObject[] LED;
    public GameObject[] Symbols;
    public GameObject[] Alphabet;
    public GameObject[] Piano;
    public GameObject[] Arrows;
    public Transform ArrowsBase;
    public GameObject[] Identity;
    public Transform BulbOFace;
    public Transform BulbIFace;
    public Light[] BulbLights;
    public MeshRenderer[] BulbGlass;
    public GameObject[] BulbFilaments;
    public GameObject[] Resistor;
    public TextMesh[] WidgetText;
    public Light MorseLight;
    public Renderer MorseMesh;
    public GameObject Meter;

    public Mesh[] WireMesh;
    List<int> WiresCut = new List<int>();

    //Component Selector Info
    readonly string[] ComponentNames = new string[] { "WIRES", "BUTTON", "ADVENTURE", "LED", "SYMBOLS", "ALPHABET", "PIANO", "ARROWS", "IDENTITY", "BULBS", "RESISTOR" };
    bool[] OnComponents = new bool[11];
    //bool[] TargetComponents = new bool[11];
    bool[] TargetComponents;
    int CurrentComponent = 0;

    ComponentInfo Info;
    Puzzle Puzzle;

    // Morse Code
    private enum Symbol
    {
        Dot,
        Dash
    }

    private static readonly Dictionary<char, Symbol[]> MorseCodeTable = new Dictionary<char, Symbol[]>()
    {
        { '0', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dash, Symbol.Dash, Symbol.Dash } },
        { '1', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash, Symbol.Dash, Symbol.Dash } },
        { '2', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dash, Symbol.Dash, Symbol.Dash } },
        { '3', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dash, Symbol.Dash } },
        { '4', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dash } },
        { '5', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
        { '6', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
        { '7', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
        { '8', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dash, Symbol.Dot, Symbol.Dot } },
        { '9', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dash, Symbol.Dash, Symbol.Dot } },
        { 'A', new[] { Symbol.Dot, Symbol.Dash } },
        { 'B', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
        { 'C', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dash, Symbol.Dot } },
        { 'D', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot } },
        { 'E', new[] { Symbol.Dot } },
        { 'F', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dash, Symbol.Dot } },
        { 'G', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot } },
        { 'H', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
        { 'I', new[] { Symbol.Dot, Symbol.Dot } },
        { 'J', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash, Symbol.Dash } },
        { 'K', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dash } },
        { 'L', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dot, Symbol.Dot } },
        { 'M', new[] { Symbol.Dash, Symbol.Dash } },
        { 'N', new[] { Symbol.Dash, Symbol.Dot } },
        { 'O', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dash } },
        { 'P', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash, Symbol.Dot } },
        { 'Q', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot, Symbol.Dash } },
        { 'R', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dot } },
        { 'S', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot } },
        { 'T', new[] { Symbol.Dash } },
        { 'U', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dash } },
        { 'V', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dash } },
        { 'W', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash } },
        { 'X', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot, Symbol.Dash } },
        { 'Y', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dash, Symbol.Dash } },
        { 'Z', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot, Symbol.Dot } },
    };
    private const float MorseCodeDotLength = 0.25f;

    // Logging
    static int ModuleIDCounter = 1;
    int ModuleID;
    private bool ModuleSolved;
    private bool Solving;
    //private Coroutine Animating;
    private bool Animating;

    // These are public variables needed to communicate with the Puzzle class.

    public bool IsModuleSolved() => ModuleSolved;
    public bool CheckValidComponents() => OnComponents.SequenceEqual(TargetComponents);
    public bool IsAnimating() => Animating != null;

    // private bool HasStruck = false; // TP Handling, send a strike handling if the module struck. To prevent excessive inputs.

    // Use these for debugging individual puzzles.
    private bool ForceComponents, ForceByModuleID;
    /*/private bool[] componentsForced;
    public bool enableBruteTest = false;
    ModkitSettings modConfig = new ModkitSettings();/*/

    void Awake ()
    {
        ModuleID = ModuleIDCounter++;
        SelectorButtons[0].OnInteract += delegate ()
        {
            ChangeDisplayComponent(SelectorButtons[0], -1);
            return false;
        };
        SelectorButtons[1].OnInteract += delegate () 
        {
            ToggleComponent();
            return false;
        };
        SelectorButtons[2].OnInteract += delegate ()
        {
            ChangeDisplayComponent(SelectorButtons[2], 1);
            return false;
        };
        /* Cruel Modkit Settings for enforcing specific components
        try
		{
			ModConfig<ModkitSettings> modkitJSON = new ModConfig<ModkitSettings>("ModkitSettings");
			modConfig = modkitJSON.Settings;

			forceComponents = modConfig.EnforceComponents;
			forceByModuleID = modConfig.EnforceByModID;

			componentsForced = new bool[] { modConfig.EnforceWires, modConfig.EnforceSymbols, modConfig.EnforceAlphabet, modConfig.EnforceLEDs, modConfig.EnforceArrows };
		}
		catch
		{
			Debug.LogErrorFormat("[The Modkit #{0}] The settings do not work as intended! Using default settings (do not force required components).", moduleId);
			forceComponents = false;
			forceByModuleID = false;
			componentsForced = new bool[] { false, false, false, false, false };
		}*/
    }

    void Start ()
    {
        SetUpComponents();
        //TryOverrideSettings();
        if (ForceComponents) // Check if the components need to be forced on.
        {
            // ForceComponents();
            // DisplayText.text = "DISABLED";
        }
        else
        {
            CalcComponents();
            DisplayText.text = ComponentNames[CurrentComponent];
        }
        /*AssignHandlers();
        for (int x = 0; x < 5; x++)
        {
            SetSelectables(x, forceComponents ? targetComponents[x] : false);
            onComponents[x] = forceComponents && targetComponents[x];
        }
        if (forceComponents)
            StartCoroutine(PlayEnforceAnim());*/
    }

    void ChangeDisplayComponent(KMSelectable Button, int i)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
        Button.AddInteractionPunch(0.5f);
        StartCoroutine(AnimateButtonPress(Button.transform, Vector3.down * 0.005f));
        if (ModuleSolved || ForceComponents)
        {
            return;
        }
        CurrentComponent += i;

        if(CurrentComponent < 0)
        {
            CurrentComponent += ComponentNames.Length;
        }
        if(CurrentComponent >= ComponentNames.Length)
        {
            CurrentComponent -= ComponentNames.Length;
        }

        DisplayText.text = ComponentNames[CurrentComponent];
        DisplayText.color = OnComponents[CurrentComponent] ? Color.green : Color.red;
    }

    void ToggleComponent()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SelectorButtons[1].transform);
        SelectorButtons[1].AddInteractionPunch(0.5f);
        StartCoroutine(AnimateButtonPress(SelectorButtons[1].transform, Vector3.down * 0.005f));
        if (ModuleSolved || Solving || ForceComponents || Animating)
        {
            return;
        }
        OnComponents[CurrentComponent] = !OnComponents[CurrentComponent];
        if(OnComponents[CurrentComponent])
        {
            DisplayText.color = new Color(0, 1, 0);
            // StartCoroutine(ShowComponent(CurrentComponent));
        }
        else
        {
            DisplayText.color = new Color(1, 0, 0);
            // StartCoroutine(HideComponent(CurrentComponent));
        }
    }

    public IEnumerator AnimateButtonPress(Transform Object, Vector3 Offset)
    {
        for (int x = 0; x < 5; x++)
        {
            Object.localPosition += Offset / 5;
            yield return new WaitForSeconds(0.01f);
        }
        for (int x = 0; x < 5; x++)
        {
            Object.localPosition -= Offset / 5;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void SetUpComponents()
    {
        Info = new ComponentInfo();
        //Set materials for Wires
        for(int i = 0; i < 7; i++)
        {
            int Color1 = Info.Wires[0][i];
            int Color2 = Info.Wires[1][i];
            if(Color1 != Color2)
            {
                Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == ComponentInfo.WireColors[Color1] + "_" + ComponentInfo.WireColors[Color2]).ToArray()[0];
            }
            else
            {
                Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == ComponentInfo.WireColors[Color1]).ToArray()[0];
            }
        }
        //Set materials for Wire LEDs
        for(int i = 0; i < WireLED.Length; i++)
        {
            WireLED[i].transform.Find("WireLEDL").GetComponentInChildren<Renderer>().material = WireLEDMats[Info.WireLED[i]];
        }
        //Set text and material for Button
        ButtonText.text = Info.ButtonText;
        Button.material = ButtonMats[Info.Button];
        if(Info.Button == 0 || Info.Button == 1 || Info.Button == 7)
        {
            ButtonText.color = ComponentInfo.ButtonTextWhite;
        }
        //Set materials for LEDs
        for(int i = 0; i < LED.Length; i++)
        {
            LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = LEDMats[Info.LED[i]];
        }
        //Set materials for Symbols
        for(int i = 0; i < Symbols.Length; i++)
        {
            Symbols[i].transform.Find("Symbol").GetComponentInChildren<Renderer>().material = SymbolMats[Info.Symbols[i]];
        }
        //Set Alphabet text
        for(int i = 0; i < Alphabet.Length; i++)
        {
            Alphabet[i].transform.Find("AlphabetText").GetComponentInChildren<TextMesh>().text = Info.Alphabet[i];
        }
        //Set materials and light colors for Arrows
        for(int i = 0; i < 9; i++)
        {
            Arrows[i].GetComponentInChildren<Renderer>().material = ArrowMats[Info.Arrows[i]];
            Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().color = Info.ArrowLights[i];
        }
        //Set materials and text for Identity
        Identity[0].transform.Find("IdentityFaceIcon").GetComponentInChildren<Renderer>().material = IdentityMats.Where(x => x.name == Info.Identity[0][0]).ToArray()[0];
        for(int i = 1; i < 4; i++)
        {
            Identity[i].transform.Find("IdentityText").GetComponentInChildren<TextMesh>().text = Info.Identity[i][0];
        }
        //Set I/O buttons and bulb colors/opacity for Bulbs
        if(Info.BulbOLeft)
        {
            var p = BulbOFace.position;
            BulbOFace.position = BulbIFace.position;
            BulbIFace.position = p;
        }
        for(int i = 0; i < 2; i++)
        {
            //Set filament visibility based on opacity of the bulb
            BulbFilaments[i].SetActive(!Info.BulbInfo[i]);
            //Set bulb glass color and opacity
            BulbGlass[i].material.color = Info.BulbColors[i];
            //Set bulb light color
            BulbLights[i].color = BulbLights[i + 2].color = Info.BulbColors[i + 2];
            //Turns the lights on or off, might be moved to a different function later
            BulbLights[i].enabled = BulbLights[i + 2].enabled = Info.BulbInfo[i + 2];
        }
        //Set materials and text for Resistor
        for(int i = 0; i < 4; i++)
        {
            Resistor[i].GetComponentInChildren<Renderer>().material = ResistorMats[Info.ResistorColors[i][0]];
        }
        Resistor[4].GetComponentInChildren<TextMesh>().text = Info.ResistorText[0];
        Resistor[5].GetComponentInChildren<TextMesh>().text = Info.ResistorText[1];
        //Set timer display text
        string TempString = System.String.Empty;
        if (Info.TimerDisplay < 10)
        {
            TempString += "0";
        }
        TempString += Info.TimerDisplay.ToString();
        WidgetText[0].text = TempString;
        //Set word display text
        WidgetText[1].text = Info.WordDisplay;
        //Set number display text
        WidgetText[2].text = Info.NumberDisplay.ToString();
        //Set morse code display
        StartCoroutine(PlayWord(Info.Morse));
        //Set meter value and color
        Meter.GetComponentInChildren<Renderer>().material = MeterMats[Info.MeterColor];
        //Changes the meter so it matches the value from Info.MeterValue; adjusts scale first then shifts the position down
        float TempNumber = 0.003882663f * Info.MeterValue; //.00388 is the original Z scale
        Meter.transform.localScale = new Vector3(0.0005912599f, 0.01419745f, TempNumber);
        TempNumber = -0.02739999f - ((0.03884f * (1 - Info.MeterValue)) / 2); //-.0273 is the original Z position, .0388 is the original length
        Meter.transform.localPosition = new Vector3(-0.04243f, 0.01436f, TempNumber);
    }

    /* Override settings based on mission ID (and unimplemented mission description support)
    void TryOverrideSettings()
    {
        try
        {
            var missionID = Application.isEditor ? "freeplay" : Game.Mission.ID ?? "unknown";
            var overwriteSuccessful = false;
            Debug.LogFormat("<The Modkit #{0}> Mission ID: {1}", moduleId, missionID);
            switch (missionID)
            {
                case "freeplay":
                case "custom":
                    Debug.LogFormat("<The Modkit #{0}> MISSION DETECTED AS FREEPLAY. NOT OVERWRITING SETTINGS.", moduleId);
                    return;
                case "mod_theBombsBlanMade_deafsHell":
                    Debug.LogFormat("<The Modkit #{0}> \"Deaf's Hell\" from \"The Bombs Blan Made\" detected.", moduleId);
                    forceComponents = true;
                    forceByModuleID = true;
                    overwriteSuccessful = true;
                    break;
            }
            if (overwriteSuccessful)
            {
                Debug.LogFormat("<The Modkit #{0}> OVERWRITE SUCCESSFUL BY MISSION ID.", moduleId);
                return;
            }
            // Regex for getting forced components directly from the mission description
			//var regexMatchOverrideDescription = Regex.Match(Game.Mission.Description ?? "", @"\[ModkitOverride\]\sEnforce(ModID|((Wires?|Symbols?|Alphabet|LED|Arrows?),)+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant );
			//if (regexMatchOverrideDescription.Success)
            //{

            //}
			//else
            //{
            Debug.LogFormat("<The Modkit #{0}> PREVENTING COMPONENTS FROM BEING OVERRIDDEN.", moduleId);
            forceComponents = false;
            //}
        }
        catch (Exception error)
        {
            Debug.LogWarningFormat("<The Modkit #{0}> Override does not work as intended! ", moduleId);
            Debug.LogException(error);
            Debug.LogWarningFormat("<The Modkit #{0}> Using default settings.", moduleId);
            forceComponents = false;
        }
    }

    // Forces components
    void ForceComponents()
    {
        Debug.LogFormat("[The Modkit #{0}] The calculation procedure for The Modkit has been overridden.", moduleId);
        var curModID = moduleId * 1;
        if (forceByModuleID)
        {
            Debug.LogFormat("[The Modkit #{0}] Enforcing components via module ID.", moduleId);
            for (int x = 0; x < componentsForced.Length; x++)
            {
                targetComponents[x] = curModID % 2 == 1;
                curModID /= 2;
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Enforcing specific components.", moduleId);
            for (int x = 0; x < componentsForced.Length; x++)
                targetComponents[x] = componentsForced[x];
        }
        Debug.LogFormat("[The Modkit #{0}] Enforced components: [ {1} ].", moduleId, componentNames.Any(x => targetComponents[Array.IndexOf(componentNames, x)]) ? componentNames.Where(x => targetComponents[Array.IndexOf(componentNames, x)]).Join(", ") : "none");
    }*/
    
    // Calculates the components required to be solved
    void CalcComponents()
	{
		var SerialNumber = Bomb.GetSerialNumber();
		var SerialNumberPairs = new List<string>();
        // 1. Separate serial number into three pairs
        for (int i = 0; i < SerialNumber.Length; i+= 2)
            SerialNumberPairs.Add(SerialNumber.Substring(i, 2));
		var Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // 2. Calculate a puzzle ID from the base36 value
        // Formula : nth pair value, P(n) = base36 of first character * base36 of second character
        // (Separated for convenience): Puzzle ID = ( P(1) + P(2) + P(3) ) % 2048
        var Products = SerialNumberPairs.Sum(x => Base36.IndexOf(x[0]) * Base36.IndexOf(x[1])) % 2048;
		TargetComponents = Products.ToString("2").PadLeft(11, '0').Select(x => x == '1').ToArray();
        Debug.LogFormat("[The Cruel Modkit #{0}] Calculated puzzle ID is {1}.", ModuleID, Products.ToString());
	}

    // Sets up the selected puzzle class
    void AssignHandlers()
    {
        /*Debug.LogFormat("[The Modkit #{0}] --------------------------------------------------", moduleId);
        if (targetComponents.SequenceEqual(new[] { true, false, false, false, false })) p = new ColorfulWires(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, false, false, false })) p = new AdjacentSymbols(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, true, false, false })) p = new EdgeworkKeys(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, false, true, false })) p = new LEDPattern(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, false, false, true })) p = new SimonShifts(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, false, false, false })) p = new RunicWires(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, true, false, false })) p = new IndexedWires(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, false, true, false })) p = new WireInstructions(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, false, false, true })) p = new WireMaze(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, true, false, false })) p = new EncryptedKeypad(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, false, true, false })) p = new SymbolicMorse(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, false, false, true })) p = new PerspectiveSymbols(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, true, true, false })) p = new SemaphoreKeys(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, true, false, true })) p = new AlphanumericOrder(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, false, true, true })) p = new ColorCompass(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, true, false, false })) p = new SequenceCut(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, false, true, false })) p = new HierarchicalWires(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, false, false, true })) p = new WireSignaling(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, true, true, false })) p = new PowerGrid(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, true, false, true })) p = new CruelWireSequence(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, false, true, true })) p = new BlinkingWires(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, true, true, false })) p = new KeyScore(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, true, false, true })) p = new LyingKeys(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, false, true, true })) p = new ColorOffset(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, false, true, true, true })) p = new LEDDirections(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, true, true, false })) p = new TheThirdWire(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, true, false, true })) p = new TheLastInLine(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, false, true, true })) p = new ColorDominance(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, false, true, true, true })) p = new PreciseWires(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { false, true, true, true, true })) p = new GatedMaze(this, moduleId, info);
        else if (targetComponents.SequenceEqual(new[] { true, true, true, true, true })) p = new ParanormalWires(this, moduleId, info);
        else p = new Puzzle(this, moduleId, info, true);*/

        for (int x = 0; x < Wires.Length; x++)
        {
            int y = x;
            Wires[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate ()
            {
                //p.OnWireCut(y);
                return false;
            };
        }
        /*for (int x = 0; x < symbols.Length; x++)
        {
            int y = x;
            symbols[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate {
                StartCoroutine(AnimateButtonPress(symbols[y].transform, Vector3.down * 0.005f));
                p.OnSymbolPress(y);
                return false;
            };
        }

        for (int x = 0; x < alphabet.Length; x++)
        {
            int y = x;
            alphabet[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate {
                StartCoroutine(AnimateButtonPress(alphabet[y].transform, Vector3.down * 0.005f));
                p.OnAlphabetPress(y);
                return false;
            };
        }
        for (int x = 0; x < arrows.Length; x++)
        {
            int y = x;
            arrows[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate {
                StartCoroutine(AnimateButtonPress(arrowsBase.transform, Vector3.down * 0.002f));
                StartCoroutine(AnimateButtonRotationPress(arrowsBase.transform, new[] { Vector3.right, Vector3.left, Vector3.back, Vector3.forward }.ElementAt(y) * 5));
                p.OnArrowPress(y);
                return false;
            };
        }

        utilityBtn.OnInteract += delegate {
            StartCoroutine(AnimateButtonPress(utilityBtn.transform, Vector3.down * 0.005f));
            p.OnUtilityPress();
            return false;
        };
        if (enableBruteTest)
            p.BruteForceTest();*/
    }

    public IEnumerator PlayWord(string Word)
    {
        while (true)
        {
            foreach (var c in Word)
            {
                var Code = MorseCodeTable[char.ToUpper(c)];
                foreach (var Symbol in Code)
                {
                    MorseLight.enabled = true;
                    MorseMesh.material = MorseMats[1];
                    yield return new WaitForSeconds(Symbol == Symbol.Dot ? MorseCodeDotLength : MorseCodeDotLength * 3);
                    MorseLight.enabled = false;
                    MorseMesh.material = MorseMats[0];
                    yield return new WaitForSeconds(MorseCodeDotLength);
                }
                yield return new WaitForSeconds(MorseCodeDotLength * 3);  // 4 dots total
            }
            yield return new WaitForSeconds(MorseCodeDotLength * 6);  // 10 dots total
        }
    }

    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}