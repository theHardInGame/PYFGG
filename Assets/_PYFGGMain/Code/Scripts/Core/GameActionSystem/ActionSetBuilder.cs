using System.Collections.Generic;
using System;
using UnityEngine;

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Builds a set of actions and their associated triggers using a fluent API.
    /// </summary>
    public class ActionSetBuilder
    {
        private readonly string name;
        private readonly Dictionary<Type, ActionDefinition> triggerMap = new();


        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSetBuilder"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the action set being built.
        /// </param>
        public ActionSetBuilder(string name)
        {
            this.name = name;
        }


        /// <summary>
        /// Adds a new action to the action set and returns a builder
        /// for registering its associated triggers.
        /// </summary>
        /// <typeparam name="TAction">
        /// The type of action to register.
        /// </typeparam>
        /// <param name="config">
        /// The configuration data for the action.
        /// </param>
        /// <param name="canStart">
        /// An optional predicate that determines whether the action
        /// can start given the current action context.
        /// </param>
        /// <returns>
        /// An <see cref="ActionRegistrationBuilder{TAction}"/> used to
        /// associate triggers with the action.
        /// </returns>
        public ActionRegistrationBuilder<TAction> AddAction<TAction>(ActionConfig config) where TAction : IAction
        {
            ActionDefinition def = new(typeof(TAction), config);
            
            return new ActionRegistrationBuilder<TAction>(this, def);
        }


        /// <summary>
        /// Associates a trigger type with an action definition.
        /// </summary>
        /// <param name="triggerType">
        /// The trigger type that will invoke the action.
        /// </param>
        /// <param name="def">
        /// The action definition to be executed when the trigger occurs.
        /// </param>
        internal void RegisterTrigger(Type triggerType, ActionDefinition def)
        {
            if (triggerMap.ContainsKey(triggerType))
            {
                Debug.LogWarning($"Trigger {triggerType} already bound, overriding.");
            }

            triggerMap[triggerType] = def;
        }


        /// <summary>
        /// Builds the final immutable action set configuration.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionSetConfig"/> containing all registered actions
        /// and their associated triggers.
        /// </returns>
        public IActionSetConfig Build()
        {
            return new ActionSetConfig(new Dictionary<Type, ActionDefinition>(triggerMap), name);
        }
    }
}
