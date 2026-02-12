using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
	public static QuestManager instance;

	public List<QuestDefinition> possibleQuests = new();

	private readonly System.Random systemRandom = new();

	[Header("Simulate player inventory")]
	public int playerAmmo;
	public int playerMoney;
	public List<TestItems> playerInventoryItems = new(); //test representation for player inventory

	[Header("Quest History")]
	public bool hasOfferedQuest;
	public bool hasActiveQuest;

	private Quest offeredQuest;
	private Quest activeQuest;

	public List<Quest> completedQuests = new();
	public List<Quest> failedQuests = new();

	private void Awake()
	{
		instance = this;
	}

	public void GenerateQuest()
	{
		if (offeredQuest != null) return;

		int questToGenerate = systemRandom.Next(0, possibleQuests.Count);

		Debug.LogError(questToGenerate);

		Quest newQuest = new();
		newQuest.InitializeQuest(possibleQuests[questToGenerate]);
		newQuest.OfferQuest();
		offeredQuest = newQuest;
		hasOfferedQuest = true;
	}

	//atm hooked upto ui buttons for testing
	public void GenerateQuestButton()
	{
		if (possibleQuests.Count == 0)
		{
			Debug.LogError("no possible quests to generate");
			return;
		}

		GenerateQuest();
	}

	public void AcceptQuestButton()
	{
		if (activeQuest != null)
		{
			Debug.LogError("already have active quest");
			return;
		}
		if (offeredQuest == null)
		{
			Debug.LogError("no offered quest");
			return;
		}

		offeredQuest.AcceptQuest();
		activeQuest = offeredQuest;
		offeredQuest = null;
		hasOfferedQuest = false;
		hasActiveQuest = true;
	}

	public void FailQuestButton()
	{
		if (activeQuest == null)
		{
			Debug.LogError("no active quest to fail");
			return;
		}

		activeQuest.FailQuest();
		failedQuests.Add(activeQuest);
		activeQuest = null;
		hasActiveQuest = false;
	}

	public void CompleteQuestButton()
	{
		if (activeQuest == null)
		{
			Debug.LogError("no active quest to complete");
			return;
		}

		activeQuest.TryCompleteQuest();
		completedQuests.Add(activeQuest);
		activeQuest = null;
		hasActiveQuest = false;
	}
}
