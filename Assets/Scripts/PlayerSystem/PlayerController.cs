using UnityEngine;
using static NPCSpawner;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(EquipmentHandler))]
[RequireComponent(typeof(InventoryHandler))]
[RequireComponent(typeof(Interactor))]
public class PlayerController : MonoBehaviour
{
    private bool _initialized = false;

    public EntityDefinition Definition;

    private CharacterController CharacterController;
    private Interactor Interactor;

    public StatsHandler StatsHandler { get; private set; }
    public InventoryHandler InventoryHandler { get; private set; }
    public EquipmentHandler EquipmentHandler { get; private set; }

    #region 1st Person Camera + Settings
    private Camera PlayerCamera;
    private readonly float lookSensitivity = 0.05f;
    private readonly float minCameraPitch = -70f;
    private readonly float maxCameraPitch = 60f;
    private float pitch;
    #endregion

    #region Ground Check + Settings
    public bool Grounded => Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundMask;

    private readonly float gravity = -9.81f;
    private float verticalVelocity;
    #endregion

    public bool IsSprinting => InputManager.Instance.Sprinting; //constant sprinting check

    #region Player Initialization
    private void Awake()
	{
        PlayerCamera = GetComponentInChildren<Camera>();
        CharacterController = GetComponent<CharacterController>();
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		InventoryHandler = GetComponent<InventoryHandler>();
        Interactor = GetComponent<Interactor>();
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
    #endregion

    private void Update()
    {
        Interactor.TickSearchForInteractables(Time.deltaTime);
        HandleMovement();
        HandleLooking();

        HandlePrimaryAction();
        HandleSecondaryAction();
        HandleReloadAction();
        HandleInteractAction();

        HandleHotbarActions();
    }

    #region Handle Player Movement and Looking
    private void HandleMovement(bool debugLog = false)
    {
        Vector2 moveInput = InputManager.Instance.Move;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed = IsSprinting ? Definition.SprintSpeed : Definition.WalkSpeed;

        if (Grounded && verticalVelocity < 0)
            verticalVelocity = -5f;

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 finalMove = move * speed;
        finalMove.y = verticalVelocity;

        CharacterController.Move(finalMove * Time.deltaTime);

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
    #endregion

    #region Handle Player Actions
    private void HandlePrimaryAction()
    {
        if (InputManager.Instance.PrimaryAction)
        {
            if (!EquipmentHandler.HasItemInHands) return;

            if (EquipmentHandler.itemInHands is RangedWeaponItem rangedWeapon)
                rangedWeapon.Shoot();
            else if (EquipmentHandler.itemInHands is MeleeWeaponItem meleeWeapon)
                meleeWeapon.LightAttack();
            else
                Debug.LogWarning("Primary action with non-weapon item in hands occured");
        }
    }

    private void HandleSecondaryAction()
    {
        if (InputManager.Instance.SecondaryAction)
        {
            if (!EquipmentHandler.HasItemInHands) return;

            if (EquipmentHandler.itemInHands is RangedWeaponItem rangedWeapon)
            {
                if (rangedWeapon.Aim == RangedWeaponItem.AimState.hipfire)
                    rangedWeapon.EnterAimDownSights();
            }
            else if (EquipmentHandler.itemInHands is MeleeWeaponItem meleeWeapon)
                meleeWeapon.HeavyAttack();
            else
                Debug.LogWarning("Secondary action with non-weapon item in hands occured");
        }
        else
        {
            if (EquipmentHandler.itemInHands is RangedWeaponItem rangedWeapon)
            {
                if (rangedWeapon.Aim == RangedWeaponItem.AimState.ads)
                    rangedWeapon.ExitAimDownSights();
            }
        }
    }

    private void HandleReloadAction()
    {
        if (InputManager.Instance.ReloadAction)
        {
            if (!EquipmentHandler.HasItemInHands) return;
            if (EquipmentHandler.itemInHands is not RangedWeaponItem rangedWeapon) return;

            rangedWeapon.Reload(InventoryHandler.ItemContainer, true);
        }
    }

    private void HandleInteractAction()
    {
        if (InputManager.Instance.InteractPressAction)
            Interactor.InteractPress();

        if (InputManager.Instance.InteractHoldAction)
            Interactor.InteractHold(true);
        else
            Interactor.InteractHold(false);
    }
    #endregion

    #region Handle Hotbar Actions
    private void HandleHotbarActions()
    {
        if (InputManager.Instance.HotbarPressed(0))
            EquipmentHandler.UnholsterWeapon(EquipmentHandler.EquipmentType.weaponOne);

        if (InputManager.Instance.HotbarPressed(1))
            EquipmentHandler.UnholsterWeapon(EquipmentHandler.EquipmentType.weaponTwo);

        if (InputManager.Instance.HotbarPressed(2))
            EquipmentHandler.UnholsterWeapon(EquipmentHandler.EquipmentType.weaponMelee);

        if (InputManager.Instance.HotbarPressed(3))
            EquipmentHandler.UseConsumable(EquipmentHandler.EquipmentType.consumableOne);

        if (InputManager.Instance.HotbarPressed(4))
            EquipmentHandler.UseConsumable(EquipmentHandler.EquipmentType.consumableTwo);

        if (InputManager.Instance.HotbarPressed(5))
            EquipmentHandler.UseConsumable(EquipmentHandler.EquipmentType.consumableThree);
    }
    #endregion
}
