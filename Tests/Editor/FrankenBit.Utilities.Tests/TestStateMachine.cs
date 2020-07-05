// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestStateMachine.cs" company="FrankenBit">
//   FrankenBit (c) 2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NUnit.Framework;

using Debug = UnityEngine.Debug;

namespace FrankenBit.Utilities.Tests
{
    /// <summary>
    ///     Test suite for the <see cref="StateMachine" />
    /// </summary>
    public sealed class TestStateMachine
    {
        /// <summary>
        ///     Test if the action state works as expected.
        /// </summary>
        [Test]
        public void TestActionState()
        {
            var machine = new StateMachine();

            var entered = false;
            var exited = false;
            var updated = 0;

            var state = new ActionState
            {
                Enter = () =>
                {
                    entered = true;
                    exited = false;
                },
                Completed = () => updated == 2,
                Exit = () => exited = true,
                Update = d => updated += 1
            };

            machine.AddTransition( DefaultState.Enter, state );

            machine.Update( 0 );

            Assert.IsTrue( entered );
            Assert.IsFalse( exited );
            Assert.AreEqual( 1, updated, double.Epsilon );

            machine.Update( 0 );

            Assert.IsTrue( entered );
            Assert.IsTrue( exited );
            Assert.AreEqual( 2, updated, double.Epsilon );

            machine.Update( 0 );
            Assert.IsTrue( entered );
            Assert.IsFalse( exited );
        }

        /// <summary>
        ///     Ensure that the source state on a transition from any state references the actual
        ///     state the machine is transitioning from and not the <see cref="DefaultState.Any" />.
        /// </summary>
        [Test]
        public void TestSourceOfTransitionFromAnyState()
        {
            var state1Entered = false;
            var state2Entered = false;

            var machine = new StateMachine();
            var state1 = new ActionState { Completed = () => false, Enter = () => state1Entered = true };
            var state2 = new ActionState { Completed = () => false, Enter = () => state2Entered = true };

            machine.AddTransition( DefaultState.Enter, state1 );

            machine.AddTransition( DefaultState.Any, state2 )
               .When(
                    s =>
                    {
                        Assert.AreNotEqual( DefaultState.Any, s );
                        return state1Entered;
                    } )
               .OnTransition( ( s, t ) => Assert.AreNotEqual( DefaultState.Any, s ) );

            machine.Update( 0 );

            Assert.IsTrue( state2Entered );
        }

        /// <summary>
        ///     Ensure that the delay state actually delays state machine execution.
        /// </summary>
        [Test]
        public void TestDelayState()
        {
            var machine = new StateMachine();
            var state1 = new CounterState();
            var state2 = new DelayState( 3 );
            var state3 = new CounterState();

            machine.Transitioning += DebugTransitions;

            machine.AddTransition( DefaultState.Enter, state1 );
            machine.AddTransition( state1, state2 );
            machine.AddTransition( state2, state3 );

            for ( var i = 0; i < 2; i++ )
            {
                machine.Update( 1 );
            }

            Assert.AreEqual( 1, state1.Entered, double.Epsilon );
            Assert.IsFalse( state2.Completed );
            Assert.AreEqual( 0, state3.Entered, double.Epsilon );

            machine.Update( 1 );

            Assert.AreEqual( 1, state1.Entered, double.Epsilon );
            Assert.IsTrue( state2.Completed );
            Assert.AreEqual( 1, state3.Entered, double.Epsilon );

            machine.Update( 1 );
            Assert.IsFalse( state2.Completed );
        }

        /// <summary>
        ///     Ensure that the exception transition works properly.
        /// </summary>
        [Test]
        public void TestExceptionTransition()
        {
            var machine = new StateMachine();
            var state1 = new VisitedState();
            var state2 = new VisitedState();
            var state3 = new VisitedState();

            machine.AddTransition( DefaultState.Enter, state1 );

            machine.AddTransition( state1, state3 ).When( s => s.Exited );
            machine.AddTransition( state1, state2 );

            machine.Update( 0 );

            Assert.IsTrue( state2.Entered );
            Assert.IsFalse( state3.Entered );

            machine.Update( 0 );
            Assert.IsTrue( state3.Entered );
        }

