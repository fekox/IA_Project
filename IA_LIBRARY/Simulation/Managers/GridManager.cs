using System;
using System.Numerics;

namespace IA_Library
{
    public class GridManager
    {
        public Vector2 size = Vector2.One;
        public float cellSize = 1;

        private Random random = new Random();

        public GridManager(int X, int Y, float cellSize)
        {
            size.X = X;
            size.Y = Y;
            this.cellSize = cellSize;
        }

        public Vector2 GetRandomValuePositionGrid()
        {
            int xIndex = random.Next(0, (int)size.X);
            int yIndex = random.Next(0, (int)size.Y);

            float x = xIndex * cellSize + cellSize / 2;
            float y = yIndex * cellSize + cellSize / 2;

            return new Vector2((int)x, (int)y);
        }

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

        bool IsInsideGrid(Vector2 position)
        {
            return position.X >= 0 && position.X < size.X && position.Y >= 0 && position.Y < size.Y;
        }
    }
}