using UnityEngine;

public class Hinge : MonoBehaviour
{
    public bool Open { get; private set; }

    [SerializeField] private Transform hinge;

    [Header("Rotation")]
    public RotationAxis rotAxis;
    public enum RotationAxis
    {
        x, y, z
    }
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 5f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        closedRotation = hinge.localRotation;

        switch (rotAxis)
        {
            case RotationAxis.x:
                openRotation = closedRotation * Quaternion.Euler(openAngle, 0f, 0f); break;

            case RotationAxis.y:
                openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f); break;

            case RotationAxis.z:
                openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle); break;
        }
    }

    private void Update()
    {
        Quaternion target = Open ? openRotation : closedRotation;
        hinge.localRotation = Quaternion.Slerp(hinge.localRotation, target, Time.deltaTime * speed);
    }

    public void OpenHinge()
    {
        Open = true;
    }
    public void CloseHinge()
    {
        Open = false;
    }
    public void Toggle()
    {
        Open = !Open;
    }
}
