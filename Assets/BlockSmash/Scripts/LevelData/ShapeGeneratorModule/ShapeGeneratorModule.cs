namespace BlockSmash
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public abstract class ShapeGeneratorModule : ICloneable
    {
        public abstract List<Shape> GenerateShapes(int count, List<Shape> availableShapes, bool[,] boardState);

        public abstract object Clone();
    }
}