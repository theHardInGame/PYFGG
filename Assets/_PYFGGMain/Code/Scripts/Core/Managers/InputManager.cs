using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class InputManager :  MonoBehaviour, IInitializable
{
    #region Unity API
    // =================
    // =   UNITY API   =
    // =================

    private void OnEnable()
    {
        if (Instance == null) return;

        EnableMovementActions();
    }

    private void OnDisable()
    {
        if (Instance == null) return;

        DisableMovementActions();
    }
    #endregion

    #region Initializable
    // =====================
    // =   INITIALIZABLE   =
    // =====================

    // ============
    // REFERENCES
    // ============

    public static InputManager Instance { get; private set; }
    [Header("INPUT ACTION ASSET"), SerializeField] InputActionAsset inputActionAsset;

    /// <summary>
    /// Initialize InputManager 
    /// Initialize before referencing the static instance
    /// Only call from bootloader
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        await Awaitable.MainThreadAsync();

        // Destroy if dupes
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Initialisation Tasks
        if (inputActionAsset == null)
        {
            Debug.LogError("InputActionAsset is null. Check SerializedField in inspector");
        }

        SetupMovementControls();

        // DDOL Setup
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
    #endregion

    #region Movement Controls
    // =========================
    // =   MOVEMENT CONTROLS   =
    // =========================

    // ===========
    // VARIABLES
    // ===========

    // === MOVEMENTMODE ENUM ===
    public enum MovementMode
    {
        TopDown,
        SideScrolling
    }

    // === SERIALIZED VARIABLES ===
    
    [Space(20)]
    [Header("MOVEMENT CONTROL")]
    [Space(10)]
    [SerializeField] string movementInputMapName = "PlayerMovement";
    [SerializeField] string topDownMovementActionName = "TopDownMovement";
    [SerializeField] string sideScrollingMovementActionName = "SideScrollingMovement";
    [SerializeField] string jumpActionName = "Jump";
    [SerializeField] string dashActionName = "Dash";


    // === REFERENCES ===

    InputActionMap movementInputMap;
    InputAction topDownMovementAction;
    InputAction sideScrollingMovementAction;
    InputAction jumpAction;
    InputAction dashAction;


    // TEMPORARY IMPLEMENTATION. REMOVE SERIALIZEFIELD TAG BEFORE BUILDTIME
    [SerializeField] private MovementMode movementMode;


    // === PUBLIC REFERENCES ===

    /*public object Move()
    {
        if(movementMode == MovementMode.SideScrolling)
        {
            return sideScrollingMovement;
        }
        else
        {
            return topDownMovement.normalized;
        }
    }*/
    public event Action<object> MovementInputEvent;
    private event Action<Vector2> topDownMovement;
    private event Action<float> sideScrollingMovement;
    public event Action<float> JumpInputEvent;
    public event Action<float> DashInputEvent;

    // =======
    // SETUP 
    // =======

    /// <summary>
    /// Setup and register movement inputs
    /// Only to be called from IInitializable implementation (once globably)
    /// </summary>
    /// <returns></returns>
    private void SetupMovementControls()
    {
        // Get References
        movementInputMap = FindInputActionMap(movementInputMapName);

        topDownMovementAction = FindInputAction(topDownMovementActionName, movementInputMap);
        sideScrollingMovementAction = FindInputAction(sideScrollingMovementActionName, movementInputMap);
        jumpAction = FindInputAction(jumpActionName, movementInputMap);
        dashAction = FindInputAction(dashActionName, movementInputMap);

        // Register References
        RegisterInputAction(topDownMovementAction, v => topDownMovement?.Invoke(v), Vector2.zero);
        RegisterInputAction(sideScrollingMovementAction, f => sideScrollingMovement?.Invoke(f), 0.0f);
        RegisterInputAction(jumpAction, f => JumpInputEvent?.Invoke(f), 0.0f);
        RegisterInputAction(dashAction, f => DashInputEvent?.Invoke(f), 0.0f);

        SetMovementMode(movementMode);

        EnableMovementActions();
    }

    private void MultiinputMovement<T>(T obj)
    {
        MovementInputEvent?.Invoke(obj);
    }

    // ============
    // PUBLIC API
    // ============

    /// <summary>
    /// Set Movement Mode. Used for switching between TopDown and SideScrolling schemes
    /// </summary>
    public void SetMovementMode(MovementMode mode)
    {
        if (mode == MovementMode.SideScrolling)
        {
            topDownMovement -= MultiinputMovement;
            sideScrollingMovement += MultiinputMovement;
        }
        else
        {
            sideScrollingMovement -= MultiinputMovement;
            topDownMovement += MultiinputMovement;
        }
    }

    // ==================
    // HANDLERS/HELPERS
    // ==================

    /// <summary>
    /// Enables player movement
    /// </summary>
    private void EnableMovementActions()
    {
        topDownMovementAction?.Enable();
        sideScrollingMovementAction?.Enable();
        jumpAction?.Enable();
        dashAction?.Enable();
    }
    
    /// <summary>
    /// Disables player movement
    /// </summary>
    private void DisableMovementActions()
    {
        topDownMovementAction?.Disable();
        sideScrollingMovementAction?.Disable();
        jumpAction?.Disable();
        dashAction?.Disable();
    }
    #endregion



    #region Handlers
    // ========================
    // =   HANLDERS/HELPERS   =
    // ========================

    /// <summary>
    /// Finds and returns InputActionMap based on string passed
    /// </summary>
    /// <param name="mapName">Name of InputActionMap. Must not be null or empty</param>
    /// <returns>InputActionMap</returns>
    private InputActionMap FindInputActionMap(string mapName)
    {
        if(!string.IsNullOrEmpty(mapName))
        {
            InputActionMap map = inputActionAsset.FindActionMap(mapName);

            if(map != null)
            {
                return map;
            }
            else
            {
                Debug.LogError($"Could not find InputActionMap: {mapName}");
                return null;
            }
        }

        Debug.LogError($"ActionMapName is null/empty! + \n + {Environment.StackTrace}");
        return null;
    }

    /// <summary>
    /// Finds and Input Action based on string and map passed
    /// </summary>
    /// <param name="actionName">Name of InputAction. Must not be null or empty</param>
    /// <param name="map">InputActionMap to find InputAction in. Must not be null</param>
    /// <returns>InputAction</returns>
    private InputAction FindInputAction(string actionName, InputActionMap map)
    {
        if (!string.IsNullOrEmpty(actionName))
        {
            if (map == null)
            {
                Debug.LogError($"Trying to find Action '{actionName}' in a null InputActionMap '{map}'");
                return null;
            }

            InputAction action = map.FindAction(actionName);

            if (action != null)
            {
                return action;
            }
            else
            {
                Debug.LogError($"Could not find InputAction: {actionName} in InputActionMap {map.name}");
                return null;
            }
        }

        Debug.LogError($"Action name is null/empty! + \n + {Environment.StackTrace}");
        return null;
    }

    /// <summary>
    /// Registers an InputAction to a public reference using a setter delegate. <br/>
    /// <para>
    /// Example use: <br/>
    /// RegisterInputAction(Jump, f => JumpEvent?.Invoke(f), 0.0f) <br/>
    /// Where:
    /// <list type="bullet">
    ///     <item> Jump: InputAction </item>
    ///     <item> JumpEvent: public event </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="T">Type of input. Must be a value type (struct).</typeparam>
    /// <param name="inputAction">The InputAction to register. Must not be null.</param>
    /// <param name="callback">Delegate to assign the value to your field.</param>
    /// <param name="defaultValue">Value assigned when the action is canceled.</param>
    private void RegisterInputAction<T>(InputAction inputAction, Action<T> callback, T defaultValue) where T : struct
    {
        if (inputAction == null)
        {
            Debug.LogError($"Attempted to register null Action + \n + {Environment.StackTrace}");
            return;
        }

        inputAction.performed += context => callback(context.ReadValue<T>());
        inputAction.canceled += context => callback(defaultValue);
    }
    #endregion
}
