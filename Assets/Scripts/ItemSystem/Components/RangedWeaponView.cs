using System;
using UnityEngine;

/// <summary>
/// will help describe points on weapon models, like attatchment locations (muzzle, scopes, foregrips etc...)
/// be an interface to simplify weapon logic to animations
/// </summary>

[RequireComponent(typeof(Animator))]
public class RangedWeaponView : MonoBehaviour
{
	public Transform MuzzlePoint;
	public Animator Animator { get; private set; }
	private string currentAnimation = "";

	private void Awake()
	{
		Animator = GetComponent<Animator>();
	}

	public void ChangeAnimation(string animation, float fade, bool forcePlayAnimation)
	{
		if (!AnimationWithStringNameExists(animation)) return;

		if (forcePlayAnimation)
			Animator.Play(animation, 0, 0f);
		else
		{
			if (currentAnimation == animation) return;
			currentAnimation = animation;
			Animator.CrossFade(animation, fade);
		}
	}
	public void SetFireRateAnimationSpeed(float fireRateRPM)
	{
		float fireRateCooldown = 60f / fireRateRPM;

		float fireAnimLength = 0.5f; //need to get animation clip length from somewhere but temporarilty this works
		float speedMultiplier = fireAnimLength / fireRateCooldown;

		speedMultiplier = Mathf.Clamp(speedMultiplier, 0.5f, 3f);

		Animator.SetFloat("fireRate", speedMultiplier);
	}

	private bool AnimationWithStringNameExists(string animation)
	{
		if (Animator.HasState(0, Animator.StringToHash(animation))) return true;

		Debug.LogWarning($"Weapon {typeof(Animator)} component doesnt contain animation with name {animation}");
		return false;
	}

	public void ResetAnimation()
	{
		Animator.playbackTime = 0;
	}

	public void PlayAnimation(string animation)
	{
		Animator.Play(animation, 0, 0f);
	}
}