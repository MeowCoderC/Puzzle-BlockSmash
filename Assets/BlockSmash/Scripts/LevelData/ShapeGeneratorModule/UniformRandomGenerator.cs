namespace BlockSmash
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

    [Serializable]
    public class UniformRandomGenerator : ShapeGeneratorModule
    {
        public override List<Shape> GenerateShapes(int count, List<Shape> availableShapes, bool[,] boardState)
        {
            var generatedShapes = new List<Shape>();

            if (availableShapes == null || availableShapes.Count == 0)
            {
                Debug.LogWarning("[ShapeGenerator] Available shapes pool is empty! Returning empty list.");
                return generatedShapes;
            }

            for (var i = 0; i < count; i++)
            {
                generatedShapes.Add(availableShapes[Random.Range(0, availableShapes.Count)]);
            }

            return generatedShapes;
        }

        public override object Clone() { return new UniformRandomGenerator(); }
    }
}