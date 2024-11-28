using System;
using System.Collections.Generic;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    /// <summary>
    /// Create the agent carnivore.
    /// </summary>
    public class AgentCarnivore : Agent
    {
        public Brain moveToFoodBrain = new Brain();
        public Brain eatBrain = new Brain();
        int counterEating = 0;
        Vector2 nearestFoodPosition;
        AgentHerbivore herbivore;

        public AgentCarnivore(Simulation simulation, GridManager gridManager,
            Brain mainBrain, Brain moveToFoodBrain, Brain eatBrain) : base(simulation, gridManager, mainBrain)
        {
            Action<bool> onHasEantenEnoughFood;
            Action<Vector2> onMove;
            Action<int> onEaten;

            maxFood = 3;

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
                        moveToFoodBrain.outputs, position, nearestFoodPosition, onMove = MoveTo, herbivore
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
                        eatBrain.outputs, position, nearestFoodPosition,
                        hasEaten, counterEating, maxFood,
                        onHasEantenEnoughFood = b =>
                            hasEaten = b,
                        onEaten = i => counterEating = i,
                        herbivore
                    };
                }
            );

            fsmController.SetTransition(Behaviours.MoveToFood, Flags.OnTransitionEat, Behaviours.Eat);
            fsmController.SetTransition(Behaviours.Eat, Flags.OnTransitionMoveToEat, Behaviours.MoveToFood);

            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        /// <summary>
        /// Update the FSM.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            ChooseNextState(mainBrain.outputs);
            fsmController.Tick();
        }

        /// <summary>
        /// Reset the fitnnes for the brains.
        /// </summary>
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

        /// <summary>
        /// Choose next state.
        /// </summary>
        /// <param name="outputs"></param>
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

        /// <summary>
        /// Move to drection.
        /// </summary>
        /// <param name="direction"></param>
        public override void MoveTo(Vector2 direction)
        {
            position = gridManager.GetNewPositionInGrid(position, direction);
        }

        /// <summary>
        /// Update the brains inputs.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void SettingBrainUpdate(float deltaTime)
        {
            nearestFoodPosition = GetNearestFoodPosition();
            herbivore = GetNearestFood();

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

        /// <summary>
        /// Get nearest food.
        /// </summary>
        /// <returns></returns>
        private AgentHerbivore GetNearestFood()
        {
            return simulation.GetNearestHerbivoreAgent(position);
        }

        /// <summary>
        /// Get nearest food pos.
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetNearestFoodPosition()
        {
            return simulation.GetNearestHerbivorePosition(position);
        }

        /// <summary>
        /// Ser eat state.
        /// </summary>
        /// <param name="state"></param>
        public override void SetEatState(bool state)
        {
            hasEaten = state;
        }

        /// <summary>
        /// Apply the fitnnes in the main brain.
        /// </summary>
        public override void ApplyFitness()
        {
            moveToFoodBrain.ApplyFitness();
            eatBrain.ApplyFitness();
            
            mainBrain.FitnessMultiplier = 1.0f;
            mainBrain.FitnessReward = 0f;
            mainBrain.FitnessReward = eatBrain.FitnessReward + moveToFoodBrain.FitnessReward;
            mainBrain.FitnessMultiplier += eatBrain.FitnessMultiplier + moveToFoodBrain.FitnessMultiplier;
            
            mainBrain.ApplyFitness();
        }
    }

    /// <summary>
    /// Create the move to eat state.
    /// </summary>
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
            Action<Vector2> onMove = parameters[3] as Action<Vector2>;
            AgentHerbivore herbivore = parameters[4] as AgentHerbivore;

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

                foreach (Vector2 direc in direction)
                {
                    onMove?.Invoke(direc);
                }

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

    /// <summary>
    /// Create the eat carnivore state.
    /// </summary>
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
            bool hasEatenEnoughFood = (bool)parameters[3];
            int counterEating = (int)parameters[4];
            int maxEating = (int)parameters[5];
            var onHasEatenEnoughFood = parameters[6] as Action<bool>;
            var onEaten = parameters[7] as Action<int>;
            AgentHerbivore herbivore = parameters[8] as AgentHerbivore;

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                if (outputs[0] >= 0f)
                {
                    if (position == nearFoodPos && !hasEatenEnoughFood)
                    {
                        if (herbivore.CanBeEaten())
                        {
                            herbivore.EatPiece();
                            onEaten(++counterEating);
                            brain.FitnessReward += 30;
                            brain.FitnessMultiplier += 0.30f;
                            
                            if (counterEating >= maxEating)
                            {
                                brain.FitnessReward += 60;
                                brain.FitnessMultiplier += 0.10f;
                                onHasEatenEnoughFood.Invoke(true);
                            }
                        }
                    }
                    
                    else if (hasEatenEnoughFood || position != nearFoodPos)
                    {
                        brain.FitnessMultiplier -= 0.05f;
                    }
                }
                
                else
                {
                    if (position == nearFoodPos && !hasEatenEnoughFood)
                    {
                        brain.FitnessMultiplier -= 0.05f;
                    }
                   
                    else if (hasEatenEnoughFood)
                    {
                        brain.FitnessMultiplier += 0.10f;
                    }
                }
            });
            
            return behaviour;
        }

        /// <summary>
        /// Get on exit behavior and apply fitnnes.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            brain.ApplyFitness();
            
            return default;
        }
    }
}