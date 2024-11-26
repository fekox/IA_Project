using System.Numerics;
using IA_Library;

namespace IA_Library_FSM
{
    public class AgentPlant : Agent
    {
        private int lives = 5;
        private bool isAlive = true;

        public AgentPlant(Simulation simulation, GridManager gridManager) : base(simulation, gridManager, null)
        {
        }
        
        public void Eat()
        {
            if (isAlive)
            {
                lives--;

                if (lives <= 0)
                {
                    isAlive = false;
                }
            }
        }

        public bool CanBeEaten()
        {
            return lives > 0;
        }

        public override void Reset()
        {
            position = gridManager.GetRandomValuePositionGrid();;
        }

        public override void Update(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public override void ChooseNextState(float[] outputs)
        {
            throw new System.NotImplementedException();
        }

        public override void MoveTo(Vector2 direction)
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 GetNearestFoodPosition()
        {
            throw new System.NotImplementedException();
        }

        public override void SettingBrainUpdate(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public override void SetEatState(bool state)
        {
            throw new System.NotImplementedException();
        }
    }
}