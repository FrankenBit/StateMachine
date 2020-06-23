// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelayState.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Delay state that expires after the specified time.
    /// </summary>
    public sealed class DelayState : IState
    {
        /// <summary>
        ///     Initial delay in seconds.
        /// </summary>
        private readonly float _delay;

        /// <summary>
        ///     Current remaining delay in seconds.
        /// </summary>
        private float _currentDelay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelayState"/> class.
        /// </summary>
        /// <param name="delay">
        ///     Delay in seconds after which the state will complete.
        /// </param>
        public DelayState( float delay )
        {
            _delay = delay;
        }

        /// <inheritdoc />
        public bool Completed =>
            _currentDelay <= 0;

        /// <inheritdoc />
        public void Enter() =>
            _currentDelay = _delay;

        /// <inheritdoc />
        public void Exit()
        {
        }

        /// <inheritdoc />
        public void Update( float deltaTime )
        {
            if ( _currentDelay > 0 ) _currentDelay -= deltaTime;
        }
    }
}