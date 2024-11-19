using System.Collections.Generic;
using System.Numerics;
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
        OnTransitionCorpse,
    }

    public abstract class Agent
    {
        public Brain mainBrain;
        protected FSM<Behaviours, Flags> fsmController;

        public Vector2 position;

        protected bool hasEaten = false;
        protected int maxFood;
        protected int currentFood = 0;


        public Agent()
        {
            fsmController = new FSM<Behaviours, Flags>();
        }

        public abstract void Update(float deltaTime);
        public abstract void ChooseNextState(float[] outputs);
        public abstract void MoveTo(Vector2 direction);
        public abstract Vector2 GetNearestFoodPosition();
        public abstract void SettingBrainUpdate(float deltaTime);

        public float[] GetMainBrainGenome()
        {
            return mainBrain.outputs;
        }
    }

    public abstract class MoveState : State
    {
        protected Vector2 position;
        protected Brain brain;
        protected float positiveHalf;
        protected float negativeHalf;

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

    public abstract class EatState : State
    {
        protected Vector2 position;
        protected Brain brain;
    }

    public abstract class DeadState : State
    {
    }

    public abstract class CorpseState : State
    {
    }
}