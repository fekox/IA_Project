using System;
using System.Numerics;

namespace IA_Library
{
    public class GridManager
    {
        public Vector2 size = Vector2.One;
        public float cellSize = 1;

        private Random random = new Random();

        public GridManager(Vector2 size, float cellSize)
        {
            this.size = size;
            this.cellSize = cellSize;
        }

        public Vector2 GetRandomValuePositionGrid()
        {
            float x = (float)(random.NextDouble() * size.X);
            float y = (float)(random.NextDouble() * size.Y);

            x = (float)Math.Floor(x / cellSize) * cellSize;
            y = (float)Math.Floor(y / cellSize) * cellSize;

            return new Vector2(x, y);
        }

        public Vector2 GetNewPositionInGrid(Vector2 currentPosition, Vector2 direction)
        {
            Vector2 movement = direction * cellSize;
            Vector2 newPosition = currentPosition + movement;

            newPosition.X = (float)Math.Floor(newPosition.X / cellSize) * cellSize;
            newPosition.Y = (float)Math.Floor(newPosition.Y / cellSize) * cellSize;

            newPosition.X = WrapAround(newPosition.X, size.X);
            newPosition.Y = WrapAround(newPosition.Y, size.Y);

            return newPosition;
        }

        public Vector2 CheckInsideGrid(Vector2 position)
        {
            float x = position.X;
            float y = position.Y;

            if (x < 0) 
            { 
                x = size.X + x; 
            }
            
            if (x >= size.X) 
            { 
                x = x - size.X; 
            }

            if (y < 0) 
            {
                y = size.Y + y; 
            }
            
            if (y >= size.Y) 
            { 
                y = y - size.Y; 
            }

            return new Vector2(x, y);
        }

        private float WrapAround(float value, float max)
        {
            if (value < 0)
            {
                return max + value;
            }
           
            if (value >= max) 
            {
                return value - max;
            }
                
            return value;
        }
    }
}