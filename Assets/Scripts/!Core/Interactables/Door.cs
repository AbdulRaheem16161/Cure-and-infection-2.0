using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour, IInteractable
{
    public bool Open { get; private set; }

    [SerializeField] private Transform doorHinge;

    [Header("Rotation")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 5f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        closedRotation = doorHinge.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        Quaternion target = Open ? openRotation : closedRotation;
        doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, target, Time.deltaTime * speed);
    }

    public void InteractPress(Interactor interactor)
    {
        Open = !Open;
    }
    public void InteractHoldComplete(Interactor interactor)
    {
        return; //not used
    }
}
