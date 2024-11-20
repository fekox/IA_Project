using System;
using System.Collections.Generic;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public class AgentScavenger : Agent
    {
        public Brain flockingBrain = new Brain();

        public Vector2 Direction;
        public float speed;
        public float rotation;

        public float radius;

        public float minEatRadius;

        public AgentScavenger(Simulation simulation, GridManager gridManager) : base(simulation, gridManager)
        {
            Action<Vector2> onMove;

            speed = 5;
            radius = 1;

            fsmController.AddBehaviour<MoveToEatScavengerState>(Behaviours.MoveToFood,
                onEnterParameters: () => { return new object[] { mainBrain, flockingBrain }; },
                
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        mainBrain.outputs, flockingBrain.outputs, rotation, (gridManager.cellSize * 4), position,
                        Direction, radius, speed, GetNearestFoodPosition(), GetNearestFoodAgent(), GetNearestAgents(),
                        onMove = MoveTo
                    };
                });

            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        public override void Update(float deltaTime)
        {
            fsmController.Tick();
            MoveTo(Direction);
        }

        public override void ChooseNextState(float[] outputs)
        {
            throw new System.NotImplementedException();
        }

        public override void MoveTo(Vector2 direction)
        {
            position = direction;
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            Vector2 nearFoodPos = GetNearestFoodPosition();
            mainBrain.inputs = new[] { position.X, position.Y, minEatRadius, nearFoodPos.X, nearFoodPos.Y };
        }

        public override Vector2 GetNearestFoodPosition()
        {
            return simulation.GetNearestDeadHerbivorePosition(position);
        }

        public AgentHerbivore GetNearestFoodAgent()
        {
            return simulation.GetNearestHerbivoreAgent(position);
        }

        private List<AgentScavenger> GetNearestAgents()
        {
            return simulation.GetNearestScavengers(position, 3);
        }
    }

    public class MoveToEatScavengerState : MoveState
    {
        public Brain flockingBrain;
        private Vector2 direction;

        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            brain = parameters[0] as Brain;
            flockingBrain = parameters[1] as Brain;

            return default;
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            BehavioursActions behaviour = new BehavioursActions();

            float[] outputsMove = parameters[0] as float[];
            float[] outputsFlocking = parameters[1] as float[];

            float rotation = (float)(parameters[2]);
            float minEatRadius = (float)(parameters[3]);

            position = (Vector2)(parameters[4]);
            direction = (Vector2)(parameters[5]);
            float radius = (float)parameters[6];
            float speed = (float)(parameters[7]);

            Vector2 nearFoodPos = (Vector2)parameters[8];
            AgentHerbivore agentDeadHerbivore = (AgentHerbivore)parameters[9];

            List<AgentScavenger> nearScavengers = (List<AgentScavenger>)parameters[10];

            var onMove = parameters[11] as Action<Vector2[]>;

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                float leftValue = outputsMove[0];
                float rightValue = outputsMove[1];

                float netRotationValue = leftValue - rightValue;
                float turnAngle = netRotationValue * MathF.PI / 180;

                var rotationMatrix = new Matrix3x2
                (
                    MathF.Cos(turnAngle), MathF.Sin(turnAngle),
                    -MathF.Sin(turnAngle), MathF.Cos(turnAngle),
                    0, 0
                );

                direction = Vector2.Transform(direction, rotationMatrix);
                direction = Vector2.Normalize(direction);
                rotation += netRotationValue;

                rotation = (rotation + 360) % 360;
            });

            behaviour.AddMultitreadableBehaviours(1, () =>
            {
                Vector2 flokingInfluence = direction * (outputsFlocking[0] + outputsFlocking[1] + outputsFlocking[2]);

                Vector2 finalDirection = direction + flokingInfluence;

                finalDirection = Vector2.Normalize(finalDirection);

                Vector2 finalPosition = position + finalDirection * speed;

                Vector2[] FinalPosition = new[] { finalPosition };

                onMove.Invoke(FinalPosition);
            });

            behaviour.AddMultitreadableBehaviours(2, () =>
            {
                float distanceFromFood = Vector2.Distance(position, nearFoodPos);

                if (distanceFromFood < minEatRadius)
                {
                    brain.FitnessReward += 1;
                    agentDeadHerbivore.EatPiece();
                }
                
                else if (distanceFromFood > minEatRadius)
                {
                    brain.FitnessMultiplier -= 0.05f;
                }

                foreach (AgentScavenger scavenger in nearScavengers)
                {
                    float diff = MathF.Abs(rotation - scavenger.rotation);

                    if (diff > 180) 
                    {
                        diff = 360 - diff;
                    }

                    if (diff > 90)
                    {
                        flockingBrain.FitnessMultiplier -= 0.05f;
                    }
                    
                    else
                    {
                        flockingBrain.FitnessReward += 1;
                    }

                    if (Vector2.Distance(position, scavenger.position) > radius * 6)
                    {
                        flockingBrain.FitnessMultiplier -= 0.05f;
                    }
                    
                    else
                    {
                        flockingBrain.FitnessReward += 1;
                    }

                    if (Vector2.Distance(position, scavenger.position) < radius * 2)
                    {
                        flockingBrain.FitnessMultiplier -= 0.05f;
                    }
                    
                    else
                    {
                        flockingBrain.FitnessReward += 1;
                    }

                    if (distanceFromFood < minEatRadius)
                    {
                        flockingBrain.FitnessReward += 1;
                    }
                    
                    else if (distanceFromFood > minEatRadius)
                    {
                        flockingBrain.FitnessMultiplier -= 0.05f;
                    }
                }
            });

            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            brain.ApplyFitness();
            flockingBrain.ApplyFitness();

            return default;
        }
    }
}