// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultState.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Provides default machine states.
    /// </summary>
    public sealed class DefaultState : IState
    {
        /// <summary>
        ///     Name of the state.
        /// </summary>
        [NotNull]
        private readonly string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultState"/> class.
        /// </summary>
        /// <param name="name">
        ///     Name of the state.
        /// </param>
        public DefaultState( [NotNull] string name )
        {
            _name = name;
        }

        /// <summary>
        ///     Gets the default any state.
        /// </summary>
        [NotNull]
        public static IState Any { get; } = new DefaultState( "<ANY>" );

        /// <summary>
        ///     Gets the default enter state.
        /// </summary>
        public static IState Enter { get; } = new DefaultState( "<ENTER>" );

        /// <summary>
        ///     Gets the default exit state.
        /// </summary>
        public static IState Exit { get; } = new DefaultState( "<EXIT>" );

        /// <inheritdoc />
        public bool Completed =>
            true;

        /// <inheritdoc />
        void IState.Enter()
        {
        }

        /// <inheritdoc />
        void IState.Exit()
        {
        }

        /// <inheritdoc />
        public override string ToString() =>
            _name;

        /// <inheritdoc />
        void IState.Update( float deltaTime )
        {
        }
    }
}