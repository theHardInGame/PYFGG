using System.Collections.Generic;
using System;

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Represents a finalized set of actions and their associated triggers.
    /// </summary>
    public class ActionSetConfig : IActionSetConfig
    {
        /// <summary>
        /// The name of the action set, primarily for debugging or manual identification.
        /// </summary>
        public string setName;


        /// <summary>
        /// Maps trigger types to their corresponding action definitions.
        /// </summary>
        private Dictionary<Type, ActionDefinition> triggerMap;


        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSetConfig"/> class.
        /// </summary>
        /// <param name="triggerMap">
        /// A dictionary mapping trigger types to action definitions.
        /// </param>
        /// <param name="name">
        /// The name of the action set, for debugging or manual identification.
        /// </param>
        public ActionSetConfig(Dictionary<Type, ActionDefinition> triggerMap, string name)
        {
            this.triggerMap = triggerMap;
            this.setName = name;
        } 


        // ==========================
        // INTERFACE IMPLEMENTATION
        // ==========================

        /// <inheritdoc/>
        public bool TryResolveTrigger(ActionTriggerBase trigger, out ActionDefinition definition)
        {
            if (triggerMap.TryGetValue(trigger.GetType(), out definition))
            {
                return true;
            }

            definition = default;
            return false;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<Type> TriggerTypes()
        {
            return triggerMap.Keys;
        }

        public string GetName()
        {
            return setName;
        }
    }
}
