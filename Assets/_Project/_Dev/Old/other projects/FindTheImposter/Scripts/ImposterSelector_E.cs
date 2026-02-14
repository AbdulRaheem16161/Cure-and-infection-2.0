using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ImposterSelector_E : MonoBehaviour
{
    [Header("Crewmates Detected (Read-Only)")]
    [SerializeField] private List<GameObject> detectedCrewMates = new List<GameObject>();

    [Header("😈 Selected Imposters")]
    [SerializeField] private List<GameObject> chosenImposters = new List<GameObject>();
    [SerializeField] private int numberOfImposters = 1;

    [Header("🎨 Visual Settings")]
    [SerializeField] private bool indicateTheImposter = false;

    private void Awake()
    {
        detectedCrewMates = GetCrewMates();

        if (detectedCrewMates.Count == 0)
        {
            Debug.LogWarning("[ImposterSelector_E] No crew mates found!");
            return;
        }

        if (numberOfImposters > detectedCrewMates.Count)
        {
            Debug.LogWarning("[ImposterSelector_E] Requested more imposters than available crew mates! Clamping.");
            numberOfImposters = detectedCrewMates.Count;
        }

        chosenImposters = ChooseUniqueRandom(detectedCrewMates, numberOfImposters);

        foreach (var imposterGO in chosenImposters)
        {
            imposterGO.tag = "Imposter";

            Imposter_E imposter = imposterGO.GetComponent<Imposter_E>();
            ImposterDisguise_E disguise = imposterGO.GetComponent<ImposterDisguise_E>();
            AmIBeenWatched_E amIBeenWatched = imposterGO.GetComponent<AmIBeenWatched_E>();

            if (imposter != null)
            {
                if (imposter != null) imposter.enabled = true;
                if (disguise != null) disguise.enabled = true;
                if (amIBeenWatched != null) amIBeenWatched.enabled = true;
                imposter.Initialize();
                Debug.Log($"[ImposterSelector_E] {imposterGO.name} is now an Imposter.");
            }
            else
            {
                Debug.LogWarning($"[ImposterSelector_E] {imposterGO.name} lacks Imposter_E component.");
            }

            if (indicateTheImposter)
            {
                StartCoroutine(DelayAndBlackenImposterSuit(imposterGO));
            }
        }
    }

    private List<GameObject> GetCrewMates()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("Crew Mate"));
    }

    private List<GameObject> ChooseUniqueRandom(List<GameObject> sourceList, int count)
    {
        List<GameObject> result = new List<GameObject>();
        List<GameObject> temp = new List<GameObject>(sourceList);

        for (int i = 0; i < count; i++)
        {
            int randIndex = Random.Range(0, temp.Count);
            result.Add(temp[randIndex]);
            temp.RemoveAt(randIndex);
        }

        return result;
    }

    private IEnumerator DelayAndBlackenImposterSuit(GameObject imposterGO)
    {
        yield return null;
        yield return null;
        yield return null;

        if (imposterGO.transform.childCount == 0)
        {
            Debug.LogWarning("[ImposterSelector_E] Chosen imposter has no children to apply shader color.");
            yield break;
        }

        Transform child = imposterGO.transform.GetChild(0);
        Renderer renderer = child.GetComponent<Renderer>();

        if (renderer == null)
        {
            Debug.LogWarning($"[ImposterSelector_E] First child of {imposterGO.name} has no Renderer.");
            yield break;
        }

        Material mat = renderer.material;

        if (!mat.shader.name.Contains("Shader Graphs/T"))
        {
            Debug.LogWarning($"[ImposterSelector_E] Shader is not 'Shader Graphs/T' on {child.name}. Found: {mat.shader.name}");
            yield break;
        }

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", Color.black);
            Debug.Log("[ImposterSelector_E] Applied black to _BaseColor.");
        }
        else if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", Color.black);
            Debug.Log("[ImposterSelector_E] Applied black to _Color.");
        }
        else
        {
            Debug.LogWarning($"[ImposterSelector_E] Material on {child.name} doesn't have _BaseColor or _Color.");
        }
    }
}
