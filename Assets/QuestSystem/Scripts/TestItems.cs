using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TestItem", menuName = "ScriptableObjects/TestItem")]
[Serializable]
public class TestItems : ScriptableObject
{
	public string itemName;
}
