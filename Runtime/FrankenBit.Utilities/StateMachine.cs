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
        ///     A dictionary of transition lists assigned to specific states.
        /// </summary>
        [NotNull]
        private readonly Dictionary<IState, Transitions> _transitions = new Dictionary<IState, Transitions>();

        /// <summary>
        ///     A list of transitions that will be evaluated independent of current state.
        /// </summary>
        [NotNull]
        private readonly Transitions _anyTransitions;

        /// <summary>
        ///     A collection of states already used in current update.
        /// </summary>
        [NotNull]
        private readonly Stack<IState> _usedStates = new Stack<IState>();

        /// <summary>
        ///     A value indicating whether the state machine has just processed its exit state.
        /// </summary>
        private bool _completed;

        /// <summary>
        ///     The current state of the machine.
        /// </summary>
        [NotNull]
        private IState _currentState = DefaultState.Enter;

        /// <summary>
        ///     A list of transitions assigned to current state.
        /// </summary>
        [CanBeNull]
        private Transitions _currentTransitions;

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
            ///     Gets a value indicating whether the transition has a custom condition assigned.
            /// </summary>
            bool HasCondition { get; }

            /// <summary>
            ///     Gets the target state of the transition.
            /// </summary>
            /// <returns>
            ///     Target <see cref="IState" /> of the transition.
            /// </returns>
            [NotNull]
            IState TargetState { get; }

            /// <summary>
            ///     Execute transition from supplied <paramref name="state" /> by calling its transition handling action,
            ///     if one was set.
            /// </summary>
            /// <param name="state">
            ///     The current state of the machine.
            /// </param>
            void Execute( [NotNull] IState state );

            /// <summary>
            ///     Gets a value indicating whether the transition is available from the supplied <paramref name="state" />.
            /// </summary>
            /// <param name="state">
            ///     The current state of the machine.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if the transition is available, <see langword="false" /> if not.
            /// </returns>
            bool IsAvailable( [NotNull] IState state );
        }

        /// <inheritdoc />
        bool IState.Completed =>
            _completed;

        /// <inheritdoc />
        void IState.Enter()
        {
            _completed = false;
            Execute( DefaultTransition.Enter );
        }

        /// <inheritdoc />
        void IState.Exit() =>
            Execute( DefaultTransition.Exit );

        /// <inheritdoc />
        [NotNull]
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

            _usedStates.Clear();
            _usedStates.Push( _currentState );

            ITransition transition = GetNextTransition();

            while ( transition != null && !_usedStates.Contains( transition.TargetState ) && Execute( transition ) )
            {
                _currentState.Update( deltaTime );
                _usedStates.Push( _currentState );
                transition = GetNextTransition();
            }
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
        ///     Get the transition to the exit state when the current state is completed
        ///     but does not provide any available outgoing transitions or explicit
        ///     transitions to the exit state.
        /// </summary>
        /// <returns>
        ///     A <see cref="ITransition" /> to the <see cref="DefaultState.Exit" /> state or
        ///     <see langword="null" /> when the current state is not complete yet.
        /// </returns>
        [CanBeNull]
        private ITransition GetExitTransition()
        {
            if ( !_currentState.Completed ) return null;

            return _currentTransitions?.ContainsTransitionTo( DefaultState.Exit ) != true
                ? DefaultTransition.Exit
                : null;
        }

        /// <summary>
        ///     Get the next best currently available transition from current machine state.
        /// </summary>
        /// <returns>
        ///     A <see cref="ITransition" /> that is currently available or <see langword="null" />
        ///     if there is no transition currently available.
        /// </returns>
        [CanBeNull]
        private ITransition GetNextTransition() =>
            _currentTransitions?.FindAvailableTransition( _currentState )
            ?? _anyTransitions.FindAvailableTransition( _currentState )
            ?? GetExitTransition();

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
        ///     Execute the supplied <paramref name="transition" />.
        /// </summary>
        /// <param name="transition">
        ///     Transition to be executed.
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
        private bool Execute( [NotNull] ITransition transition )
        {
            IState targetState = transition.TargetState;
            if ( targetState == _currentState ) return false;

            _currentState.Exit();

            transition.Execute( _currentState );
            Transitioning?.Invoke( _currentState, targetState );

            _currentState = targetState;
            _transitions.TryGetValue( _currentState, out _currentTransitions );

            _currentState.Enter();

            _completed = targetState == DefaultState.Exit;
            return !_completed;
        }

        /// <summary>
        ///     A transition to a default state.
        /// </summary>
        private sealed class DefaultTransition : ITransition
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="DefaultTransition"/> class.
            /// </summary>
            /// <param name="state">
            ///     The target state of the default transition.
            /// </param>
            private DefaultTransition( [NotNull] IState state )
            {
                TargetState = state;
            }

            /// <inheritdoc />
            public bool HasCondition =>
                false;

            /// <inheritdoc />
            public IState TargetState { get; }

            /// <summary>
            ///     Gets the instance of the enter state transition.
            /// </summary>
            internal static ITransition Enter { get; } = new DefaultTransition( DefaultState.Enter );

            /// <summary>
            ///     Gets the instance of the exit state transition.
            /// </summary>
            internal static ITransition Exit { get; } = new DefaultTransition( DefaultState.Exit );

            /// <inheritdoc />
            public void Execute( IState state )
            {
            }

            /// <inheritdoc />
            public bool IsAvailable( IState state ) =>
                true;
        }
    }
}