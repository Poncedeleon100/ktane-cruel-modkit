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
    public Light MorseLight;
    public Renderer MorseMesh;
    public GameObject Meter;

    public Mesh[] WireMesh;
    List<int> WiresCut = new List<int>();

    //Fixes light sizes on different bomb sizes
    public Light[] LightsArray;

    //Component Selector Info
    [Flags]
    public enum ComponentsEnum : byte {
        Wires = 1,
        Button = 2,
        LED = 4,
        Symbols = 8,
        Alphabet = 16,
        Piano = 32,
        Arrows = 64,
        Bulbs = 128
    }

    public int CountComponents(ComponentsEnum comps) {
        return new BitArray(new[] {(byte)comps}).OfType<bool>().Count(x => x);
    }

    byte OnComponents = 0;
    byte TargetComponents = 0;
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
            SelectModule = "Timer Timings";
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
        StartCoroutine(AnimateButtonPress(SelectorButtons[1].transform, Vector3.down * 0.005f));
        if (ModuleSolved || Solving || ForceComponents || Animating)
        {
            return;
        }
        
        if(((ComponentsEnum)OnComponents & CurrentComponent) == CurrentComponent)
        {
            OnComponents -= (byte)CurrentComponent;
            DisplayText.color = new Color(1, 0, 0);
            StartCoroutine(ShowComponent(CurrentComponent));
        }
        else
        {
            OnComponents += (byte)CurrentComponent;
            DisplayText.color = new Color(0, 1, 0);
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
        Dictionary<ComponentsEnum, float> floats = new Dictionary<ComponentsEnum, float>(){
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
        for (int i = 0; i < 10; i++)
        {
            Components[index].transform.localPosition += new Vector3(0, -0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
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
        Dictionary<ComponentsEnum, float> floats = new Dictionary<ComponentsEnum, float>(){
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
            Doors[index].transform.localPosition += new Vector3(floats[Component], 0, 0);
            yield return new WaitForSeconds(0.025f);
        }
        for (int i = 0; i < 10; i++)
        {
            Components[index].transform.localPosition += new Vector3(0, -0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        Doors[index].SetActive(true);
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
        WiresCut.Add(Wire);
    }

    public IEnumerator ButtonStrike(bool IsSymbols, int Button)
    {
        if (IsSymbols)
        {
            Symbols[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[6];
            yield return new WaitForSeconds(1f);
            Symbols[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[0];
        }
        else
        {
            Alphabet[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[6];
            yield return new WaitForSeconds(1f);
            Alphabet[Button].transform.Find("KeyLED").GetComponentInChildren<Renderer>().material = KeyLightMats[0];
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

    IEnumerator PlaySolveAnim()
    {
        // Solve animation is split into stages so that the doors don't overlap
        for (int i = 0; i < CountComponents((ComponentsEnum)OnComponents); i++)
        {
            if ((i == 4 && (((OnComponents & 24) != 0) || (((OnComponents & 6) != 0) && (((OnComponents & 96) != 0))) || (((OnComponents & 1) != 0) && (((OnComponents & 192) != 0))))))
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
                TargetComponents = 0;
                Puzzle = new TimerTimings(this, ModuleID, Info, true, TargetComponents);
                break;
            case "Test Puzzle":
                TargetComponents = 255;
                Puzzle = new TestPuzzle(this, ModuleID, Info, true, TargetComponents);
                break;
            default:
                TargetComponents = 255;
                Puzzle = new TestPuzzle(this, ModuleID, Info, true, TargetComponents);
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
            StartCoroutine(AnimateButtonPress(Button.transform, Vector3.down * 0.0014f, 1));
            Puzzle.OnButtonPress();
            return false;
        };

        Button.GetComponentInChildren<KMSelectable>().OnInteractEnded += delegate ()
        {
            StartCoroutine(AnimateButtonPress(Button.transform, Vector3.down * 0.0014f, 2));
            Puzzle.OnButtonRelease();
        };

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
                StartCoroutine(AnimateButtonPress(ArrowsBase.transform, Vector3.down * 0.0002f));
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
                StartCoroutine(AnimateButtonPress(Bulbs[y].transform, Vector3.down * 0.001f));
                Puzzle.OnBulbButtonPress(y);
                return false;
            };
        }

        UtilityButton.OnInteract += delegate
        {
            StartCoroutine(AnimateButtonPress(UtilityButton.transform, Vector3.down * 0.00184f));
            Puzzle.OnUtilityPress();
            return false;
        };
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
        if (CurrentComponent == ComponentsEnum.Wires && i == -1)
            CurrentComponent = ComponentsEnum.Bulbs;
        else if (CurrentComponent == ComponentsEnum.Bulbs && i == 1)
            CurrentComponent = ComponentsEnum.Wires;
        else CurrentComponent = (ComponentsEnum)Math.Pow(2, (int)(Math.Log((int)CurrentComponent, 2) + .1) + i);

        DisplayText.text = CurrentComponent.ToString("F").ToUpper();
        DisplayText.color = ((ComponentsEnum)OnComponents & CurrentComponent) == CurrentComponent ? Color.green : Color.red;
    }

    void SetUpComponents()
    {
        Info = new ComponentInfo();
        //Set materials for Wires
        for(int i = 0; i < Wires.Length; i++)
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
            Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().color = Info.ArrowLightColors[Info.Arrows[i]];
            Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().intensity += (Info.Arrows[i] == 8) ? 10 : 0;
        }
        //Set I/O buttons and bulb colors/opacity for Bulbs
        if (Info.BulbInfo[4])
        {
            var p = Bulbs[3].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh;
            Bulbs[3].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh = Bulbs[2].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh;
            Bulbs[2].transform.Find("BulbFace").GetComponentInChildren<MeshFilter>().mesh = p;
        }
        for(int i = 0; i < 2; i++)
        {
            //Set filament visibility based on opacity of the bulb
            Bulbs[i].transform.Find("Filament").gameObject.SetActive(!Info.BulbInfo[i]);
            //Set bulb glass color and opacity
            Color TempBulbColor = Info.BulbColorsArray[Info.BulbColors[i]];
            TempBulbColor[3] = Info.BulbInfo[i] ? 1f : .55f;
            Bulbs[i].transform.Find("Glass").GetComponentInChildren<Renderer>().material.color = TempBulbColor;
            //Set bulb light color
            Bulbs[i].transform.Find("BulbLight").GetComponentInChildren<Light>().color = Bulbs[i].transform.Find("BulbLight2").GetComponentInChildren<Light>().color = TempBulbColor;
            //Turns the lights on or off
            Bulbs[i].transform.Find("BulbLight").GetComponentInChildren<Light>().enabled = Bulbs[i].transform.Find("BulbLight2").GetComponentInChildren<Light>().enabled = Info.BulbInfo[i + 2];
        }
        //Set materials and text for Identity
        Identity[0].transform.GetComponentInChildren<Renderer>().material = IdentityMats[Info.Identity[0]];
        Identity[1].transform.GetComponentInChildren<TextMesh>().text = Info.IdentityItems[Info.Identity[1]];
        Identity[2].transform.GetComponentInChildren<TextMesh>().text = Info.IdentityLocations[Info.Identity[2]];
        Identity[3].transform.GetComponentInChildren<TextMesh>().text = Info.IdentityRarity[Info.Identity[3]];
        //Set materials and text for Resistor
        for (int i = 0; i < ResistorStrips.Length; i++)
            ResistorStrips[i].GetComponentInChildren<Renderer>().material = ResistorMats[Info.ResistorColors[i]];
        for (int i = 0; i < ResistorText.Length; i++)
            ResistorText[i].text = Info.ResistorText[i];
        for (int i = 0; i < Info.ResistorReversed.Length; i++)
        {
            if (Info.ResistorReversed[i])
            {
                ResistorStrips[i + 2].transform.localPosition += new Vector3(.001852f, 0, 0);
                ResistorStrips[i + 4].transform.localPosition += new Vector3(.001852f, 0, 0);
            }
        }
        //Set timer display text
        WidgetText[0].text = Info.TimerDisplay.ToString().PadLeft(5, '0');
        //Set word display text
        WidgetText[1].text = Info.WordDisplay;
        //Set number display text
        WidgetText[2].text = Info.NumberDisplay.ToString();
        //Set morse code display
        StartCoroutine(PlayWord(Info.Morse));
        //Set meter value and color
        Meter.GetComponentInChildren<Renderer>().material = MeterMats[Info.MeterColor];
        //Changes the meter so it matches the value from Info.MeterValue; adjusts scale first then shifts the position down
        float TempNumber = 0.003882663f * (float)Info.MeterValue; //.00388 is the original Z scale
        Meter.transform.localScale = new Vector3(0.0005912599f, 0.01419745f, TempNumber);
        TempNumber = -0.02739999f - ((0.03884f * (1 - (float)Info.MeterValue)) / 2); //-.0273 is the original Z position, .0388 is the original length
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
        //Alternate calculation method: Convert serial number from base36 to base10, then to binary, then calculate the components
        /*double AltPuzzleID = 0;
        foreach (char c in SerialNumber)
            AltPuzzleID += (Base36.IndexOf(c) * Math.Pow(36, (SerialNumber.Length - 1) - SerialNumber.IndexOf(c)));
        var AltPuzzleBinary = Convert.ToString(Convert.ToInt64(AltPuzzleID), 2).PadLeft(8, '0');
        Debug.LogFormat("[The Cruel Modkit #{0}] Alternate Puzzle ID is {1}. Binary conversion is {2}. Trimmed binary number is {3}.", ModuleID, AltPuzzleID, Convert.ToString(Convert.ToInt64(AltPuzzleID), 2), AltPuzzleBinary.Substring(1, 8));
        TargetComponents = AltPuzzleBinary.Substring(1, 8).Select(x => x == '1').ToArray();
        Debug.LogFormat("[The Cruel Modkit #{0}] Alternate calculated components are: [{1}].", ModuleID, GetTargetComponents());*/
    }

    // Logging
    public string GetOnComponents()
    {
        return OnComponents == 0 ? "None" : OnComponents.ToString("F");
    }

    public string GetTargetComponents()
    {
        return TargetComponents == 0 ? "None" : TargetComponents.ToString("F");
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
                    { "DropdownItems", new List<object> { "Timer Timings", "Test Puzzle" } }
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
