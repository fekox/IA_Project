using System;
using System.Collections.Generic;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public class AgentScavenger : Agent
    {
        public Brain flockingBrain;
        float minEatRadius;
        public  Vector2 dir = new Vector2(1, 1);
        protected Vector2 nearFoodPos;
        protected AgentHerbivore nearHerbivore;
        protected List<AgentScavenger> nearBudis = new List<AgentScavenger>();
        public float rotation = 0;
        public  float speed = 5;
        protected float radius = 2;
        private float deltaTime = 0;

        public AgentScavenger(Simulation simulation, GridManager gridManager, Brain mainBrain, Brain flockingBrain) :
            base(simulation,
                gridManager, mainBrain)
        {
            this.flockingBrain = flockingBrain;
            minEatRadius = 4f;

            Action<Vector2> setDir;
            Action<int> setEatingCounter;

            fsmController.AddBehaviour<MoveToEatScavengerState>(Behaviours.MoveToFood,
                
                onEnterParameters: () => 
                { 
                    return new object[] { mainBrain, position, minEatRadius, flockingBrain }; 
                },
             
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        mainBrain.outputs, position, nearFoodPos, minEatRadius, hasEaten, nearHerbivore,
                        setDir = MoveTo, currentFood, setEatingCounter = b => currentFood = b, dir, rotation, speed,
                        radius, nearBudis, deltaTime
                    };
                }
            );

            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        public override void Update(float deltaTime)
        {
            fsmController.Tick();
            Move(deltaTime);
        }

        public override void Reset()
        {
            hasEaten = false;
            position = gridManager.GetRandomValuePositionGrid();
            currentFood = 0;
            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        public override void ChooseNextState(float[] outputs)
        {
            throw new NotImplementedException();
        }

        public override void MoveTo(Vector2 direction)
        {
            dir = direction;
        }

        public void Move(float deltaTime)
        {
            position += dir * speed * deltaTime;
            position = gridManager.GetOpositeSide(position);
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            this.deltaTime = deltaTime;
            nearFoodPos = GetNearestFoodPosition();

            mainBrain.inputs = new[] { position.X, position.Y, minEatRadius, nearFoodPos.X, nearFoodPos.Y };

            nearBudis = GetNearestAgents();
            nearHerbivore = GetNearestFoodAgent();

            flockingBrain.inputs = new[]
            {
                position.X, position.Y,
                nearBudis[0].position.X, nearBudis[0].position.Y,
                nearBudis[1].position.X, nearBudis[1].position.Y,
                nearBudis[0].rotation, nearBudis[1].rotation
            };
        }

        public override Vector2 GetNearestFoodPosition()
        {
            return simulation.GetNearestDeadHerbivorePosition(position) ??
                   simulation.GetNearestHerbivorePosition(position);
        }

        public AgentHerbivore GetNearestFoodAgent()
        {
            return simulation.GetNearestHerbivoreAgent(position);
        }

        private List<AgentScavenger> GetNearestAgents()
        {
            return simulation.GetNearestScavengers(position, 3);
        }

        public override void SetEatState(bool state)
        {
            hasEaten = state;
        }

        public override void ApplyFitness()
        {
            flockingBrain.ApplyFitness();

            mainBrain.FitnessMultiplier = 0f;
            mainBrain.FitnessReward = 0f;
            mainBrain.FitnessReward += flockingBrain.FitnessReward + (hasEaten ? flockingBrain.FitnessReward : 0);
            mainBrain.FitnessMultiplier += flockingBrain.FitnessMultiplier + (hasEaten ? 1 : 0);

            mainBrain.ApplyFitness();
        }
    }

    public class MoveToEatScavengerState : MoveState
    {
        private float MinEatRadius;
        private int counter;
        private Vector2 dir;
        private float speed;
        private float radius;
        private Brain flockingBrain;
        
        Vector2 Aligment;
        Vector2 Cohesion;
        Vector2 Separation;
        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            brain = parameters[0] as Brain;
            position = (Vector2)(parameters[1]);
            MinEatRadius = (float)(parameters[2]);
            positiveHalf = Neuron.Sigmoid(0.5f, brain.p);
            negativeHalf = Neuron.Sigmoid(-0.5f, brain.p);
            flockingBrain = parameters[3] as Brain;

            return default;
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            BehavioursActions behaviour = new BehavioursActions();

            float[] outputs = parameters[0] as float[];
            position = (Vector2)(parameters[1]);
            Vector2 nearFoodPos = (Vector2)parameters[2];
            MinEatRadius = (float)(parameters[3]);
            bool hasEatenFood = (bool)parameters[4];
            AgentHerbivore herbivore = parameters[5] as AgentHerbivore;
            var onMove = parameters[6] as Action<Vector2>;
            counter = (int)parameters[7];
            var onEat = parameters[8] as Action<int>;
            dir = (Vector2)parameters[9];
            float rotation = (float)(parameters[10]);
            speed = (float)(parameters[11]);
            radius = (float)(parameters[12]);
            List<AgentScavenger> nearScavengers = parameters[13] as List<AgentScavenger>;
            float deltaTime = (float)parameters[14];

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                List<Vector2> newPositions = new List<Vector2> { nearFoodPos };
                float distanceFromFood = GetDistanceFrom(newPositions);

                if (distanceFromFood < MinEatRadius && !hasEatenFood)
                {
                    counter++;
                    onEat.Invoke(counter);
                    brain.FitnessReward += 1;

                    if (counter >= 20)
                    {
                        brain.FitnessReward += 20;
                        brain.FitnessMultiplier += 0.10f;
                        hasEatenFood = true;
                    }
                }
                
                else if (distanceFromFood > MinEatRadius)
                {
                    brain.FitnessMultiplier -= 0.05f;
                }

                float leftValue = outputs[0];
                float rightValue = outputs[1];

                float netRotationValue = leftValue - rightValue;
                float turnAngle = netRotationValue * MathF.PI / 180;

                var rotationMatrix = new Matrix3x2(
                    MathF.Cos(turnAngle), MathF.Sin(turnAngle),
                    -MathF.Sin(turnAngle), MathF.Cos(turnAngle),
                    0, 0
                );

                dir = Vector2.Transform(dir, rotationMatrix);
                dir = Vector2.Normalize(dir);
                
                rotation += netRotationValue;

                rotation = (rotation + 360) % 360;
            });

            behaviour.AddMultitreadableBehaviours(1, () =>
            {
                Aligment = new Vector2(flockingBrain.outputs[0], flockingBrain.outputs[1]);
                Aligment = Vector2.Normalize(Aligment);
                Cohesion  = new Vector2(flockingBrain.outputs[2], flockingBrain.outputs[3]);
                Cohesion = Vector2.Normalize(Cohesion);
                Separation = new Vector2(flockingBrain.outputs[4], flockingBrain.outputs[5]);
                Separation = Vector2.Normalize(Separation);
                float aligmentWeight = 2;
                float cohesionWeight = 2;
                float separationWeight = 2;
                Vector2 flokingInfluence = Aligment * aligmentWeight + Cohesion * cohesionWeight +
                                           Separation * separationWeight + dir;

                Vector2 finalDirection = Vector2.Normalize(flokingInfluence);
                onMove.Invoke(finalDirection);
                position += finalDirection * speed * deltaTime;

        
                //Alignment
                if (AlignmentCalc(nearScavengers) != Aligment)
                {
                    flockingBrain.FitnessMultiplier -= 0.05f;
                }
                
                else
                {
                    flockingBrain.FitnessReward += 1;
                }

                //Cohesion
                if (CohesionCalc(nearScavengers) != Cohesion)
                {
                    flockingBrain.FitnessMultiplier -= 0.05f;
                }
                
                else
                {
                    flockingBrain.FitnessReward += 1;
                }

                //Separation
                if (SeparationCalc(nearScavengers) != Separation)
                {
                    flockingBrain.FitnessMultiplier -= 0.05f;
                }

                else
                {
                    flockingBrain.FitnessReward += 1;
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
        
        public Vector2 AlignmentCalc(List<AgentScavenger> scavengers)
        {
            Vector2 avg = Vector2.Zero;
            
            foreach (AgentScavenger b in scavengers)
            {
                avg += b.dir * b.speed;
            }
            
            avg /= scavengers.Count;
            avg = Vector2.Normalize(avg);
            
            return avg;
        }

        public Vector2 CohesionCalc(List<AgentScavenger> scavengers)
        {
            Vector2 avg = Vector2.Zero;
         
            foreach (AgentScavenger b in scavengers)
            {
                avg += b.position;
            }
            
            avg /= scavengers.Count;
            avg = (avg - position);
            avg = Vector2.Normalize(avg);
            
            return avg;
        }

        public Vector2 SeparationCalc(List<AgentScavenger> scavengers)
        {
            Vector2 avg = Vector2.Zero;
            
            foreach (AgentScavenger b in scavengers)
            {
                avg += (b.position - position);
            }
            
            avg /= scavengers.Count;
            avg *= -1;
            avg = Vector2.Normalize(avg);
            
            return avg;
        }
    }
}