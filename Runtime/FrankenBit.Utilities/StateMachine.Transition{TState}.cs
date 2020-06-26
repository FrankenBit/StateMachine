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
        ///     Type of the source state.
        /// </typeparam>
        /// <typeparam name="TTargetState">
        ///     Type of the target state.
        /// </typeparam>
        private sealed class Transition<TSourceState, TTargetState> : ITransition,
                                                                      IStateTransition<TSourceState, TTargetState>
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
            ///     Condition that has to be met for the transition to be available.
            /// </summary>
            private Func<TSourceState, bool> _condition = s => s.Completed;

            /// <summary>
            ///     Action to be executed during the transition.
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
            public IState GetTargetState()
            {
                if ( !_condition( _sourceState ) ) return null;

                _onTransition( _sourceState, _targetState );
                return _targetState;
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
        }
    }
}