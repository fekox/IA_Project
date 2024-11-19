using System;
using System.Collections.Generic;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public class AgentHerbivore : Agent
    {
        public Brain moveToFoodBrain;
        public Brain moveToEscapeBrain;
        public Brain eatBrain;

        private int maxMovementPerTurn = 3;

        private int lives = 3;
        private int insideFood;

        public AgentHerbivore()
        {
            maxFood = 5;
            
            fsmController.AddBehaviour<MoveToEatHerbivoreState>(Behaviours.MoveToFood,
                onEnterParameters: () => { return new object[] { moveToFoodBrain }; },
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        moveToFoodBrain.outputs, position, GetNearestFoodPosition()
                    };
                });

            fsmController.AddBehaviour<EatHerbivoreState>(Behaviours.Eat,
                onEnterParameters: () => { return new object[] { eatBrain }; },
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        eatBrain.outputs, position, GetNearestFoodPosition(), GetNearestFood(), hasEaten, currentFood,
                        maxFood
                    };
                });

            fsmController.AddBehaviour<MoveToEscapeHerbivoreState>(Behaviours.MoveEscape,
                onEnterParameters: () => { return new object[] { moveToEscapeBrain }; },
                onTickParameters: () =>
                {
                    return new object[]
                    {
                        moveToEscapeBrain.outputs, position, GetNearestEnemiesPosition()
                    };
                });

            fsmController.AddBehaviour<DeathHerbivoreState>(Behaviours.Death);
            fsmController.AddBehaviour<CorpseHerbivoreState>(Behaviours.Corpse);

            fsmController.SetTransition(Behaviours.MoveToFood, Flags.OnTransitionEat, Behaviours.Eat);
            fsmController.SetTransition(Behaviours.MoveEscape, Flags.OnTransitionEat, Behaviours.Eat);

            fsmController.SetTransition(Behaviours.Eat, Flags.OnTransitionMoveEscape, Behaviours.MoveEscape);
            fsmController.SetTransition(Behaviours.MoveToFood, Flags.OnTransitionMoveEscape, Behaviours.MoveEscape);

            fsmController.SetTransition(Behaviours.Eat, Flags.OnTransitionMoveToEat, Behaviours.MoveToFood);
            fsmController.SetTransition(Behaviours.MoveEscape, Flags.OnTransitionMoveToEat, Behaviours.MoveToFood);
        }

        public override void Update(float deltaTime)
        {
            ChooseNextState(mainBrain.outputs);

            fsmController.Tick();
        }

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

        public override void MoveTo(Vector2 direction)
        {
            position += direction;
        }

        private AgentPlant GetNearestFood()
        {
            //TODO: hacer que busque su comida
            throw new NotImplementedException();
        }

        public override Vector2 GetNearestFoodPosition()
        {
            //TODO: hacer que busque su comida
            throw new NotImplementedException();
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            List<Vector2> enemies = GetNearestEnemiesPosition();
            Vector2 nearestFoodPosition = GetNearestFoodPosition();

            mainBrain.inputs = new[]
            {
                position.X, position.Y, nearestFoodPosition.X, nearestFoodPosition.Y, hasEaten ? 1 : -1,
                enemies[0].X, enemies[0].Y, enemies[1].X, enemies[1].Y, enemies[2].X,
                enemies[2].Y
            };
            moveToFoodBrain.inputs = new[] { position.X, position.Y, nearestFoodPosition.X, nearestFoodPosition.Y };
            eatBrain.inputs = new[]
                { position.X, position.Y, nearestFoodPosition.X, nearestFoodPosition.Y, hasEaten ? 1 : -1 };

            moveToEscapeBrain.inputs = new[]
            {
                position.X, position.Y, enemies[0].X, enemies[0].Y, enemies[1].X, enemies[1].Y, enemies[2].X,
                enemies[2].Y
            };
        }

        private List<Vector2> GetNearestEnemiesPosition()
        {
            //TODO: hacer que busque a los carnivoros
            throw new NotImplementedException();
        }

        public void ReceiveDamage()
        {
            lives--;
            if (lives <= 0)
            {
                fsmController.ForcedState(Behaviours.Death);
            }
        }

        public void EatPiece()
        {
            insideFood--;
            if (insideFood <= 0)
            {
                fsmController.ForcedState(Behaviours.Corpse);
            }
        }

        public bool CanBeEaten()
        {
            if (fsmController.currentState == (int)Behaviours.Death)
            {
                return true;
            }

            return false;
        }

        public HerbivoreStates GetState()
        {
            if (fsmController.currentState == (int)Behaviours.Death)
            {
                return HerbivoreStates.Death;
            }
            else if (fsmController.currentState == (int)Behaviours.Corpse)
            {
                return HerbivoreStates.Corpse;
            }
            else
            {
                return HerbivoreStates.Alive;
            }
        }
    }

    public class MoveToEatHerbivoreState : MoveState
    {
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
                else if (outputs[0] < 0 && outputs[0] < negativeHalf)
                {
                    movementPerTurn = 1;
                }
                else if (outputs[0] < negativeHalf)
                {
                    movementPerTurn = 0;
                }

                Vector2[] direction = new Vector2[movementPerTurn];
                for (int i = 0; i < 3; i++)
                {
                    direction[i] = GetDir(outputs[i + 1]);
                }

                if (movementPerTurn > 0)
                {
                    foreach (Vector2 dir in direction)
                    {
                        //TODO : CHEQUEAR SI ESTAS SALIENDO DEL MAPA
                        position += dir;
                    }

                    List<Vector2> newPositions = new List<Vector2>();
                    newPositions.Add(nearFoodPos);

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
            var onMove = parameters[3] as Action<Vector2[]>;

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
                else if (outputs[0] < 0 && outputs[0] < negativeHalf)
                {
                    movementPerTurn = 1;
                }
                else if (outputs[0] < negativeHalf)
                {
                    movementPerTurn = 0;
                }

                Vector2[] direction = new Vector2[movementPerTurn];
                for (int i = 0; i < 3; i++)
                {
                    direction[i] = GetDir(outputs[i + 1]);
                }

                if (movementPerTurn > 0)
                {
                    foreach (Vector2 dir in direction)
                    {
                        onMove.Invoke(direction);
                        position += dir;
                        //Todo: Make a way to check the limit of the grid
                    }

                    float distanceFromEnemies = GetDistanceFrom(nearEnemyPositions);
                    if (distanceFromEnemies <= previousDistance)
                    {
                        brain.FitnessReward += 20;
                        brain.FitnessMultiplier += 0.05f;
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
            AgentPlant plant = parameters[3] as AgentPlant;
            bool maxEaten = (bool)parameters[4];
            int currentFood = (int)parameters[5];
            int maxEating = (int)parameters[6];

            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                if (plant == null)
                {
                    return;
                }

                if (outputs[0] >= 0f)
                {
                    if (position == nearFoodPos && !maxEaten)
                    {
                        if (plant.CanBeEaten())
                        {
                            plant.Eat();
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

    public class CorpseHerbivoreState : CorpseState
    {
        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}