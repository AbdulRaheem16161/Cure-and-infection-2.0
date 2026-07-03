using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestDefinition), true)]
public class QuestDefinitionEditor : Editor
{
	QuestDefinition data;

	void OnEnable()
	{
		data = (QuestDefinition)target;
	}

	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector(); // for other non-HideInInspector fields

		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.Space();

		// update the current values into the serialized object and propreties
		serializedObject.Update();

		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questName)));
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questDialogues)));

		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questType)));
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questMinDifficulty)));
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questMaxDifficulty)));

		if (data.questMaxDifficulty < data.questMinDifficulty) //stop values exceding one another
			data.questMaxDifficulty = data.questMinDifficulty;
		if (data.questMinDifficulty > data.questMaxDifficulty)
			data.questMinDifficulty = data.questMaxDifficulty;

		EditorGUILayout.Space();

		if (data.questType == QuestDefinition.QuestType.deliverItem || data.questType == QuestDefinition.QuestType.findItem)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questItemsNeeded)));
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questRewardType)));

		if (data.questRewardType == QuestDefinition.QuestRewardType.item)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questRewards)));
		}
		else
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questRewardCurrency)));
		}

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.questFailMoneyLoss)));

		// Write back changed values
		// This also handles all marking dirty, saving, undo/redo etc
		serializedObject.ApplyModifiedProperties();
	}
}
