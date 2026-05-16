using System;
using UnityEngine;

[RequireComponent(typeof(InventoryHandler))]
public class Interactor : MonoBehaviour
{
    public InventoryHandler Inventory { get; private set; }

    [SerializeField] private float holdTime = 1f;
    [SerializeField] private LayerMask interactLayerMask;

    private bool CanInteract => current != null;
    private IInteractable current;
    private float holdTimer;

    public event Action<float> OnHoldProgress;
    public event Action OnInteractChanged;

    protected virtual void Awake()
    {
        Inventory = GetComponent<InventoryHandler>();
    }

    protected virtual void Update()
    {
        SetCurrentInteractable(FindInteractable());
        HandleInput();
    }

    #region Handle Interact Inputs (press and hold)
    private void HandleInput()
    {
        if (!CanInteract) return;

        PressInteract();

        StartHoldInteract();

        EndHoldInteract();
    }

    private void PressInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            current.InteractPress(this);
        }
    }
    private void StartHoldInteract()
    {
        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            float progress = holdTimer / holdTime;
            OnHoldProgress?.Invoke(progress);

            if (holdTimer >= holdTime)
            {
                current.InteractHoldComplete(this);
                ResetHold();
            }
        }
    }
    private void EndHoldInteract()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            ResetHold();
        }
    }

    private void ResetHold()
    {
        holdTimer = 0f;
        OnHoldProgress?.Invoke(0f);
    }
    #endregion

    private void SetCurrentInteractable(IInteractable newInteractable) 
    {
        if (current == newInteractable) return;
        current = newInteractable;
        ResetHold();
        OnInteractChanged?.Invoke();
    }
    private IInteractable FindInteractable()
    {
        Ray ray = new(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, interactLayerMask))
        {
            return hit.collider.GetComponentInParent<IInteractable>();
        }
        return null;
    }
}
