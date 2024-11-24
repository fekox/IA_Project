using System;
using System.Collections.Generic;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public class AgentCarnivore : Agent
    {
        public Brain moveToFoodBrain = new Brain();
        public Brain eatBrain = new Brain();

        public AgentCarnivore(Simulation simulation, GridManager gridManager,
            Brain mainBrain, Brain moveToFoodBrain, Brain eatBrain) : base(simulation, gridManager, mainBrain)
        {
            Action<Vector2> onMove;

            this.moveToFoodBrain = moveToFoodBrain;
            this.eatBrain = eatBrain;

            fsmController.AddBehaviour<MoveToEatCarnivoreState>(Behaviours.MoveToFood,
                
                onEnterParameters: () => 
                { 
                    return new object[] { moveToFoodBrain }; 
                },
               
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        moveToFoodBrain.outputs, position, GetNearestFoodPosition(), GetNearestFood(), onMove = MoveTo
                    };
                }
            );

            fsmController.AddBehaviour<EatCarnivoreState>(Behaviours.Eat,
                
                onEnterParameters: () => 
                { 
                    return new object[] { eatBrain }; 
                },
                
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        eatBrain.outputs, position, GetNearestFoodPosition(), GetNearestFood(), hasEaten, currentFood,
                        maxFood
                    };
                }
            );

            fsmController.SetTransition(Behaviours.MoveToFood, Flags.OnTransitionEat, Behaviours.Eat);
            fsmController.SetTransition(Behaviours.Eat, Flags.OnTransitionMoveToEat, Behaviours.MoveToFood);
        }

        public override void Update(float deltaTime)
        {
            ChooseNextState(mainBrain.outputs);
            fsmController.Tick();
        }

        public override void Reset()
        {
            mainBrain.FitnessMultiplier = 1;
            mainBrain.FitnessReward = 0;

            moveToFoodBrain.FitnessMultiplier = 1;
            moveToFoodBrain.FitnessReward = 0;

            eatBrain.FitnessMultiplier = 1;
            eatBrain.FitnessReward = 0;

            currentFood = 0;
            hasEaten = false;
            position = gridManager.GetRandomValuePositionGrid();

            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        public override void ChooseNextState(float[] outputs)
        {
            if (outputs[0] > 0.0f)
            {
                fsmController.Transition(Flags.OnTransitionMoveToEat);
            }

            else if (outputs[1] > 0.0f)
            {
                fsmController.Transition(Flags.OnTransitionEat);
            }
        }

        public override void MoveTo(Vector2 direction)
        {
            position = gridManager.GetNewPositionInGrid(position, direction);
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            Vector2 nearestFoodPosition = GetNearestFoodPosition();

            mainBrain.inputs = new[]
            { 
                position.X, position.Y, nearestFoodPosition.X, nearestFoodPosition.Y, hasEaten ? 1 : -1, 
            };
            
            moveToFoodBrain.inputs = new[] 
            { 
                position.X, position.Y, nearestFoodPosition.X, nearestFoodPosition.Y 
            };
            
            eatBrain.inputs = new[]
            { 
                position.X, position.Y, nearestFoodPosition.X, nearestFoodPosition.Y, hasEaten ? 1 : -1 
            };
        }

        private AgentHerbivore GetNearestFood()
        {
            return simulation.GetNearestHerbivoreAgent(position);
        }

        public override Vector2 GetNearestFoodPosition()
        {
            return simulation.GetNearestHerbivorePosition(position);
        }
    }

    public class MoveToEatCarnivoreState : MoveState
    {
        private int movesPerTurn = 2;
        private float previousDistance;

        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            brain = parameters[0] as Brain;
            positiveHalf = Neuron.Sigmoid(0.5f, brain.p);
            negativeHalf = Neuron.Sigmoid(-0.5f, brain.p);
            
            return default;
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            BehavioursActions behaviour = new BehavioursActions();

            float[] outputs = parameters[0] as float[];
            position = (Vector2)parameters[1];
            Vector2 nearFoodPos = (Vector2)parameters[2];
            AgentHerbivore herbivore = parameters[3] as AgentHerbivore;
            var onMove = parameters[4] as Action<Vector2[]>;

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                if (position == nearFoodPos)
                {
                    herbivore.ReceiveDamage();
                }

                Vector2[] direction = new Vector2[movesPerTurn];
                
                for (int i = 0; i < direction.Length; i++)
                {
                    direction[i] = GetDir(outputs[i]);
                }

                onMove.Invoke(direction);

                List<Vector2> newPositions = new List<Vector2> { nearFoodPos };
                float distanceFromFood = GetDistanceFrom(newPositions);
                
                if (distanceFromFood <= previousDistance)
                {
                    brain.FitnessReward += 20;
                    brain.FitnessMultiplier += 0.05f;
                }

                else
                {
                    brain.FitnessMultiplier -= 0.05f;
                }

                previousDistance = distanceFromFood;
            });
            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            brain.ApplyFitness();
            return default;
        }
    }

    public class EatCarnivoreState : EatState
    {
        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            brain = parameters[0] as Brain;
            return default;
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            BehavioursActions behaviour = new BehavioursActions();

            float[] outputs = parameters[0] as float[];
            position = (Vector2)parameters[1];
            Vector2 nearFoodPos = (Vector2)parameters[2];
            AgentHerbivore herbivore = parameters[3] as AgentHerbivore;
            bool maxEaten = (bool)parameters[4];
            int currentFood = (int)parameters[5];
            int maxEating = (int)parameters[6];

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                if (herbivore == null)
                {
                    return;
                }

                if (outputs[0] >= 0f)
                {
                    if (position == nearFoodPos && !maxEaten)
                    {
                        if (herbivore.CanBeEaten())
                        {
                            herbivore.EatPiece();
                            currentFood++;

                            brain.FitnessReward += 20;

                            if (currentFood == maxEating)
                            {
                                brain.FitnessReward += 30;
                            }
                        }
                    }

                    else if (maxEaten || position != nearFoodPos)
                    {
                        brain.FitnessMultiplier -= 0.05f;
                    }
                }

                else
                {
                    if (position == nearFoodPos && !maxEaten)
                    {
                        brain.FitnessMultiplier -= 0.05f;
                    }

                    else if (maxEaten)
                    {
                        brain.FitnessMultiplier += 0.10f;
                    }
                }
            });

            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            brain.ApplyFitness();
            return default;
        }
    }
}