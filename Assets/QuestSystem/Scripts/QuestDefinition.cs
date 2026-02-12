using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "ScriptableObjects/QuestDefinition")]
[Serializable]
public class QuestDefinition : ScriptableObject
{
	[Header("Quest Dialogue")]
	public string questName;
	public List<QuestDialogueNode> questDialogues = new(); //can be changed to a better system

	[Header("Quest Type")]
	public QuestType questType;

	public enum QuestType
	{
		unset, defendZone, deliverItem, findItem, ProtectNpcs
	}

	[Header("Quest Difficulty")]
	[Range(0, 1f)]
	public float questMinDifficulty;
	[Range(0, 1f)]
	public float questMaxDifficulty;

	[Header("Quest Vehicle Version")]
	public bool forceVehicleVersion;
	[Range(0, 1f)]
	public float chanceForVehicleVersion;

	[Header("Quest Needed Items")]
	[Tooltip("item(s) needed for quest completion")]
	public List<TestItems> questItemsNeeded = new(); //type can be specified later on in dev

	[Header("Quest Rewards")]

	public QuestRewardType questRewardType;

	public enum QuestRewardType
	{
		unset, item, money, ammo
	}
	[Tooltip("item(s) rewards from quest")]
	public List<TestItems> questRewards = new(); //type can be specified later on in dev

	[Tooltip("can represent ammo")]
	public int questRewardCurrency;

	[Header("Quest Fail Punishment")]
	public int questFailMoneyLoss;

	public string GetQuestDialogue(string key)
	{
		foreach (var questDialogue in questDialogues)
		{
			if (key == questDialogue.questDialogueKey)
				return questDialogue.questDialogueText;
		}

		string errorInfo = $"Failed to find matching key for: {key}";
		Debug.LogError(errorInfo);
		return errorInfo;
	}
}

//for now store as key + plain english text
[Serializable]
public class QuestDialogueNode
{
	public string questDialogueKey;
	public string questDialogueText;
}
