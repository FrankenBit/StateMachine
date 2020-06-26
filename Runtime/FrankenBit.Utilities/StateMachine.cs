// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateMachine.cs" company="FrankenBit">
//     FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FrankenBit.Utilities
{
    /// <summary>
    ///     Implementation of a simple state machine.
    /// </summary>
    public sealed partial class StateMachine : IState
    {
        /// <summary>
        ///     Dictionary of transition lists assigned to specific states.
        /// </summary>
        [NotNull]
        private readonly Dictionary<IState, Transitions> _transitions = new Dictionary<IState, Transitions>();

        /// <summary>
        ///     List of transitions that will be evaluated independent of current state.
        /// </summary>
        [NotNull]
        private readonly ITransitions _anyTransitions;

        /// <summary>
        ///     Collection of states already used in current update.
        /// </summary>
        [NotNull]
        private readonly Stack<IState> _usedStates = new Stack<IState>();

        /// <summary>
        ///     Set to <see langword="true"/> when the state machine has just processed its exit state.
        /// </summary>
        private bool _completed;

        /// <summary>
        ///     Current state of the machine.
        /// </summary>
        [NotNull]
        private IState _currentState = DefaultState.Enter;

        /// <summary>
        ///     List of transitions assigned to current state.
        /// </summary>
        [NotNull]
        private ITransitions _currentTransitions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateMachine"/> class.
        /// </summary>
        public StateMachine()
        {
            _anyTransitions = GetTransitions( DefaultState.Any );
            _currentTransitions = GetTransitions( DefaultState.Enter );
            AddTransition( DefaultState.Exit, DefaultState.Enter );
        }

        /// <summary>
        ///     Occurs when the state machine is transitioning from one state to a different one.
        /// </summary>
        public event Action<IState, IState> Transitioning;

        /// <summary>
        ///     Interface for a state transition.
        /// </summary>
        private interface ITransition
        {
            /// <summary>
            ///     Try to get the target state of the transition.
            /// </summary>
            /// <returns>
            ///     Target <see cref="IState" /> of the transition if the transition is
            ///     available or <see langword="null" /> if the transition is not available now.
            /// </returns>
            [CanBeNull]
            IState GetTargetState();
        }

        /// <summary>
        ///     Interface for a collection of zero or more transitions.
        /// </summary>
        private interface ITransitions
        {
            /// <summary>
            ///     Try to find an available transition from the supplied <paramref name="state" />.
            /// </summary>
            /// <param name="state">
            ///     The state to perform a transition from.
            /// </param>
            /// <returns>
            ///     Target <seealso cref="IState" /> to which a transition from the supplied source
            ///     <paramref name="state" /> is available or <see langword="null" /> if no
            ///     transition is currently available.
            /// </returns>
            [CanBeNull]
            IState FindTransition( [NotNull] IState state );
        }

        /// <inheritdoc />
        bool IState.Completed =>
            _completed;

        /// <inheritdoc />
        void IState.Enter()
        {
            _completed = false;
            TransitionTo( DefaultState.Enter );
        }

        /// <inheritdoc />
        void IState.Exit() =>
            TransitionTo( DefaultState.Exit );

        /// <inheritdoc />
        public override string ToString() =>
            $"[{_currentState}] ({_currentTransitions})";

        /// <summary>
        ///     Update state machine for current frame.
        /// </summary>
        /// <param name="deltaTime">
        ///     Time in seconds that has passed since the previous update call.
        /// </param>
        public void Update( float deltaTime )
        {
            _currentState.Update( deltaTime );
            _usedStates.Push( _currentState );

            IState targetState = GetTargetState();

            while ( !_usedStates.Contains( targetState ) && TransitionTo( targetState ) )
            {
                targetState.Update( deltaTime );

                _usedStates.Push( targetState );
                targetState = GetTargetState();
            }

            _usedStates.Clear();
        }

        /// <summary>
        ///     Add a transition from the supplied <paramref name="sourceState" /> to the supplied
        ///     <paramref name="targetState" /> when the <paramref name="sourceState" /> indicates
        ///     that it has completed its processing.
        /// </summary>
        /// <typeparam name="TSourceState">
        ///     Type of the source state the transition originates from.
        /// </typeparam>
        /// <typeparam name="TTargetState">
        ///     Type of the target state the transition points to.
        /// </typeparam>
        /// <param name="sourceState">
        ///     State that has to be current for the transition to be considered.
        /// </param>
        /// <param name="targetState">
        ///     State to add a transition to, or <see langword="null" /> if the transition should
        ///     target the internal exit state.
        /// </param>
        /// <returns>
        ///     A state transition that may be used to set an additional condition or specify
        ///     alternative transition targets when certain conditions are met.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourceState" /> or <paramref name="targetState" /> is <see langword="null" />.
        /// </exception>
        [NotNull]
        public IStateTransition<TSourceState, TTargetState> AddTransition<TSourceState, TTargetState>(
            [NotNull] TSourceState sourceState,
            [NotNull] TTargetState targetState )
            where TSourceState : class, IState
            where TTargetState : class, IState
        {
            if ( sourceState == null ) throw new ArgumentNullException( nameof( sourceState ) );
            if ( targetState == null ) throw new ArgumentNullException( nameof( targetState ) );

            Transition<TSourceState, TTargetState> transition = Transition.Between( sourceState, targetState );
            GetTransitions( sourceState ).Add( transition );
            return transition;
        }

        /// <summary>
        ///     Get current state of the machine.
        /// </summary>
        /// <returns>
        ///     Current state of the machine.
        /// </returns>
        [NotNull]
        private IState GetTargetState() =>
            _anyTransitions.FindTransition( _currentState )
            ?? _currentTransitions.FindTransition( _currentState )
            ?? _currentState;

        /// <summary>
        ///     Get collection of transitions for the supplied <paramref name="state" />.
        /// </summary>
        /// <param name="state">
        ///     State to get the collection of transitions for.
        /// </param>
        /// <returns>
        ///     A collection of transitions for the supplied <paramref name="state" />.
        /// </returns>
        [NotNull]
        private Transitions GetTransitions( [NotNull] IState state )
        {
            if ( _transitions.TryGetValue( state, out Transitions transitions ) ) return transitions;

            transitions = new Transitions();
            _transitions[ state ] = transitions;
            return transitions;
        }

        /// <summary>
        ///     Transition to supplied <paramref name="targetState" />.
        /// </summary>
        /// <param name="targetState">
        ///     Target state to transition the state machine to.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a transition to a different state was performed
        ///     and the new state is not the exit state,
        ///     <see langword="false" /> if current state remains current.
        /// </returns>
        /// <remarks>
        ///     The method returns <see langword="false" /> when transitioning to the exit state
        ///     to prevent the state machine from looping directly over the enter state in the same
        ///     update tick to allow eventual parent state machines to move on to the next state.
        /// </remarks>
        private bool TransitionTo( [NotNull] IState targetState )
        {
            if ( targetState == _currentState ) return false;

            _currentState.Exit();

            Transitioning?.Invoke( _currentState, targetState );

            _currentState = targetState;
            _currentState.Enter();

            _currentTransitions = _transitions.TryGetValue( _currentState, out Transitions currentTransitions )
                ? currentTransitions
                : ExitTransitions.Instance;

            if ( targetState != DefaultState.Exit ) return true;

            _completed = true;
            return false;
        }
    }
}