using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindButtonUi : MonoBehaviour, IUiPanel
{
    [Header("UI")]
    public GameObject rebindUi;
    public TMP_Text bindingText;
    private Button rebindButton;

    [Header("Input")]
    [SerializeField] private InputActionReference actionReference;
    [SerializeField] private int bindingIndex;

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    private void Awake()
    {
        if (actionReference == null)
            Debug.LogError($"RebindUi's actionReference null, Assign one in inspector");
        if (actionReference.action == null)
            Debug.LogError($"RebindUi's actionReference.action null");
        if (bindingText == null)
            Debug.LogError($"RebindUi's bindingText null, Assign one in inspector");

        rebindButton = GetComponentInChildren<Button>();
        rebindButton.onClick.AddListener(StartRebind);
    }

    public void ShowUi(UiContext uiContext)
    {
        UpdateBindingDisplay();
        rebindUi.SetActive(true);
    }

    public void HideUi()
    {
        rebindUi.SetActive(false);
    }

    private void StartRebind()
    {
        if (rebindOperation != null) return;

        InputAction action = actionReference.action;
        action.Disable();
        bindingText.text = "Press New Input...";

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

    private void UpdateBindingDisplay()
    {
        if (actionReference == null || actionReference.action == null || bindingText == null) return;
        bindingText.text = actionReference.action.GetBindingDisplayString(bindingIndex);
    }

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
}
