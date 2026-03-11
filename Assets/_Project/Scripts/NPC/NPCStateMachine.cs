using UnityEngine;

public class NPCStateMachine : StateMachine
{
    #region properties
    public Animator Animator => animator;
    #endregion

    #region fields
    [SerializeField] public Animator animator;
    #endregion 

    protected void Awake()
    {
       // SwitchState(new NPCIdleState(this));
    }
}
