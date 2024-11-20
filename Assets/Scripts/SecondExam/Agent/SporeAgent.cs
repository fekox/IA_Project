﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace Miner.SecondExam.Agent
{
    public abstract class SporeMoveState : State
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

    public abstract class SporeEatState : State
    {
        protected Vector2 position;
        protected Brain brain;
    }

    public abstract class SporeDeadState : State
    {
    }

    public abstract class SporeCorpseState : State
    {
    }

    public abstract class SporeAgent
    {
        public Vector2 position;
    }
    public abstract class SporeAgent<AgentStates,AgentFlags>: SporeAgent where AgentStates :Enum  where AgentFlags : Enum 
    {
        protected SporeManager populationManager;
        public Brain mainBrain;
        public bool hasEaten = false;
        protected bool isActive;
        protected FSM<AgentStates, AgentFlags> fsm;
        public SporeAgent(SporeManager populationManager)
        {
            fsm = new FSM<AgentStates, AgentFlags>();
            this.populationManager = populationManager;
        }

        public abstract void DecideState(float[] outputs);
        public abstract void PreUpdate(float deltaTime);
        public abstract void Update(float deltaTime);
        public abstract void MoveTo(Vector2 dir);
    }
}