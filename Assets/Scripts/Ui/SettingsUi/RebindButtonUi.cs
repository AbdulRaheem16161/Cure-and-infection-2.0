using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindButtonUi : MonoBehaviour
{
    [Header("UI")]
    public GameObject rebindUi;
    public TMP_Text InputActionNameText;
    public TMP_Text InputActionText;
    private Button rebindButton;

    private InputActionReference actionReference;
    private int bindingIndex;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    #region Initialize Rebind Ui
    public void InitializeUi(InputActionReference actionReference, int bindingIndex)
    {
        if (actionReference == null)
            Debug.LogError($"RebindUi's actionReference null, Assign one in inspector");
        if (actionReference.action == null)
            Debug.LogError($"RebindUi's actionReference.action null");
        if (InputActionNameText == null)
            Debug.LogError($"RebindUi's InputActionNameText null, Assign one in inspector");
        if (InputActionText == null)
            Debug.LogError($"RebindUi's InputActionText null, Assign one in inspector");

        this.actionReference = actionReference;
        this.bindingIndex = bindingIndex;

        rebindButton = GetComponentInChildren<Button>();
        rebindButton.onClick.AddListener(StartRebind);

        InputActionNameText.text = SetInputActionName();
        UpdateBindingDisplay();
    }
    private string SetInputActionName()
    {
        string actionName = Regex.Replace(
            actionReference.action.name,
            @"(?<!^)([A-Z])",
            " $1");

        var binding = actionReference.action.bindings[bindingIndex];

        if (binding.isPartOfComposite)
        {
            string partName = Regex.Replace(
                binding.name,
                @"(?<!^)([A-Z])",
                " $1");

            return $"{actionName} {partName}";
        }

        return actionName;
    }
    #endregion

    #region Update Ui Input Display
    public void UpdateBindingDisplay()
    {
        if (actionReference == null || actionReference.action == null || InputActionNameText == null || InputActionText == null) return;
        InputActionText.text = actionReference.action.GetBindingDisplayString(bindingIndex);
    }
    #endregion

    #region Reset Binding (called via ControlSettingsUi)
    public void ResetBinding()
    {
        actionReference.action.RemoveAllBindingOverrides();
        UpdateBindingDisplay();
    }
    #endregion

    #region Start Rebind + Outcomes
    private void StartRebind()
    {
        if (rebindOperation != null) return;

        InputAction action = actionReference.action;
        action.Disable();
        InputActionText.text = "Press New Input...";

        rebindOperation = action
            .PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnCancel(operation => CancelRebinding(operation, action))
            .OnComplete(operation => CompleteRebinding(operation, action))
            .Start();
    }

    private void CancelRebinding(InputActionRebindingExtensions.RebindingOperation operation, InputAction action)
    {
        action.Enable();
        operation.Dispose();
        UpdateBindingDisplay();
        rebindOperation = null;
    }
    private void CompleteRebinding(InputActionRebindingExtensions.RebindingOperation operation, InputAction action)
    {
        action.Enable();
        operation.Dispose();

        UpdateBindingDisplay();
        InputManager.SaveInputControls();

        var conflict = FindConflictingBinding(action, bindingIndex, action.bindings[bindingIndex].effectivePath);

        if (!string.IsNullOrEmpty(conflict.effectivePath))
        {
            //later add some kinda warning to ui that indicates conflicting bindings. for now just leave as log warning
            Debug.LogWarning($"Key already used by another action.");
        }

        rebindOperation = null;
        Debug.Log("New binding: " + action.bindings[bindingIndex].effectivePath);
    }
    #endregion

    #region Find Conflicting Bindings to exclude
    private InputBinding FindConflictingBinding(
    InputAction currentAction,
    int currentBindingIndex,
    string path)
    {
        foreach (var map in currentAction.actionMap.asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    // Ignore the binding we just changed
                    if (action == currentAction && i == currentBindingIndex)
                        continue;

                    if (action.bindings[i].effectivePath == path)
                        return action.bindings[i];
                }
            }
        }

        return default;
    }
    #endregion
}
