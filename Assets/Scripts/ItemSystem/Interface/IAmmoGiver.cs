public interface IAmmoGiver
{
	/// <summary>
	/// get ammo without relying on or modifying container
	/// </summary>
	public int GetAmmo(ProjectileDefinition projectileDefinition, int amountNeeded);

	/// <summary>
	/// take ammo from containers
	/// </summary>
	public int TakeAmmo(ProjectileDefinition projectileDefinition, int amountNeeded);

	/// <summary>
	/// check for ammo availability 
	/// </summary>
	public bool AmmoAvailable(ProjectileDefinition projectileDefinition);
}
