using PYFGG.GameActionSystem;
using UnityEngine;

public class JumpAction : ActionBase<JumpAction.Data, PlayerActionAgent.Context, JumpActionConfig>
{
    public JumpAction(JumpActionConfig config, PlayerActionAgent.Context context, Data data) : base(config, context, data)
    {
    }

    protected override void OnKill()
    {
        context.motorController.SetBaseVelocityY(0f);
        context.motorController.DisableGravity();
        context.motorController.EnableGravity();
    }

    protected override void OnUpdate()
    {

    }

    protected override bool OnRun()
    {
        if (context.motorController.CurrentVelocity.y > 0f)
        {
            UpdateJumpedHeight();
            UpdateCurrentPhaseIndex();
            EarlyRealeaseShortJump();
            return true;
        }

        return false;
    }

    protected override void OnStart()
    {
        startLocY = context.motorController.Position.y;

        CalculateJump(GetJumpType());
        CreateDynamicPhases();
        StartJump();
    }

    private float startLocY;
    private float verticalVel;
    private Vector3 impulseVel;
    private float impulseTime;

    private float jumpedHeight;

    private enum JumpType
    {
        Neutral,
        Walk,
        Sprint
    }

    private JumpType GetJumpType()
    {
        if (!context.movementController.IsWalking) return JumpType.Neutral;

        return context.movementController.IsSprinting ? JumpType.Sprint : JumpType.Walk;
    }

    private void CalculateJump(JumpType type)
    {
        float g = context.motorController.GravityStrength;

        float height = config.maxJumpHeight;
        float horizontalBoost;

        switch (type)
        {
            case JumpType.Sprint:
                horizontalBoost = config.sprintJumpBoost;
                break;

            case JumpType.Walk:
                horizontalBoost = config.walkJumpBoost;
                break;

            default:
                horizontalBoost = 0f;
                break;
        }

        verticalVel = Mathf.Sqrt(2f * g * height);

        impulseTime = verticalVel * 2 / g;

        if (horizontalBoost > 0f)
        {
            Vector3 hVel = context.motorController.CurrentVelocity;
            impulseVel = new Vector3(hVel.x, 0f, hVel.z).normalized * horizontalBoost;
        }
    }

    private void CreateDynamicPhases()
    {
        config.actionPhases = new ActionPhase[2];

        // Initialize each phase
        for (int i = 0; i < config.actionPhases.Length; i++)
        {
            config.actionPhases[i] = new ActionPhase();
        }

        config.actionPhases[0].interruptPolicy = PhaseInterruptibility.None;
        config.actionPhases[1].interruptPolicy = PhaseInterruptibility.Any;
    }

    private void StartJump()
    {
        context.motorController.DisableGravity();
        context.motorController.EnableGravity();
        
        context.motorController.SetBaseVelocityY(verticalVel);

        if (impulseVel.sqrMagnitude > 0.01f)
        {
            context.motorController.SetImpulseVelocity(impulseVel, impulseTime);
        }
    }

    private void UpdateJumpedHeight()
    {
        jumpedHeight = context.motorController.Position.y - startLocY;
    }

    private void UpdateCurrentPhaseIndex()
    {
        if (jumpedHeight > config.minUninterruptedHeight) currentPhaseIndex = 1;
        else currentPhaseIndex = 0;
    }

    private void EarlyRealeaseShortJump()
    {
        if (data.buttonRelease && jumpedHeight > config.minJumpHeight)
        {
            context.motorController.SetBaseVelocityY(verticalVel * config.earlyReleaseFactor);
        }
    }

    public class Data : IActionData
    {
        public bool buttonRelease;

        public Data(bool buttonRelease)
        {
            this.buttonRelease = buttonRelease;
        }
    }
}
