// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.Transitions.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        private sealed class Transitions : ITransitions
        {
            /// <summary>
            ///     Encapsulated list of transitions.
            /// </summary>
            private readonly List<ITransition> _transitions = new List<ITransition>();

            /// <inheritdoc />
            public override string ToString() =>
                $"[{_transitions.Count}]";

            /// <inheritdoc />
            public IState FindTransition( IState state )
            {
                foreach ( ITransition transition in _transitions )
                {
                    IState targetState = transition.GetTargetState();
                    if ( targetState != null ) return targetState;
                }

                return null;
            }

            /// <summary>
            ///     Add a new <paramref name="transition" /> to the collection.
            /// </summary>
            /// <param name="transition">
            ///     Transition to be added to the collection.
            /// </param>
            internal void Add( [NotNull] ITransition transition ) =>
                _transitions.Add( transition );
        }
    }
}