using System;
using System.Collections.Generic;
using IA_Library;
using IA_Library.Brain;
using Vector2 = System.Numerics.Vector2;

namespace IA_Library_FSM
{
    /// <summary>
    /// Create the agent hebivore.
    /// </summary>
    public class AgentHerbivore : Agent
    {
        public Brain moveToFoodBrain;
        public Brain moveToEscapeBrain;
        public Brain eatBrain;

        private int maxMovementPerTurn = 3;
        private int livesUntilCountdownDissapears = 30;

        public int lives = 3;

        private List<Vector2> nearEnemy = new List<Vector2>();
        private List<Vector2> nearFood = new List<Vector2>();
        private List<Vector2> nearEnemiesPositions;
        private Vector2 nearFoodPosition;
        private AgentPlant nearestFood;

        public AgentHerbivore(Simulation simulation, GridManager gridManager,
            Brain mainBrain, Brain moveToFoodBrain, Brain moveToEscapeBrain, Brain eatBrain) : base(simulation,
            gridManager, mainBrain)
        {
            maxFood = 2;

            this.moveToFoodBrain = moveToFoodBrain;
            this.moveToEscapeBrain = moveToEscapeBrain;
            this.eatBrain = eatBrain;

            Action<Vector2> onMove;
            Action<bool> onEatenFood;
            Action<int> onEat;
            onMove = MoveTo;

            fsmController.AddBehaviour<MoveToEatHerbivoreState>(Behaviours.MoveToFood,
                
                onEnterParameters: () => 
                { 
                    return new object[] { moveToFoodBrain }; 
                },
                
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        moveToFoodBrain.outputs, position, nearFoodPosition, onMove, nearestFood,
                    };
                }
            );

