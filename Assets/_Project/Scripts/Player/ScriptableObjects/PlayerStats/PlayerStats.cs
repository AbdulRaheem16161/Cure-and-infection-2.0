using UnityEngine;

[CreateAssetMenu(menuName = "Player/Stats")]
public class PlayerStats : ScriptableObject
{
    public float runSpeed;
    public float walkSpeed;
    public float jumpMoveSpeed;
    public float jumpHeight;
}