using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animal", menuName = "ScriptableObjects/Entities/Animals")]
public class AnimalDefinition : EntityDefinition
{
	#region Unique Animal Attack
	[Header("Unique Animal Attack")]
	/// <summary>
	/// reuse humanoid equipment system to define melee attack data for animals
	/// </summary>
	[SerializeField] private WeaponMeleeDefinition animalAttack;
	#endregion

	#region read only
	public WeaponMeleeDefinition AnimalAttack => animalAttack;
	#endregion
}
