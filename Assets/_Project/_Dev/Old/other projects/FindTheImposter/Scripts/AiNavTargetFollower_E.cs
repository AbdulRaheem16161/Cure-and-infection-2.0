using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RandomFootstepsAudioManager_E))]
public class NavAgentController_E : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private RandomFootstepsAudioManager_E footstepAudio;

    [Header("Footstep Timing")]
    [Tooltip("Time between steps is inversely proportional to movement speed. Lower = faster steps.")]
    [SerializeField] private float footstepRateMultiplier = 0.5f; // tweak this to sync with animation

    private float footstepCooldown = 0f;
    public Transform target;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        footstepAudio = GetComponent<RandomFootstepsAudioManager_E>();
    }

    private void Update()
    {
        if (!agent.enabled || target == null)
            return;

        agent.SetDestination(target.position);

        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f;

        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            footstepCooldown -= Time.deltaTime;

            float dynamicCooldown = Mathf.Clamp(footstepRateMultiplier / speed, 0.1f, 1.5f); // Prevents tiny or huge intervals

            if (footstepCooldown <= 0f)
            {
                footstepAudio.PlayRandomFootstep();
                footstepCooldown = dynamicCooldown;
            }
        }
    }
}
