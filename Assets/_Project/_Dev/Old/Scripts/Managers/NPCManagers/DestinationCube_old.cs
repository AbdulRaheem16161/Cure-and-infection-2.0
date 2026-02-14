// Summary: 
// This script is attached to a destination cube object. 
// When another collider enters its trigger area, it notifies the assigned StoryManager 

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DestinationCube : MonoBehaviour
{
    [HideInInspector]
    public StoryManager Manager;

    private void OnTriggerEnter(Collider other)
    {
        if (Manager != null)
            Manager.OnDestinationTrigger(other);
    }
}
