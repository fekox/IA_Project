using System;
using System.Numerics;

namespace IA_Library
{
    /// <summary>
    /// Manager of the map.
    /// </summary>
    public class GridManager
    {
        public Vector2 size = Vector2.One;
        public float cellSize = 1;

        private Random random = new Random();

        /// <summary>
        /// Create the map.
        /// </summary>
        /// <param name="X">X axix</param>
        /// <param name="Y">Y axis</param>
        /// <param name="cellSize">Set the cell size</param>
        public GridManager(int X, int Y, float cellSize)
        {
            size.X = X;
            size.Y = Y;
            this.cellSize = cellSize;
        }

        /// <summary>
        /// Get a random position on the map.
        /// </summary>
        /// <returns>Random pos</returns>
        public Vector2 GetRandomValuePositionGrid()
        {
            int xIndex = random.Next(0, (int)size.X);
            int yIndex = random.Next(0, (int)size.Y);

            float x = xIndex * cellSize + cellSize / 2;
            float y = yIndex * cellSize + cellSize / 2;

            return new Vector2((int)x, (int)y);
        }

        /// <summary>
        /// Gets a new position on the grid.
        /// </summary>
        /// <param name="currentPosition">Current pos</param>
        /// <param name="direction">The direction of the agent</param>
        /// <returns>New pos</returns>
        public Vector2 GetNewPositionInGrid(Vector2 currentPosition, Vector2 direction)
        {
            Vector2 movement = direction * cellSize;
            Vector2 newPosition = currentPosition + movement;

            newPosition.X = (float)Math.Floor(newPosition.X / cellSize) * cellSize;
            newPosition.Y = (float)Math.Floor(newPosition.Y / cellSize) * cellSize;

            if (IsInsideGrid(newPosition))
            {
                return newPosition;
            }

            return GetOpositeSide(newPosition);
        }

        /// <summary>
        /// Teleports the agent to the opposite side of the grid depending on which side he came out from.
        /// </summary>
        /// <param name="position">The position of the agent</param>
        /// <returns>The new pos</returns>
        public Vector2 GetOpositeSide(Vector2 position)
        {

            if (position.X <= 0)
            {
                position.X = size.X - 1;
            }
 
            else if (position.X >= size.X)
            {
                position.X = 0;
            }
            
            if ( position.Y <= 0)
            {
                position.Y = size.Y - 1;
            }
            
            else if (position.Y >= size.Y)
            {
                position.Y = 0;
            }

            return position;
        }

        /// <summary>
        /// Checks if the agent is in the map or not.
        /// </summary>
        /// <param name="position">Agent position</param>
        /// <returns>Is inside or not</returns>
        bool IsInsideGrid(Vector2 position)
        {
            return position.X >= 0 && position.X < size.X && position.Y >= 0 && position.Y < size.Y;
        }
    }
}