

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Base class for action triggers.
    /// Handles lifecycle management and delegates activation logic to subclasses.
    /// </summary>
    public abstract class ActionTriggerBase
    {
        private TriggerBuffer buffer;
        private IActionSetConfig actionSetConfig;
        private float bufferLife;

        /// <summary>
        /// Initializes the trigger and activates it.
        /// Should be called when the trigger becomes relevant or enabled.
        /// </summary>
        internal void Initialize(TriggerBuffer buffer, IActionSetConfig actionSetConfig, float bufferLife)
        {
            this.buffer = buffer;
            this.actionSetConfig = actionSetConfig;
            this.bufferLife = bufferLife;
            Activate();
        }

        /// <summary>
        /// Disposes the trigger and deactivates it.
        /// Should be called when the trigger is no longer needed.
        /// </summary>
        internal void Dispose()
        {
            Deactivate();
        }

        /// <summary>
        /// Called during initialization to enable the trigger.
        /// Implementations should register listeners, callbacks, or conditions here.
        /// </summary>
        protected abstract void Activate();

        /// <summary>
        /// Called during disposal to disable the trigger.
        /// Implementations should unregister listeners and clean up resources here.
        /// </summary>
        protected abstract void Deactivate();

        /// <summary>
        /// Called when trigger recieves actuation to fire
        /// Must be called by inheritors based on actuation logic
        /// </summary>
        protected void FireTrigger<TActionData>(TActionData data) where TActionData : IActionData
        {
            if (buffer == null) return;
            actionSetConfig.TryResolveTrigger(this, out ActionDefinition definition);

            buffer.Register(new(definition, data, UnityEngine.Time.time + bufferLife));
        }
    }
}
