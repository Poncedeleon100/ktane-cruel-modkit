using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class cruelModkitScript : MonoBehaviour {

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
	//Here, take these colors too (should probably change this so it's consistent with the Bulb colors)
	public Color[] ArrowLightColors;
	public Material[] IdentityMats;
	public Material[] ResistorMats;
	//public TextMesh[] ResistorText;
	//public TextMesh[] WidgetText;
	public Material[] MorseMats;
	public Material[] MeterMats;

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

	public Mesh[] WireMesh;
	List<int> WiresCut = new List<int>();

	//Component Selector Info
	readonly string[] ComponentNames = new string[] {"WIRES", "BUTTON", "ADVENTURE", "LED", "SYMBOLS", "ALPHABET", "PIANO", "ARROWS", "IDENTITY", "BULBS", "RESISTOR"};
	bool[] OnComponents = new bool[11];
	bool[] TargetComponents = new bool[11];
	int CurrentComponent = 0;

	ComponentInfo Info;
	Puzzle Puzzle;

	// Logging
	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;
    private bool Animating;
    private bool Solving;

	// private bool HasStruck = false; // TP Handling, send a strike handling if the module struck. To prevent excessive inputs.

	// Use these for debugging individual puzzles.
	private bool ForceComponents, ForceByModuleID;
	/*/private bool[] componentsForced;
	public bool enableBruteTest = false;
	ModkitSettings modConfig = new ModkitSettings();/*/

	void Awake () {
		ModuleId = ModuleIdCounter++;
		SelectorButtons[0].OnInteract += delegate () {
			ChangeDisplayComponent(SelectorButtons[0], -1);
			return false;
			};
		SelectorButtons[1].OnInteract += delegate () {
			ToggleComponent();
			return false;
			};
		SelectorButtons[2].OnInteract += delegate() {
			ChangeDisplayComponent(SelectorButtons[2], 1);
			return false;
		};
	}

	void Start () {
		SetUpComponents();
		if (ForceComponents) { // Check if the components need to be forced on.
			// ForceComponents();
			// DisplayText.text = "DISABLED";
		}
		else {
			// CalcComponents();
			DisplayText.text = ComponentNames[CurrentComponent];
		}
	}

	// Triggered once per frame
	void Update () {
		// Ex: How many modules have been solved?
	}

	void ChangeDisplayComponent(KMSelectable Button, int i) {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
        Button.AddInteractionPunch(0.5f);
		StartCoroutine(AnimateButtonPress(Button.transform, Vector3.down * 0.005f));
		if (ModuleSolved || ForceComponents) {
			return;
		}
		CurrentComponent += i;

		if(CurrentComponent < 0) {
			CurrentComponent += ComponentNames.Length;
		}
		if(CurrentComponent >= ComponentNames.Length) {
			CurrentComponent -= ComponentNames.Length;
		}

		DisplayText.text = ComponentNames[CurrentComponent];
		DisplayText.color = OnComponents[CurrentComponent] ? Color.green : Color.red;
	}

	void ToggleComponent() {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SelectorButtons[1].transform);
        SelectorButtons[1].AddInteractionPunch(0.5f);
		StartCoroutine(AnimateButtonPress(SelectorButtons[1].transform, Vector3.down * 0.005f));
		if (ModuleSolved || Solving || ForceComponents || Animating) {
			return;
		}
		OnComponents[CurrentComponent] = !OnComponents[CurrentComponent];
		if(OnComponents[CurrentComponent]) {
			DisplayText.color = new Color(0, 1, 0);
			// StartCoroutine(ShowComponent(CurrentComponent));
		}
		else {
			DisplayText.color = new Color(1, 0, 0);
			// StartCoroutine(HideComponent(CurrentComponent));
		}
	}

	public IEnumerator AnimateButtonPress(Transform Object, Vector3 Offset) {
        for (int x = 0; x < 5; x++) {
			Object.localPosition += Offset / 5;
			yield return new WaitForSeconds(0.01f);
        }
        for (int x = 0; x < 5; x++) {
			Object.localPosition -= Offset / 5;
			yield return new WaitForSeconds(0.01f);
        }
    }

	void SetUpComponents() {
		Info = new ComponentInfo();
		//Set materials for Wires
		for(int i = 0; i < 7; i++) {
			int Color1 = Info.Wires[0][i];
			int Color2 = Info.Wires[1][i];
			if(Color1 != Color2) {
				Wires[i].transform.GetComponentInChildren<Renderer>().material = WireMats.Where(x => x.name == ComponentInfo.WireColors[Color1] + "_" + ComponentInfo.WireColors[Color2]).ToArray()[0];
			}
			else {
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
		if(Info.Button == 0 || Info.Button == 1 || Info.Button == 7) {
			ButtonText.color = ComponentInfo.ButtonTextWhite;
		}
		//Set materials for LEDs
		for(int i = 0; i < LED.Length; i++)
		{
			LED[i].transform.Find("LEDL").GetComponentInChildren<Renderer>().material = LEDMats[Info.LED[i]];
		}
		//Set materials for Symbols
		for(int i = 0; i < Symbols.Length; i++) {
			Symbols[i].transform.Find("Symbol").GetComponentInChildren<Renderer>().material = SymbolMats[Info.Symbols[i]];
		}
		//Set Alphabet text
		for(int i = 0; i < Alphabet.Length; i++) {
			Alphabet[i].transform.Find("AlphabetText").GetComponentInChildren<TextMesh>().text = Info.Alphabet[i];
		}
		//Set materials and light colors for Arrows
		for(int i = 0; i < 9; i++) {
			//x and y are here so that the generated arrow colors can be in a jagged array. In this case, i keeps track of the button from the Unity prefab
			int x = 0;
			int y = 0;
			if(i > 3 && i <= 7) {
				x = 1;
				y = 4;
			}
			else if (i > 7) {
				x = 2;
				y = 8;
			}
			Arrows[i].GetComponentInChildren<Renderer>().material = ArrowMats[Info.Arrows[x][i - y]];
    		Arrows[i].transform.Find("ArrowLight").GetComponentInChildren<Light>().color = ArrowLightColors[Info.Arrows[x][i - y]];
		}
		//Set materials and text for Identity
		Identity[0].transform.Find("IdentityFaceIcon").GetComponentInChildren<Renderer>().material = IdentityMats.Where(x => x.name == Info.Identity[0][0]).ToArray()[0];
		for(int i = 1; i < 4; i++) {
			Identity[i].transform.Find("IdentityText").GetComponentInChildren<TextMesh>().text = Info.Identity[i][0];
		}
		//Set I/O buttons and bulb colors/opacity for Bulbs
		if(Info.BulbOLeft) {
			var p = BulbOFace.position;
			BulbOFace.position = BulbIFace.position;
			BulbIFace.position = p;
		}
		for(int i = 0; i < 2; i++) {
			//Set filament visibility based on opacity of the bulb
			BulbFilaments[i].SetActive(!Info.BulbInfo[i][0]);
			//Set bulb glass color and opacity
			BulbGlass[i].material.color = Info.BulbColors[i];
			//Set bulb light color
			BulbLights[i].color = BulbLights[i + 2].color = Info.BulbColors[i + 2];
		}
		//Set materials and text for Resistor
		for(int i = 0; i < 4; i++) {
			Resistor[i].GetComponentInChildren<Renderer>().material = ResistorMats[Info.ResistorColors[i][0]];
		}
		Resistor[4].GetComponentInChildren<TextMesh>().text = Info.ResistorText[0];
		Resistor[5].GetComponentInChildren<TextMesh>().text = Info.ResistorText[1];
	}
}