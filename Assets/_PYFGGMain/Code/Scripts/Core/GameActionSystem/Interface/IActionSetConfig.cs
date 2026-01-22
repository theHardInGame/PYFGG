using System.Collections.Generic;
using System;

namespace PYFGG.GameActionSystem
{
    /// <summary>
    /// Represents a read-only configuration of an action set,
    /// providing access to actions and their associated triggers.
    /// </summary>
    public interface IActionSetConfig
    {
        /// <summary>
        /// Attempts to retrieve the <see cref="ActionDefinition"/> associated with a given trigger.
        /// </summary>
        /// <param name="trigger">
        /// The trigger instance for which the associated action definition is requested.
        /// </param>
        /// <param name="definition">
        /// When this method returns, contains the <see cref="ActionDefinition"/> associated
        /// with the trigger, if found; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a definition was found for the trigger; otherwise, <c>false</c>.
        /// </returns>
        bool TryResolveTrigger(ActionTriggerBase trigger, out ActionDefinition definition);


        /// <summary>
        /// Returns all trigger types defined in this action set.
        /// </summary>
        /// <returns>
        /// A read-only collection of <see cref="Type"/> objects representing all trigger types.
        /// </returns>
        IReadOnlyCollection<Type> TriggerTypes();


        /// <summary>
        /// Returns name of this <see cref="ActionSetConfig">.
        /// </summary>
        /// <returns>String name.</returns>
        string GetName();
    }
}

