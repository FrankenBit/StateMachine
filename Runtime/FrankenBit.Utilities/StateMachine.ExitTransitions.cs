// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.ExitTransitions.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Nested <see cref="ExitTransitions" /> class of the <see cref="StateMachine" /> class.
    /// </summary>
    public sealed partial class StateMachine
    {
        /// <summary>
        ///     Specialized emulated transitions collection for states that do not define any outgoing transitions.
        /// </summary>
        private sealed class ExitTransitions : ITransitions
        {
            /// <summary>
            ///     Prevents a default instance of the <see cref="ExitTransitions"/> class from being created.
            /// </summary>
            private ExitTransitions()
            {
            }

            /// <summary>
            ///     Gets the instance of the exit transitions.
            /// </summary>
            internal static ITransitions Instance { get; } = new ExitTransitions();

            /// <inheritdoc />
            public IState FindTransition( IState state ) =>
                state.Completed ? DefaultState.Exit : state;
        }
    }
}