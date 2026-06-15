using System;

public interface IInteractable
{
    public string InteractableName { get; }
    public void InteractPress(Interactor interactor);
    public void InteractHoldComplete(Interactor interactor);
}
