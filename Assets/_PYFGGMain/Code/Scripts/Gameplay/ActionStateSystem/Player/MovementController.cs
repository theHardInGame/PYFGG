using System;
using UnityEngine;

/// <summary>
/// Override modes that modify how player-controlled movement is applied.
/// </summary>
public enum MovementOverrideMode
{
    /// <summary>
    /// No override; normal movement behavior.
    /// </summary>
    None, 

    /// <summary>
    /// Player-controlled movement is completely disabled.
    /// Base velocity is forced to zero.
    /// </summary>
    Suppressed, 

    /// <summary>
    /// Player-controlled movement is scaled by a multiplier.
    /// External forces are unaffected.
    /// </summary>
    Scaled
}


/// <summary>
/// Player horizontal movement controller.
/// <para>
/// This component interprets player input and converts it into horizontal
/// base velocity commands issued to a <see cref="MotorController"/>.
/// </para>
/// <para>
/// Key characteristics:
/// <list type="bullet">
/// <item>Handles only X/Z movement (no vertical control)</item>
/// <item>Uses direct velocity on ground</item>
/// <item>Uses acceleration-based movement in air</item>
/// <item>Supports suppression and scaling overrides</item>
/// </list>
/// </para>
/// <para>
/// Vertical motion (jumping, falling, gravity) is handled by other systems.
/// </para>
/// </summary>
public class MovementController : MonoBehaviour
{
    [Header("Ground Movement Config")]
    [Tooltip("Base walking speed when grounded.")]
    [SerializeField] private float walkSpeed;

    [Tooltip("Multiplier applied to walk speed while sprinting.")]
    [SerializeField] private float sprintMultiplier;


    [Header("Air Movement Config")]
    [Tooltip("Maximum horizontal speed while airborne.")]
    [SerializeField] private float maxAirSpeed = 5f;
    
    [Tooltip("Acceleration applied while steering in the air.")]
    [SerializeField] private float airAcceleration = 10f;
    
    [Tooltip("Deceleration applied when no air input is present.")]
    [SerializeField] private float airBrakeAcceleration = 6f;
    
    [Tooltip("Difference to target velocity below which interpolation is used instead of linear acceleration.")]
    [SerializeField] private float airLerpThreshold = 1.5f;


    [Header("Modules")]
    [Tooltip("Facade used to apply movement to the Rigidbody system.")]
    [SerializeField] private MotorController motorController;



    /// <summary>
    /// Current horizontal BaseVelocity derived from the motor.
    /// Vertical velocity is intentionally ignored.
    /// </summary>
    private Vector3 CurrentHorizontalBaseVelocity => new Vector3(motorController.BaseVelocity.x, 0f, motorController.BaseVelocity.z);

    /// <summary>
    /// Current horizontal Composite velocity derived from the motor
    /// Vertical veloctiy is intentionally ignored.
    /// </summary>
    private Vector3 CurrentHorizontalVelocity => new(motorController.CurrentVelocity.x, 0f, motorController.CurrentVelocity.z);

    // =================
    // INPUT STATE
    // =================

    // Raw input values
    private float xInput;
    private float zInput;
    private bool sprintInput;

    /// <summary>
    /// Player movement intent in world space.
    /// </summary>
    private Vector3 HorizontalInput => new Vector3(xInput, 0f, zInput);
    

    // =================
    // OVERRIDE STATE
    // =================

    private MovementOverrideMode overrideMode;
    private float overrideScale = 1f;

    /// <summary>
    /// Scale applied only to player-intentional movement.
    /// External forces such as impulses, knockback, and gravity
    /// are intentionally unaffected.
    /// </summary>
    private float MovementScale => overrideMode == MovementOverrideMode.Scaled ? overrideScale : 1f;



    #region UNITY API
    // =================
    // =   UNITY API   =
    // =================

    private InputManager inputManager;

    private void Awake()
    {
        inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        inputManager.MovementInputEvent += ReadMoveInput;
        inputManager.DashInputEvent += ReadSprintInput;
    }

    private void OnDisable()
    {
        inputManager.MovementInputEvent -= ReadMoveInput;
        inputManager.DashInputEvent -= ReadSprintInput;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }
    #endregion


    // =================
    // INPUT HANDLING
    // =================

    /// <summary>
    /// Reads either single-axis (side-scroller) or planar (top-down) movement input.
    /// </summary>
    private void ReadMoveInput(object obj)
    {
        switch (obj)
        {
            case float x:
                xInput = x;
                zInput = 0f;
                break;

            case Vector2 v:
                xInput = v.x;
                zInput = v.y;
                break;

            default:
                xInput = default;
                zInput = default;
                break;
        }

        if (HorizontalInput.sqrMagnitude > 0.001f) LookDirection = HorizontalInput;
    }

    /// <summary>
    /// Reads sprint input.
    /// </summary>
    private void ReadSprintInput(float input)
    {
        sprintInput = input > 0;
    }


    // =================
    // MOVEMENT ROUTING
    // =================

    /// <summary>
    /// Applies movement logic based on grounded state.
    /// </summary>
    private void ApplyMovement()
    {
        if (overrideMode == MovementOverrideMode.Suppressed) return;

        if (motorController == null)
        {
            Debug.LogError($"No MotorController found in MovementController \n { Environment.StackTrace }");
            return;
        }

        if (motorController.IsGrounded)
        {
            ApplyGroundMovement();
        }
        else
        {
            ApplyAirMovement();
        }
    }


