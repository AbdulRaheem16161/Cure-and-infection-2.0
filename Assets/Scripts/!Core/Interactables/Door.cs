using UnityEngine;

[RequireComponent(typeof(Hinge))]
public class Door : MonoBehaviour, IInteractable
{
    public string InteractableName
    {
        get
        {
            if (Open) return $"Close Door";
            return $"Open Door";
        }
    }
    public bool Open { get; private set; }

    private Hinge hinge;

    private void Awake()
    {
        hinge = GetComponent<Hinge>();
        Open = false;
        hinge.CloseHinge();
    }

    public void InteractPress(Interactor interactor)
    {
        Open = !Open;
        hinge.Toggle();
    }
    public void InteractHoldComplete(Interactor interactor)
    {
        return; //not used
    }
}
