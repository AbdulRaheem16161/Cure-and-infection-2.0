using UnityEngine;
using UnityEngine.AI;

public class DeathManager_E : MonoBehaviour
{
    public bool IsDead = false;

    [Tooltip("Assign the specific AudioSource used for death sounds.")]
    [SerializeField] private AudioSource deathAudioSource;

    private Animator animator;

    private void Awake()
    {
        // You can also do this via Inspector now instead of relying on order
        if (deathAudioSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length > 1)
                deathAudioSource = sources[1]; // or [0] depending on your setup
            else
                deathAudioSource = sources[0];
        }

        animator = GetComponent<Animator>();
    }

    public void Die()
    {
        Debug.Log("💀 Die() called");

        if (IsDead) return;

        Debug.Log($"{gameObject.name} has died.");
        IsDead = true;

        ChangeColourToIndicateDeath();
        PlayDeathSound();
        GiveRandomRotation();
        DisableNavMesh();
        ResetTagAndLayer();
        StopAnimation();
    }

    private void PlayDeathSound()
    {
        if (deathAudioSource?.clip != null)
            deathAudioSource.Play();
    }

    private void GiveRandomRotation()
    {
        int choice = Random.Range(0, 3);
        switch (choice)
        {
            case 0: transform.rotation = Quaternion.Euler(90, 0, 0); break;
            case 1: transform.rotation = Quaternion.Euler(-90, 0, 0); break;
            case 2: transform.rotation = Quaternion.Euler(0, 0, 90); break;
        }

        transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
    }

    private void ChangeColourToIndicateDeath()
    {
        if (!CompareTag("Imposter")) return;

        Color deathColor = Color.black;
        for (int i = 0; i < transform.childCount; i++)
        {
            Renderer r = transform.GetChild(i).GetComponent<Renderer>();
            if (r != null)
            {
                foreach (Material mat in r.materials)
                    mat.color = mat.color;
            }
        }
    }

    private void DisableNavMesh()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
    }

    private void ResetTagAndLayer()
    {
        gameObject.tag = "Untagged";
        gameObject.layer = 0;
    }

    private void StopAnimation()
    {
        if (animator != null)
        {
            Debug.Log("🛑 Animator disabled.");
            animator.enabled = false;
        }
    }
}
