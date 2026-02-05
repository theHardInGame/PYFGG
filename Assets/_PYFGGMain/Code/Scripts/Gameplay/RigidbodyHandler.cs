using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;


[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class RigidbodyHandler : MonoBehaviour
{
    #region Variables
    // =================
    // =   VARIABLES   =
    // =================

    public Vector3 CurrentCompVelocity => hasOverride ? OverrideVelocity : BaseVelocity + AdditiveVelocity + ImpulseVelocity + GravityVelocity;
    public Vector3 CurrentRBDVelocity => rbd.linearVelocity;
    public Vector3 CurrentPosition => rbd.position;
    

    // === BASE VELOCITY VARIABLES ===
    public Vector3 BaseVelocity { get; private set; }


    // === ADDITIVE VELOCITY VARIABLES ===
    private readonly List<IAdditiveVelocitySource> additiveVelocities = new();
    public IReadOnlyList<IAdditiveVelocitySource> AdditiveVelocities => additiveVelocities.AsReadOnly();
    public Vector3 AdditiveVelocity { get; private set; }


    // === IMPULSE VELOCITY VARAIABLES ===
    private bool hasImpulse;
    private float impulseDecayTime;
    private Vector3 currentMaxImpulseVelocity;
    public Vector3 ImpulseVelocity { get; private set; }


    // === GRAVITY VARIABLES ===
    public Vector3 GravityVelocity { get; private set; }


    // === OVERRIDE VELOCITY VARIABLES  ===
    private bool hasOverride = false;
    private Vector3 overrideDestination;
    public Vector3 OverrideVelocity { get; private set; }


    // === COLLIDER DATA ===
    private List<ColliderData> RBHColliders = new();


    // === REQUIRED COMPONENT REFERENCE ===
    private Rigidbody rbd;
    #endregion

    #region Unity API
    // =================
    // =   UNITY API   =
    // =================

    private void Awake()
    {
        rbd = GetComponent<Rigidbody>();
        if (rbd == null) Debug.LogError($"Gameobject does not have Rigidbody component {Environment.StackTrace}");
    }

    private void FixedUpdate()
    {
        CalculateAdditiveVelocity();
        HandleOverrideMotion();
        ApplyVelocities();
        ImpulseDecay();
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint c in collision.contacts)
        {
            Vector3 normal = c.normal;

            if (Vector3.Dot(normal, rbd.transform.up) < 0f)
            {
                if (Vector3.Dot(BaseVelocity, normal) < 0f) BaseVelocity -= Vector3.Project(BaseVelocity, normal);
                if (Vector3.Dot(ImpulseVelocity, normal) < 0f) ImpulseVelocity -= Vector3.Project(ImpulseVelocity, normal);
            }
        }
    }

    #endregion

    #region Movement Calculations
    // =============================
    // =   MOVEMENT CALCULATIONS   =
    // =============================

    /// <summary>
    /// Apply all the velocities to Rigidbody component
    /// </summary>
    private void ApplyVelocities()
    {
        Vector3 compositeVelocity;
        if (hasOverride)
        {
            compositeVelocity = OverrideVelocity;
        }
        else 
        {
            compositeVelocity = BaseVelocity + AdditiveVelocity + ImpulseVelocity + GravityVelocity;
        }
        
        //movePosition = CollisionDetection(movePosition, rbd.position);

        Vector3 delta = compositeVelocity - CurrentRBDVelocity;

        // Apply as velocity change
        rbd.AddForce(delta, ForceMode.VelocityChange);


        //rbd.linearVelocity = compositeVelocity;

        //rbd.MovePosition(rbd.position + movePosition);
    }


    // ================
    // HANDLER/HELPER
    // ================

    /// <summary>
    /// Add all AdditiveVelocities together. To be called everytime new velocities are added.
    /// </summary>
    private void CalculateAdditiveVelocity()
    {
        AdditiveVelocity = Vector3.zero;

        if (additiveVelocities.Count == 0)
        {
            return;
        }

        foreach (IAdditiveVelocitySource source in additiveVelocities)
        {
            AdditiveVelocity += source.Velocity;
        }
    }
    
    /// <summary>
    /// Decay ImpulseVelocity every FixedUpdate within impulseDecayTime when the ImpulseVelocity is greater than 0. To be called every FixedUpdate.
    /// </summary>
    private void ImpulseDecay()
    {
        if (!hasImpulse) return;

        // If impulse is basically gone, stop
        if (ImpulseVelocity.sqrMagnitude < 0.0001f)
        {
            ImpulseVelocity = Vector3.zero;
            hasImpulse = false;
            return;
        }

        // Otherwise decay normally
        float decayRate = currentMaxImpulseVelocity.magnitude / impulseDecayTime;

        ImpulseVelocity = Vector3.MoveTowards(
            ImpulseVelocity,
            Vector3.zero,
            decayRate * Time.fixedDeltaTime
        );
    }

    /// <summary>
    /// Checks if Rigidbody has reached overrideDestination via OverrideVelocity and resets override when done. To be called every FixedUpdate.
    /// </summary>
    private void HandleOverrideMotion()
    {
        if (!hasOverride) return;

        Vector3 next = rbd.position + (OverrideVelocity * Time.fixedDeltaTime);

        if ((overrideDestination - rbd.position).sqrMagnitude < (overrideDestination - next).sqrMagnitude)
        {
            OverrideVelocity = Vector3.zero;
            rbd.position = overrideDestination;
            hasOverride = false;
            return;
        }

        if (Vector3.Distance(rbd.position, overrideDestination) < 0.001f)
        {
            OverrideVelocity = Vector3.zero;
            hasOverride = false;
        }
    }
    #endregion


    #region Public API
    // ==================
    // =   PUBLIC API   =
    // ==================

    // ===============
    // BASE VELOCITY
    // ===============

    /// <summary>
    /// Sets BaseVelocity for Rigidbody component. Best used for conscious movements like player inputs or npc movement.
    /// </summary>
    /// <param name="velocity">BaseVelocity</param>
    public void SetBaseVelocity(Vector3 velocity)
    {
        BaseVelocity = velocity;
    }


    // ==================
    // GRAVITY VELOCITY
    // ==================

    /// <summary>
    /// Sets GravityVelocity for the Rigidody component. This is only static gravity velocity; Acceleration is to be precalculated by Gravity component separately.
    /// </summary>
    /// <param name="velocity">Gravity velocity for the frame.</param>
    public void SetGravityVelocity(Vector3 velocity)
    {
        GravityVelocity = velocity;
    }


    // ===================
    // ADDITIVE VELOCITY
    // ===================

    /// <summary>
    /// Adds AdditiveVelocity to the effect list for the Rigidbody.
    /// </summary>
    /// <param name="velocity">Velocity from the effect.</param>
    /// <param name="id">ID of the effect.</param>
    public void RegisterAdditiveVelocity(IAdditiveVelocitySource source)
    {
        if (additiveVelocities.Contains(source))
        {
            Debug.LogWarning($"AdditiveVelocity is being added to velocity calculation more than once! \n {Environment.StackTrace}");
        }

        additiveVelocities.Add(source);
    }

    /// <summary>
    /// Removes AdditiveVelocity of ID from the effect list of the Rigidbody.
    /// </summary>
    /// <param name="id">ID of the effect.</param>
    public void RemoveAdditiveVelocity(IAdditiveVelocitySource source)
    {
        if (!additiveVelocities.Contains(source))
        {
            Debug.LogError($"Trying to remove AdditiveVelocity which was never added! \n { Environment.StackTrace }");
            return;
        }

        additiveVelocities.Remove(source);
    }


    // ==================
    // IMPULSE VELOCITY
    // ==================

    /// <summary>
    /// Sets ImpulseVelocity which decays in decayTime. Only one ImpulseVelocity can affect a Rigidbody at a time.
    /// </summary>
    /// <param name="velocity">ImpulseVelocity. Must not be 0.</param>
    /// <param name="decayTime">decayTime for ImpulseVelocity. Must not be 0.</param>
    public void SetImpulseVelocity(Vector3 velocity, float decayTime)
    {
        if (velocity == Vector3.zero)
        {
            Debug.LogError($"ImpulseVelocity must not be set to 0 \n { Environment.StackTrace }");
        }
        if (decayTime <= Time.fixedDeltaTime)
        {
            Debug.LogError($"decayTime is smaller than time for each PhysicStep. decayTime is not alloved to be set to zero mannually \n { Environment.StackTrace }");
        }

        ImpulseVelocity = currentMaxImpulseVelocity = velocity;
        impulseDecayTime = decayTime;

        hasImpulse = true;
    }

    /// <summary>
    /// Sets new decayTime for ImpulseVelocity on the Rigidbody.
    /// </summary>
    /// <param name="decayTime">decayTime for ImpulseVelocity. Must not be 0.</param>
    [Obsolete("Reset decayTime for ImpulseVelocity using SetImpulseVelocity(). For effects like short jump, ImpulseVelocity for the Rigidbody is publically exposed and can be passed to SetImpulseVelocity()")]
    public void SetImpulseDecayTime(float decayTime)
    {
        currentMaxImpulseVelocity = ImpulseVelocity;

        if (Mathf.Approximately(decayTime, 0f))
        {
            Debug.LogError($"Trying to end decay manually by setting decayTime to 0 is not allowed \n { Environment.StackTrace }");
        }

        impulseDecayTime = decayTime;
    }


    // ===================
    // OVERRIDE VELOCITY
    // ===================

    /// <summary>
    /// Sets OverrideVelocity based on position destination and velocity
    /// </summary>
    /// <param name="position">Destination of overriden motion</param>
    /// <param name="velocity">Velocity of overriden motion. Must not be 0.</param>
    public void SetOverrideVelocity(Vector3 position, float velocity)
    {
        if (Mathf.Approximately(0.0f, velocity))
        {
            Debug.LogError($"Override velocity must not be ~0.0f \n { Environment.StackTrace }");
        }

        overrideDestination = position;

        OverrideVelocity = Vector3.Normalize(overrideDestination - rbd.position) * velocity;
        hasOverride = true;
    }

    public void SetOverride(bool set)
    {
        hasOverride = set;
    }
    #endregion
}