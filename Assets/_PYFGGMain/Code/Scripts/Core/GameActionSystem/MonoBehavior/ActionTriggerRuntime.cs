using System;
using System.Collections.Generic;
using UnityEngine;

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Runtime manager responsible for creating, initializing,
    /// and disposing action triggers based on the active action set.
    /// </summary>
    public class ActionTriggerRuntime : MonoBehaviour
    {
        [SerializeField] private ActionRunner actionRunner;


        private TriggerBuffer triggerBuffer;
        private IActionSetConfig actionSetConfig;
        private float bufferLife;


        /// <summary>
        /// Active trigger instances managed at runtime.
        /// </summary>
        private List<ActionTriggerBase> triggers = new();

        /// <summary>
        /// Creates and initializes all triggers defined by the current action set configuration.
        /// </summary>
        private void InitializeTriggers()
        {
            foreach(Type type in actionSetConfig.TriggerTypes())
            {
                CreateTrigger(type);
            }
        }

        /// <summary>
        /// Creates and initializes a trigger of the given type if it does not already exist.
        /// </summary>
        /// <param name="type">
        /// Concrete <see cref="ActionTriggerBase"/> type to instantiate.
        /// </param>
        private void CreateTrigger(Type type)
        {
            if (triggers.Exists(t => t.GetType() == type)) return;

            ActionTriggerBase trigger = (ActionTriggerBase)Activator.CreateInstance(type);
            trigger.Initialize(triggerBuffer, actionSetConfig, bufferLife);

            triggers.Add(trigger);
        }

        /// <summary>
        /// Disposes and removes a trigger of the given type if it exists.
        /// </summary>
        /// <param name="type">
        /// Concrete <see cref="ActionTriggerBase"/> type to dispose.
        /// </param>
        public void DisposeTrigger(Type type)
        {
            var trigger = triggers.Find(t => t.GetType() == type);
            if (trigger == null) return;

            trigger.Dispose();
            triggers.Remove(trigger);
        }

        #region Untiy API
        // =================
        // =   UNITY API   =
        // =================

        private void Awake()
        {
            triggerBuffer = actionRunner.Buffer;
            actionSetConfig = actionRunner.ActionSetConfig;
            bufferLife = actionRunner.bufferLife;
            InitializeTriggers();
        }
        #endregion
    }
}