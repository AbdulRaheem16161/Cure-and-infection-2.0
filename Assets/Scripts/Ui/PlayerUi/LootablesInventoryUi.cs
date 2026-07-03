using UnityEngine;

public class LootablesInventoryUi : MonoBehaviour, IUiPanel
{
    public EquipmentUi playerEquipmentPanel;
    public InventoryUi playerInventoryPanel;

    public EquipmentUi otherEquipmentPanel;
    public InventoryUi otherInventoryPanel;

    #region Blocked Inputs for Ui Screen api
    public InputManager.InputBlock GetInputBlock()
    {
        return InputManager.InputBlock.Look | InputManager.InputBlock.Combat;
    }
    #endregion

    public void ShowUi(UiContext uiContext)
    {
        playerEquipmentPanel.UpdateObjectReferences(true, uiContext);
        playerInventoryPanel.UpdateObjectReferences(true, uiContext);

        playerEquipmentPanel.ShowUi(uiContext);
        playerInventoryPanel.ShowUi(uiContext);

        playerEquipmentPanel.gameObject.SetActive(true);
        playerInventoryPanel.gameObject.SetActive(true);

        if (uiContext.otherRef == null) { Debug.LogError("UiContext.otherRef null, Failed to show Lootable Ui"); return; }

        otherInventoryPanel.UpdateObjectReferences(false, uiContext);
        otherInventoryPanel.ShowUi(uiContext);
        otherInventoryPanel.SetUiAnchorPosition(uiContext.otherEquipment != null); //based on equipment existing push right more
        otherInventoryPanel.gameObject.SetActive(true);

        if (uiContext.otherEquipment == null) { gameObject.SetActive(true); return; }

        otherEquipmentPanel.UpdateObjectReferences(false, uiContext);
        otherEquipmentPanel.ShowUi(uiContext);
        otherEquipmentPanel.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }
    public void HideUi()
    {
        gameObject.SetActive(false);

        playerEquipmentPanel.gameObject.SetActive(false);
        playerInventoryPanel.gameObject.SetActive(false);

        playerEquipmentPanel.HideUi();
        playerInventoryPanel.HideUi();

        otherEquipmentPanel.gameObject.SetActive(false);
        otherInventoryPanel.gameObject.SetActive(false);

        otherEquipmentPanel.HideUi();
        otherInventoryPanel.HideUi();
    } 
}
