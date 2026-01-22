using UnityEngine;
using System;

public struct ColliderData
{
    #region Properties

    // ==================
    // =   PROPERTIES   =
    // ==================

    public enum ColliderType { Box, Sphere, Capsule }
    public ColliderType Type;
    public Vector3 Center;
    private float radius;
    public float Radius 
    { 
        get
        {
            if (Type == ColliderType.Box) throw new InvalidOperationException($"Box collider can't have a radius \n { Environment.StackTrace }");
            return radius;
        }

        set 
        {
            if (Type == ColliderType.Box) throw new InvalidOperationException($"Box collider can't have a radius \n { Environment.StackTrace }");
            radius = value;
        }
    }
    private float height;
    public float Height
    { 
        get
        {
            if (Type == ColliderType.Box) throw new InvalidOperationException($"Box/Sphere collider can't have a height \n { Environment.StackTrace }");
            return height;
        }

        set
        {
            if (Type == ColliderType.Box) throw new InvalidOperationException($"Box/Sphere collider can't have a height \n { Environment.StackTrace }");
            height = value;
        }
    }
    private Vector3 size;
    public Vector3 Size
    { 
        get
        {
            if (Type != ColliderType.Box) throw new InvalidOperationException($"Sphere/Capsule collider can't have a size \n { Environment.StackTrace }");
            return size;
        }

        set{
            if (Type != ColliderType.Box) throw new InvalidOperationException($"Sphere/Capsule collider can't have a size \n { Environment.StackTrace }");
            size = value;
        }
    }
    #endregion

    #region Contructors

    // ===================
    // =   CONTRUCTORS   =
    // ===================

    public ColliderData(BoxCollider collider)
    {
        Vector3 lossy = collider.transform.lossyScale;

        Type = ColliderType.Box;
        Center = collider.center;
        size = Vector3.Scale(collider.size, lossy);

        radius = default;
        height = default;
    }

    public ColliderData(SphereCollider collider)
    {
        Vector3 lossy = collider.transform.lossyScale;

        Type = ColliderType.Sphere;
        Center = collider.center;
        radius = collider.radius * Mathf.Max(lossy.x, lossy.y, lossy.z);

        size = default;
        height = default;
    }

    public ColliderData(CapsuleCollider collider)
    {
        Vector3 lossy = collider.transform.lossyScale;

        Type = ColliderType.Capsule;
        Center = collider.center;
        radius = collider.radius * Mathf.Max(lossy.x, lossy.z);
        height = collider.height * lossy.y;

        size = default;
    }

    public ColliderData(Collider col)
    {
        switch (col)
        {
            case BoxCollider b:
                this = new ColliderData(b);
                break;

            case SphereCollider s:
                this = new ColliderData(s);
                break;

            case CapsuleCollider c:
                this = new ColliderData(c);
                break;

            case MeshCollider m:
                Debug.LogWarning($"Storing on MeshCollider in ColliderData is not allowed! (Use Unity's 3D primitive colliders) \n { Environment.StackTrace }");
                Type = default;
                Center = default;
                radius = default;
                height = default;
                size = default;
                break;

            default:
                Debug.LogError($"Trying to construct ColliderData with unexpected Collider (Use Unity's 3D primitive collider) \n { Environment.StackTrace }");
                Type = default;
                Center = default;
                radius = default;
                height = default;
                size = default;
                break;

        }
    }
    #endregion

    #region Methods

    // ===============
    // =   METHODS   =
    // ===============

    /// <summary>
    /// Collision detection.
    /// Only works on static scale primitive colliders.
    /// </summary>
    /// <param name="nextStepPos">Position in next frame/Position requested to be reached.</param>
    /// <param name="origin">Current position.</param>
    /// <param name="colliderTransform">Collider's transform in global space.</param>
    /// <param name="hit">Raycast hit</param>
    /// <returns>Returns true if collision is detected.</returns>
    public bool CollisionDetection(Vector3 nextStepPos, Vector3 origin, Transform colliderTransform, out RaycastHit hit, LayerMask collisionLayer)
    {
        hit = default;

        if (nextStepPos.sqrMagnitude < 0.000001f) return false;

        float detectionBuffer = nextStepPos.magnitude + 0.001f;


        switch (Type)
        {
            case ColliderType.Capsule:
                Vector3 top = origin + Center + colliderTransform.up * (Height/2 - Radius);
                Vector3 bottom = origin + Center - colliderTransform.up * (Height/2 - Radius);

                return Physics.CapsuleCast(bottom, top, Radius, nextStepPos.normalized, out hit, detectionBuffer, collisionLayer, QueryTriggerInteraction.Ignore);

            case ColliderType.Sphere:
                return Physics.SphereCast(origin + Center, Radius, nextStepPos.normalized, out hit, detectionBuffer, collisionLayer, QueryTriggerInteraction.Ignore);


            case ColliderType.Box:
                return Physics.BoxCast(origin + Center, Size/2, nextStepPos.normalized, out hit, colliderTransform.rotation, detectionBuffer, collisionLayer, QueryTriggerInteraction.Ignore);

            default:
                return false;
        }
    }
    #endregion
}
