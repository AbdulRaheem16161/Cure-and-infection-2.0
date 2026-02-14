using UnityEngine;

public class TestInventoryManager : MonoBehaviour
{
	public static TestInventoryManager Instance;

	public InventoryHandler playerInventory;

	public InventoryHandler npcInventory;

	private void Awake()
	{
		Instance = this;
	}
}
