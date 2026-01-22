using UnityEngine;
using System.Linq;
using System;

namespace PYFGG.GameActionSystem
{
    #region Action Config Helpers
    
    // =============================
    // =   ACTION CONFIG HELPERS   =
    // =============================

    /// <summary>
    /// Mode of action.
    /// </summary>
    public enum ActionMode { Discrete, Continuous }

    /// <summary>
    /// Policies for how an action phase can be interrupted.
    /// </summary>
    public enum PhaseInterruptibility { None, Any, HigherPriority, ExplicitOnly }


    /// <summary>
    /// Policies for how an action can interrupt others
    /// <para>
    /// <list type="bullet">
    /// <item><see cref="Never"/>: Never interrupt any running action.</item>
    /// <item><see cref="IfAllowed"/>: If runningAction phase condition allow.</item>
    /// <item><see cref="Force"/>: Bypass any conditions and interrupt.</item>
    /// </list>
    /// </para>
    /// </summary>
    public enum ActionInterruptionAuthority 
    {
        /// <summary>
        /// Never interrupt any running action.
        /// </summary>
        Never,

        /// <summary>
        /// If runningAction phase condition allow.
        /// </summary>
        IfAllowed,

        /// <summary>
        /// Bypass any conditions and interrupt.
        /// </summary>
        Force 
    }

    /// <summary>
    /// Identifiers for the different phases of an action.
    /// </summary>
    public enum ActionPhaseID { StartUp, LockedIn, Cancellable, Recovery }


    /// <summary>
    /// Represents a single phase of an action, including its duration and interruption rules.
    /// </summary>
    [Serializable]
    public class ActionPhase
    {
        /// <summary>
        /// The phase identifier.
        /// </summary>
        [Tooltip("PhaseID")]
        public ActionPhaseID actionPhaseID;

        /// <summary>
        /// Phase Duration in Seconds
        /// </summary>
        [Tooltip("Phase Duration in Seconds"), Min(0f)]
        public float duration;

        /// <summary>
        /// Duration of this phase in seconds.
        /// </summary>
        [Tooltip("Intteruption priority of the phase")]
        public PhaseInterruptibility interruptPolicy;
    }
    #endregion


    #region ActionConfigBase

    // ==========================
    // =   ACTION CONFIG BASE   =
    // ==========================

    /// <summary>
    /// Base configuration for an action, including timing, priority, and phases.
    /// </summary>
    public class ActionConfig : ScriptableObject
    {
        /// <summary>
        /// Human-readable name of the action (for debugging or manual identification).
        /// </summary>
        public string actionName;


        /// <summary>
        /// Mode of this action.
        /// </summary>
        public ActionMode actionMode;


        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Interruption policy for the action \nNever: Never interrupt any running action. \nIfAllowed: If runningAction phase condition allow. \nForce: Bypass any conditions and interrupt.")]
        public ActionInterruptionAuthority actionInterruptionPolicy = ActionInterruptionAuthority.IfAllowed;

        /// <summary>
        /// Base priority of the action.
        /// Higher priority actions can interrupt lower priority actions under the defined rules.
        /// </summary>
        [Tooltip("Base priority of action (Higher priority can interrupt lower priority under conditions)")]
        public int priority;


        /// <summary>
        /// Phases of the action, used for animation and gameplay timing.
        /// </summary>
        [Header("Animation/Action Timing Data")]
        public ActionPhase[] actionPhases;


        /// <summary>
        /// Total duration of the action, calculated by summing the durations of all phases.
        /// </summary>
        public float TotalDuration => actionPhases.Sum(p => p.duration);

        public virtual bool CanStart(ActionContext context) => true;
    }
    #endregion
}
