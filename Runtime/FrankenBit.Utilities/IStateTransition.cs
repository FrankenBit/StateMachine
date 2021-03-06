﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStateTransition.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Interface for a state transition generated by a state machine.
    /// </summary>
    /// <typeparam name="TSourceState">
    ///     The type of the state the transition is originating from.
    /// </typeparam>
    /// <typeparam name="TTargetState">
    ///     The type of the state the transition is targeting to.
    /// </typeparam>
    public interface IStateTransition<out TSourceState, out TTargetState>
    {
        /// <summary>
        ///     Set an <paramref name="action"/> to be executing during the transition.
        /// </summary>
        /// <param name="action">
        ///     An action to be executed during the transition.
        /// </param>
        /// <returns>
        ///     The <see cref="IStateTransition{TSourceState,TTargetState}" /> the <paramref name="action" />
        ///     was assigned to.
        /// </returns>
        [NotNull]
        IStateTransition<TSourceState, TTargetState> OnTransition(
            [NotNull] Action<TSourceState, TTargetState> action );

        /// <summary>
        ///     Specify the <paramref name="condition"/> to be met for the transition to be available.
        /// </summary>
        /// <param name="condition">
        ///     A condition that has to be met to make the transition available.
        /// </param>
        /// <returns>
        ///     The <see cref="IStateTransition{TSourceState,TTargetState}" /> the <paramref name="condition" />
        ///     was assigned to.
        /// </returns>
        [NotNull]
        IStateTransition<TSourceState, TTargetState> When( [NotNull] Func<TSourceState, bool> condition );
    }
}