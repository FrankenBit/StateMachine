// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IState.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Interface for a state machine state.
    /// </summary>
    public interface IState
    {
        /// <summary>
        ///     Gets a value indicating whether the state has completed its processing.
        /// </summary>
        bool Completed { get; }

        /// <summary>
        ///     Called when the state is entered.
        /// </summary>
        void Enter();

        /// <summary>
        ///     Called when the state is exited.
        /// </summary>
        void Exit();

        /// <summary>
        ///     Update state while it is active.
        /// </summary>
        /// <param name="deltaTime">
        ///     The time in seconds that has passed since the previous update call.
        /// </param>
        void Update( float deltaTime );
    }
}