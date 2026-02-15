using UnityEngine;
using Game.MyPlayer;
using Game.MyNPC;

public class MeleeHitBox : MonoBehaviour
{
    #region Fields
    public PlayerStateMachine stateMachine;
    private bool hasDamaged = false;
    #endregion

    private void Awake()
    {
        #region Disable Hitbox Initially
        gameObject.SetActive(false);
        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        #region 
        if (hasDamaged) return;

        if (stateMachine.TargetTags.Contains(other.tag))
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.RecieveDamage(stateMachine.Damage);
                hasDamaged = true; // prevent multiple hits
            }
        }
        #endregion
    }

    public void OnEnable()
    {
        hasDamaged = false;
    }
}
