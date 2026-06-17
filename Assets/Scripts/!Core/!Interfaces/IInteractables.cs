using System;

public interface IInteractable
{
    public bool CanInteract { get; }
    public string InteractableName { get; }
    public void InteractPress(Interactor interactor);
    public void InteractHoldComplete(Interactor interactor);
}
