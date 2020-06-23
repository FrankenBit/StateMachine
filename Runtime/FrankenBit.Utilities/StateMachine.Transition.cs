// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.Transition.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Nested <see cref="Transition" /> class of the <see cref="StateMachine" /> class.
    /// </summary>
    public sealed partial class StateMachine
    {
        /// <summary>
        ///     Static transition class providing a convenient method to create new transitions.
        /// </summary>
        private static class Transition
        {
            /// <summary>
            ///     Create a transition between the <paramref name="sourceState" /> and the <paramref name="targetState" />.
            /// </summary>
            /// <typeparam name="TState">
            ///     Type of the source state.
            /// </typeparam>
            /// <param name="sourceState">
            ///     The source state to create the transition from.
            /// </param>
            /// <param name="targetState">
            ///     The target state to create the transition to.
            /// </param>
            /// <returns>
            ///     A new <see cref="Transition{TState}" /> from the <paramref name="sourceState" />
            ///     to the <paramref name="targetState" />.
            /// </returns>
            [NotNull]
            internal static Transition<TState> Between<TState>( [NotNull] TState sourceState, [NotNull] IState targetState )
                where TState : class, IState =>
                new Transition<TState>( sourceState, targetState );
        }
    }
}