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
            public override string ToString() =>
                $"{_transitions.Count}";

            /// <summary>
            ///     Try to find an available transition.
            /// </summary>
            /// <returns>
            ///     Target <seealso cref="IState" /> to which a transition is available
            ///     or <see langword="null" /> if no transition is currently available.
            /// </returns>
            [CanBeNull]
            public IState FindTransition() =>
                FindTransition( t => t.HasCondition ) ?? FindTransition( t => !t.HasCondition );

            /// <summary>
            ///     Add a new <paramref name="transition" /> to the collection.
            /// </summary>
            /// <param name="transition">
            ///     Transition to be added to the collection.
            /// </param>
            internal void Add( [NotNull] ITransition transition ) =>
                _transitions.Add( transition );

            /// <summary>
            ///     Find an available transition matching the supplied <paramref name="condition" />.
            /// </summary>
            /// <param name="condition">
            ///     Condition that has to be met by the transition to be found.
            /// </param>
            /// <returns>
            ///     Target state returned by a matching transition or <see langword="null" />
            ///     if there was no available transition matching the supplied <paramref name="condition" />.
            /// </returns>
            [CanBeNull]
            private IState FindTransition( [NotNull] Func<ITransition, bool> condition )
            {
                foreach ( ITransition transition in _transitions )
                {
                    if ( !condition( transition ) ) continue;

                    IState targetState = transition.GetTargetState();
                    if ( targetState != null ) return targetState;
                }

                return null;
            }
        }
    }
}