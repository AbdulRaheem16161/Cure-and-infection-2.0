using Game.GenericStateMachine;
using UnityEngine;
using Game.MyPlayer;

public class InteractionController : MonoBehaviour
{
    #region References
    [Header("References")]
    public Camera playerCamera;
    public PlayerStateMachine stateMachine;
    public IInteractable interactable;
    #endregion

    #region Ray Settings
    [Header("Ray Settings")]
    public bool showRay = true;                
    public Color rayColor = Color.green;       
    public float rayDistance = 3f;
    #endregion

    #region live Values
    public bool InteractableDetected;
    #endregion

    #region Runtime Values
    private Vector3 screenCenter;
    private RaycastHit lastHitInfo;
    #endregion

    private void Start()
    {
        #region initialize
        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
        #endregion
    }

    private void Update()
    {
        #region Handle Interaction Input
        HandleInteraction();
        stateMachine.InputReader.InteractTriggered = false;
        interactable = null;
        #endregion
    }

    private void HandleInteraction()
    {
        #region Raycast
        Ray ray = playerCamera.ViewportPointToRay(Vector3.one * 0.5f);


        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayDistance))
        {
            #region interact if Interactable found

            InteractableDetected = hitInfo.collider.TryGetComponent<IInteractable>(out interactable);

            if (interactable != null)
            {
                if (stateMachine.InputReader.InteractTriggered)
                {
                    interactable.Interact();
                }
            }
            #endregion

            #region Store Hit Info
            lastHitInfo = hitInfo;
            #endregion
        }
        else
        {
            #region Reset Hit Info
            InteractableDetected = false;
            lastHitInfo = new RaycastHit();
            #endregion 
        }
        #endregion
    }

    private void OnDrawGizmos()
    {
        #region Draw Interaction Ray
        if (!showRay || playerCamera == null)
            return;

        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        Vector3 endPoint;

        endPoint = ray.origin + ray.direction * rayDistance;

        Gizmos.color = rayColor;
        Gizmos.DrawLine(ray.origin, endPoint);
        Gizmos.DrawSphere(endPoint, 0.05f);
        #endregion
    }
}