        /// <summary>
        ///     Ensure that the right transition is used when a state has multiple outgoing transitions.
        /// </summary>
        [Test]
        public void TestMultipleTransitions()
        {
            var machine = new StateMachine();

            var rightState = new VisitedState();
            var wrongState1 = new VisitedState();
            var wrongState2 = new VisitedState();

            machine.AddTransition( DefaultState.Enter, wrongState1 )
               .When( s => false );

            machine.AddTransition( DefaultState.Enter, rightState );

            machine.AddTransition( DefaultState.Enter, wrongState2 )
               .When( s => false );
        }

        /// <summary>
        ///     Ensure that the nested state machine is properly exited.
        /// </summary>
        [Test]
        public void TestNestedMachineExit()
        {
            var parent = new StateMachine();
            var child = new StateMachine();

            var childState = new VisitedState();
            var parentState = new VisitedState();

            parent.AddTransition( DefaultState.Enter, child );
            parent.AddTransition( child, parentState );

            child.AddTransition( DefaultState.Enter, childState );

            parent.Update( 0 );
            Assert.IsTrue( childState.Exited );
            Assert.IsTrue( parentState.Entered );
        }

        /// <summary>
        ///     Ensure that nested state machine is properly executed as part of a parent machine.
        ///     Also ensure that an empty state (that immediately reports completion) is not updated.
        /// </summary>
        [Test]
        public void TestNestedMachineWithEmptyState()
        {
            var parent = new StateMachine();
            var child = new StateMachine();

            var state = new VisitedState();

            child.AddTransition( DefaultState.Enter, state );
            parent.AddTransition( DefaultState.Enter, child );

            parent.Update( 0 );

            Assert.IsTrue( state.Entered, "Nested state machine was not entered." );
            Assert.IsTrue( state.Updated, "Nested state machine was not updated." );
            Assert.IsTrue( state.Exited, "Nested state machine was not exited." );
        }

        /// <summary>
        ///     Ensure that nested state machine is properly executed as part of a parent machine.
        ///     Also ensure that a non-empty state is properly updated.
        /// </summary>
        [Test]
        public void TestNestedMachineWithNonEmptyState()
        {
            var parent = new StateMachine();
            var child = new StateMachine();

            parent.AddTransition( DefaultState.Enter, child );

            var state = new CounterState();
            child.AddTransition( DefaultState.Enter, state );

            child.AddTransition( state, DefaultState.Exit )
               .When( s => s.Updated > 1 );

            parent.Update( 0 );

            Assert.AreEqual( 1, state.Entered, double.Epsilon );
            Assert.AreEqual( 1, state.Updated, double.Epsilon );
            Assert.AreEqual( 0, state.Exited, double.Epsilon );

            parent.Update( 0 );

            Assert.AreEqual( 2, state.Updated, double.Epsilon );
            Assert.AreEqual( 1, state.Exited, double.Epsilon );
        }

        /// <summary>
        ///     Ensure that the state machine does not loop infinitely when all states
        ///     immediately report completion.
        /// </summary>
        [Test]
        public void TestLargeTransitionLoop()
        {
            var runs = 0;

            var machine = new StateMachine();

            var state = new ActionState
            {
                Enter = () =>
                {
                    Assert.AreEqual( 0, runs, double.Epsilon );
                    runs++;
                }
            };

            machine.AddTransition( DefaultState.Enter, state );
            machine.Update( 0 );

            Assert.AreEqual( 1, runs, double.Epsilon );
        }

        /// <summary>
        ///     Ensure that the state machine does not loop infinitely between states
        ///     immediately reporting completion and linked in a loop with transitions.
        /// </summary>
        [Test]
        public void TestSmallTransitionLoop()
        {
            var runs = 0;

            var machine = new StateMachine();

            var state1 = new ActionState
            {
                Enter = () => runs++
            };

            var state2 = new ActionState
            {
                Enter = () => runs++
            };

            machine.AddTransition( DefaultState.Enter, state1 );
            machine.AddTransition( state1, state2 );
            machine.AddTransition( state2, state1 );

            machine.Update( 0 );
            Assert.AreEqual( 2, runs, double.Epsilon );
        }

