using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LootableContainer))]
public class LootableContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LootableContainer lootableContainer = (LootableContainer)target;

        GUILayout.Space(10);
        GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);

        if (GUILayout.Button("Spawn Lootable Items"))
        {
            if (!ApplicationPlaying()) return;

            lootableContainer.SpawnLootableItemsInContainer();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Reset Loot Container"))
        {
            if (!ApplicationPlaying()) return;

            lootableContainer.ItemContainer.ResetContainer();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Press Interact"))
        {
            if (!ApplicationPlaying()) return;

            lootableContainer.InteractPress(null);
        }
    }

    private bool ApplicationPlaying()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Must be in Play Mode");
            return false;
        }

        return true;
    }
}
