using System;

public interface IInteractable
{
    public void InteractPress(Interactor interactor);
    public void InteractHoldComplete(Interactor interactor);
}
