using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class CruelModkitScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public KMSelectable[] mainButtons;
	public KMSelectable ultilityButton;

	public TextMesh display;

	public GameObject[] doors, components;

	public Material[] wireMats;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	private bool[] onComponents = new bool[11];
	private bool[] targetComponents;

	private Coroutine isAnimating;

	// These are public variables needed to communicate with the Puzzle class.

	public bool IsModuleSolved() => moduleSolved;
	public bool CheckValidComponents() => onComponents.SequenceEqual(targetComponents);
	public bool IsAnimating() => isAnimating != null;

	void Awake()
    {

		moduleId = moduleIdCounter++;

		/*
		foreach (KMSelectable button in Buttons)
			button.OnInteract() += delegate () { ButtonPress(button); return false; };
		*/

		//Button.OnInteract += delegate () { ButtonPress(); return false; };

    }

	
	void Start()
    {
		var sn = Bomb.GetSerialNumber();

		var snPairs = new List<string>();

		for (int i = 0; i < sn.Length; i+= 2)
			snPairs.Add(sn.Substring(i, 2));

		var base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		var products = snPairs.Sum(x => base36.IndexOf(x[0]) * base36.IndexOf(x[1]) % 2048) % 2048;

		targetComponents = products.ToString("2").PadLeft(11, '0').Select(x => x == '1').ToArray();


    }
	
	
	void Update()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