            fsmController.AddBehaviour<EatHerbivoreState>(Behaviours.Eat,
                
                onEnterParameters: () => 
                { 
                    return new object[] { eatBrain }; 
                },
                
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        eatBrain.outputs, position, nearFoodPosition, hasEaten, currentFood, maxFood,
                        onEatenFood = b => { hasEaten = b; }, onEat = a => currentFood = a, nearestFood,
                    };
                }
            );

            fsmController.AddBehaviour<MoveToEscapeHerbivoreState>(Behaviours.MoveEscape,
                
                onEnterParameters: () => 
                { 
                    return new object[] { moveToEscapeBrain }; 
                },
                
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        moveToEscapeBrain.outputs, position, nearEnemiesPositions, onMove = MoveTo
                    };
                }
            );

            fsmController.AddBehaviour<DeathHerbivoreState>(Behaviours.Death,
            onTickParameters: () => { return new object[] { lives }; });
            
            fsmController.AddBehaviour<CorpseHerbivoreState>(Behaviours.Corpse);

            fsmController.SetTransition(Behaviours.MoveToFood, Flags.OnTransitionEat, Behaviours.Eat);
            fsmController.SetTransition(Behaviours.MoveEscape, Flags.OnTransitionEat, Behaviours.Eat);

            fsmController.SetTransition(Behaviours.Eat, Flags.OnTransitionMoveEscape, Behaviours.MoveEscape);
            fsmController.SetTransition(Behaviours.MoveToFood, Flags.OnTransitionMoveEscape, Behaviours.MoveEscape);

            fsmController.SetTransition(Behaviours.Eat, Flags.OnTransitionMoveToEat, Behaviours.MoveToFood);
            fsmController.SetTransition(Behaviours.MoveEscape, Flags.OnTransitionMoveToEat, Behaviours.MoveToFood);

            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        /// <summary>
        /// Update the FSM states.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            ChooseNextState(mainBrain.outputs);
            fsmController.Tick();
        }

        /// <summary>
        /// Reset the fitnnes.
        /// </summary>
        public override void Reset()
        {
            mainBrain.FitnessMultiplier = 1;
            mainBrain.FitnessReward = 0;

            moveToFoodBrain.FitnessMultiplier = 1;
            moveToFoodBrain.FitnessReward = 0;

            moveToEscapeBrain.FitnessMultiplier = 1;
            moveToEscapeBrain.FitnessReward = 0;

            eatBrain.FitnessMultiplier = 1;
            eatBrain.FitnessReward = 0;

            lives = 3;
            currentFood = 0;
            hasEaten = false;
            position = gridManager.GetRandomValuePositionGrid();

            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        /// <summary>
        /// Chose next state.
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
                fsmController.Transition(Flags.OnTransitionMoveEscape);
            }
            
            else if (outputs[2] > 0.0f)
            {
                fsmController.Transition(Flags.OnTransitionEat);
            }
        }

        /// <summary>
        /// Move to direction.
        /// </summary>
        /// <param name="direction"></param>
        public override void MoveTo(Vector2 direction)
        {
            position = gridManager.GetNewPositionInGrid(position, direction);
        }

        /// <summary>
        /// Get nearest food.
        /// </summary>
        /// <returns></returns>
        private AgentPlant GetNearestFood()
        {
            return simulation.GetNearestPlantAgents(position);
        }

        /// <summary>
        /// Get nearest food position.
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetNearestFoodPosition()
        {
            return simulation.GetNearestPlantPosition(position);
        }

        /// <summary>
        /// Get neares enemy posirion.
        /// </summary>
        /// <returns></returns>
        private List<Vector2> GetNearestEnemiesPosition()
        {
            return simulation.GetNearestCarnivoresPositions(position, 3);
        }

        /// <summary>
        /// Set the inputs of the brains.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void SettingBrainUpdate(float deltaTime)
        {
            nearFoodPosition = GetNearestFoodPosition();
            nearestFood = GetNearestFood();
            nearEnemiesPositions = GetNearestEnemiesPosition();

            mainBrain.inputs = new[]
            {
                position.X, position.Y, nearFoodPosition.X, nearFoodPosition.Y, hasEaten ? 1 : -1,
                nearEnemiesPositions[0].X, nearEnemiesPositions[0].Y,
                nearEnemiesPositions[1].X, nearEnemiesPositions[1].Y,
                nearEnemiesPositions[2].X, nearEnemiesPositions[2].Y
            };
            
            moveToFoodBrain.inputs = new[] 
            { 
                position.X, position.Y, nearFoodPosition.X, nearFoodPosition.Y 
            };
            
            eatBrain.inputs = new[]
            { 
                position.X, position.Y, nearFoodPosition.X, nearFoodPosition.Y, hasEaten ? 1 : -1 
            };

            moveToEscapeBrain.inputs = new[]
            {
                position.X, position.Y,
                nearEnemiesPositions[0].X, nearEnemiesPositions[0].Y,
                nearEnemiesPositions[1].X, nearEnemiesPositions[1].Y,
                nearEnemiesPositions[2].X, nearEnemiesPositions[2].Y
            };
        }

        /// <summary>
        /// Recibe damage.
        /// </summary>
        public void ReceiveDamage()
        {
            lives--;

            if (lives <= 0)
            {
                fsmController.ForcedState(Behaviours.Death);
            }
        }

        /// <summary>
        /// Eat a piece.
        /// </summary>
        public void EatPiece()
        {
            fsmController.ForcedState(Behaviours.Corpse);
        }

        /// <summary>
        /// Returns if the entity can be eating or not.
        /// </summary>
        /// <returns></returns>
        public bool CanBeEaten()
        {
            return fsmController.currentState == (int)Behaviours.Death;
        }

        /// <summary>
        /// Return if the entity if corpse or not.
        /// </summary>
        /// <returns></returns>
        public bool IsCorpse()
        {
            return fsmController.currentState == (int)Behaviours.Corpse;
        }

        /// <summary>
        /// Sets eat state.
        /// </summary>
        /// <param name="state"></param>
        public override void SetEatState(bool state)
        {
            hasEaten = state;
        }

        /// <summary>
        /// Apply fitnnes.
        /// </summary>
        public override void ApplyFitness()
        {
            moveToFoodBrain.ApplyFitness();
            moveToEscapeBrain.ApplyFitness();
            eatBrain.ApplyFitness();

            mainBrain.FitnessMultiplier = 1.0f;
            mainBrain.FitnessReward = 0f;
            
            mainBrain.FitnessReward += eatBrain.FitnessReward + moveToFoodBrain.FitnessReward + moveToEscapeBrain.FitnessReward;
            
            mainBrain.FitnessMultiplier += eatBrain.FitnessMultiplier + moveToFoodBrain.FitnessMultiplier +
                                           moveToEscapeBrain.FitnessMultiplier;
            
            mainBrain.ApplyFitness();
        }
    }

    /// <summary>
    /// Create the move to eat state.
    /// </summary>
    public class MoveToEatHerbivoreState : MoveState
    {
        List<Vector2> nearEnemyPositions = new List<Vector2>();
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
            var onMove = parameters[3] as Action<Vector2>;
            AgentPlant plant = parameters[4] as AgentPlant;

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                positiveHalf = Neuron.Sigmoid(0.5f, brain.p);
                negativeHalf = Neuron.Sigmoid(-0.5f, brain.p);

                int movementPerTurn = 0;

                if (outputs[0] > positiveHalf)
                {
                    movementPerTurn = 3;
                }
                
                else if (outputs[0] < positiveHalf && outputs[0] > 0)
                {
                    movementPerTurn = 2;
                }
                
                else if (outputs[0] < 0 && outputs[0] > negativeHalf)
                {
                    movementPerTurn = 1;
                }
                
                else if (outputs[0] < negativeHalf)
                {
                    movementPerTurn = 0;
                }

                Vector2[] direction = new Vector2[movementPerTurn];
                
                for (int i = 0; i < movementPerTurn; i++)
                {
                    direction[i] = GetDir(outputs[i + 1]);
                }

                if (movementPerTurn > 0)
                {
                    foreach (Vector2 direc in direction)
                    {
                        onMove.Invoke(direc);
                    }

                    List<Vector2> newPositions = new List<Vector2>();
                    newPositions.Add(nearFoodPos);

                    float distanceFromFood = GetDistanceFrom(newPositions);
                    
                    if (distanceFromFood <= previousDistance)
                    {
                        brain.FitnessReward += 20;
                        brain.FitnessMultiplier += 0.10f;
                    }
                
                    else
                    {
                        brain.FitnessMultiplier -= 0.05f;
                    }

                    previousDistance = distanceFromFood;
                }
            });
            
            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }

    /// <summary>
    /// Create the move to scape state.
    /// </summary>
    public class MoveToEscapeHerbivoreState : MoveState
    {
        List<Vector2> nearEnemyPositions = new List<Vector2>();
        float positiveHalf;
        float negativeHalf;
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
            nearEnemyPositions = parameters[2] as List<Vector2>;
            var onMove = parameters[3] as Action<Vector2>;

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                positiveHalf = Neuron.Sigmoid(0.5f, brain.p);
                negativeHalf = Neuron.Sigmoid(-0.5f, brain.p);

                int movementPerTurn = 0;

                if (outputs[0] > positiveHalf)
                {
                    movementPerTurn = 3;
                }
                
                else if (outputs[0] < positiveHalf && outputs[0] > 0)
                {
                    movementPerTurn = 2;
                }
                
                else if (outputs[0] < 0 && outputs[0] > negativeHalf)
                {
                    movementPerTurn = 1;
                }
                
                else if (outputs[0] < negativeHalf)
                {
                    movementPerTurn = 0;
                }

                Vector2[] direction = new Vector2[movementPerTurn];
                
                for (int i = 0; i < movementPerTurn; i++)
                {
                    direction[i] = GetDir(outputs[i + 1]);
                }

                if (movementPerTurn > 0)
                {
                    foreach (Vector2 direc in direction)
                    {
                        onMove.Invoke(direc);
                    }

                    float distanceFromEnemies = GetDistanceFrom(nearEnemyPositions);
                    if (distanceFromEnemies <= previousDistance)
                    {
                        brain.FitnessReward += 20;
                        brain.FitnessMultiplier += 0.10f;
                    }
                
                    else
                    {
                        brain.FitnessMultiplier -= 0.05f;
                    }

                    previousDistance = distanceFromEnemies;
                }
            });
            
            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }

    /// <summary>
    /// Create the eat hebivore state.
    /// </summary>
    public class EatHerbivoreState : EatState
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
            AgentPlant plant = parameters[8] as AgentPlant;

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                if (plant == null)
                {
                    return;
                }

                if (outputs[0] >= 0f)
                {
                    if (position == nearFoodPos && !hasEatenEnoughFood)
                    {
                        if (plant.CanBeEaten())
                        {
                            plant.Eat();
                            onEaten(++counterEating);
                            brain.FitnessReward += 20;
                    
                            if (counterEating == maxEating)
                            {
                                brain.FitnessReward += 30;
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

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            brain.ApplyFitness();
            
            return default;
        }
    }

    /// <summary>
    /// Create the dead herbivore state.
    /// </summary>
    public class DeathHerbivoreState : DeadState
    {
        private int lives;

        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            BehavioursActions behaviour = new BehavioursActions();

            lives = (int)parameters[0];

            behaviour.SetTransitionBehaviour(() =>
            {
                if (lives <= 0)
                {
                    OnFlag.Invoke(Flags.OnTransitionCorpse);
                }
            });

            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }

    /// <summary>
    /// Create the corpse herbivore state.
    /// </summary>
    public class CorpseHerbivoreState : CorpseState
    {
        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}