    // =================
    // GROUND MOVEMENT
    // =================

    /// <summary>
    /// Applies direct velocity-based movement while grounded.
    /// </summary>
    private void ApplyGroundMovement()
    {
        Vector3 input = HorizontalInput * MovementScale;

        Vector3 targetVel = new(CalculateGroundSpeed(input.x), 0f, CalculateGroundSpeed(input.z));

        SetHorizontalBaseVelocity(targetVel);
    }

    /// <summary>
    /// Converts input into ground velocity, accounting for sprinting.
    /// </summary>
    private float CalculateGroundSpeed(float input)
    {
        if (Mathf.Abs(input) < 0.1f)
        {
            return 0f;
        }

        float vel = input * walkSpeed;
        if(sprintInput)
        {
            vel *= sprintMultiplier;
        }

        return vel;
    }

    // ==============
    // AIR MOVEMENT
    // ============== 


    /// <summary>
    /// Applies acceleration-based movement while airborne.
    /// </summary>
    private void ApplyAirMovement()
    {
        Vector3 input = HorizontalInput;

        if (input.sqrMagnitude < 0.001f)
        {
            ApplyAirBraking();
            return;
        }

        ApplyAirAcceleration(input.normalized);
    }


    /// <summary>
    /// Gradually slows horizontal movement when no air input is present.
    /// </summary>
    private void ApplyAirBraking()
    {
        Vector3 vel = CurrentHorizontalBaseVelocity;

        vel = Vector3.MoveTowards(
            vel,
            Vector3.zero,
            airBrakeAcceleration * Time.fixedDeltaTime
        );

        vel *= MovementScale;
        SetHorizontalBaseVelocity(vel);
    }


    /// <summary>
    /// Accelerates horizontal velocity toward the desired air movement direction.
    /// </summary>
    private void ApplyAirAcceleration(Vector3 inputDir)
    {
        float maxSpeed = maxAirSpeed * MovementScale;
        Vector3 vel = CurrentHorizontalVelocity;
        Vector3 target = inputDir * maxSpeed;

        Vector3 delta = target - vel;

        // Switch between linear acceleration and smooth interpolation
        // to avoid jitter when close to target speed.
        if (delta.magnitude > airLerpThreshold)
            vel += delta.normalized * airAcceleration * Time.fixedDeltaTime;
        else
            vel = Vector3.Lerp(vel, target, airAcceleration * Time.fixedDeltaTime);

        if (vel.magnitude > maxSpeed)
            vel = vel.normalized * maxSpeed;

        SetHorizontalBaseVelocity(vel);
    }


    // =================
    //  MOTOR INTERFACE
    // =================

    /// <summary>
    /// Writes only horizontal base velocity to the motor.
    /// </summary>
    private void SetHorizontalBaseVelocity(Vector3 vel)
    {
        if (motorController == null)
        {
            Debug.LogError($"No MotorController found in MovementController \n { Environment.StackTrace }");
            return;
        }

        motorController.SetBaseVelocityX(vel.x);
        motorController.SetBaseVelocityZ(vel.z);
    }



    #region PUBLIC API
    // ==================
    // =   PUBLIC API   =
    // ==================

    /// <summary>
    /// Completely disables player-controlled movement.
    /// Base velocity is cleared immediately.
    /// Other velocity layers (impulse, gravity) are unaffected.
    /// </summary>
    public void SupressMovement()
    {
        motorController.SetBaseVelocity(Vector3.zero);
        overrideMode = MovementOverrideMode.Suppressed;
    }

    /// <summary>
    /// Scales all player-controlled movement by a factor.
    /// Useful for slow, stun, or environmental effects.
    /// </summary>
    /// <param name="scale">
    /// Multiplier in range [0, 1]. Values are clamped.
    /// </param>
    public void ScaleMovement(float scale)
    {
        overrideMode = MovementOverrideMode.Scaled;
        overrideScale = Mathf.Clamp01(scale);
    }

    /// <summary>
    /// Clears any active movement override and restores normal movement.
    /// </summary>
    public void ClearOverride()
    {
        overrideMode = MovementOverrideMode.None;
    }

    /// <summary>
    /// Returns true while sprint input is held.
    /// </summary>
    public bool IsSprinting => sprintInput;

    /// <summary>
    /// Returns true when meaningful movement input is present.
    /// Independent of actual velocity.
    /// </summary>
    public bool IsWalking => Mathf.Abs(xInput) > 0.1f || Mathf.Abs(zInput) > 0.1f;

    public Vector3 LookDirection { get; private set; } = Vector3.right;
    #endregion


    #region Debug

    // =============
    // =   DEBUG   =
    // =============

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (motorController == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        // ----- Movement Intent -----
        Vector3 intent = HorizontalInput.normalized;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + intent);
        Gizmos.DrawSphere(origin + intent, 0.05f);

        // ----- Current Horizontal Velocity -----
        Vector3 vel = Application.isPlaying ? CurrentHorizontalBaseVelocity : Vector3.zero;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + vel * 0.2f);

        // ----- Grounded State -----
        Gizmos.color = motorController.IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, 0.15f);
    }
#endif
    #endregion
}