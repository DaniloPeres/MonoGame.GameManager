using MonoGame.GameManager.Core;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.StateMachines
{
    /// <summary>
    /// A finite state machine for game states, enemy AI, animations... States are identified by a value (usually
    /// an enum) and have optional enter, update and exit callbacks (or an <see cref="IState"/>). Transitions with
    /// conditions are checked on every update.
    /// </summary>
    /// <example>
    /// <code>
    /// var ai = new StateMachine&lt;EnemyState&gt;()
    ///     .AddState(EnemyState.Patrol, onUpdate: Patrol)
    ///     .AddState(EnemyState.Chase, onEnter: () => speed = 200, onUpdate: Chase)
    ///     .AddTransition(EnemyState.Patrol, EnemyState.Chase, () => DistanceToPlayer() &lt; 150)
    ///     .AddTransition(EnemyState.Chase, EnemyState.Patrol, () => DistanceToPlayer() > 300);
    /// ai.SetInitialState(EnemyState.Patrol);
    /// Scheduler.Add(ai); // or call ai.Update(deltaSeconds) yourself
    /// </code>
    /// </example>
    public class StateMachine<TState> : IUpdatable
    {
        private readonly Dictionary<TState, StateCallbacks> states;
        private readonly List<Transition> transitions = new List<Transition>();
        private readonly List<Transition> anyStateTransitions = new List<Transition>();
        private readonly IEqualityComparer<TState> comparer;
        private bool isChangingState;
        private bool hasPendingState;
        private TState pendingState;

        public StateMachine(IEqualityComparer<TState> comparer = null)
        {
            this.comparer = comparer ?? EqualityComparer<TState>.Default;
            states = new Dictionary<TState, StateCallbacks>(this.comparer);
        }

        /// <summary>The current state (default when there is none).</summary>
        public TState CurrentState { get; private set; }

        /// <summary>The previous state (default when there is none).</summary>
        public TState PreviousState { get; private set; }

        /// <summary>True once the machine has a state.</summary>
        public bool HasState { get; private set; }

        /// <summary>Seconds spent in the current state.</summary>
        public float TimeInState { get; private set; }

        /// <summary>Raised after a state change, with the previous and the new state.</summary>
        public event Action<TState, TState> StateChanged;

        /// <summary>Registers callbacks for a state. Registering a state is optional.</summary>
        public StateMachine<TState> AddState(TState state, Action onEnter = null, Action<float> onUpdate = null, Action onExit = null)
        {
            states[state] = new StateCallbacks(onEnter, onUpdate, onExit);
            return this;
        }

        /// <summary>Registers a state implemented as a class.</summary>
        public StateMachine<TState> AddState(TState state, IState implementation)
        {
            if (implementation == null)
                throw new ArgumentNullException(nameof(implementation));
            states[state] = new StateCallbacks(implementation.Enter, implementation.Update, implementation.Exit);
            return this;
        }

        /// <summary>Changes from <paramref name="from"/> to <paramref name="to"/> when the condition is true.</summary>
        public StateMachine<TState> AddTransition(TState from, TState to, Func<bool> condition)
        {
            transitions.Add(new Transition(from, to, condition ?? throw new ArgumentNullException(nameof(condition))));
            return this;
        }

        /// <summary>Changes from any state to <paramref name="to"/> when the condition is true (checked first).</summary>
        public StateMachine<TState> AddAnyTransition(TState to, Func<bool> condition)
        {
            anyStateTransitions.Add(new Transition(default, to, condition ?? throw new ArgumentNullException(nameof(condition))));
            return this;
        }

        /// <summary>Sets the first state (its enter callback is invoked).</summary>
        public void SetInitialState(TState state) => ChangeState(state);

        public bool IsInState(TState state) => HasState && comparer.Equals(CurrentState, state);

        /// <summary>
        /// Changes the state: the exit callback of the current state and the enter callback of the new state are
        /// invoked. Returns false if the machine is already in that state.
        /// </summary>
        public bool ChangeState(TState state)
        {
            if (HasState && comparer.Equals(CurrentState, state))
                return false;

            if (isChangingState)
            {
                // A callback changed the state again: apply it after the current change.
                pendingState = state;
                hasPendingState = true;
                return true;
            }

            isChangingState = true;
            try
            {
                var previous = CurrentState;
                var hadState = HasState;
                if (hadState && states.TryGetValue(previous, out var previousCallbacks))
                    previousCallbacks.OnExit?.Invoke();

                PreviousState = previous;
                CurrentState = state;
                HasState = true;
                TimeInState = 0f;

                if (states.TryGetValue(state, out var callbacks))
                    callbacks.OnEnter?.Invoke();

                StateChanged?.Invoke(previous, state);
            }
            finally
            {
                isChangingState = false;
            }

            if (hasPendingState)
            {
                hasPendingState = false;
                ChangeState(pendingState);
            }

            return true;
        }

        /// <summary>Checks the transitions and updates the current state.</summary>
        public void Update(float deltaSeconds)
        {
            if (!HasState)
                return;

            if (!TryTransition(anyStateTransitions, true))
                TryTransition(transitions, false);

            TimeInState += deltaSeconds;
            if (states.TryGetValue(CurrentState, out var callbacks))
                callbacks.OnUpdate?.Invoke(deltaSeconds);
        }

        private bool TryTransition(List<Transition> candidates, bool fromAnyState)
        {
            foreach (var transition in candidates)
            {
                if (!fromAnyState && !comparer.Equals(transition.From, CurrentState))
                    continue;
                if (comparer.Equals(transition.To, CurrentState) || !transition.Condition())
                    continue;

                ChangeState(transition.To);
                return true;
            }
            return false;
        }

        private sealed class StateCallbacks
        {
            public StateCallbacks(Action onEnter, Action<float> onUpdate, Action onExit)
            {
                OnEnter = onEnter;
                OnUpdate = onUpdate;
                OnExit = onExit;
            }

            public Action OnEnter { get; }

            public Action<float> OnUpdate { get; }

            public Action OnExit { get; }
        }

        private sealed class Transition
        {
            public Transition(TState from, TState to, Func<bool> condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            public TState From { get; }

            public TState To { get; }

            public Func<bool> Condition { get; }
        }
    }
}
