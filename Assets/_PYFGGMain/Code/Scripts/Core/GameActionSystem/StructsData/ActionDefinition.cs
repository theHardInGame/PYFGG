using System;

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Describes an action type and its associated configuration data.
    /// </summary>
    public sealed class ActionDefinition
    {
        /// <summary>
        /// Gets the concrete <see cref="Type"/> of the action to be created.
        /// </summary>
        internal Type ActionType { get; }


        /// <summary>
        /// Gets the configuration data used when creating the action instance.
        /// </summary>
        internal ActionConfig config { get; }

        internal Func<ActionContext, bool> CanStart { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDefinition"/> class.
        /// </summary>
        /// <param name="actionType">
        /// The concrete action type. Must implement <see cref="IAction"/>.
        /// </param>
        /// <param name="config">
        /// Optional configuration data passed to the action constructor.
        /// </param>
        internal ActionDefinition(Type actionType, ActionConfig config = null)
        {
            this.ActionType = actionType;
            this.config = config;
            this.CanStart = this.config.CanStart;
        }


        /// <summary>
        /// Creates an <see cref="IAction"/> instance using this definition.
        /// </summary>
        /// <param name="context">
        /// The execution context provided to the action instance.
        /// </param>
        /// <returns>
        /// A new <see cref="IAction"/> instance.
        /// </returns>
        internal IAction Create(ActionContext context, IActionData data)
        {
            return (IAction)Activator.CreateInstance(ActionType, config, context, data);
        }
    }
}
