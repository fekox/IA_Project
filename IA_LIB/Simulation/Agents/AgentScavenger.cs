using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Numerics;
using IA_Library;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public class AgentScavenger : Agent
    {
        public Brain flockingBrain = new Brain();
        
        public float minEatRadius;


        public Vector2 currentDirection;
        public float speed = 1f;
        public float rotation = 1f;

        public AgentScavenger(Simulation simulation) : base(simulation)
        {
            fsmController.AddBehaviour<MoveToEatScavengerState>(Behaviours.MoveToFood,
            
                onTickParameters: () => 
                { 
                    return new object[] { mainBrain }; 
                }
            );
            
            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        public override void Update(float deltaTime)
        {
            fsmController.Tick();

            MoveTo(currentDirection);

            Rotate();
        }

        public override void ChooseNextState(float[] outputs)
        {
            throw new System.NotImplementedException();
        }

        public override void MoveTo(Vector2 direction)
        {
            direction = Normalize(direction);
            position += direction * speed;
        }

        private void Rotate() 
        {
            rotation %= 360;

            float angleInRadians = rotation * (float)Math.PI / 180f;
            currentDirection = new Vector2((float)Math.Cos(angleInRadians), (float)Math.Sin(angleInRadians));
        }

        private Vector2 Normalize(Vector2 vector)
        {
            float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (magnitude > 0)
            {
                return new Vector2(vector.X / magnitude, vector.Y / magnitude);
            }

            return vector;
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            Vector2 nearFoodPos = GetNearestFoodPosition();
            mainBrain.inputs = new[] { position.X, position.Y, minEatRadius, nearFoodPos.X, nearFoodPos.Y};
        }

        public override Vector2 GetNearestFoodPosition()
        {
            throw new System.NotImplementedException();
        }


        private List<AgentScavenger> GetThreeNearestAgents()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class MoveToEatScavengerState : MoveState
    {
        public override BehavioursActions GetOnEnterBehaviour(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public override BehavioursActions GetTickBehaviour(params object[] parameters)
        {
            BehavioursActions behaviour = new BehavioursActions();

            float[] outputs = parameters[0] as float[];
            position = (Vector2)(parameters[1]);
            Vector2 nearFoodPos = (Vector2)parameters[2];
            float minEatRadius = (float)(parameters[3]);
            
            behaviour.AddMultitreadableBehaviours(0, () =>
            {
                List<Vector2> newPositions = new List<Vector2> { nearFoodPos };
                float distanceFromFood = GetDistanceFrom(newPositions);

                if (distanceFromFood < minEatRadius)
                {
                    brain.FitnessReward += 1;
                
                }
                else if (distanceFromFood > minEatRadius)
                {
                    brain.FitnessMultiplier -= 0.05f;
                }
            });

            return behaviour;
        }

        public override BehavioursActions GetOnExitBehaviour(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}