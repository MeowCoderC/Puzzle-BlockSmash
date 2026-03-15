namespace BlockSmash
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

    [Serializable]
    public class SmartSurvivalGenerator : ShapeGeneratorModule
    {
        [Tooltip("Maximum number of retries if no valid moves are generated.")]
        [SerializeField] private int maxRetries = 10;

        public override List<Shape> GenerateShapes(int count, List<Shape> availableShapes, bool[,] boardState)
        {
            var generatedShapes = new List<Shape>();

            if (availableShapes == null || availableShapes.Count == 0)
            {
                Debug.LogWarning("[SmartSurvivalGenerator] Available shapes pool is empty!");
                return generatedShapes;
            }

            for (var i = 0; i < count; i++)
            {
                generatedShapes.Add(availableShapes[Random.Range(0, availableShapes.Count)]);
            }

            if (boardState == null) return generatedShapes;

            bool hasValidMove = this.CheckIfAnyShapeFits(generatedShapes, boardState);
            int retries = 0;

            while (!hasValidMove && retries < this.maxRetries)
            {
                int biggestShapeIndex = this.GetBiggestShapeIndex(generatedShapes);
                
                generatedShapes[biggestShapeIndex] = availableShapes[Random.Range(0, availableShapes.Count)];
                
                hasValidMove = this.CheckIfAnyShapeFits(generatedShapes, boardState);
                retries++;
            }

            if (!hasValidMove)
            {
                Debug.Log($"[SmartSurvivalGenerator] Failed to find a survivable combination after {this.maxRetries} retries. Player will likely Game Over.");
            }

            return generatedShapes;
        }


        private bool CheckIfAnyShapeFits(List<Shape> shapes, bool[,] boardState)
        {
            foreach (var shape in shapes)
            {
                if (this.CanPlaceShape(shape, boardState)) return true;
            }
            return false;
        }

        private bool CanPlaceShape(Shape shape, bool[,] boardState)
        {
            int boardWidth = boardState.GetLength(0);
            int boardHeight = boardState.GetLength(1);

            for (int startX = 0; startX < boardWidth; startX++)
            {
                for (int startY = 0; startY < boardHeight; startY++)
                {
                    if (this.CheckPlacementAt(shape, boardState, startX, startY))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckPlacementAt(Shape shape, bool[,] boardState, int startX, int startY)
        {
            int boardWidth = boardState.GetLength(0);
            int boardHeight = boardState.GetLength(1);

            for (int x = 0; x < shape.GridSize; x++)
            {
                for (int y = 0; y < shape.GridSize; y++)
                {
                    if (shape.GetCell(x, y))
                    {
                        int boardX = startX + x;
                        int boardY = startY + y;

                        if (boardX >= boardWidth || boardY >= boardHeight) return false;

                        if (boardState[boardX, boardY]) return false;
                    }
                }
            }
            return true;
        }

        private int GetBiggestShapeIndex(List<Shape> shapes)
        {
            int maxCells = -1;
            int index = 0;
            
            for (int i = 0; i < shapes.Count; i++)
            {
                int cellCount = 0;
                for (int c = 0; c < shapes[i].Cells.Count; c++)
                {
                    if (shapes[i].Cells[c]) cellCount++;
                }

                if (cellCount > maxCells)
                {
                    maxCells = cellCount;
                    index = i;
                }
            }
            return index;
        }

        public override object Clone() { return new SmartSurvivalGenerator() { maxRetries = this.maxRetries }; }
    }
}