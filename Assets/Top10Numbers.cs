using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class Top10Numbers : MonoBehaviour {

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBossModule Boss;
	public KMBombInfo Bomb;
	private string[] ignoredModules = { "Top 10 Numbers", "OmegaForget", "14", " Bamboozling Time Keeper", " Brainf---", " Forget Enigma", " Forget Everything", " Forget It Not", " Forget Me Not", " Forget Me Later", " Forget Perspective", " Forget The Colors", " Forget Them All", " Forget This", " Forget Us Not", " Iconic", " Organization", " Purgatory", " RPS Judging", " Simon Forgets", " Simon's Stages", " Souvenir", " Tallordered Keys", " The Time Keeper", " The Troll", " The Twin", " The Very Annoying Button", " Timing Is Everything", " Turn The Key", " Ultimate Custom Night", "Übermodule" };
	public KMSelectable[] Buttons;
	static private int _moduleIdCounter = 1;
	private int _moduleId;
	private static readonly string[] Binary = {"0001", "0010", "0011", "0100", "0101", "0110", "1000", "1001", "1010", "1100"};
	private int[] NumberPriority = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
	private int Stage, Stage2, maxStage, stageQueue, DislikeCooldown = 8;
	public TextMesh[] Texts;
	private List<int> StageNums = new List<int>(), StageAnswers = new List<int>(), IgnoreNums = new List<int>(), Answers = new List<int>();
	private List<bool> StageBools = new List<bool>();
	private bool solved;
	void Awake()
	{
		_moduleId = _moduleIdCounter++;
		string[] ingore = Boss.GetIgnoredModules(Module, ignoredModules);
		if (ingore != null)
			ignoredModules = ingore;

		for (byte i = 0; i < Buttons.Length; i++)
		{
			KMSelectable btn = Buttons[i];
			btn.OnInteract += delegate
			{
				HandlePress(btn);
				return false;
			};
		}
	}
	void HandlePress(KMSelectable btn)
	{
		int index = Array.IndexOf(Buttons, btn);
		Audio.PlaySoundAtTransform((index + 1).ToString(),btn.transform);
		btn.AddInteractionPunch(.1f);
		if (Stage == maxStage && Stage2 == maxStage && !solved)
		{
			Answers.Add(index + 1);
			if (Answers.Join("") == StageAnswers.GetRange(0, Answers.Count).Join(""))
			{
				Debug.LogFormat("[Top 10 Numbers #{0}]: Correct! Number {1} is number {2}!", _moduleId, maxStage - Answers.Count + 1, Answers.Last());
				Texts[0].text = (maxStage - Answers.Count).ToString();
				Texts[1].text = "-";
				Texts[1].color = new Color32(0, 86, 255,255);
				if (Answers.Count() == StageAnswers.Count())
				{
					Debug.LogFormat("[Top 10 Numbers #{0}]: Thank you for joining me on this top {1} today!", _moduleId, maxStage);
					Module.HandlePass();
					StartCoroutine(SolveLoop());
					solved = true;
				}
			}
            else
            {
				Debug.LogFormat("[Top 10 Numbers #{0}]: Oop. Number {1} is number {2}, not number {3}!", _moduleId, maxStage - Answers.Count + 1, StageAnswers[Answers.Count - 1], Answers.Last());
				Module.HandleStrike();
				Answers = Answers.GetRange(0, Answers.Count - 1);
				Texts[1].text = StageAnswers[Answers.Count].ToString();
                if(StageBools[Answers.Count]) Texts[1].color = new Color32(255, 0, 0, 255);
			}
		}
	}
	IEnumerator SolveLoop()
    {
		yield return null;
		Texts[1].color = new Color32(145, 65, 186, 255);
        while (true)
        {
			for(int i = maxStage; i > 0; i--)
            {
				Texts[0].text = i.ToString();
				Texts[1].text = StageAnswers[maxStage - i].ToString();
				yield return new WaitForSeconds(0.5f);
            }
        }
    }
	// Use this for initialization
	void Start()
	{
		maxStage = Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count();
		if (maxStage == 0)
		{
			Debug.LogFormat("[Top 10 Numbers #{0}]: Hey everyone! It's time for another top 10, and this week we're kic- Oh? You didn't put other modules on this bomb? *sigh* Autosolving...", _moduleId);
			solved = true;
			Module.HandlePass();
		}
		else
		{
			Debug.LogFormat("[Top 10 Numbers #{0}]: NOTE! PHRASE SUGGESTIONS OPEN! IF YOU WANT YOUR PHASE IN THIS MODULE, DM ME (Cooldoom5#0789) WITH THE PHRASE AND WHERE!", _moduleId);
			Debug.LogFormat("[Top 10 Numbers #{0}]: Hey everyone! It's time for another top {1}, and this week we're kicking it old school! With a classic that is backkick, top {1} numbers, from 1 through 10!", _moduleId, maxStage);
			StartCoroutine(TickDown());
			stageQueue++;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Stage2 < Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() && !solved)
		{
			Stage2++;
			if (Stage2 != maxStage)
				stageQueue++;
		}
	}
	IEnumerator TickDown()
    {
		while (true)
        {
			yield return null;
			if (stageQueue > 0)
            {
				GenerateStage();
				yield return new WaitForSeconds(1f);
				Stage++;
				stageQueue--;
			}
			if (Stage == maxStage && Stage2 == maxStage)
				break;
		}
		Texts[0].color = new Color32(247, 13, 186, 255);
		Texts[0].text = maxStage.ToString();
		Texts[1].text = "-";
	}

	void GenerateStage()
    {
		StageNums.Add(Rnd.Range(0, 10));
		Texts[1].text = (StageNums.Last() + 1).ToString();
		Texts[0].text = (maxStage - Stage).ToString();
		if (DislikeCooldown == 0)
		{
			Texts[1].color = new Color32(255, 0, 0, 255);
			DislikeCooldown = Rnd.Range(7, 14);
			StageBools.Add(true);
			DislikeNums();
		}
		else
		{
			StageBools.Add(false);
			DislikeCooldown--;
			Texts[1].color = new Color32(0, 86, 255, 255);
			if (Array.IndexOf(IgnoreNums.ToArray(), NumberPriority[StageNums.Last()]) == -1 || StageNums.Last() == 0)
			{
				StageAnswers.Add(NumberPriority[StageNums.Last()]);
				MoveToBack(StageNums.Last());
			}
			else if (Array.IndexOf(IgnoreNums.ToArray(), NumberPriority[StageNums.Last() - 1]) == -1 || StageNums.Last() == 1)
			{
				StageAnswers.Add(NumberPriority[StageNums.Last() - 1]);
				MoveToBack(StageNums.Last() - 1);
			}
			else
			{
				StageAnswers.Add(NumberPriority[StageNums.Last() - 2]);
				MoveToBack(StageNums.Last() - 2);
			}
		}
		LoggingDecider();

	}

	void MoveToBack(int Position)
    {
		int store = NumberPriority[Position];
		for (int i = Position; i < 9; i++)
		NumberPriority[i] = NumberPriority[i + 1];
		NumberPriority[9] = store;
	}

	void DislikeNums()
    {
		IgnoreNums = new List<int>();
		string str = Binary[StageNums.Last()];
		for(int i = 0; i < 4; i++)
        {
			int m = 0;
			int store = 0;
			if (str[i] == '1')
			{
				switch (i)
				{
					case 0: IgnoreNums.Add(StageAnswers.Last());  break;
					case 1: IgnoreNums.Add(NumberPriority[0]);  break;
					case 2:
						m = -1;
						store = 0;
						for (int j = 0; j < 10; j++)
						{
							int k = CountOcc(StageNums, NumberPriority[9-j]);
							if (k < m || m == -1)
							{
								m = k;
								store = NumberPriority[9-j];
							}
						}
						IgnoreNums.Add(store);
						break;
					case 3:
						m = 0;
						store = 0;
						for (int j = 0; j < 10; j++)
						{
							int k = CountOcc(StageNums, NumberPriority[j]);
							if (k > m)
							{
								m = k;
								store = NumberPriority[j];
							}
						}
						IgnoreNums.Add(store);
						break;
				}
			}
		 }
		if (IgnoreNums.Count == 1)
            switch (Rnd.Range(0, 3))
            {
				case 0: Debug.LogFormat("[Top 10 Numbers {0}]: Heck you number {1}! You're garbage!", _moduleId, IgnoreNums[0]); break;
				case 1: Debug.LogFormat("[Top 10 Numbers {0}]: And now, for an honorable mention just falling outside our top {2}: {1}!", _moduleId, IgnoreNums[0], maxStage); break;
				case 2: if (Bomb.GetSolvableModuleNames().Count() % 2 == 0) Debug.LogFormat("[Top 10 Numbers {0}]: Oh, I hate number {1}! Just about as ugly as the number of modules on this bomb!", _moduleId, IgnoreNums[0]); else Debug.LogFormat("[Top 10 Numbers {0}]: Heck you number {1}! You're garbage!", _moduleId, IgnoreNums[0]); break;
            }
		else
			Debug.LogFormat("[Top 10 Numbers {0}]: Heck you numbers {1} and {2}! You're garbage!", _moduleId, IgnoreNums[0], IgnoreNums[1]);
		StageAnswers.Add(NumberPriority[0]);
		MoveToBack(0);
	}

	void LoggingDecider()
    {
		int RandStage = Rnd.Range(0, Stage);
		if(Stage == 0) Debug.LogFormat("[Top 10 Numbers #{0}]: Starting us off at number {1}, it's number {2}!", _moduleId, maxStage-Stage, StageAnswers.Last());
		else if (maxStage - Stage == 1) Debug.LogFormat("[Top 10 Numbers #{0}]: So close {1}, but there can only be one number one, and that one number at number one is number {2}!", _moduleId, StageAnswers[Stage-1], StageAnswers.Last());
		else
        {
            switch (Rnd.Range(0, 25))
            {
				case 0: Debug.LogFormat("[Top 10 Numbers #{0}]: At number {1}, it's number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 1: Debug.LogFormat("[Top 10 Numbers #{0}]: And at number {1}, it's number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 2: Debug.LogFormat("[Top 10 Numbers #{0}]: But look out {3}, at number {1}, it's number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage - 1]); break;
				case 3: Debug.LogFormat("[Top 10 Numbers #{0}]: Quick recap, because number {3} was number {4}, which you'll need to know the context for this next number, because it's number {1}, for number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last(), maxStage - RandStage, StageAnswers[RandStage]); break;
				case 4: Debug.LogFormat("[Top 10 Numbers #{0}]: But number {1} this week is also number {2}! That's right folks, there are 2 number {2}s, at number {3} and number {1}!", _moduleId, maxStage - Stage, StageAnswers.Last(), maxStage - RandStage, maxStage - RandStage); break;
				case 5: Debug.LogFormat("[Top 10 Numbers #{0}]: And the number coming in at number {1} in this list of numbers is number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 6: Debug.LogFormat("[Top 10 Numbers #{0}]: Now, number {3} is certainly quite the number, but edging it out at number {1} is number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage-1]); break;
				case 7: Debug.LogFormat("[Top 10 Numbers #{0}]: At number {1} is one of the numbers of all time... Give it up for number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 8: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {1} is even more number than number {3}, it’s number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last(),StageAnswers[Stage-1]); break;
				case 9: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {1} may be a bit controversial, but it's number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 10: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {1} is quite grandiose, it's number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 11: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {3} cannot compete with number {1} at all, number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage-1]); break;
				case 12: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {2} is number {1} on the list!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 13: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {2} is such a good number that it's number {1}!", _moduleId, maxStage - Stage, StageAnswers.Last()); break;
				case 14: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {2} barely edges out number {3}, putting it at number {1}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage-1]); break;
				case 15: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {3} is good, but number {2} is just a bit better, earning number {1}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage-1]); break;
				case 16: Debug.LogFormat("[Top 10 Numbers #{0}]: For number {1}, we don't have number {3}, but number {2}!", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11)); break;
				case 17: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {3} is quite popular, but it feels more fitting that number {2} is number {1} instead!", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11)); break;
				case 18: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {3} was quite good, but we have to make room for number {2}, sitting at number {1}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage-1]); break;
				case 19: Debug.LogFormat("[Top 10 Numbers #{0}]: For number {1}, we have number {2}! Sorry to the fans of number {3} out there.", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11)); break;
				case 20: Debug.LogFormat("[Top 10 Numbers #{0}]: Quite fitting how number {3} doesn't take the spot of number {1}, but that number {2} takes that spot instead!", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11)); break;
				case 21: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {2} goes the extra mile, outdoing number {3}, earning it the spot of number {1}!", _moduleId, maxStage - Stage, StageAnswers.Last(), StageAnswers[Stage - 1]); break;
				case 22: Debug.LogFormat("[Top 10 Numbers #{0}]: Did you expect number {3} to be number {1}? No! We have number {2} at number {1} instead!", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11)); break;
				case 23: Debug.LogFormat("[Top 10 Numbers #{0}]: The best fact about number {2} is that when you add it to it's ranking, number {1}, it makes {3}! (please don't sue us)", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11)); break;
				case 24: Debug.LogFormat("[Top 10 Numbers #{0}]: Number {1} goes to number {2}, being objectively better than number {3} and {4}!", _moduleId, maxStage - Stage, StageAnswers.Last(), Rnd.Range(1,11), StageAnswers[Stage - 1]); break;

			}
		}
    }
	int CountOcc(List<int> List, int Count)
    {
		int TotalCount = 0;
		for(int i = 0; i < List.Count; i++)
        {
			if (List[i] == Count) TotalCount++;
        }
		return TotalCount;
    }

	//twitch plays
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press 10 7 9 8 7 9 6 4 5 4 4 2 3 5 2 1 1 5 [Inputs the specified numbers]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length == 1)
				yield return "sendtochaterror Please specify at least one number!";
			else
			{
				for (int i = 1; i < parameters.Length; i++)
				{
					int temp = -1;
					if (!int.TryParse(parameters[i], out temp))
					{
						yield return "sendtochaterror!f The specified number '" + parameters[i] + "' is invalid!";
						yield break;
					}
					if (temp < 1 || temp > 10)
					{
						yield return "sendtochaterror The specified number '" + parameters[i] + "' is invalid!";
						yield break;
					}
				}
				if (Stage != maxStage || Stage2 != maxStage)
                {
					yield return "sendtochaterror The module is not ready for input!";
					yield break;
				}
				yield return null;
				for (int i = 1; i < parameters.Length; i++)
				{
					Buttons[int.Parse(parameters[i]) - 1].OnInteract();
					yield return new WaitForSeconds(.3f);
				}
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		while (Stage != maxStage || Stage2 != maxStage) yield return true;
		while (!solved)
        {
			Buttons[StageAnswers[Answers.Count] - 1].OnInteract();
			yield return new WaitForSeconds(.3f);
		}
    }
}