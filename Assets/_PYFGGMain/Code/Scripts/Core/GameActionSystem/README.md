# Game Action System (GAS)

A modular and extensible **Game Action System** for Unity.  
This system allows users to create actions, triggers, and configurations in a structured way. It is designed for flexibility while keeping runtime management clean.

---

## Folder Structure

```
GameActionSystem/
├── BaseClass/
│ ├── ActionBase.cs
│ └── ActionTrigger.cs
├── Config/
│ ├── ActionConfig.cs
│ └── ActionSetConfig.cs
├── Interfaces/
│ ├── IAction.cs
│ ├── IActionAgent.cs
│ ├── IActionData.cs
│ └── IActionSetConfig.cs
├── Monobehaviors/
│ ├── ActionAgentType.cs
│ ├── ActionRunner.cs
│ ├── ActionTriggerRuntime.cs
│ └── AgentTypeActionSetFactory.cs
├── StructsData/
│ ├── ActionDefinition.cs
│ └── ActionRequest.cs
├── Builders/
│ ├── ActionRegistrationBuilder.cs
│ └── ActionSetBuilder.cs
├── TriggerBuffer.cs
├── GameActionSystem.asmdef # Assembly Definition for the system.
└── README.md # This file
```

---

## Overview by Folder

### **BaseClass**
- **ActionBase.cs** – Base class for all actions. Users must inherit with `<ActionData, Context, ActionConfig>`. Contains helpers.  
- **ActionTrigger.cs** – Base trigger class. Users inherit and implement `FireTrigger()`.

### **Config**
- **ActionConfig.cs** – Base ScriptableObject for action configuration. Users create derived configs with `CanStart` logic for global conditions.  
- **ActionSetConfig.cs** – Holds the set of actions for an agent. Created by the `ActionSetFactory`.

### **Interfaces**
- **IAction.cs** – Interface for actions.  
- **IActionAgent.cs** – Interface for agent types.  
- **IActionData.cs** – Interface for ActionData classes.  
- **IActionSetConfig.cs** – Interface for action set configs.

### **Monobehaviors**
- **ActionAgentType.cs** – Agent MonoBehaviour. Users inherit to create custom agents.  
- **ActionRunner.cs** – Handles runtime management of triggers and actions.  
- **ActionTriggerRuntime.cs** – Manages trigger execution at runtime.  
- **AgentTypeActionSetFactory.cs** – Factory for creating action sets. Users inherit and implement `Create()`.

### **StructsData**
- **ActionDefinition.cs** – Holds action data for runtime. Sealed class, do not modify.  
- **ActionRequest.cs** – Constructed by triggers, forwarded to buffer.

### **Builders**
- **ActionRegistrationBuilder.cs** – Fluent API for registering actions. Sealed/internal.  
- **ActionSetBuilder.cs** – Fluent API to build action sets. Used in factories.

### **Other**
- **TriggerBuffer.cs** – Buffer for triggers. Managed via `ActionRunner` inspector fields.

