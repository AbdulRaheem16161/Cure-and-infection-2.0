using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ImposterDisguise_E : MonoBehaviour
{
    [Header("🔗 Component References")]
    [Tooltip("Script that tells whether this imposter is being watched.")]
    public AmIBeenWatched_E amIBeenWatched;

    [Tooltip("Where the imposter should go when disguising.")]
    public Transform pointToFollowForDisguise;

    [Header("⏱️ Disguise Timing")]
    [SerializeField] private float minWaitBeforeDisguise = 1f;
    [SerializeField] private float maxWaitBeforeDisguise = 3f;
    [SerializeField] private float minWaitBeforeStartKillingAgain = 1f;
    [SerializeField] private float maxWaitBeforeStartKillingAgain = 3f;

    [Header("🎭 Disguise Mode Movement")]
    [SerializeField] private float disguiseModeSpeed = 2f;
    [SerializeField] private float disguiseModeAcceleration = 4f;

    [Header("🔪 Killing Mode Movement")]
    [SerializeField] private float killingModeSpeed = 6f;
    [SerializeField] private float killingModeAcceleration = 10f;

    private NavMeshAgent agent;
    private NavAgentController_E navAgentController;
    private Imposter_E imposter;

    private bool wasSeenPreviously = false;
    private float currentSpeed;
    private float currentAcceleration;

    private Coroutine disguiseCoroutine;
    private Coroutine killCoroutine;

    [SerializeField] private bool Disguising = false;
    [SerializeField] private bool BackToKilling = false;

    private void Awake()
    {
        amIBeenWatched = GetComponent<AmIBeenWatched_E>();
        agent = GetComponent<NavMeshAgent>();
        navAgentController = GetComponent<NavAgentController_E>();
        imposter = GetComponent<Imposter_E>();
        ShiftToKillingMovement();
    }

    private void Update()
    {
        if (amIBeenWatched == null) return;

        ToggleDisguiseAndKillMode();
        ApplyMovementModifiers();
    }

    private void ToggleDisguiseAndKillMode()
    {
        bool isBeingWatched = amIBeenWatched.isBeingWatched;

        if (!wasSeenPreviously && isBeingWatched)
        {
            Disguise();
        }
        else if (wasSeenPreviously && !isBeingWatched)
        {
            StartKilling();
        }

        wasSeenPreviously = isBeingWatched;
    }

    private void Disguise()
    {
        StopKilling();
        ShiftToDisguiseMovement();
        Disguising = true;
        BackToKilling = false;

        // Cancel both coroutines to ensure exclusivity
        CancelAllCoroutines();

        disguiseCoroutine = StartCoroutine(DisguiseRoutine());
    }

    private void StartKilling()
    {
        Disguising = false;
        BackToKilling = true;
        // Cancel both coroutines to ensure exclusivity
        CancelAllCoroutines();

        killCoroutine = StartCoroutine(KillRoutine());
    }

    private IEnumerator DisguiseRoutine()
    {
        float waitTime = Random.Range(minWaitBeforeDisguise, maxWaitBeforeDisguise);
        yield return new WaitForSeconds(waitTime);

        if (navAgentController != null)
            navAgentController.target = pointToFollowForDisguise;

        disguiseCoroutine = null;
    }

    private IEnumerator KillRoutine()
    {
        float waitTime = Random.Range(minWaitBeforeStartKillingAgain, maxWaitBeforeStartKillingAgain);
        yield return new WaitForSeconds(waitTime);

        if (imposter != null)
        {
            imposter.ChooseNewTarget();
            imposter.stopKilling = false;
        }

        ShiftToKillingMovement();
        killCoroutine = null;
    }

    private void CancelAllCoroutines()
    {
        if (disguiseCoroutine != null)
        {
            StopCoroutine(disguiseCoroutine);
            disguiseCoroutine = null;
        }

        if (killCoroutine != null)
        {
            StopCoroutine(killCoroutine);
            killCoroutine = null;
        }
    }

    private void StopKilling()
    {
        if (imposter != null)
            imposter.stopKilling = true;
    }

    private void ShiftToDisguiseMovement()
    {
        currentSpeed = disguiseModeSpeed;
        currentAcceleration = disguiseModeAcceleration;
    }

    private void ShiftToKillingMovement()
    {
        currentSpeed = killingModeSpeed;
        currentAcceleration = killingModeAcceleration;
    }

    private void ApplyMovementModifiers()
    {
        if (agent == null) return;

        agent.speed = currentSpeed;
        agent.acceleration = currentAcceleration;
    }
}
