using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IA_Library_FSM
{
    public class FSM<EnumState, EnumFlag>
        where EnumState : Enum
        where EnumFlag : Enum
    {
        private const int UNNASSSIGNED_TRANSITION = -1;

        public int currentState = 0;
        private Dictionary<int, State> behaviours;
        private Dictionary<int, Func<object[]>> behaviourTickParameters;
        private Dictionary<int, Func<object[]>> behaviourOnEnterParameters;
        private Dictionary<int, Func<object[]>> behaviourOnExitParameters;

        private (int destinationState, Action onTransition)[,] transitions;

        ParallelOptions parallelsOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

        private BehavioursActions GetCurrentStateOnEnterBehaviours => behaviours[currentState]
            .GetOnEnterBehaviour(behaviourOnEnterParameters[currentState]?.Invoke());

        private BehavioursActions GetCurrentStateOnExitBehaviours => behaviours[currentState]
            .GetOnExitBehaviour(behaviourOnExitParameters[currentState]?.Invoke());

        private BehavioursActions GetCurrentStateTickBehaviours => behaviours[currentState]
            .GetTickBehaviour(behaviourTickParameters[currentState]?.Invoke());

        public FSM()
        {
            int states = Enum.GetValues(typeof(EnumState)).Length;
            int flags = Enum.GetValues(typeof(EnumFlag)).Length;

            behaviours = new Dictionary<int, State>();
            transitions = new (int, Action)[states, flags];

            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < flags; j++)
                {
                    transitions[i, j] = (UNNASSSIGNED_TRANSITION, null);
                }
            }

            behaviourTickParameters = new Dictionary<int, Func<object[]>>();
            behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
        }

        public void AddBehaviour<T>(EnumState state, Func<object[]> onTickParameters = null,
            Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
        {
            int stateIndex = Convert.ToInt32(state);
            if (!behaviours.ContainsKey(stateIndex))
            {
                State newbehaviour = new T();
                newbehaviour.OnFlag += Transition;
                behaviours.Add(stateIndex, newbehaviour);
                behaviourTickParameters.Add(stateIndex, onTickParameters);
                behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
                behaviourOnExitParameters.Add(stateIndex, onExitParameters);
            }
        }

        public void ForcedState(EnumState state)
        {
            currentState = Convert.ToInt32(state);
            ExecuteBehaviour(GetCurrentStateOnEnterBehaviours);
        }

        public void SetTransition(EnumState originState, EnumFlag flag, EnumState destinationState,
            Action onTransition = null)
        {
            transitions[Convert.ToInt32(originState), Convert.ToInt32(flag)] =
                (Convert.ToInt32(destinationState), onTransition);
        }

        public void Transition(Enum flag)
        {
            if (transitions[currentState, Convert.ToInt32(flag)].destinationState != UNNASSSIGNED_TRANSITION)
            {
                ExecuteBehaviour(GetCurrentStateOnExitBehaviours);

                transitions[currentState, Convert.ToInt32(flag)].onTransition?.Invoke();

                currentState = transitions[currentState, Convert.ToInt32(flag)].destinationState;

                ExecuteBehaviour(GetCurrentStateOnEnterBehaviours);
            }
        }

        public void Tick()
        {
            if (behaviours.ContainsKey(currentState))
            {
                ExecuteBehaviour(GetCurrentStateTickBehaviours);
            }
        }

        private void ExecuteBehaviour(BehavioursActions behavioursActions)
        {
            if (behavioursActions.Equals(default(BehavioursActions)))
                return;

            int executionOrder = 0;

            while ((behavioursActions.MainThreadBehaviour != null && behavioursActions.MainThreadBehaviour.Count > 0) ||
                   (behavioursActions.MultithreadablesBehaviour != null &&
                    behavioursActions.MultithreadablesBehaviour.Count > 0))
            {
                Task multithreadbleBehaviour = new Task(() =>
                {
                    if (behavioursActions.MultithreadablesBehaviour != null)
                    {
                        if (behavioursActions.MultithreadablesBehaviour.ContainsKey(executionOrder))
                        {
                            Parallel.ForEach(behavioursActions.MultithreadablesBehaviour[executionOrder],
                                parallelsOptions, (behaviours) => { behaviours?.Invoke(); });
                            behavioursActions.MultithreadablesBehaviour.TryRemove(executionOrder, out _);
                        }
                    }
                });

                multithreadbleBehaviour.Start();

                if (behavioursActions.MainThreadBehaviour != null)
                {
                    if (behavioursActions.MainThreadBehaviour.ContainsKey(executionOrder))
                    {
                        foreach (Action behaviour in behavioursActions.MainThreadBehaviour[executionOrder])
                        {
                            behaviour?.Invoke();
                        }

                        behavioursActions.MainThreadBehaviour.Remove(executionOrder);
                    }
                }

                multithreadbleBehaviour.Wait();

                executionOrder++;
            }

            behavioursActions.TransitionBehaviour?.Invoke();
        }
    }
}