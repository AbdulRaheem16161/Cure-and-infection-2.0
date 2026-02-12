using UnityEngine;
using Game.MyPlayer;
using Game.MyNPC;

public class NPCHitbox : MonoBehaviour
{
    #region Fields
    private NPCStateMachine stateMachine;
    private bool hasDamaged = false;
    #endregion

    private void Awake()
    {
        #region Cache State Machine
        stateMachine = GetComponentInParent<NPCStateMachine>();
        #endregion

        #region Disable Hitbox Initially
        gameObject.SetActive(false);
        #endregion
    }


    private void OnTriggerEnter(Collider other)
    {
        if (hasDamaged) return; // already damaged one this activation

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null && stateMachine.TargetTags.Contains(other.tag))
        {
            damageable.RecieveDamage(stateMachine.Damage);
            hasDamaged = true; // prevent multiple hits
        }
    }

    public void OnEnable()
    {
        hasDamaged = false;
    }

}

