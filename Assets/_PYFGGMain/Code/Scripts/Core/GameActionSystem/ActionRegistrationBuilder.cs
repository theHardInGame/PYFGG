

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Provides a fluent API for configuring and registering an action
    /// within an <see cref="ActionSetBuilder"/>.
    /// </summary>
    /// <typeparam name="TAction">
    /// The action type being registered. Must implement <see cref="IAction"/>.
    /// </typeparam>
    public class ActionRegistrationBuilder<TAction> where TAction : IAction
    {
        private ActionSetBuilder parent;
        private ActionDefinition def;

        /// <summary>
        /// Builds and registers an action within an <see cref="ActionSetBuilder"/>.
        /// </summary>
        /// <param name="parent">
        /// The parent <see cref="ActionSetBuilder"/> that owns this action registration.
        /// </param>
        /// <param name="def">
        /// The definition describing the action to be registered.
        /// </param>
        internal ActionRegistrationBuilder(ActionSetBuilder parent, ActionDefinition def)
        {
            this.parent = parent;
            this.def = def;
        }

        /// <summary>
        /// Registers a trigger type that will invoke this action.
        /// </summary>
        /// <typeparam name="TTrigger">
        /// he trigger type associated with this action. Must derive from
        /// <see cref="ActionTriggerBase"/>.
        /// </typeparam>
        /// <returns>
        /// The current <see cref="ActionRegistrationBuilder{TAction}"/> instance
        /// to allow method chaining.
        /// </returns>
        public ActionRegistrationBuilder<TAction> AddTrigger<TTrigger>() where TTrigger : ActionTriggerBase
        {
            parent.RegisterTrigger(typeof(TTrigger), def);
            return this;
        }
    }
}
