using System;
using UnityEngine;


[RequireComponent(typeof(RigidbodyHandler))]
public class Gravity : MonoBehaviour
{
    [SerializeField] private Collider groundCollider;
    private ColliderData groundColliderData;
    public bool IsGrounded { get; private set; }
    public bool UseGravity { get; private set; } = true;
    public float Strength => gravityStrength;

    [SerializeField] private float gravityStrength = 50.0f;
    [SerializeField] private float terminalVelocity = 40.0f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask gorundCollisionLayer = ~0;

    private float verticalVelocity;

    private RigidbodyHandler rbHandler;

    #region Unity API

    private void Awake()
    {
        rbHandler = GetComponent<RigidbodyHandler>();
        CacheColliderData();
    }

    private void FixedUpdate()
    {
        if (UseGravity) ApplyGravity();
        GroundCheck();
    }

    #endregion

    private void CacheColliderData()
    {
        if (groundCollider == null) Debug.LogError($"Gravity component does not have GroundCollider assigned in Inspector Windows! \n { Environment.StackTrace }");

        groundColliderData = new ColliderData(groundCollider);
    }


    private void GroundCheck()
    {
        IsGrounded = false;

        if (rbHandler.CurrentRBDVelocity.y > 0.01f) return;

        Vector3 direction = Vector3.down * groundCheckDistance;
        Vector3 origin = transform.position;

        if (groundColliderData.CollisionDetection(direction, origin, transform, out RaycastHit hit, gorundCollisionLayer))
        {
            IsGrounded = true;
            HitGround?.Invoke();
        }
    }


    private void ApplyGravity()
    {
        if (!IsGrounded)
        {
            if (rbHandler.CurrentCompVelocity.y <= -terminalVelocity) return;

            verticalVelocity -= gravityStrength * Time.fixedDeltaTime;
        }
        else
        {
            if (Mathf.Abs(rbHandler.CurrentRBDVelocity.y) < 0.01f)
            {
                verticalVelocity = 0f;
            }
        }

        rbHandler.SetGravityVelocity(Vector3.up * verticalVelocity);
    }

    public void StopGravity()
    {
        UseGravity = false;
        verticalVelocity = 0;
        rbHandler.SetGravityVelocity(Vector3.zero);
    }
    public void StartGravity()
    {
        UseGravity = true;
    }

    public event Action HitGround;
}
