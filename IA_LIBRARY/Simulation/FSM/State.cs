    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

namespace IA_Library_FSM
{
    /// <summary>
    /// Manage the behaviurs for the FSM.
    /// </summary>
    public struct BehavioursActions
    {
        private Dictionary<int, List<Action>> mainThreadBehaviour;
        private ConcurrentDictionary<int, ConcurrentBag<Action>> multithreadablesBehaviour;
        private Action transitionBehaviour;

        /// <summary>
        /// Add main thread behaviour.
        /// </summary>
        /// <param name="executionOrder">The execution order</param>
        /// <param name="behaviour">The action</param>
        public void AddMainThreadBehaviour(int executionOrder, Action behaviour)
        {
            if (mainThreadBehaviour == null)
            {
                mainThreadBehaviour = new Dictionary<int, List<Action>>();
            }

            if (!mainThreadBehaviour.ContainsKey(executionOrder))
            {
                mainThreadBehaviour.Add(executionOrder, new List<Action>());
            }

            mainThreadBehaviour[executionOrder].Add(behaviour);
        }

        /// <summary>
        /// Add multitreadable behaviours.
        /// </summary>
        /// <param name="executionOrder">The execution order</param>
        /// <param name="behaviour">The behaviour</param>
        public void AddMultitreadableBehaviours(int executionOrder, Action behaviour)
        {
            if (multithreadablesBehaviour == null)
            {
                multithreadablesBehaviour = new ConcurrentDictionary<int, ConcurrentBag<Action>>();
            }

            if (!multithreadablesBehaviour.ContainsKey(executionOrder))
            {
                multithreadablesBehaviour.TryAdd(executionOrder, new ConcurrentBag<Action>());
            }

            multithreadablesBehaviour[executionOrder].Add(behaviour);
        }

        /// <summary>
        /// Sets the transition behaviours.
        /// </summary>
        /// <param name="behaviour">The behaviours</param>
        public void SetTransitionBehaviour(Action behaviour)
        {
            transitionBehaviour = behaviour;
        }

        public Dictionary<int, List<Action>> MainThreadBehaviour => mainThreadBehaviour;
        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultithreadablesBehaviour => multithreadablesBehaviour;
        public Action TransitionBehaviour => transitionBehaviour;
    }

    /// <summary>
    /// State abstract class.
    /// </summary>
    public abstract class State
    {
        public Action<Enum> OnFlag;
        public abstract BehavioursActions GetOnEnterBehaviour(params object[] parameters);
        public abstract BehavioursActions GetTickBehaviour(params object[] parameters);
        public abstract BehavioursActions GetOnExitBehaviour(params object[] parameters);
    }
}