---
## ActionConfig explanation
![Inherited ActionConfig parameters](https://github.com/user-attachments/assets/311a7f26-4dd5-4d86-86e0-42fd99eeb3a0)  
User created actionconfigs will inherit these paramter from base `ActionConfig`.
- Action Name: Name of action. Used for debugging.
- Action Mode:
  - Discrete: `ActionRunner` cannot update this type of action.
  - Continuous: `ActionRunner` can update this type of action. If action requires differing data from Triggers, trigger may be actuated once again to buffer update for the action (if the said action is already running, action will be updated instead of starting new).
- ActionInterruptionPolicy:
  - Never: Will never override running action until the running action is completed fully.
  - If Allowed: This action will query running action if it can interrupt it or not.
  - Force: No checks are made, running action is interrupted for this one.
- ActionPhases: These are to be used with tangent to animations but can be manually managed in code inside respective action class.
  - Action Phase ID: For debugging purposes
  - Duration: Duration of the phase (set here only if animation requires interruption logic, otherwise, phases can be managed manually in code and this can be ignored)
  - Interrupt Policy: This decides whether any action can interrupt this action during this phase
    - None: No action can interrupt this action during this phase.
    - Any: Any action can interrupt this action during this pahse.
    - Higher Priority: Action with higher priority will win during this phase.
    - Explicit Only: Only explicitly mentioned actions can interrupt during this phase. (THIS IS NOT IMPLEMENTED YET. User may create their own logic)
---

## Quick Example: Setting Up JumpAction for Player
`JumpAction`
```csharp
using PYFGG.GameActionSystem;
using UnityEngine;

public class JumpAction : ActionBase<JumpAction.Data, PlayerActionAgent.Context, JumpActionConfig>
{
    public JumpAction(JumpActionConfig config, PlayerActionAgent.Context context, Data data) : base(config, context, data)
    {
    }

    protected override void OnKill()
    {
        context.motorController.DisableGravity();
        context.motorController.EnableGravity();
        context.motorController.SetBaseVelocityY(0f);
    }

    protected override void OnUpdate()
    {

    }

    protected override bool OnRun()
    {
        if (context.motorController.CurrentVelocity.y > 0)
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
```

`JumpActionConfig`
```csharp
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

        return ctx.motorController.IsGrounded || Time.fixedTime - ctx.motorController.LastGroundedTime <= coyoteTime;
    }
}       
```

`JumpActionTrigger`
```csharp
using PYFGG.GameActionSystem;
using UnityEngine.UI;

public class JumpActionTrigger : ActionTriggerBase
{
    protected override void Activate()
    {
        PrepareData();

        InputManager.Instance.JumpInputEvent += OnJump;
    }

    protected override void Deactivate()
    {
        InputManager.Instance.JumpInputEvent -= OnJump;
    }

    private void OnJump(float f)
    {
        if(f > 0.5f)
        {
            FireTrigger(data);
            data.buttonRelease = false;
            return;
        }

        data.buttonRelease = true;
    }
    

    private JumpAction.Data data;
    private void PrepareData()
    {
        data = new(default);
    }
}
```

`PlayerActionSet`
```csharp
using UnityEngine;
using PYFGG.GameActionSystem

public class PlayerActionSet : AgentTypeActionSetFactory
{
    [SerializeField] private JumpActionConfig jumpConfig;

    public override IActionSetConfig Create()
    {
        ActionSetBuilder builder = new("PlayerActionSet");

        builder.AddAction<JumpAction>(jumpConfig).AddTrigger<JumpActionTrigger>();

        return builder.Build();
    }
}
```

`PlayerActionAgent`
```csharp
using PYFGG.GameActionSystem;
using UnityEngine;

public sealed class PlayerActionAgent : ActionAgentType
{
    [SerializeField] private MotorController motorController;
    [SerializeField] private MovementController movementController;

    public override ActionContext CreateContext()
    {
        return new Context(animator, this.motorController, this.movementController);
    }

    public sealed class Context : ActionContext
    {
        public Context(Animator animator, MotorController motorController, MovementController movementController) : base(animator)
        {
            this.motorController = motorController;
            this.movementController = movementController;
        }
        
        public MotorController motorController;
        public MovementController movementController;
    }
}
```
Make sure to include "using PYFGG.GameActionSystem"

---

## Usage in Scene:
1. Add `ActionAgentType` to your player GameObject.
2. Add `ActionRunner` to any GameObject to manage actions.
3. Add your custom `PlayerActionSet` Factory to create the action set.
4. Add `ActionTriggerRuntime` if you want automatic trigger handling.

## Tips for users
User are to create their own Action, Trigger and Config for a given action and AgentType and AgentTypeActionSetFactories for given agent.
- Actions: Always inherit from ActionBase<ActionData, Context, ActionConfig>.
- Triggers: Inherit from `ActionTrigger` and implement `FireTrigger()` logic.
- Configs: Inherit from `ActionConfig` and optionally override `CanStart()` for global conditions.
- Factories: Use `AgentTypeActionSetFactory` and `ActionSetBuilde` to register actions and triggers.
