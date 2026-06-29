using System;
using UnityEngine;

[RequireComponent(typeof(InventoryHandler))]
public class Interactor : MonoBehaviour
{
    public InventoryHandler Inventory { get; private set; }
    public bool IsPlayerInteractor { get; private set; }

    [SerializeField] private LayerMask interactLayerMask;

    private IInteractable current;
    [SerializeField] private float holdTime = 2f;
    private float holdTimer;

    private float interactablesSearchTime = 0.1f;
    private float interactablesSearchTimer;

    public event Action<float> OnHoldProgress;
    public event Action<IInteractable> OnInteractChanged;
    public event Action<IInteractable> OnInteractCompleted;

    private void Awake()
    {
        interactLayerMask = LayerMask.GetMask("Interactable", "CharacterDetection");
        Inventory = GetComponent<InventoryHandler>();
        IsPlayerInteractor = TryGetComponent(out PlayerController playerController);
    }

    public void TickSearchForInteractables(float deltaTime)
    {
        interactablesSearchTimer -= deltaTime;
        if (interactablesSearchTimer > 0) return;

        interactablesSearchTimer = interactablesSearchTime;
        SetCurrentInteractable(FindInteractable());
    }

    public void InteractPress()
    {
        if (current == null) return;

        current.InteractPress(this);
        OnInteractCompleted?.Invoke(current);
    }

    public void InteractHold(bool holding)
    {
        if (current == null) return;

        if (!holding)
        {
            ResetHold();
            return;
        }

        holdTimer += Time.deltaTime;

        float progress = holdTimer / holdTime;
        OnHoldProgress?.Invoke(progress);

        if (holdTimer >= holdTime)
        {
            current.InteractHoldComplete(this);
            OnInteractCompleted?.Invoke(current);
            ResetHold();
        }
    }

    private void ResetHold()
    {
        holdTimer = 0f;
        OnHoldProgress?.Invoke(0f);
    }

    private void SetCurrentInteractable(IInteractable newInteractable)
    {
        if (current == newInteractable) return;

        current = newInteractable;
        ResetHold();

        if (newInteractable != null && !newInteractable.CanInteract) //if cant interact dont show pop up
            OnInteractChanged?.Invoke(null);
        else
            OnInteractChanged?.Invoke(newInteractable);
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
