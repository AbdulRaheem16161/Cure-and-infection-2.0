using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UiManager;

public class ControlSettingsUi : MonoBehaviour, IUiPanel
{
    public GameObject settingsUi;
    public GameObject rebindButtonUiContentParentObject;
    public GameObject rebindButtonUiPrefab;

    public Button backButton;
    public Button resetBindingsButton;

    public List<RebindButtonUi> rebindButtonUis = new();

    public List<InputActionReference> gameplayActionReferences = new();
    public List<InputActionReference> uiActionReferences = new();
    public List<InputActionReference> unbindableActionReferences = new();

    #region Initialize Ui + Button Listeners
    private void Start()
    {
        CheckForMissmatchedInputReferences(unbindableActionReferences, gameplayActionReferences, true);
        CheckForMissmatchedInputReferences(unbindableActionReferences, uiActionReferences, true);
        CheckForMissmatchedInputReferences(gameplayActionReferences, uiActionReferences, false);
        InitializeUi();
    }

    private void InitializeUi()
    {
        backButton.onClick.AddListener(() => ShowScreen(new(UiScreens.controlSettings)));
        resetBindingsButton.onClick.AddListener(ResetAllBindings);
        AutoSetupRebindButtonUiElements(gameplayActionReferences);
        AutoSetupRebindButtonUiElements(uiActionReferences);
    }
    #endregion

    /// <summary>
    /// would be nice to add inputs missing an entry from any list to also be logged but not really important atm
    /// </summary>
    #region Check For Missmatched Input References
    private void CheckForMissmatchedInputReferences(List<InputActionReference> source, List<InputActionReference> other, bool sourceIsUnbindable)
    {
        var seen = new HashSet<InputActionReference>();
        var otherSet = new HashSet<InputActionReference>(other);

        foreach (var actionReference in source) //check for dups in same list then other
        {
            if (!seen.Add(actionReference))
                Debug.LogError($"{actionReference} is duplicated within {source}");

            if (otherSet.Contains(actionReference))
            {
                string log = sourceIsUnbindable ? "if action should be unbindable remove from bindable list." : 
                    $"{actionReference} exists in both lists. Remove duplicate entry.";

                Debug.LogError($"{actionReference} in {source} also exists in {other}, " + log);
            }
        }
    }
    #endregion

    #region Setup RebindButtonUis automatically
    private void AutoSetupRebindButtonUiElements(List<InputActionReference> actionReferences)
    {
        rebindButtonUis.Clear();

        foreach (var actionReference in actionReferences)
        {
            if (unbindableActionReferences.Contains(actionReference))
                Debug.LogError($"{actionReference} in {actionReferences} exists in unbindableActionReferences, " +
                    $"if should be unbindable, remove from {actionReferences} list");

            var bindings = actionReference.action.bindings;

            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].isComposite)
                    continue;

                CreateNewRebindButtonUi(actionReference, i);
            }
        }
    }

    private void CreateNewRebindButtonUi(InputActionReference actionReference, int bindingIndex)
    {
        RebindButtonUi buttonUi = Instantiate(rebindButtonUiPrefab, rebindButtonUiContentParentObject.transform).GetComponent<RebindButtonUi>();
        buttonUi.InitializeUi(actionReference, bindingIndex);
        rebindButtonUis.Add(buttonUi);
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveAllListeners();
    }
    #endregion

    #region Show/Hide Ui Api
    public void ShowUi(UiContext uiContext)
    {
        foreach (RebindButtonUi rebindButtonUi in rebindButtonUis)
            rebindButtonUi.UpdateBindingDisplay();

        settingsUi.SetActive(true);
    }
    public void HideUi()
    {
        settingsUi.SetActive(false);
    }
    #endregion

    #region Reset All Bindings Button Action
    private void ResetAllBindings()
    {
        foreach (RebindButtonUi rebindButtonUi in rebindButtonUis)
            rebindButtonUi.ResetBinding();
    }
    #endregion
}
