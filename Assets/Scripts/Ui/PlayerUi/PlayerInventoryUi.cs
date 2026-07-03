using UnityEngine;

public class PlayerInventoryUi : MonoBehaviour, IUiPanel
{
    public EquipmentUi equipmentPanel;
    public InventoryUi inventoryPanel;

    #region Blocked Inputs for Ui Screen api
    public InputManager.InputBlock GetInputBlock()
    {
        return InputManager.InputBlock.Look | InputManager.InputBlock.Combat;
    }
    #endregion

    public void ShowUi(UiContext uiContext)
    {
        equipmentPanel.UpdateObjectReferences(true, uiContext);
        inventoryPanel.UpdateObjectReferences(true, uiContext);

        equipmentPanel.ShowUi(uiContext);
        inventoryPanel.ShowUi(uiContext);

        equipmentPanel.gameObject.SetActive(true);
        inventoryPanel.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }
    public void HideUi()
    {
        gameObject.SetActive(false);

        equipmentPanel.gameObject.SetActive(false);
        inventoryPanel.gameObject.SetActive(false);

        equipmentPanel.HideUi();
        inventoryPanel.HideUi();
    }
}
