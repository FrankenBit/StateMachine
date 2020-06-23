// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.Transition{TState}.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Nested <see cref="Transition{TState}" /> class of the <seealso cref="StateMachine" /> class.
    /// </summary>
    public sealed partial class StateMachine
    {
        /// <summary>
        ///     Transition between two machine states.
        /// </summary>
        /// <typeparam name="TState">
        ///     Type of the source state.
        /// </typeparam>
        private sealed class Transition<TState> : ITransition, IStateTransition<TState>
            where TState : class, IState
        {
            /// <summary>
            ///     Collection of transition exceptions that will target different
            ///     states when certain conditions are met.
            /// </summary>
            private readonly List<Exception> _exceptions = new List<Exception>();

            /// <summary>
            ///     The source state of the transition.
            /// </summary>
            [NotNull]
            private readonly TState _sourceState;

            /// <summary>
            ///     The target state of the transition.
            /// </summary>
            [NotNull]
            private readonly IState _targetState;

            /// <summary>
            ///     Condition that has to be met for the transition to be available.
            /// </summary>
            private Func<TState, bool> _condition = s => s.Completed;

            /// <summary>
            ///     Initializes a new instance of the <see cref="Transition{TState}"/> class.
            /// </summary>
            /// <param name="sourceState">
            ///     The source state of the transition.
            /// </param>
            /// <param name="targetState">
            ///     The target state of the transition.
            /// </param>
            internal Transition( [NotNull] TState sourceState, [NotNull] IState targetState )
            {
                _sourceState = sourceState ?? throw new ArgumentNullException( nameof( sourceState ) );
                _targetState = targetState ?? throw new ArgumentNullException( nameof( targetState ) );
            }

            /// <inheritdoc />
            public void ButWhen( Func<TState, bool> condition, IState state ) =>
                _exceptions.Add( new Exception( _sourceState, condition, state ) );

            /// <inheritdoc />
            public IState GetTargetState()
            {
                foreach ( Exception exception in _exceptions )
                {
                    IState targetState = exception.GetTargetState();
                    if ( targetState != null ) return targetState;
                }

                return _condition( _sourceState ) ? _targetState : null;
            }

            /// <inheritdoc />
            public void When( Func<TState, bool> condition ) =>
                _condition = condition ?? throw new ArgumentNullException( nameof( condition ) );

            /// <summary>
            ///     An exception of the regular transition.
            /// </summary>
            private sealed class Exception : ITransition
            {
                /// <summary>
                ///     Condition that has to be met for the exception to be used.
                /// </summary>
                [NotNull]
                private readonly Func<TState, bool> _condition;

                /// <summary>
                ///     Source state the transition exception originates from.
                /// </summary>
                [NotNull]
                private readonly TState _sourceState;

                /// <summary>
                ///     Target state the transition exception will result in.
                /// </summary>
                [NotNull]
                private readonly IState _targetState;

                /// <summary>
                ///     Initializes a new instance of the <see cref="Exception"/> class.
                /// </summary>
                /// <param name="sourceState">
                ///     Source state the transition exception originates from.
                /// </param>
                /// <param name="condition">
                ///     Condition that has to be met for the exception to be used.
                /// </param>
                /// <param name="targetState">
                ///     Target state the exception will result in.
                /// </param>
                internal Exception(
                    [NotNull] TState sourceState,
                    [NotNull] Func<TState, bool> condition,
                    [NotNull] IState targetState )
                {
                    _sourceState = sourceState ?? throw new ArgumentNullException( nameof( sourceState ) );
                    _condition = condition ?? throw new ArgumentNullException( nameof( condition ) );
                    _targetState = targetState ?? throw new ArgumentNullException( nameof( targetState ) );
                }

                /// <inheritdoc />
                public IState GetTargetState() =>
                    _condition( _sourceState ) ? _targetState : null;
            }
        }
    }
}