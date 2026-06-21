using static InputManager;

public interface IUiPanel
{
    public void ShowUi(UiContext uiContext);
    public void HideUi();

    InputBlock GetInputBlock();
}
