using System;
using TMPro;
using UnityEngine;



/// <summary>
/// Facade component responsible for issuing movement-related commands to a
/// <see cref="RigidbodyHandler"/> from external systems (input, AI, abilities, etc.).
/// <para>
/// This class does <b>not</b> own or store any velocity state itself.
/// It simply forwards commands to lower-level movement systems.
/// </para>
/// <para>
/// Responsibilities:
/// <list type="bullet">
/// <item>Set and modify base velocity</item>
/// <item>Register and remove additive velocity sources</item>
/// <item>Apply impulse velocities with decay</item>
/// <item>Control gravity state and query grounding information</item>
/// </list>
/// </para>
/// </summary>
public class MotorController : MonoBehaviour
{
    [Tooltip("Optional identifier for debugging or tooling purposes.")]
    [SerializeField] private string controllerName;

    [Tooltip("Handles Rigidbody velocity composition and application.")]
    [SerializeField] private RigidbodyHandler rbHandler;

    [Tooltip("Handles gravity and grounding logic.")]
    [SerializeField] private Gravity gravity;

    [SerializeField] private TextMeshProUGUI debugText;

    /// <summary>
    /// Sets the X component of the base velocity while preserving
    /// the existing Y and Z components.
    /// </summary>
    /// <param name="x">New X-axis velocity.</param>
    public void SetBaseVelocityX(float x)
    {
        Vector3 bvel = rbHandler.BaseVelocity;
        bvel.x = x;
        SetBaseVelocity(bvel);
    }

    /// <summary>
    /// Sets the Y component of the base velocity while preserving
    /// the existing X and Z components.
    /// </summary>
    /// <param name="y">New Y-axis velocity.</param>
    public void SetBaseVelocityY(float y)
    {
        Vector3 bvel = rbHandler.BaseVelocity;
        bvel.y = y;
        SetBaseVelocity(bvel);
    }

    /// <summary>
    /// Sets the Z component of the base velocity while preserving
    /// the existing X and Y components.
    /// </summary>
    /// <param name="z">New Z-axis velocity.</param>
    public void SetBaseVelocityZ(float z)
    {
        Vector3 bvel = rbHandler.BaseVelocity;
        bvel.z = z;
        SetBaseVelocity(bvel);
    }

    /// <summary>
    /// Replaces the current base velocity.
    /// <para>
    /// Base velocity typically represents continuous movement such as
    /// player input, AI navigation, or locomotion systems.
    /// </para>
    /// </summary>
    /// <param name="bVel">The new base velocity vector.</param>
    public void SetBaseVelocity(Vector3 bVel)
    {
        rbHandler.SetBaseVelocity(bVel);
    }


    /// <summary>
    /// Registers an additive velocity source.
    /// <para>
    /// Additive velocities are combined with the base velocity
    /// and are typically used for effects such as moving platforms,
    /// wind, conveyors, or buffs.
    /// </para>
    /// </summary>
    /// <param name="source">The additive velocity source to register.</param>
    public void RegisterAdditiveVelocity(IAdditiveVelocitySource source)
    {
        rbHandler.RegisterAdditiveVelocity(source);
    }

    /// <summary>
    /// Removes a previously registered additive velocity source.
    /// </summary>
    /// <param name="source">The additive velocity source to remove.</param>
    public void RemoveAdditiveVelocity(IAdditiveVelocitySource source)
    {
        rbHandler.RemoveAdditiveVelocity(source);
    }

    /// <summary>
    /// Applies an impulse velocity that decays over time.
    /// <para>
    /// Impulse velocities are typically used for actions such as jumps,
    /// knockbacks, dashes, or explosions.
    /// </para>
    /// </summary>
    /// <param name="iVel">Initial impulse velocity.</param>
    /// <param name="decayTime">Time in seconds for the impulse to fully decay.</param>
    public void SetImpulseVelocity(Vector3 iVel, float decayTime)
    {
        rbHandler.SetImpulseVelocity(iVel, decayTime);
    }

    /// <summary>
    /// Gets the current velocity of Unity's Rigidbody component.
    /// </summary>
    public Vector3 CurrentRBDVelocity => rbHandler.CurrentRBDVelocity;

    /// <summary>
    /// Gets the final composed velocity currently applied to the Rigidbody.
    /// </summary>
    public Vector3 CurrentVelocity => rbHandler.CurrentCompVelocity;

    /// <summary>
    /// Gets the current BaseVelocity applied on the Rigidbody.
    /// </summary>
    public Vector3 BaseVelocity => rbHandler.BaseVelocity;

    /// <summary>
    /// Gets the current ImpulseVelocity applied on the Rigidbody.
    /// Will return the current actively acting one i.e. decay will be considered.
    /// </summary>
    public Vector3 ImpulseVelocity => rbHandler.ImpulseVelocity;

    /// <summary>
    /// Gets current GraivtyVelocity applied on the Rigidbody.
    /// </summary>
    public Vector3 GravityVelocity => rbHandler.GravityVelocity;

    /// <summary>
    /// Gets the addition of all AdditiveVelocities acting on the Rigidbody.
    /// </summary>
    public Vector3 AdditiveVelocities => rbHandler.AdditiveVelocity;

    /// <summary>
    /// Enables gravity simulation.
    /// </summary>
    public void EnableGravity() => gravity.StartGravity();

    /// <summary>
    /// Disables gravity simulation.
    /// </summary>
    public void DisableGravity() => gravity.StopGravity();

    /// <summary>
    /// Gets whether the object is currently grounded.
    /// </summary>
    public bool IsGrounded => gravity.IsGrounded;

    /// <summary>
    /// Time stamp since last grounded state.
    /// </summary>
    public float AirTime { get; private set; }

    /// <summary>
    /// Gets the current gravity strength.
    /// </summary>
    public float GravityStrength => gravity.Strength;

    /// <summary>
    /// Current position of the rigidbody.
    /// </summary>
    public Vector3 Position => rbHandler.CurrentPosition;

    private void FixedUpdate()
    {
        UpdateLastGroundedTime();

        debugText.text = $"RBD Velocity: { CurrentRBDVelocity }\nBase Velocity: { BaseVelocity }\nImpulse Velocity: { ImpulseVelocity }\nGravity Velocity: { GravityVelocity }\n{( IsGrounded ? "Grounded" : "Airborne") }";
    }

    private bool wasGrounded;
    private void UpdateLastGroundedTime()
    {
        if (!IsGrounded)
        {
            AirTime += Time.fixedDeltaTime;
        }
        if (IsGrounded && !wasGrounded)
        {
            AirTime = 0f;
        }

        wasGrounded = gravity.IsGrounded;
    }
}