        /// <summary>
        ///     Ensure that the ToString conversion of the state machine matches to expected value.
        /// </summary>
        [Test]
        public void TestStringConversion()
        {
            var machine = new StateMachine();

            var state = new DefaultState( "Test" );

            machine.AddTransition( DefaultState.Enter, state );

            machine.AddTransition( state, DefaultState.Exit )
               .When( s => false );

            machine.Update( 0 );

            string text = machine.ToString();
            Assert.AreEqual( "[Test] (1)", text );
        }

        /// <summary>
        ///     Ensure that a transition action is executed during a transition.
        /// </summary>
        [Test]
        public void TestTransitionAction()
        {
            var machine = new StateMachine();

            int a = 0, b = 0;

            var state1 = new CounterState();
            var state2 = new DefaultState( "S2" );
            var state3 = new DefaultState( "S3" );

            machine.AddTransition( DefaultState.Enter, state1 );

            machine.AddTransition( state1, state2 )
               .When( s => s.Entered == 1 )
               .OnTransition( ( s, t ) => a = s.Updated );

            machine.AddTransition( state1, state3 )
               .When( s => s.Entered == 2 )
               .OnTransition( ( s, t ) => b = s.Updated );

            machine.Update( 0 );

            Assert.AreEqual( 1, a, double.Epsilon );
            Assert.AreEqual( 0, b, double.Epsilon );

            machine.Update( 0 );

            Assert.AreEqual( 1, a, double.Epsilon );
            Assert.AreEqual( 2, b, double.Epsilon );
        }

        /// <summary>
        ///     Ensure that the transitions work at all.
        /// </summary>
        [Test]
        public void TestTransitionFromEnterState()
        {
            var machine = new StateMachine();

            var state = new VisitedState();

            machine.AddTransition( DefaultState.Enter, state );
            machine.Update( 0 );

            Assert.True( state.Entered, $"State machine is not in expected state ({machine})." );
        }

        /// <summary>
        ///     Ensure that a state is updated only once per state machine update.
        /// </summary>
        [Test]
        public void TestUpdateCount()
        {
            var machine = new StateMachine();
            var state = new CounterState();

            machine.AddTransition( DefaultState.Enter, state );

            machine.Update( 0 );

            Assert.AreEqual( 1, state.Updated, double.Epsilon );
        }

        /// <summary>
        ///     Debug transitions of a state machine from a <paramref name="source"/>
        ///     to a <paramref name="target"/> state.
        /// </summary>
        /// <param name="source">
        ///     The source state the state machine transitions from.
        /// </param>
        /// <param name="target">
        ///     The target state the state machine transitions to.
        /// </param>
        private static void DebugTransitions( IState source, IState target ) =>
            Debug.Log( $"Transitioning from {source} to {target}." );

        /// <summary>
        ///     Test helper state that counts how often it was visited.
        /// </summary>
        private sealed class CounterState : IState
        {
            /// <inheritdoc />
            public bool Completed { get; } = true;

            /// <summary>
            ///     Gets the number of times the state has been entered.
            /// </summary>
            internal int Entered { get; private set; }

            /// <summary>
            ///     Gets the number of times the state has been exited.
            /// </summary>
            internal int Exited { get; private set; }

            /// <summary>
            ///     Gets the number of times the state has been updated.
            /// </summary>
            internal int Updated { get; private set; }

            /// <inheritdoc />
            public void Enter() =>
                Entered++;

            /// <inheritdoc />
            public void Exit() =>
                Exited++;

            /// <inheritdoc />
            public void Update( float deltaTime ) =>
                Updated++;
        }

        /// <summary>
        ///     Test helper state that remembers if it was visited at least once.
        /// </summary>
        private sealed class VisitedState : IState
        {
            /// <inheritdoc />
            public bool Completed { get; } = true;

            /// <summary>
            ///     Gets a value indicating whether the state was entered at least once.
            /// </summary>
            internal bool Entered { get; private set; }

            /// <summary>
            ///     Gets a value indicating whether the state was exited at least once.
            /// </summary>
            internal bool Exited { get; private set; }

            /// <summary>
            ///     Gets a value indicating whether the state was updated at least once.
            /// </summary>
            internal bool Updated { get; private set; }

            /// <inheritdoc />
            public void Enter() =>
                Entered = true;

            /// <inheritdoc />
            public void Exit() =>
                Exited = true;

            /// <inheritdoc />
            public void Update( float deltaTime ) =>
                Updated = true;
        }
    }
}
