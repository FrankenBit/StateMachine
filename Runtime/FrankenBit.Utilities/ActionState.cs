// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionState.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     A state for the state machine that allows to directly assign actions / delegates to be executed
    ///     on certain state machine events.
    /// </summary>
    public sealed class ActionState : IState
    {
        /// <inheritdoc />
        bool IState.Completed =>
            Completed?.Invoke() ?? true;

        /// <summary>
        ///     Gets or sets the <see cref="Func{TResult}" /> to be executed to check completion state of the state.
        /// </summary>
        [CanBeNull]
        public Func<bool> Completed { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Action"/> to be executed when the state is entered.
        /// </summary>
        [CanBeNull]
        public Action Enter { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Action"/> to be executed when the state is exited.
        /// </summary>
        [CanBeNull]
        public Action Exit { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Action{T}"/> to be executed when the state is updated.
        /// </summary>
        [CanBeNull]
        public Action<float> Update { get; set; }

        /// <inheritdoc />
        void IState.Enter() =>
            Enter?.Invoke();

        /// <inheritdoc />
        void IState.Exit() =>
            Exit?.Invoke();

        /// <inheritdoc />
        void IState.Update( float deltaTime ) =>
            Update?.Invoke( deltaTime );
    }
}