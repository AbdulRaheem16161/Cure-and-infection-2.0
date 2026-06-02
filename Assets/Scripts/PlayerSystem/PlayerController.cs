using UnityEngine;
using static NPCSpawner;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(EquipmentHandler))]
[RequireComponent(typeof(InventoryHandler))]
public class PlayerController : MonoBehaviour
{
    private bool _initialized = false;

    public EntityDefinition Definition;

    #region 1st Person Camera + Settings
    private Camera PlayerCamera;
    private readonly float lookSensitivity = 0.05f;
    private readonly float minCameraPitch = -70f;
    private readonly float maxCameraPitch = 60f;
    private float pitch;
    #endregion

    private CharacterController CharacterController;
    private StatsHandler StatsHandler;
    private EquipmentHandler EquipmentHandler;
	private InventoryHandler InventoryHandler;

    public bool IsSprinting => InputManager.Instance.Sprinting;

	private void Awake()
	{
        PlayerCamera = GetComponentInChildren<Camera>();
        CharacterController = GetComponent<CharacterController>();
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		InventoryHandler = GetComponent<InventoryHandler>();
	}

    private void Start()
    {
        if (!_initialized)
        {
            if (Definition != null)
                InitializePlayer(Definition, StatsHandler.Team); //keep current team
            else
                Debug.LogError($"{typeof(EntityDefinition)} null, assign reference in inspector when not using a NpcSpawner");
        }
    }

    public void InitializePlayer(EntityDefinition definition, Teams team)
    {
        if (definition == null)
        {
            Debug.LogError($"{typeof(EntityDefinition)} null, NpcSpawner failed to assign definition");
            return;
        }

        Definition = definition;
        gameObject.name = Definition.Name;

        StatsHandler.InitializeStats(team, Definition);
        InventoryHandler.InitializeInventoryHandler();
        EquipmentHandler.InitializeEquipmentHandler(Definition);
        _initialized = true;
    }

    private void Update()
    {
        HandleMovement();
        HandleLooking();
    }

    private void HandleMovement(bool debugLog = false)
    {
        Vector2 moveInput = InputManager.Instance.Move;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (IsSprinting)
            CharacterController.Move(Definition.SprintSpeed * Time.deltaTime * move);
        else
            CharacterController.Move(Definition.WalkSpeed * Time.deltaTime * move);

        if (debugLog) Debug.Log($"Move: {moveInput}");
    }
    private void HandleLooking(bool debugLog = false)
    {
        Vector2 lookInput = InputManager.Instance.Look;
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minCameraPitch, maxCameraPitch);

        PlayerCamera.gameObject.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (debugLog) Debug.Log($"Look: {lookInput}");
    }
}
