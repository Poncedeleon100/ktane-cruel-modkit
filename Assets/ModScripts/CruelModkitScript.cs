using System;
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
    public GameObject Button;
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
    readonly string[] ComponentNames = new string[] { "Wires", "Button", "Adventure", "LED", "Symbols", "Alphabet", "Piano", "Arrows", "Identity", "Bulbs", "Resistor" };
    bool[] OnComponents = new bool[11];
    bool[] TargetComponents = new bool[11];
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
    private bool Animating;

    // These are public variables needed to communicate with the Puzzle class.

    public bool IsModuleSolved() => ModuleSolved;
    public bool CheckValidComponents() => OnComponents.SequenceEqual(TargetComponents);
    public bool IsAnimating() => Animating;

    private bool HasStruck = false; // TP Handling, send a strike handling if the module struck. To prevent excessive inputs.

    // Use these for debugging individual puzzles.
    private bool ForceComponents, ForceByModuleID;
    // Settings

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
        // Settings
    }

    void Start ()
    {
        SetUpComponents();
        // Settings
        if (ForceComponents) // Settings - Check if the components need to be forced on.
        {
            // Settings
        }
        else
        {
            CalcComponents();
            DisplayText.text = ComponentNames[CurrentComponent];
        }
        AssignHandlers();
        for (int i = 0; i < 11; i++)
        {
            SetSelectables(i, ForceComponents ? TargetComponents[i] : false);
            OnComponents[i] = ForceComponents && TargetComponents[i];
        }
        // Settings
    }

    // Animations
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
            StartCoroutine(ShowComponent(CurrentComponent));
        }
        else
        {
            DisplayText.color = new Color(1, 0, 0);
            StartCoroutine(HideComponent(CurrentComponent));
        }
    }

    public IEnumerator AnimateButtonPress(Transform Object, Vector3 Offset, int Index = 0)
    {
        switch (Index)
        {
            case 0:
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition += Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition -= Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            case 1:
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition += Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            case 2:
                for (int i = 0; i < 5; i++)
                {
                    Object.localPosition -= Offset / 5;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
        }
    }

    public IEnumerator ShowComponent(int Component)
    {
        Animating = true;
        SetSelectables(Component, true);
        Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        Doors[Component].transform.localPosition += new Vector3(0, -0.001f, 0);
        switch (Component)
        {
            case 0:
            case 3:
            case 4:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(-0.08f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 1:
            case 2:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(-0.0374f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 5:
            case 6:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(0.08f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 7:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(0.0489f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 8:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(0.0298f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 9:
            case 10:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(0.0237f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
        }
        Doors[Component].SetActive(false);
        for (int i = 0; i < 10; i++)
        {
            Components[Component].transform.localPosition += new Vector3(0, 0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        Animating = false;
    }

    public IEnumerator HideComponent(int Component)
    {
        Animating = true;
        Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        for (int i = 0; i < 10; i++)
        {
            Components[Component].transform.localPosition += new Vector3(0, -0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        Doors[Component].SetActive(true);
        switch (Component)
        {
            case 0:
            case 3:
            case 4:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(0.08f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 1:
            case 2:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(0.0374f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 5:
            case 6:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(-0.08f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 7:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(-0.0489f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 8:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(-0.0298f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
            case 9:
            case 10:
                for (int i = 0; i < 10; i++)
                {
                    Doors[Component].transform.localPosition += new Vector3(-0.0237f, 0, 0);
                    yield return new WaitForSeconds(0.025f);
                }
                break;
        }
        Doors[Component].transform.localPosition += new Vector3(0, 0.001f, 0);
        SetSelectables(Component, false);
        Animating = false;
    }

    public void SetSelectables(int Component, bool Enable)
    {
        switch (Component)
        {
            case 0:
                    foreach (GameObject Wire in Wires)
                        Wire.SetActive(Enable);
                    break;
            case 1:
                    Button.SetActive(Enable);
                    break;
            case 2:
                    foreach (GameObject Adventure in Adventure)
                        Adventure.SetActive(Enable);
                    break;
            case 4:
                    foreach (GameObject Symbol in Symbols)
                        Symbol.SetActive(Enable);
                    break;
            case 5:
                    foreach (GameObject Alphabet in Alphabet)
                        Alphabet.SetActive(Enable);
                    break;
            case 6:
                    foreach (GameObject Key in Piano)
                        Key.SetActive(Enable);
                    break;
            case 7:
                    foreach (GameObject Arrow in Arrows)
                        Arrow.SetActive(Enable);
                    break;
            case 8:
                    foreach (GameObject Identity in Identity)
                        Identity.SetActive(Enable);
                    break;
            case 10:
                    foreach (GameObject Resistor in Resistor)
                        Resistor.SetActive(Enable);
                    break;
        }
    }

    public void CutWire(int Wire)
    {
        Wires[Wire].transform.Find("WireHL").gameObject.SetActive(false);
        Wires[Wire].GetComponent<MeshFilter>().mesh = WireMesh[1];
        WiresCut.Add(Wire);
    }

    public void CauseButtonStrike(bool IsSymbols,int Button)
    {
        StartCoroutine(ButtonStrike(IsSymbols, Button));
    }

    public IEnumerator ButtonStrike(bool IsSymbols, int Button)
    {
        if (IsSymbols)
        {
            Symbols[Button].transform.Find("SymbolLED").GetComponentInChildren<Renderer>().material = KeyLightMats[6];
            yield return new WaitForSeconds(1f);
            Symbols[Button].transform.Find("SymbolLED").GetComponentInChildren<Renderer>().material = KeyLightMats[0];
        }
        else
        {
            Alphabet[Button].transform.Find("AlphabetLED").GetComponentInChildren<Renderer>().material = KeyLightMats[6];
            yield return new WaitForSeconds(1f);
            Alphabet[Button].transform.Find("AlphabetLED").GetComponentInChildren<Renderer>().material = KeyLightMats[0];
        }
    }

    public void RegenWires()
    {
        Info.RegenWires();
        WiresCut.Clear();
        StartCoroutine(RegenWiresAnim());
    }

    public IEnumerator RegenWiresAnim()
    {
        yield return HideComponent(0);

        for (int i = 0; i < 7; i++)
        {
            int Color1 = Info.Wires[0][i];
            int Color2 = Info.Wires[1][i];
            if (Color1 != Color2)
            {
                Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == ComponentInfo.WireColors[Color1] + "_" + ComponentInfo.WireColors[Color2]).ToArray()[0];
            }
            else
            {
                Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == ComponentInfo.WireColors[Color1]).ToArray()[0];
            }

            Wires[i].transform.Find("WireHL").gameObject.SetActive(true);
            Wires[i].GetComponent<MeshFilter>().mesh = WireMesh[0];
        }

        for (int i = 0; i < WireLED.Length; i++)
        {
            WireLED[i].transform.Find("WireLEDL").GetComponentInChildren<Renderer>().material = WireLEDMats[Info.WireLED[i]];
        }

        yield return ShowComponent(0);
    }

    // Animations but also sets up Puzzle class
    void AssignHandlers()
    {
        Puzzle = new Puzzle(this, ModuleID, Info, true, TargetComponents);

        for (int i = 0; i < Wires.Length; i++)
        {
            int y = i;
            Wires[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate ()
            {
                Puzzle.OnWireCut(y);
                return false;
            };
        }

        Button.GetComponentInChildren<KMSelectable>().OnInteract += delegate ()
        {
            StartCoroutine(AnimateButtonPress(Button.transform, Vector3.down * 0.0014f, 1));
            Puzzle.OnButtonPress();
            return false;
        };

        Button.GetComponentInChildren<KMSelectable>().OnInteractEnded += delegate ()
        {
            StartCoroutine(AnimateButtonPress(Button.transform, Vector3.down * 0.0014f, 2));
            Puzzle.OnButtonRelease();
        };

        for (int i = 0; i < Adventure.Length; i++)
        {
            int y = i;
            Adventure[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate ()
            {
                StartCoroutine(AnimateButtonPress(Adventure[y].transform, Vector3.down * 0.0011f));
                Puzzle.OnAdventurePress(y);
                return false;
            };
        }

        for (int i = 0; i < Symbols.Length; i++)
        {
            int y = i;
            Symbols[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(AnimateButtonPress(Symbols[y].transform, Vector3.down * 0.00258f));
                Puzzle.OnSymbolPress(y);
                return false;
            };
        }

        for (int i = 0; i < Alphabet.Length; i++)
        {
            int y = i;
            Alphabet[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(AnimateButtonPress(Alphabet[y].transform, Vector3.down * 0.00258f));
                Puzzle.OnAlphabetPress(y);
                return false;
            };
        }

        /*for (int x = 0; x < alphabet.Length; x++)
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
    

    // Materials
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

        if (CurrentComponent < 0)
        {
            CurrentComponent += ComponentNames.Length;
        }
        if (CurrentComponent >= ComponentNames.Length)
        {
            CurrentComponent -= ComponentNames.Length;
        }

        DisplayText.text = ComponentNames[CurrentComponent].ToUpper();
        DisplayText.color = OnComponents[CurrentComponent] ? Color.green : Color.red;
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
        Button.transform.GetComponentInChildren<Renderer>().material = ButtonMats[Info.Button];
        Button.transform.Find("ButtonText").GetComponentInChildren<TextMesh>().text = Info.ButtonText;
        if(Info.Button == 0 || Info.Button == 1 || Info.Button == 7)
        {
            Button.transform.Find("ButtonText").GetComponentInChildren<TextMesh>().color = ComponentInfo.ButtonTextWhite;
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

    // Solve/Strike Handling
    public void CauseStrike()
    {
        Module.HandleStrike();
        HasStruck = true;
    }

    public void StartSolve()
    {
        Solving = true;
    }

    // Calculation
    void CalcComponents()
    {
        var SerialNumber = Bomb.GetSerialNumber();
        var SerialNumberPairs = new List<string>();
        // 1. Separate serial number into three pairs
        for (int i = 0; i < SerialNumber.Length; i += 2)
            SerialNumberPairs.Add(SerialNumber.Substring(i, 2));
        var Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // 2. Calculate a puzzle ID from the base36 value
        // Formula : nth pair value, P(n) = base36 of first character * base36 of second character
        // (Separated for convenience): Puzzle ID = ( P(1) + P(2) + P(3) ) % 2048
        //
        // Note: This will need to be modified since serial numbers can't contain O or Y and will
        //       always have at least two numbers and two letters. The puzzle ID range is 100 < x < 1855 as a result
        var Products = SerialNumberPairs.Sum(x => Base36.IndexOf(x[0]) * Base36.IndexOf(x[1])) % 2048;
        TargetComponents = Products.ToString("2").PadLeft(11, '0').Select(x => x == '1').ToArray();
        Debug.LogFormat("[The Cruel Modkit #{0}] Puzzle ID is {1}.", ModuleID, Products.ToString());
    }

    // Logging
    public string GetOnComponents()
    {
        return OnComponents.Any(a => a)
            ? ComponentNames.Where(x => OnComponents[Array.IndexOf(ComponentNames, x)]).Join(", ")
            : "None";
    }

    public string GetTargetComponents()
    {
        return TargetComponents.Any(a => a)
            ? ComponentNames.Where(x => TargetComponents[Array.IndexOf(ComponentNames, x)]).Join(", ")
            : "None";
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