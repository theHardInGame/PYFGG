using PYFGG.GameActionSystem;
using UnityEngine;

public class DashAction : ActionBase<DashAction.Data, PlayerActionAgent.Context, DashActionConfig>
{
    public DashAction(DashActionConfig config, PlayerActionAgent.Context context, Data data) : base(config, context, data)
    {
    }

    protected override void OnKill()
    {
        context.movementController.ClearOverride();
        context.motorController.EnableGravity();
    }

    protected override bool OnRun()
    {
        dashDuration -= Time.fixedDeltaTime;

        if (dashDuration <= 0) return false;

        currentPhaseIndex = 0;
        return true;
    }

    protected override void OnStart()
    {
        CreateDynamicPhase();
        CalculateDash();
        StartDash();
    }

    protected override void OnUpdate()
    {
        // Discrete action
    }

    private float dashDuration;
    private Vector3 DashVector;

    private void CalculateDash()
    {
        dashDuration = 2 * config.dashDistance / config.dashSpeed;
        DashVector = context.movementController.LookDirection * config.dashSpeed;
    }

    private void StartDash()
    {
        context.movementController.SupressMovement();
        context.motorController.DisableGravity();
        context.motorController.SetImpulseVelocity(DashVector, dashDuration);
    }

    private void CreateDynamicPhase()
    {
        config.actionPhases = new ActionPhase[1];

        config.actionPhases[0] = new ActionPhase();
        config.actionPhases[0].interruptPolicy = PhaseInterruptibility.None;
    }

    public class Data : IActionData
    {
        
    }
}
