using UnityEngine;

public class LootablesInventoryUi : MonoBehaviour, IUiPanel
{
    public EquipmentUi playerEquipmentPanel;
    public InventoryUi playerInventoryPanel;

    public EquipmentUi otherEquipmentPanel;
    public InventoryUi otherInventoryPanel;

    public void ShowUi(UiContext uiContext)
    {
        playerEquipmentPanel.UpdateObjectReferences(true, uiContext.playerRef, uiContext.playerEquipment, uiContext.playerContainer);
        playerInventoryPanel.UpdateObjectReferences(true, uiContext.playerRef, uiContext.playerEquipment, uiContext.playerContainer);

        playerEquipmentPanel.ShowUi(uiContext);
        playerInventoryPanel.ShowUi(uiContext);

        playerEquipmentPanel.gameObject.SetActive(true);
        playerInventoryPanel.gameObject.SetActive(true);

        if (uiContext.otherRef == null) { Debug.LogError("UiContext.otherRef null, Failed to show Lootable Ui"); return; }

        otherInventoryPanel.UpdateObjectReferences(false, uiContext.otherRef, uiContext.otherEquipment, uiContext.otherContainer);
        otherInventoryPanel.ShowUi(uiContext);
        otherInventoryPanel.SetUiAnchorPosition(uiContext.otherEquipment != null); //based on equipment existing push right more
        otherInventoryPanel.gameObject.SetActive(true);

        if (uiContext.otherEquipment == null) { gameObject.SetActive(true); return; }

        otherEquipmentPanel.UpdateObjectReferences(false, uiContext.otherRef, uiContext.otherEquipment, uiContext.otherContainer);
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
