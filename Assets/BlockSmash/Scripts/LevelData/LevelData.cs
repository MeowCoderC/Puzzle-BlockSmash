namespace BlockSmash
{
    using CahtFramework;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public struct ShapeWave
    {
        public Shape[] shapes;
    }

    public class LevelData : IdentifiedObject
    {
        [SerializeField] private int gridSize = 8;
        [SerializeField, HideInInspector] private bool[] cells = new bool[64];
        [SerializeField, HideInInspector] private int[] blockColors = new int[64]; 
        
        [SerializeField] private List<ShapeWave> predefinedWaves = new();
        [SerializeField] private List<Shape> availableShapes = new();

        [SerializeReference, SubclassSelector]
        private ShapeGeneratorModule generatorModule;

        public int GridSize => this.gridSize;
        public IReadOnlyList<ShapeWave> PredefinedWaves => this.predefinedWaves;
        public IReadOnlyList<Shape> AvailableShapes => this.availableShapes;
        public ShapeGeneratorModule GeneratorModule => this.generatorModule;
        public bool HasGeneratorModule => this.generatorModule != null;

        public void ValidateData()
        {
            int targetSize = this.gridSize * this.gridSize;
            if (this.cells == null || this.cells.Length != targetSize)
            {
                var newCells = new bool[targetSize];
                for (int i = 0; i < newCells.Length; i++) newCells[i] = true;
                this.cells = newCells;
            }
            if (this.blockColors == null || this.blockColors.Length != targetSize)
            {
                this.blockColors = new int[targetSize];
                for (int i = 0; i < this.blockColors.Length; i++) this.blockColors[i] = -1;
            }
        }

        public bool GetCell(int x, int y)
        {
            if (x < 0 || x >= this.gridSize || y < 0 || y >= this.gridSize) return false;
            return this.cells[y * this.gridSize + x];
        }

        public int GetBlockColor(int x, int y)
        {
            if (x < 0 || x >= this.gridSize || y < 0 || y >= this.gridSize) return -1;
            return this.blockColors[y * this.gridSize + x];
        }

        public void SetBlockData(int x, int y, bool isPlayable, int colorIndex)
        {
            if (x < 0 || x >= this.gridSize || y < 0 || y >= this.gridSize) return;
            int idx = y * this.gridSize + x;
            this.cells[idx] = isPlayable;
            this.blockColors[idx] = isPlayable ? colorIndex : -1;
        }

        public void ResizeGrid(int newSize)
        {
            if (newSize == this.gridSize) return;
            var newCells = new bool[newSize * newSize];
            var newColors = new int[newSize * newSize];
            for (int i = 0; i < newColors.Length; i++)
            {
                newCells[i] = true;
                newColors[i] = -1;
            }

            int minSize = Mathf.Min(this.gridSize, newSize);
            for (int y = 0; y < minSize; y++)
            {
                for (int x = 0; x < minSize; x++)
                {
                    newCells[y * newSize + x] = this.cells[y * this.gridSize + x];
                    newColors[y * newSize + x] = this.blockColors[y * this.gridSize + x];
                }
            }
            this.cells = newCells;
            this.blockColors = newColors;
            this.gridSize = newSize;
        }

        public void FillAll(bool playable, int colorIndex)
        {
            for (int i = 0; i < this.cells.Length; i++)
            {
                this.cells[i] = playable;
                this.blockColors[i] = playable ? colorIndex : -1;
            }
        }
    }
}