using System.Numerics;
using IA_Library;

namespace IA_Library_FSM
{
    /// <summary>
    /// Create the agent plant.
    /// </summary>
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

        /// <summary>
        /// Return if the agent can be eaten or not.
        /// </summary>
        /// <returns></returns>
        public bool CanBeEaten()
        {
            return lives > 0;
        }

        /// <summary>
        /// Reset the lives.
        /// </summary>
        public override void Reset()
        {
            position = gridManager.GetRandomValuePositionGrid();;
            lives = 5;
            isAlive = true;
        }

        /// <summary>
        /// Update the agent.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Update(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Chose next state.
        /// </summary>
        /// <param name="outputs"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void ChooseNextState(float[] outputs)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Move to direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void MoveTo(Vector2 direction)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get nearest food.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Vector2 GetNearestFoodPosition()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Setting the inputs of the brains.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SettingBrainUpdate(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Set eat state.
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SetEatState(bool state)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Apply fitnnes.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void ApplyFitness()
        {
            throw new System.NotImplementedException();
        }
    }
}