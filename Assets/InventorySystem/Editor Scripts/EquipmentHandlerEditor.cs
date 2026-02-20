using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquipmentHandler))]
public class EquipmentHandlerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EquipmentHandler equipment = (EquipmentHandler)target;

		GUILayout.Space(10);
		GUILayout.Label("DEBUG CONTROLS", EditorStyles.boldLabel);

		#region equip item buttons
		GUILayout.Label("Equipping Items", EditorStyles.boldLabel);
		equipment.itemToEquip = (ItemDefinition)EditorGUILayout.ObjectField("Item To Equip", equipment.itemToEquip, typeof(ItemDefinition), false);
		equipment.slotToEquipItemTo = (EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Equip Item To", equipment.slotToEquipItemTo);
		equipment.itemToEquipCount = EditorGUILayout.IntField("Item To Equip Count", equipment.itemToEquipCount);

		if (GUILayout.Button("Equip Item (destroys any equipped one)"))
		{
			if (!ApplicationPlaying()) return;

			if (equipment.itemToEquip == null)
			{
				Debug.LogError("no item specified in itemToEquip field");
				return;
			}

			if (equipment.itemToEquipCount > equipment.itemToEquip.StackLimit)
			{
				Debug.LogWarning($"{equipment.itemToEquipCount} is higher then stack limit of {equipment.itemToEquip.StackLimit}, setting to max");
				equipment.itemToEquipCount = equipment.itemToEquip.StackLimit;
			}
			if (equipment.itemToEquipCount < 1)
			{
				Debug.LogWarning($"{equipment.itemToEquipCount} is smaller then minimum limit of 1, setting to 1");
				equipment.itemToEquipCount = 1;
			}

			equipment.EquipItem(equipment.itemToEquip, equipment.itemToEquipCount, equipment.slotToEquipItemTo);
		}

		GUILayout.Space(10);

		GUILayout.Label("Equipping Item From Inventory", EditorStyles.boldLabel);
		equipment.equipItemFromSlot = EditorGUILayout.IntField("Equip Item From Slot", equipment.equipItemFromSlot);
		equipment.slotToEquipItemTo = (EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Equip Item To", equipment.slotToEquipItemTo);

		if (GUILayout.Button("Equip Item From Inventory"))
		{
			if (!ApplicationPlaying()) return;

			if (equipment.itemToEquip == null)
			{
				Debug.LogError("no item specified in itemToEquip field");
				return;
			}

			equipment.EquipItemFromInventory(equipment.equipItemFromSlot, equipment.slotToEquipItemTo);
		}
		#endregion

		GUILayout.Space(10);

		#region unequip item button
		GUILayout.Label("Unequipping Items", EditorStyles.boldLabel);
		equipment.equipmentSlotToUnequip = 
			(EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Unequip", equipment.equipmentSlotToUnequip);

		if (GUILayout.Button("Unequip Item"))
		{
			if (!ApplicationPlaying()) return;

			equipment.UnequipItem(equipment.equipmentSlotToUnequip);
		}
		#endregion

		GUILayout.Space(10);

		#region use consumable in equipment slot button
		GUILayout.Label("Use Consumable In Slot", EditorStyles.boldLabel);
		equipment.consumableSlotToUse =
			(EquipmentHandler.EquipmentType)EditorGUILayout.EnumPopup("Slot To Unequip", equipment.consumableSlotToUse);

		if (GUILayout.Button("Use Consumable Item"))
		{
			if (!ApplicationPlaying()) return;

			equipment.UseConsumable(equipment.consumableSlotToUse);
		}
		#endregion
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
