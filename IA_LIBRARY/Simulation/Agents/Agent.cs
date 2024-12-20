using System.Collections.Generic;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public enum Behaviours
    {
        MoveToFood,
        MoveEscape,
        Eat,
        Death,
        Corpse
    }

    public enum Flags
    {
        None,
        OnTransitionMoveToEat,
        OnTransitionMoveEscape,
        OnTransitionEat,
        OnTransitionDeath,
        OnTransitionCorpse
    }

    /// <summary>
    /// Create the agent.
    /// </summary>
    [System.Serializable]
    public abstract class Agent
    {
        public Brain mainBrain;
        protected FSM<Behaviours, Flags> fsmController;
        protected Simulation simulation;
        protected GridManager gridManager;

        public Vector2 position;

        public bool hasEaten = false;
        public int maxFood;
        public int currentFood = 0;

        public Agent(Simulation simulation, GridManager gridManager, Brain mainBrain)
        {
            fsmController = new FSM<Behaviours, Flags>();

            this.simulation = simulation;
            this.gridManager = gridManager;

            position = Vector2.Zero;

            this.mainBrain = mainBrain;
        }

        public abstract void Reset();
        public abstract void Update(float deltaTime);
        public abstract void ChooseNextState(float[] outputs);
        public abstract void MoveTo(Vector2 direction);
        public abstract Vector2 GetNearestFoodPosition();
        public abstract void SettingBrainUpdate(float deltaTime);
        public abstract void SetEatState(bool state);
        public abstract void ApplyFitness();
        public float[] GetMainBrainGenome()
        {
            return mainBrain.outputs;
        }
    }

    /// <summary>
    /// The move state for the agent.
    /// </summary>
    public abstract class MoveState : State
    {
        protected Vector2 position;
        protected Brain brain;
        protected float positiveHalf;
        protected float negativeHalf;

        /// <summary>
        /// Get the direction in X.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected Vector2 GetDir(float x)
        {
            Vector2 dir = new Vector2();
            
            if (x > positiveHalf)
            {
                dir = new Vector2(1, 0);
            }
            
            else if (x < positiveHalf && x > 0)
            {
                dir = new Vector2(-1, 0);
            }
            
            else if (x < 0 && x < negativeHalf)
            {
                dir = new Vector2(0, 1);
            }
            
            else if (x < negativeHalf)
            {
                dir = new Vector2(0, -1);
            }

            return dir;
        }

        /// <summary>
        /// Get the distance from a position.
        /// </summary>
        /// <param name="enemies"></param>
        /// <returns></returns>
        protected float GetDistanceFrom(List<Vector2> enemies)
        {
            float distance = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                float newDistance = Vector2.Distance(position, enemy);
                
                if (distance > newDistance)
                {
                    distance = newDistance;
                }
            }

            return distance;
        }
    }

    /// <summary>
    /// Crate the eat state.
    /// </summary>
    public abstract class EatState : State
    {
        protected Vector2 position;
        protected Brain brain;
    }

    /// <summary>
    /// Create the dead state.
    /// </summary>
    public abstract class DeadState : State
    {
    }

    /// <summary>
    /// Create the corpse state.
    /// </summary>
    public abstract class CorpseState : State
    {
    }
}