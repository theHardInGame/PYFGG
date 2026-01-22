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