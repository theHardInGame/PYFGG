using System;
using UnityEngine;

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Provides shared runtime data required by actions.
    /// Acts as a container for external systems an action may interact with.
    /// </summary>
    public class ActionContext
    {
        /// <summary>
        /// Creates a new <see cref="ActionContext"/>.
        /// </summary>
        /// <param name="animator">
        /// Animator used by actions to control character or object animations.
        /// </param>
        public ActionContext(Animator animator)
        {
            this.animator = animator;
        }

        /// <summary>
        /// Animator instance associated with the action.
        /// Must be set externally.
        /// </summary>
        public Animator animator;
    }


    /// <summary>
    /// Base class for all actions.
    /// Provides shared configuration, context, and common runtime state.
    /// </summary>
    public abstract class ActionBase<TActionData, TContext, TConfig> : IAction 
    where TActionData : IActionData
    where TContext : ActionContext
    where TConfig : ActionConfig
    {
        /// <summary>
        /// Configuration data that defines how this action behaves.
        /// </summary>
        protected readonly TConfig config;

        /// <summary>
        /// Runtime context shared across actions (e.g. animator, controllers).
        /// </summary>
        protected readonly TContext context;

        /// <summary>
        /// Runtime action specific data
        /// </summary>
        protected TActionData data;

        /// <summary>
        /// Index of the currently active phase in the action.
        /// </summary>
        protected int currentPhaseIndex;

        /// <summary>
        /// Time elapsed since the current phase started.
        /// </summary>
        protected float phaseTime;

        protected float totalRunTime = 0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionBase"/> class.
        /// </summary>
        /// <param name="config">
        /// Configuration describing the action’s phases and behavior.
        /// </param>
        /// <param name="context">
        /// Runtime context providing access to external systems.
        /// </param>
        public ActionBase(TConfig config, TContext context, TActionData data)
        {
            this.config = config;
            this.context = context;
            this.data = data;
        }

        // ==========================
        // Interface Implementation
        // ==========================

        void IAction.Start()
        {
            OnStart();
        }

        void IAction.Update()
        {
            OnUpdate();
        }
        
        bool IAction.Run()
        {
            totalRunTime += Time.fixedDeltaTime;
            return OnRun();
        }

        bool IAction.CanInterrupt(ActionDefinition definition)
        {
            switch (config.actionPhases[currentPhaseIndex].interruptPolicy)
            {
                case PhaseInterruptibility.None:
                    return false;
                    
                case PhaseInterruptibility.Any:
                    return true;

                case PhaseInterruptibility.ExplicitOnly:
                    return true;

                case PhaseInterruptibility.HigherPriority:
                    if (this.config.priority < definition.config.priority) 
                        return true;
                    else 
                        return false;
                    
                default:
                    return true;
            }
        }

        void IAction.Kill()
        {
            OnKill();
        }


        protected abstract void OnStart();
        protected abstract void OnUpdate();
        protected abstract bool OnRun();
        protected abstract void OnKill();
    }
}
