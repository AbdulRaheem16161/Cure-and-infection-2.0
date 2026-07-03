using System.Collections;
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
    public Interactor Interactor { get; private set; }

    public StatsHandler StatsHandler { get; private set; }
    public InventoryHandler InventoryHandler { get; private set; }
    public EquipmentHandler EquipmentHandler { get; private set; }

    #region 1st Person Camera + Settings
    [Header("Player Camera Settings")]
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private Camera PlayerCamera;
    private LayerMask hitMask;
    [SerializeField] private float lookSensitivity = 0.05f;
    [SerializeField] private float minCameraPitch = -70f;
    [SerializeField] private float maxCameraPitch = 60f;
    private float pitch;
    #endregion

    #region Camera Flinch On Hit Settings
    [Header("Camera Flinch On Hit Settings")]
    [SerializeField] private float flinchAngle = 5f;
    [SerializeField] private float flinchDuration = 0.15f;
    private Coroutine cameraFlinchRoutine;
    #endregion

    #region Ground Check + Settings
    public bool Grounded => Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private readonly float gravity = -9.81f;
    private float verticalVelocity;
    #endregion

    public bool IsSprinting => InputManager.Instance.Sprinting; //constant sprinting check

    #region Player Initialization
    private void Awake()
	{
        PlayerCamera = cameraPivot.GetComponentInChildren<Camera>();
        CharacterController = GetComponent<CharacterController>();
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();
		InventoryHandler = GetComponent<InventoryHandler>();
        Interactor = GetComponent<Interactor>();

        hitMask = LayerMask.GetMask("Environment", "EnvironmentCover", "CharacterDetection");
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

        StatsHandler.OnHit += OnHit;
    }
    private void OnDestroy()
    {
        StatsHandler.OnHit -= OnHit;
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
        HandleToggleFireModeAction();
        HandleInteractAction();

        HandleHotbarActions();
    }

    private void LateUpdate()
    {
        EquipmentHandler.PivotItemInHandsToAimPoint(GetAimPoint());
    }

    #region Get AimPoint For Player
    private Vector3 GetAimPoint()
    {
        Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, 10000, hitMask))
            return hit.point;

        return ray.origin + ray.direction * 10000;
    }
    #endregion

    #region Player Camera Flinch On Hit
    private void OnHit(DamageContext damageContext)
    {
        ApplyCameraFlinch(damageContext);
    }

    private void ApplyCameraFlinch(DamageContext damageContext)
    {
        if (damageContext.ImpactType == DamageContext.HitImpact.none) return;

        float flinchAngle = this.flinchAngle;

        if (damageContext.ImpactType == DamageContext.HitImpact.knockback)
            flinchAngle *= 3f;

        if (cameraFlinchRoutine != null)
            StopCoroutine(cameraFlinchRoutine);

        cameraFlinchRoutine = StartCoroutine(CameraFlinch(flinchAngle));
    }

    private IEnumerator CameraFlinch(float flinchAngle)
    {
        float randomPitch = Random.Range(-flinchAngle, flinchAngle);
        float randomYaw = Random.Range(-flinchAngle, flinchAngle);

        Quaternion targetRotation = Quaternion.identity * Quaternion.Euler(randomPitch, randomYaw, 0f);
        float elapsed = 0f;

        // Flinch in
        while (elapsed < flinchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flinchDuration;

            PlayerCamera.transform.localRotation = Quaternion.Slerp(Quaternion.identity, targetRotation, t);

            yield return null;
        }

        elapsed = 0f;

        // Return to neutral (0,0,0)
        while (elapsed < flinchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flinchDuration;

            PlayerCamera.transform.localRotation =
                Quaternion.Slerp(targetRotation, Quaternion.identity, t);

            yield return null;
        }

        PlayerCamera.transform.localRotation = Quaternion.identity;
    }
    #endregion

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

        cameraPivot.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (debugLog) Debug.Log($"Look: {lookInput}");
    }
    #endregion

    #region Handle Player Actions
    private void HandlePrimaryAction()
    {
        if (!EquipmentHandler.HasItemInHands) return;

        if (EquipmentHandler.itemInHands is RangedWeaponItem rangedWeapon)
        {
            if (rangedWeapon.CanHoldFire && InputManager.Instance.PrimaryActionHeld)
                rangedWeapon.Shoot();
            else if (!rangedWeapon.CanHoldFire && InputManager.Instance.PrimaryActionPressed)
                rangedWeapon.Shoot();
        }
        else if (EquipmentHandler.itemInHands is MeleeWeaponItem meleeWeapon && InputManager.Instance.PrimaryActionPressed)
            meleeWeapon.LightAttack();
        else
            Debug.LogWarning("Primary action with non-weapon item in hands occured");
    }

    private void HandleSecondaryAction()
    {
        if (!EquipmentHandler.HasItemInHands) return;

        if (EquipmentHandler.itemInHands is RangedWeaponItem)
            EquipmentHandler.SetAimDownSights(InputManager.Instance.SecondaryActionHeld);
        else if (EquipmentHandler.itemInHands is MeleeWeaponItem meleeWeapon && InputManager.Instance.SecondaryActionPressed)
            meleeWeapon.HeavyAttack();
        else
            Debug.LogWarning("Secondary action with non-weapon item in hands occured");
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

    private void HandleToggleFireModeAction()
    {
        if (!EquipmentHandler.HasItemInHands) return;

        if (EquipmentHandler.itemInHands is RangedWeaponItem rangedWeapon && InputManager.Instance.ToggleFireModeAction)
            rangedWeapon.CycleFireMode();
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
