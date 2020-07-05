// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.Transition{TState}.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Nested <see cref="Transition{TSourceState,TTargetState}" /> class
    ///     of the <seealso cref="StateMachine" /> class.
    /// </summary>
    public sealed partial class StateMachine
    {
        /// <summary>
        ///     Transition between two machine states.
        /// </summary>
        /// <typeparam name="TSourceState">
        ///     The type of the source state.
        /// </typeparam>
        /// <typeparam name="TTargetState">
        ///     The type of the target state.
        /// </typeparam>
        private sealed class Transition<TSourceState, TTargetState>
            : ITransition, IStateTransition<TSourceState, TTargetState>
            where TSourceState : class, IState
            where TTargetState : class, IState
        {
            /// <summary>
            ///     The source state of the transition.
            /// </summary>
            [NotNull]
            private readonly TSourceState _sourceState;

            /// <summary>
            ///     The target state of the transition.
            /// </summary>
            [NotNull]
            private readonly TTargetState _targetState;

            /// <summary>
            ///     A condition that has to be met for the transition to be available.
            /// </summary>
            [CanBeNull]
            private Func<TSourceState, bool> _condition;

            /// <summary>
            ///     An action to be executed during the transition.
            /// </summary>
            private Action<TSourceState, TTargetState> _onTransition = ( s, t ) => { };

            /// <summary>
            ///     Initializes a new instance of the <see cref="Transition{TSourceState,TTargetState}"/> class.
            /// </summary>
            /// <param name="sourceState">
            ///     The source state of the transition.
            /// </param>
            /// <param name="targetState">
            ///     The target state of the transition.
            /// </param>
            internal Transition( [NotNull] TSourceState sourceState, [NotNull] TTargetState targetState )
            {
                _sourceState = sourceState ?? throw new ArgumentNullException( nameof( sourceState ) );
                _targetState = targetState ?? throw new ArgumentNullException( nameof( targetState ) );
            }

            /// <inheritdoc />
            public bool HasCondition =>
                _condition != null;

            /// <inheritdoc />
            public IState TargetState =>
                _targetState;

            /// <inheritdoc />
            /// <exception cref="Exception">
            ///     A delegate callback throws an exception.
            /// </exception>
            public void Execute( IState state )
            {
                TSourceState sourceState = ResolveSourceState( state );

                _onTransition?.Invoke( sourceState, _targetState );
            }

            /// <inheritdoc />
            /// <exception cref="Exception">
            ///     A delegate callback throws an exception.
            /// </exception>
            public bool IsAvailable( IState state )
            {
                TSourceState sourceState = ResolveSourceState( state );
                return _condition?.Invoke( sourceState ) ?? sourceState.Completed;
            }

            /// <inheritdoc />
            public IStateTransition<TSourceState, TTargetState> OnTransition(
                Action<TSourceState, TTargetState> action )
            {
                _onTransition = action ?? throw new ArgumentNullException( nameof( action ) );
                return this;
            }

            /// <inheritdoc />
            public IStateTransition<TSourceState, TTargetState> When( Func<TSourceState, bool> condition )
            {
                _condition = condition ?? throw new ArgumentNullException( nameof( condition ) );
                return this;
            }

            /// <summary>
            ///     Resolve actual source state using the supplied <paramref name="state" />.
            /// </summary>
            /// <param name="state">
            ///     The current machine state reported by the state machine.
            /// </param>
            /// <returns>
            ///     The state that should be used for transition callbacks or
            ///     <see langword="null" /> if the machine state does not match the transition source state.
            /// </returns>
            [NotNull]
            private TSourceState ResolveSourceState( [NotNull] IState state ) =>
                _sourceState == DefaultState.Any ? (TSourceState)state : _sourceState;
        }
    }
}