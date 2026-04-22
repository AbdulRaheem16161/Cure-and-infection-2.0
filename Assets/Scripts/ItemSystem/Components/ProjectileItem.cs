using UnityEngine;

public class ProjectileItem : Item<ProjectileDefinition>
{
	[SerializeField] private ProjectileDefinition projectileDefinition;

	public ProjectileDefinition ProjectileDefinition => projectileDefinition;

	public override void InitializeItem(ProjectileDefinition definition, int itemStack)
	{
		base.InitializeItem(definition, itemStack);
		projectileDefinition = definition;

		//projectile-specific setup here
	}
}
