using System.Collections.Generic;
using System.Numerics;
using IA_Library.Brain;

namespace IA_Library_FSM
{
    public class AgentScavenger : Agent
    {
        public Brain flockingBrain;
        
        public float rotation;
        public float minEatRadius;
        public float speed = 1f;

        public AgentScavenger()
        {
            fsmController.AddBehaviour<MoveToEatScavengerState>(Behaviours.MoveToFood,
                onTickParameters: () => { return new object[] { mainBrain }; });
            
            fsmController.ForcedState(Behaviours.MoveToFood);
        }

        public override void Update(float deltaTime)
        {
            fsmController.Tick();
        }

        public override void ChooseNextState(float[] outputs)
        {
            throw new System.NotImplementedException();
        }

        public override void MoveTo(Vector2 direction)
        {
            Vector2 forwardDirection = new Vector2((float)System.Math.Cos(rotation), (float)System.Math.Sin(rotation));
            position += forwardDirection * speed;
        }

        public override Vector2 GetNearestFoodPosition()
        {
            throw new System.NotImplementedException();
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            Vector2 nearFoodPos = GetNearestFoodPosition();
            mainBrain.inputs = new[] { position.X, position.Y, minEatRadius, nearFoodPos.X, nearFoodPos.Y};
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