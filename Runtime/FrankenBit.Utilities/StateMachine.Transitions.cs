// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.Transitions.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Nested <see cref="Transitions" /> class of the <seealso cref="StateMachine" /> class.
    /// </summary>
    public sealed partial class StateMachine
    {
        /// <summary>
        ///     Collection of transitions.
        /// </summary>
        private sealed class Transitions
        {
            /// <summary>
            ///     Encapsulated list of transitions.
            /// </summary>
            private readonly List<ITransition> _transitions = new List<ITransition>();

            /// <inheritdoc />
            [NotNull]
            public override string ToString() =>
                $"{_transitions.Count}";

            /// <summary>
            ///     Add a new <paramref name="transition" /> to the collection.
            /// </summary>
            /// <param name="transition">
            ///     Transition to be added to the collection.
            /// </param>
            internal void Add( [NotNull] ITransition transition ) =>
                _transitions.Add( transition );

            /// <summary>
            ///     Check if there is at least one transition to the supplied <paramref name="state" />.
            /// </summary>
            /// <param name="state">
            ///     The state a transition should target.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if there is at least one transition that targets
            ///     the supplied <paramref name="state" />, <see langword="false" /> if not.
            /// </returns>
            internal bool ContainsTransitionTo( [NotNull] IState state )
            {
                foreach ( ITransition transition in _transitions )
                {
                    if ( transition.TargetState == state ) return true;
                }

                return false;
            }

            /// <summary>
            ///     Try to find an available transition from supplied <paramref name="state" />.
            /// </summary>
            /// <param name="state">
            ///     The current state of the machine.
            /// </param>
            /// <returns>
            ///     A currently available <seealso cref="ITransition" /> or
            ///     <see langword="null" /> if no transition is currently available.
            /// </returns>
            /// <remarks>
            ///     The supplied <paramref name="state" /> should already match the source state of all
            ///     transitions in the collection, and is used only to resolve to the actual machine state
            ///     in the case that the source state of the transition is the <see cref="DefaultState.Any" /> state.
            /// </remarks>
            [CanBeNull]
            internal ITransition FindAvailableTransition( IState state ) =>
                FindAvailableTransition( state, t => t.HasCondition )
                ?? FindAvailableTransition( state, t => !t.HasCondition );

            /// <summary>
            ///     Find an available transition from the supplied <paramref name="state"/>
            ///     matching the supplied <paramref name="condition" />.
            /// </summary>
            /// <param name="state">
            ///     The current state of the machine.
            /// </param>
            /// <param name="condition">
            ///     Condition that has to be met by the transition to be found.
            /// </param>
            /// <returns>
            ///     A currently available transition matching the supplied <paramref name="condition"/> or
            ///     <see langword="null" /> if there was no available transition matching the supplied <paramref name="condition" />.
            /// </returns>
            [CanBeNull]
            private ITransition FindAvailableTransition( IState state, [NotNull] Func<ITransition, bool> condition )
            {
                foreach ( ITransition transition in _transitions )
                {
                    if ( condition( transition ) && transition.IsAvailable( state ) ) return transition;
                }

                return null;
            }
        }
    }
}