public interface IAmmoGiver
{
	/// <summary>
	/// take ammo from containers
	/// </summary>
	public int TakeAmmo(ProjectileDefinition projectileDefinition, int amountNeeded, bool takeForFree);

	/// <summary>
	/// check for ammo availability 
	/// </summary>
	public bool AmmoAvailable(ProjectileDefinition projectileDefinition);
}
