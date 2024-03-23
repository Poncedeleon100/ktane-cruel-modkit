using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class cruelModkitScript : MonoBehaviour {

	public KMAudio Audio;
	public KMBombInfo Bomb;

	public GameObject[] Doors;
	public GameObject[] Components;
	public KMSelectable[] SelectorButtons;
	public KMSelectable[] UtilityButton;
	public TextMesh DisplayText;

	public Material[] WireMats;
	// public Material[] WireLEDMats;

	readonly string[] ComponentNames = new string[] {"WIRES", "BUTTON", "ADVENTURE", "LED", "SYMBOLS", "ALPHABET", "PIANO", "ARROWS", "IDENTITY", "BULBS", "RESISTOR"};
	bool[] OnComponents = new bool[] {false, false, false, false, false, false, false, false, false, false, false};
	int CurrentComponent = 0;

	// Logging
	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;
    private bool Animating = false;
    private bool Solving = false;

	private bool ForceComponents, ForceByModuleID = false;

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

	void Start () {
		if (ForceComponents) { // Check if the components need to be forced on.
			// ForceComponents();
			// displayText.text = "DISABLED";
		}
		else
		{
			// CalcComponents();
			DisplayText.text = ComponentNames[CurrentComponent];
		}
	}
	
	// Triggered once per frame
	void Update () {
		// Ex: How many modules have been solved?
	}
}