using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Quest
{
	public QuestDefinition questDefinition;

	public float questDifficulty;

	private readonly System.Random systemRandom = new();

	public void InitializeQuest(QuestDefinition questDefinition)
	{
		if (questDefinition != null)
			this.questDefinition = questDefinition;
		else
		{
			Debug.LogError("quest definition null");
			return;
		}

		if (questDefinition.questType == QuestDefinition.QuestType.unset)
		{
			Debug.LogError("quest type unset");
			return;
		}

		if (questDefinition.questRewardType == QuestDefinition.QuestRewardType.unset)
		{
			Debug.LogError("quest reward type unset");
			return;
		}

		questDifficulty = (float)(systemRandom.NextDouble() * 
			(questDefinition.questMaxDifficulty - questDefinition.questMinDifficulty) + questDefinition.questMinDifficulty);
	}

	public void OfferQuest()
	{
		Debug.Log(questDefinition.GetQuestDialogue("Quest_Offer"));
	}

	public void AcceptQuest()
	{
		Debug.Log(questDefinition.GetQuestDialogue("Quest_intro"));
	}

	public void FailQuest()
	{
		Debug.Log(questDefinition.GetQuestDialogue("Quest_Fail"));
		Debug.Log($"Lost {questDefinition.questFailMoneyLoss} money");
		QuestManager.instance.playerMoney += questDefinition.questFailMoneyLoss;
	}

	public void TryCompleteQuest() //has additional checks to pass, atm auto complete
	{
		if (!CheckIfQuestCompleted()) return;
		CompleteQuest();
	}
	private void CompleteQuest()
	{
		Debug.Log(questDefinition.GetQuestDialogue("Quest_Complete"));

		string rewardInfo = "Gained ";

		switch (questDefinition.questRewardType)
		{
			case QuestDefinition.QuestRewardType.unset:
			Debug.LogError("quest reward type unset");
			break;
			case QuestDefinition.QuestRewardType.item:
			for (int i = 0; i < questDefinition.questRewards.Count; i++)
			{
				if (i <= questDefinition.questRewards.Count - 1)
					rewardInfo += questDefinition.questRewards[i].itemName + ", ";
				else
					rewardInfo += questDefinition.questRewards[i].itemName;

				QuestManager.instance.playerInventoryItems.Add(questDefinition.questRewards[i]);
			}
			break;
			case QuestDefinition.QuestRewardType.money:
			rewardInfo += $"{questDefinition.questRewardCurrency} money";
			QuestManager.instance.playerMoney += questDefinition.questRewardCurrency;
			break;
			case QuestDefinition.QuestRewardType.ammo:
			rewardInfo += $"{questDefinition.questRewardCurrency} ammo"; //include ammo type
			QuestManager.instance.playerAmmo += questDefinition.questRewardCurrency;
			break;
		}

		Debug.Log(rewardInfo);
	}

	//quest complete checks
	private bool CheckIfQuestCompleted()
	{
		if (questDefinition.questType == QuestDefinition.QuestType.deliverItem ||
			questDefinition.questType == QuestDefinition.QuestType.findItem)
		{
			List<TestItems> missingItems = new();
			int itemsNeededToMatch = questDefinition.questItemsNeeded.Count;
			int matchedItems = 0;

			foreach (TestItems itemNeeded in questDefinition.questItemsNeeded)
			{
				foreach (TestItems item in QuestManager.instance.playerInventoryItems)
				{
					if (itemNeeded != item)
						missingItems.Add(itemNeeded);
					else
						matchedItems++;
				}
			}

			if (matchedItems == itemsNeededToMatch)
				return true;
			else
			{
				MissingQuestItems(missingItems);
				return false;
			}
		}

		return true;
	}
	public void MissingQuestItems(List<TestItems> missingItems)
	{
		//tell player of missing items in ui

		string missingItemsInfo = "Missing Items: ";
		for (int i = 0; i < missingItems.Count; i++)
		{
			if (i <= missingItems.Count - 1)
				missingItemsInfo += missingItems[i].itemName + ", ";
			else
				missingItemsInfo += missingItems[i].itemName;
		}

		Debug.LogError(missingItemsInfo);
	}
}
