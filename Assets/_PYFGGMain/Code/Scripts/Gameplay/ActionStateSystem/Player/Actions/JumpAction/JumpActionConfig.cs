using PYFGG.GameActionSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Jump", menuName = "Gameplay/Actions/Player/Jump")]
public class JumpActionConfig : ActionConfig
{
    [Header("Jump Data")]

    [Min(0f)]
    public float maxJumpHeight;

    [Min(0f)]
    public float minJumpHeight;

    [Min(0f)]
    public float minUninterruptedHeight;

    [Min(0f)]
    public float walkJumpBoost;

    [Min(0f)]
    public float sprintJumpBoost;

    [Range(0.01f, 1f)]
    public float earlyReleaseFactor;

    [Min(0f)]
    public float coyoteTime;

    public override bool CanStart(ActionContext context)
    {
        if (context is not PlayerActionAgent.Context ctx)
        {
            Debug.LogError($"Incorrect ActioContext passed to the action");
            return false;
        }

        return ctx.motorController.IsGrounded || ctx.motorController.AirTime <= coyoteTime;
    }
}       
