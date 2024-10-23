using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static ComponentInfo;

public class CruelModkitScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    //Component Selector
    public GameObject[] Doors;
    public GameObject[] Components;
    public KMSelectable[] SelectorButtons;
    public KMSelectable UtilityButton;
    public TextMesh DisplayText;

    //Materials
    public Material[] WireMats;
    public Material[] WireLEDMats;
    public Material[] ButtonMats;
    public Material[] LEDMats;
    public Material[] KeyLightMats;
    public Material[] SymbolMats;
    public AudioClip[] PianoSounds;
    public Material[] ArrowMats;
    public AudioClip[] ArrowSounds;
    public AudioClip[] BulbSounds;
    public Material[] IdentityMats;
    public Material[] ResistorMats;
    public Material[] MorseMats;
    public Material[] MeterMats;

    //Module Objects
    public GameObject[] Wires;
    public GameObject[] WireLED;
    public GameObject Button;
    public GameObject[] LED;
    public GameObject[] Symbols;
    public GameObject[] Alphabet;
    public GameObject[] Piano;
    public GameObject[] Arrows;
    public Transform ArrowsBase;
    public GameObject[] Bulbs;
    public GameObject[] Identity;
    public GameObject[] ResistorStrips;
    public TextMesh[] ResistorText;
    public TextMesh[] WidgetText;
    public GameObject MorseLED;
    public GameObject Meter;

    public Mesh[] WireMesh;
    public Mesh[] BulbButtonFaceMesh;

    //Fixes light sizes on different bomb sizes
    public Light[] LightsArray;

    //Component Selector Info
    [Flags]
    public enum ComponentsEnum : byte
    {
        Wires = 128,
        Button = 64,
        LED = 32,
        Symbols = 16,
        Alphabet = 8,
        Piano = 4,
        Arrows = 2,
        Bulbs = 1,
        None = 0,
    }

    public int CountComponents(ComponentsEnum comps)
    {
        return new BitArray(new[] {(byte)comps}).OfType<bool>().Count(x => x);
    }

    byte OnComponents = (byte)ComponentsEnum.None;
    byte TargetComponents = (byte)ComponentsEnum.None;
    ComponentsEnum CurrentComponent = ComponentsEnum.Wires;

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
    private IEnumerator MorseCodeAnimation = null;

    // Logging
    static int ModuleIDCounter = 1;
    int ModuleID;
    private bool ModuleSolved;
    private bool Solving;
    private bool Animating;

    // These are public variables needed to communicate with the Puzzle class.
    public bool IsModuleSolved() => ModuleSolved;
    public bool CheckValidComponents()
    {
        return OnComponents == TargetComponents;
    }
    public bool IsAnimating() => Animating;

    private bool HasStruck = false; // TP Handling, send a strike handling if the module struck. To prevent excessive inputs.

    // Use these for debugging individual puzzles.
    readonly private bool ForceComponents, ForceByModuleID;
    CruelModkitSettings ModConfig = new CruelModkitSettings();
    private string SelectModule;

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

        try
        {
            ModConfig<CruelModkitSettings> CruelModkitJSON = new ModConfig<CruelModkitSettings>("CruelModkitSettings");
            ModConfig = CruelModkitJSON.Read();

            SelectModule = ModConfig.SelectModule;
        }
        catch
        {
            Debug.LogErrorFormat("[The Cruel Modkit #{0}] The settings encountered an error and are going back to the default behavior.", ModuleID);
            SelectModule = ""; // Overwrites any value previously entered so that the later switch statement will use "default"
        }
    }

    void Start ()
    {
        //Fixes light sizes on different size bombs
        float scalar = transform.lossyScale.x;
        for (int i = 0; i < LightsArray.Length; i++)
            LightsArray[i].range *= scalar;

        SetUpComponents();
        // Settings
        if (ForceComponents) // Settings - Check if the components need to be forced on.
        {
            // Settings
        }
        else
        {
            CalcComponents();
            DisplayText.text = CurrentComponent.ToString("F");
        }
        AssignHandlers();
        for (int i = 0; i < Components.Length; i++)
        {
            ComponentsEnum comp = (ComponentsEnum)Math.Pow(2, i);
            SetSelectables(comp, ForceComponents ? ((ComponentsEnum)TargetComponents & comp) == comp : false);
            //OnComponents[i] = ForceComponents && TargetComponents[i];
        }
        // Settings
    }

    // Animations
    void ToggleComponent()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SelectorButtons[1].transform);
        SelectorButtons[1].AddInteractionPunch(0.5f);
        StartCoroutine(Puzzle.AnimateButtonPress(SelectorButtons[1].transform, Vector3.down * 0.005f));
        if (ModuleSolved || Solving || ForceComponents || Animating)
            return;
        
        if(((ComponentsEnum)OnComponents & CurrentComponent) == CurrentComponent)
        {
            OnComponents -= (byte)CurrentComponent;
            DisplayText.color = new Color(1, 0, 0);
            StartCoroutine(HideComponent(CurrentComponent));
        }
        else
        {
            OnComponents += (byte)CurrentComponent;
            DisplayText.color = new Color(0, 1, 0);
            StartCoroutine(ShowComponent(CurrentComponent));
        }
    }

    public IEnumerator AnimatePianoPress(Transform PianoKey)
    {
        for (int i = 0; i < 5; i++)
        {
            PianoKey.Rotate(0.6f, 0, 0, Space.Self);
            yield return new WaitForSeconds(0.01f);
        }
        for (int i = 0; i < 5; i++)
        {
            PianoKey.Rotate(-0.6f, 0, 0, Space.Self);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator AnimateButtonRotationPress(Transform Object, Vector3 Angle)
    {
        for (int i = 0; i < 5; i++)
        {
            Object.localEulerAngles += Angle / 5;
            yield return new WaitForSeconds(0.01f);
        }
        for (int i = 0; i < 5; i++)
        {
            Object.localEulerAngles -= Angle / 5;
            yield return new WaitForSeconds(0.01f);
        }
    }
    public void HandleBulbScrew(int Bulb, bool ScrewIn, bool KeepLightOn = true)
    {
        StartCoroutine(AnimateBulbScrew(Bulb, ScrewIn, KeepLightOn));
    }

    public IEnumerator AnimateBulbScrew(int Bulb, bool ScrewIn, bool KeepLightOn = true)
    {
        Animating = true;

        if (ScrewIn)
            Bulbs[Bulb].transform.Find("BulbLight").GetComponentInChildren<Light>().enabled = Bulbs[Bulb].transform.Find("BulbLight2").GetComponentInChildren<Light>().enabled = false;

        for (int i = 0; i < 100; i++)
        {
            Bulbs[Bulb].transform.localEulerAngles += new Vector3(0, (ScrewIn ? 540 : -540), 0) / 100;
            Bulbs[Bulb].transform.localPosition += new Vector3(0, (ScrewIn ? 0.0055f : -0.0055f), 0) / 100;
            yield return new WaitForSeconds(0.01f);
        }

        if (!ScrewIn)
            Bulbs[Bulb].transform.Find("BulbLight").GetComponentInChildren<Light>().enabled = Bulbs[Bulb].transform.Find("BulbLight2").GetComponentInChildren<Light>().enabled = KeepLightOn;

        Animating = false;
    }

    public IEnumerator ShowComponent(ComponentsEnum Component)
    {
        Animating = true;
        Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        Dictionary<ComponentsEnum, float> floats = new Dictionary<ComponentsEnum, float>()
        {
            {ComponentsEnum.Wires,    -.08f   },
            {ComponentsEnum.Button,   -.0374f },
            {ComponentsEnum.LED,      -.08f   },
            {ComponentsEnum.Symbols,  -.08f   },
            {ComponentsEnum.Alphabet,  .08f   },
            {ComponentsEnum.Piano,     .08f   },
            {ComponentsEnum.Arrows,    .0489f },
            {ComponentsEnum.Bulbs,     .0237f }
        };
        int index = floats.Keys.ToList().IndexOf(Component);
        SetSelectables(Component, true);
        Doors[index].transform.localPosition += new Vector3(0, -0.001f, 0);
        for (int i = 0; i < 10; i++)
        {
            Doors[index].transform.localPosition += new Vector3(floats[Component], 0, 0);
            yield return new WaitForSeconds(0.025f);
        }
        Doors[index].SetActive(false);
        for (int i = 0; i < 10; i++)
        {
            Components[index].transform.localPosition += new Vector3(0, 0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        Animating = false;
    }

    // "Wires", "Button", "LED", "Symbols", "Alphabet", "Piano", "Arrows", "Bulbs"
    public IEnumerator HideComponent(ComponentsEnum Component)
    {
        Animating = true;
        Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        Dictionary<ComponentsEnum, float> floats = new Dictionary<ComponentsEnum, float>()
        {
            {ComponentsEnum.Wires,     .08f   },
            {ComponentsEnum.Button,    .0374f },
            {ComponentsEnum.LED,       .08f   },
            {ComponentsEnum.Symbols,   .08f   },
            {ComponentsEnum.Alphabet, -.08f   },
            {ComponentsEnum.Piano,    -.08f   },
            {ComponentsEnum.Arrows,   -.0489f },
            {ComponentsEnum.Bulbs,    -.0237f }
        };
        int index = floats.Keys.ToList().IndexOf(Component);

        for (int i = 0; i < 10; i++)
        {
            Components[index].transform.localPosition += new Vector3(0, -0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        Doors[index].SetActive(true);
        for (int i = 0; i < 10; i++)
        {
            Doors[index].transform.localPosition += new Vector3(floats[Component], 0, 0);
            yield return new WaitForSeconds(0.025f);
        }
        
        Doors[index].transform.localPosition += new Vector3(0, 0.001f, 0);
        SetSelectables(Component, false);
        Animating = false;
    }

    public void SetSelectables(ComponentsEnum Component, bool Enable)
    {
        switch (Component)
        {
            case ComponentsEnum.Wires:
                    foreach (GameObject Wire in Wires)
                        Wire.SetActive(Enable);
                    break;
            case ComponentsEnum.Button:
                    Button.SetActive(Enable);
                    break;
            case ComponentsEnum.Symbols:
                    foreach (GameObject Symbol in Symbols)
                        Symbol.SetActive(Enable);
                    break;
            case ComponentsEnum.Alphabet:
                    foreach (GameObject Alphabet in Alphabet)
                        Alphabet.SetActive(Enable);
                    break;
            case ComponentsEnum.Piano:
                    foreach (GameObject Key in Piano)
                        Key.SetActive(Enable);
                    break;
            case ComponentsEnum.Arrows:
                    foreach (GameObject Arrow in Arrows)
                        Arrow.SetActive(Enable);
                    break;
            case ComponentsEnum.Bulbs:
                    foreach (GameObject Bulbs in Bulbs)
                        Bulbs.SetActive(Enable);
                    break;
        }
    }

    public void CutWire(int Wire)
    {
        Wires[Wire].transform.Find("WireHL").gameObject.SetActive(false);
        Wires[Wire].GetComponent<MeshFilter>().mesh = WireMesh[1];
    }

    public IEnumerator ButtonStrike(bool IsSymbols, int Button)
    {
        if (IsSymbols)
        {
            Symbols[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[(int)KeyColors.Red];
            yield return new WaitForSeconds(1f);
            Symbols[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[(int)KeyColors.Black];
        }
        else
        {
            Alphabet[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[(int)KeyColors.Red];
            yield return new WaitForSeconds(1f);
            Alphabet[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[(int)KeyColors.Black];
        }
    }

    public void RegenWires()
    {
        StartCoroutine(RegenWiresAnim());
    }

    private IEnumerator RegenWiresAnim()
    {
        yield return HideComponent(ComponentsEnum.Wires);

        SetWires();
        SetWireLEDs();
        ResetWires();

        yield return ShowComponent(ComponentsEnum.Wires);
    }

    public void RegenButton()
    {
        StartCoroutine(RegenButtonAnim());
    }

    private IEnumerator RegenButtonAnim()
    {
        yield return HideComponent(ComponentsEnum.Button);

        SetButton();

        yield return ShowComponent(ComponentsEnum.Button);
    }

    public void RegenSymbols()
    {
        StartCoroutine(RegenSymbolsAnim());
    }

    private IEnumerator RegenSymbolsAnim()
    {
        yield return HideComponent(ComponentsEnum.Symbols);

        SetSymbols();

        yield return ShowComponent(ComponentsEnum.Symbols);
    }

    public void RegenAlphabet()
    {
        StartCoroutine(RegenAlphabetAnim());
    }

    private IEnumerator RegenAlphabetAnim()
    {
        yield return HideComponent(ComponentsEnum.Alphabet);

        SetAlphabet();

        yield return ShowComponent(ComponentsEnum.Alphabet);
    }

    public void RegenArrows()
    {
        StartCoroutine(RegenArrowsAnim());
    }

    private IEnumerator RegenArrowsAnim()
    {
        yield return HideComponent(ComponentsEnum.Arrows);

        SetArrows();

        yield return ShowComponent(ComponentsEnum.Arrows);
    }

    public void RegenBulbs()
    {
        StartCoroutine(RegenBulbsAnim());
    }

    private IEnumerator RegenBulbsAnim()
    {
        yield return HideComponent(ComponentsEnum.Bulbs);

        SetBulbs();

        yield return ShowComponent(ComponentsEnum.Bulbs);
    }

    private void ResetWires()
    {
        for (int i = 0; i < Wires.Length; i++)
        {
            Wires[i].transform.Find("WireHL").gameObject.SetActive(true);
            Wires[i].GetComponent<MeshFilter>().mesh = WireMesh[0];
        }
    }

    IEnumerator PlaySolveAnim()
    {
        // Animation must pause halfway through if adjacent components are active
        // Symbols and Alphabet
        bool Pause1 = (OnComponents & (byte)(ComponentsEnum.Symbols | ComponentsEnum.Alphabet)) == (byte)(ComponentsEnum.Symbols | ComponentsEnum.Alphabet);
        // Button/LED and Piano
        bool Pause2 = (OnComponents & (byte)ComponentsEnum.Piano) != 0 && (OnComponents & (byte)(ComponentsEnum.Button | ComponentsEnum.LED)) != 0;
        // Wires/Button and Arrows
        bool Pause3 = (OnComponents & (byte)ComponentsEnum.Arrows) != 0 && (OnComponents & (byte)(ComponentsEnum.Wires | ComponentsEnum.Button)) != 0;
        // Wires and Bulbs
        bool Pause4 = (OnComponents & (byte)(ComponentsEnum.Wires | ComponentsEnum.Bulbs)) == (byte)(ComponentsEnum.Wires | ComponentsEnum.Bulbs);
        for (int i = 7; i > -1; i--)
        {
            if ((i == 3 && (Pause1 | Pause2 | Pause3 | Pause4)))
                yield return new WaitForSeconds(1f);
            if ((OnComponents & (byte)Math.Pow(2, i)) != 0)
                StartCoroutine(HideComponent((ComponentsEnum)Math.Pow(2, i)));
        }
    }

    // Animations but also sets up Puzzle class
    void AssignHandlers()
    {
        switch (SelectModule)
        {
            case "Timer Timings":
                TargetComponents = (byte)(ComponentsEnum.None);
                Puzzle = new TimerTimings(this, ModuleID, Info, TargetComponents);
                break;
            case "Unscrew Maze":
                TargetComponents = (byte)(ComponentsEnum.Arrows | ComponentsEnum.Bulbs);
                Puzzle = new UnscrewMaze(this, ModuleID, Info, TargetComponents);
                break;
            case "Piano Decryption":
                TargetComponents = (byte)(ComponentsEnum.Piano);
                Puzzle = new PianoDecryption(this, ModuleID, Info, TargetComponents);
                break;
            case "AV Input":
                TargetComponents = (byte)(ComponentsEnum.Piano | ComponentsEnum.Bulbs);
                Puzzle = new AVInput(this, ModuleID, Info, TargetComponents);
                break;
            case "Who's Who":
                TargetComponents = (byte)(ComponentsEnum.LED | ComponentsEnum.Bulbs);
                Puzzle = new WhosWho(this, ModuleID, Info, TargetComponents);
                break;
            case "Simon Skips":
                TargetComponents = (byte)(ComponentsEnum.LED | ComponentsEnum.Arrows);
                Puzzle = new SimonSkips(this, ModuleID, Info, TargetComponents);
                break;
            case "Metered Button":
                TargetComponents = (byte)(ComponentsEnum.Button);
                Puzzle = new MeteredButton(this, ModuleID, Info, TargetComponents);
                break;
            case "Stumbling Symphony":
                TargetComponents = (byte)(ComponentsEnum.Button | ComponentsEnum.Piano);
                Puzzle = new StumblingSymphony(this, ModuleID, Info, TargetComponents);
                break;
            case "Deranged Keypad":
                TargetComponents = (byte)(ComponentsEnum.Button | ComponentsEnum.Alphabet);
                Puzzle = new DerangedKeypad(this, ModuleID, Info, TargetComponents);
                break;
            case "Logical Color Combinations":
                TargetComponents = (byte)(ComponentsEnum.Button | ComponentsEnum.LED | ComponentsEnum.Arrows);
                Puzzle = new LogicalColorCombinations(this, ModuleID, Info, TargetComponents);
                break;
            case "Lying Wires":
                TargetComponents = (byte)(ComponentsEnum.Wires | ComponentsEnum.Button);
                Puzzle = new LyingWires(this, ModuleID, Info, TargetComponents);
                break;
            case "Test Puzzle":
            default:
                TargetComponents = (byte)(ComponentsEnum.Wires | ComponentsEnum.Button | ComponentsEnum.LED | ComponentsEnum.Symbols | ComponentsEnum.Alphabet | ComponentsEnum.Piano | ComponentsEnum.Arrows | ComponentsEnum.Bulbs);
                Puzzle = new TestPuzzle(this, ModuleID, Info, TargetComponents);
                break;
        }

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
            StartCoroutine(Puzzle.AnimateButtonPress(Button.transform, Vector3.down * 0.0014f, 1));
            Puzzle.OnButtonPress();
            return false;
        };

        Button.GetComponentInChildren<KMSelectable>().OnInteractEnded += delegate ()
        {
            StartCoroutine(Puzzle.AnimateButtonPress(Button.transform, Vector3.down * 0.0014f, 2));
            Puzzle.OnButtonRelease();
        };

        for (int i = 0; i < Symbols.Length; i++)
        {
            int y = i;
            Symbols[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(Puzzle.AnimateButtonPress(Symbols[y].transform, Vector3.down * 0.00258f));
                Puzzle.OnSymbolPress(y);
                return false;
            };
        }

        for (int i = 0; i < Alphabet.Length; i++)
        {
            int y = i;
            Alphabet[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(Puzzle.AnimateButtonPress(Alphabet[y].transform, Vector3.down * 0.00258f));
                Puzzle.OnAlphabetPress(y);
                return false;
            };
        }

        for (int i = 0; i < Piano.Length; i++)
        {
            int y = i;
            Piano[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(AnimatePianoPress(Piano[y].transform));
                Puzzle.OnPianoPress(y);
                return false;
            };
        }

        for (int i = 0; i < Arrows.Length; i++)
        {
            int y = i;
            Arrows[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(Puzzle.AnimateButtonPress(ArrowsBase.transform, Vector3.down * 0.0002f));
                StartCoroutine(AnimateButtonRotationPress(ArrowsBase.transform, new[] { Vector3.right, Vector3.back, Vector3.left, Vector3.forward , Vector3.right + Vector3.back, Vector3.left + Vector3.back, Vector3.left + Vector3.forward, Vector3.right + Vector3.forward, Vector3.zero }.ElementAt(y) * 5));
                Puzzle.OnArrowPress(y);
                return false;
            };
        }

        for (int i = 0; i < 2; i++)
        {
            int y = i;
            Bulbs[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                Puzzle.OnBulbInteract(y);
                return false;
            };
        }

        for (int i = 2; i < 4; i++)
        {
            int y = i;
            Bulbs[i].GetComponentInChildren<KMSelectable>().OnInteract += delegate
            {
                StartCoroutine(Puzzle.AnimateButtonPress(Bulbs[y].transform, Vector3.down * 0.001f, 1));
                Puzzle.OnBulbButtonPress(y);
                return false;
            };
            Bulbs[i].GetComponentInChildren<KMSelectable>().OnInteractEnded += delegate
            {
                StartCoroutine(Puzzle.AnimateButtonPress(Bulbs[y].transform, Vector3.down * 0.001f, 2));
                Puzzle.OnBulbButtonRelease(y);
            };
        }

        UtilityButton.OnInteract += delegate
        {
            StartCoroutine(Puzzle.AnimateButtonPress(UtilityButton.transform, Vector3.down * 0.00184f));
            Puzzle.OnUtilityPress();
            return false;
        };
    }
    

    // Materials
    void ChangeDisplayComponent(KMSelectable Button, int i)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
        Button.AddInteractionPunch(0.5f);
        StartCoroutine(Puzzle.AnimateButtonPress(Button.transform, Vector3.down * 0.005f));
        if (ModuleSolved || ForceComponents)
            return;

        if (CurrentComponent == ComponentsEnum.Wires && i == -1)
            CurrentComponent = ComponentsEnum.Bulbs;
        else if (CurrentComponent == ComponentsEnum.Bulbs && i == 1)
            CurrentComponent = ComponentsEnum.Wires;
        else CurrentComponent = (ComponentsEnum)Math.Pow(2, (int)(Math.Log((int)CurrentComponent, 2) + .1) - i);

        DisplayText.text = CurrentComponent.ToString("F").ToUpper();
        DisplayText.color = ((ComponentsEnum)OnComponents & CurrentComponent) == CurrentComponent ? Color.green : Color.red;
    }

    void SetUpComponents()
    {
        Info = new ComponentInfo();
        SetWires();
        SetWireLEDs();
        SetButton();
        SetLEDs();
        SetSymbols();
        SetAlphabet();
        SetArrows();
        SetBulbs();
        SetResistors();
        SetIdentity();
        SetTimer();
        SetWord();
        SetNumber();
        SetMorse();
        SetMeter();
    }

    public void SetWires()
    {
        for (int i = 0; i < Wires.Length; i++)
        {
            int Color1 = Info.Wires[0][i];
            int Color2 = Info.Wires[1][i];
            if (Color1 != Color2)
                Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == Enum.GetName(typeof(WireColors), Color1) + "_" + Enum.GetName(typeof(WireColors), Color2)).ToArray()[0];
            else
                Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == Enum.GetName(typeof(WireColors), Color1)).ToArray()[0];
        }
    }

    public void SetWireLEDs()
    {
        for (int i = 0; i < WireLED.Length; i++)
            WireLED[i].transform.Find("WireLEDL").GetComponentInChildren<Renderer>().material = WireLEDMats[Info.WireLED[i]];
    }

    public void SetButton()
    {
        int[] DarkColors = { (int)MainColors.Black, (int)MainColors.Blue, (int)MainColors.Purple };
        Button.transform.GetComponentInChildren<Renderer>().material = ButtonMats[Info.Button];
        Button.transform.Find("ButtonText").GetComponentInChildren<TextMesh>().text = Info.ButtonText;
        Button.transform.Find("ButtonText").GetComponentInChildren<TextMesh>().color = Array.IndexOf(DarkColors, Info.Button) >= 0 ? new Color(1, 1, 1) : new Color(0, 0, 0);
    }

    public void SetLEDs()
    {
        for (int i = 0; i < LED.Length; i++)
            LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = LEDMats[Info.LED[i]];
    }

    public void SetSymbols()
    {
        for (int i = 0; i < Symbols.Length; i++)
            Symbols[i].transform.Find("Symbol").GetComponentInChildren<Renderer>().material = SymbolMats[Info.Symbols[i]];
    }

    public void SetAlphabet()
    {
        for (int i = 0; i < Alphabet.Length; i++)
            Alphabet[i].transform.Find("AlphabetText").GetComponentInChildren<TextMesh>().text = Info.Alphabet[i];
    }

    public void SetArrows()
    {
        for (int i = 0; i < 9; i++)
        {
            Arrows[i].GetComponentInChildren<Renderer>().material = ArrowMats[Info.Arrows[i]];
            Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().color = ArrowLightColors[Info.Arrows[i]];
            Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().intensity += (Info.Arrows[i] == (int)ArrowColors.Grey) ? 10 : 0; // Grey is too dark, so the light needs to be brighter
        }
    }

    public void SetBulbs()
    {
        // Swaps I/O symbols if necessary
        if (Info.BulbInfo[4])
        {
            Bulbs[2].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh = BulbButtonFaceMesh[0];
            Bulbs[3].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh = BulbButtonFaceMesh[1];
        }
        else
        {
            Bulbs[2].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh = BulbButtonFaceMesh[1];
            Bulbs[3].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh = BulbButtonFaceMesh[0];
        }
        for (int i = 0; i < 2; i++)
        {
            //Set filament visibility based on opacity of the bulb
            Bulbs[i].transform.Find("Filament").gameObject.SetActive(!Info.BulbInfo[i]);
            //Set bulb glass color and opacity
            Color TempBulbColor = BulbColorValues[Info.BulbColors[i]];
            TempBulbColor[3] = Info.BulbInfo[i] ? 1f : .55f;
            Bulbs[i].transform.Find("Glass").GetComponentInChildren<Renderer>().material.color = TempBulbColor;
            //Set bulb light color
            Bulbs[i].transform.Find("BulbLight").GetComponentInChildren<Light>().color = Bulbs[i].transform.Find("BulbLight2").GetComponentInChildren<Light>().color = TempBulbColor;
            //Turns the lights on or off
            Bulbs[i].transform.Find("BulbLight").GetComponentInChildren<Light>().enabled = Bulbs[i].transform.Find("BulbLight2").GetComponentInChildren<Light>().enabled = Info.BulbInfo[i + 2];
        }
    }

    public void SetResistors()
    {
        for (int i = 0; i < ResistorStrips.Length; i++)
            ResistorStrips[i].GetComponentInChildren<Renderer>().material = ResistorMats[Info.ResistorColors[i]];
        for (int i = 0; i < ResistorText.Length; i++)
            ResistorText[i].text = Info.ResistorText[i];
        for (int i = 0; i < Info.ResistorReversed.Length; i++)
        {
            float[] DefaultResistorXValue = { -0.02438f, -0.022356f };
            float ShiftValue = .001852f;
            Vector3[] ResistorPosition = { ResistorStrips[i + 2].transform.localPosition, ResistorStrips[i + 4].transform.localPosition };

            for (int j = 0; j < ResistorPosition.Length; j++)
            {
                ResistorPosition[j].x = DefaultResistorXValue[j] + (Info.ResistorReversed[i] ? ShiftValue : 0);
            }

            ResistorStrips[i + 2].transform.localPosition = ResistorPosition[0];
            ResistorStrips[i + 4].transform.localPosition = ResistorPosition[1];
        }
    }

    public void SetIdentity()
    {
        Identity[0].transform.GetComponentInChildren<Renderer>().material = IdentityMats[Info.Identity[0]];
        Identity[1].transform.GetComponentInChildren<TextMesh>().text = IdentityItems[Info.Identity[1]];
        Identity[2].transform.GetComponentInChildren<TextMesh>().text = IdentityLocations[Info.Identity[2]];
        Identity[3].transform.GetComponentInChildren<TextMesh>().text = IdentityRarity[Info.Identity[3]];
    }

    public void SetTimer()
    {
        WidgetText[0].text = Info.TimerDisplay.ToString().PadLeft(5, '0');
    }

    public void SetWord()
    {
        WidgetText[1].text = Info.WordDisplay;
    }

    public void SetNumber()
    {
        WidgetText[2].text = Info.NumberDisplay.ToString();
    }

    public void SetMorse()
    {
        // End the coroutine in case it's currently playing to prevent the light from doing weird stuff
        try
        {
            StopCoroutine(MorseCodeAnimation);
        }
        catch (NullReferenceException)
        {
            // NullReferenceException here is pretty chill since MorseCodeAnimation starts as null
            // Anything else and we're in big trouble
        }
        MorseLED.transform.Find("MorseBulbLight").GetComponentInChildren<Light>().enabled = false;
        MorseLED.transform.GetComponentInChildren<MeshRenderer>().material = MorseMats[0];
        MorseCodeAnimation = PlayWord(Info.Morse);
        StartCoroutine(MorseCodeAnimation);
    }

    public IEnumerator PlayWord(string Word)
    {
        while (true)
        {
            yield return new WaitForSeconds(MorseCodeDotLength * 6);  // 10 dots total
            foreach (var c in Word)
            {
                var Code = MorseCodeTable[char.ToUpper(c)];
                foreach (var Symbol in Code)
                {
                    MorseLED.transform.Find("MorseBulbLight").GetComponentInChildren<Light>().enabled = true;
                    MorseLED.transform.GetComponentInChildren<MeshRenderer>().material = MorseMats[1];
                    yield return new WaitForSeconds(Symbol == Symbol.Dot ? MorseCodeDotLength : MorseCodeDotLength * 3);
                    MorseLED.transform.Find("MorseBulbLight").GetComponentInChildren<Light>().enabled = false;
                    MorseLED.transform.GetComponentInChildren<MeshRenderer>().material = MorseMats[0];
                    yield return new WaitForSeconds(MorseCodeDotLength);
                }
                yield return new WaitForSeconds(MorseCodeDotLength * 3);  // 4 dots total
            }
        }
    }

    public void SetMeter()
    {
        Meter.GetComponentInChildren<Renderer>().material = MeterMats[Info.MeterColor];

        Vector3 MeterScale = Meter.transform.localScale;
        Vector3 MeterPosition = Meter.transform.localPosition;

        MeterScale.z *= (float)Info.MeterValue;
        Meter.transform.localScale = MeterScale;

        float DefaultMeterLength = 0.03884f;
        MeterPosition.z -= ((DefaultMeterLength * (1 - (float)Info.MeterValue)) / 2);
        Meter.transform.localPosition = MeterPosition;
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

    public bool IsSolving()
    {
        return Solving;
    }

    public void Solve() // Disarms The Cruel Modkit
    {
        Module.HandlePass();
        ModuleSolved = true;
        StartCoroutine(PlaySolveAnim());
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
        // (Separated for convenience): Puzzle ID = ( P(1) + P(2) + P(3) ) % 256
        var Products = SerialNumberPairs.Sum(x => Base36.IndexOf(x[0]) * Base36.IndexOf(x[1])) % 256;
        Debug.LogFormat("[The Cruel Modkit #{0}] Puzzle ID is {1}. Binary conversion is {2}.", ModuleID, Products.ToString(), Convert.ToString(Products, 2).PadLeft(8, '0'));
        TargetComponents = (byte)Products;
        Debug.LogFormat("[The Cruel Modkit #{0}] Calculated components are: [{1}].", ModuleID, GetTargetComponents());
    }

    // Logging
    public string GetOnComponents()
    {
        return ((ComponentsEnum)OnComponents).ToString("G");
    }

    public string GetTargetComponents()
    {
        return ((ComponentsEnum)TargetComponents).ToString("G");
    }

    // Mod settings
    public static readonly Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "CruelModkitSettings.json" },
            { "Name", "Cruel Modkit Settings" },
            { "Listings", new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Key", "SelectModule" },
                    { "Text", "Select Module" },
                    { "Description", "Select the module that is chosen when testing The Cruel Modkit." },
                    { "Type", "Dropdown" },
                    { "DropdownItems", new List<object> { "Timer Timings", "Unscrew Maze", "Piano Decryption", "AV Input", "Who's Who", "Simon Skips", "Metered Button", "Stumbling Symphony", "Deranged Keypad", "Logical Color Combinations", "Test Puzzle" } }
                },
            }
            },
        }
    };

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
