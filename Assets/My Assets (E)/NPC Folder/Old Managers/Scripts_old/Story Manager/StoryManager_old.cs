using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [Header("Story Path")]
    public List<Transform> Destinations; // the cubes/waypoints
    public List<float> WaitBeforeNextDestination; // seconds to wait at each milestone
    public string TargetTag; // tag of the object that triggers the milestones

    private int DestinationsCovered = 0;
    private bool isWaiting = false;

    [Header("Cube / Marker")]
    public DestinationCube MovingCube; // reference to the cube

    private void Start()
    {
        if (Destinations.Count > 0 && MovingCube != null)
            MovingCube.transform.position = Destinations[0].position;

        // Assign the manager to the cube
        if (MovingCube != null)
            MovingCube.Manager = this;
    }

    public void OnDestinationTrigger(Collider other)
    {
        if (other.CompareTag(TargetTag) && !isWaiting && DestinationsCovered < Destinations.Count)
        {
            StartCoroutine(MoveCubeToNextDestination());
        }
    }

    private IEnumerator MoveCubeToNextDestination()
    {
        isWaiting = true;

        // Wait for the specified time
        float waitTime = 0f;
        if (DestinationsCovered < WaitBeforeNextDestination.Count)
            waitTime = WaitBeforeNextDestination[DestinationsCovered];

        yield return new WaitForSeconds(waitTime);

        // Move cube to next destination
        if (DestinationsCovered < Destinations.Count && MovingCube != null)
            MovingCube.transform.position = Destinations[DestinationsCovered + 1].position;

        DestinationsCovered++;
        isWaiting = false;
    }
}
