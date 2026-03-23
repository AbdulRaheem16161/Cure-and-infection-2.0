using UnityEngine;

public interface IDamageable
{
    void RecieveDamage(int damageAmount, GameObject Attacker = null);
}