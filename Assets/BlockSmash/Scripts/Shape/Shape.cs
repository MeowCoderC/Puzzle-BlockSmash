namespace BlockSmash
{
    using CahtFramework;
    using UnityEngine;
    using System.Collections.Generic;

    public class Shape : IdentifiedObject
    {
        [SerializeField, Range(1, 10)]    private int    gridSize = 5;
        [SerializeField, HideInInspector] private bool[] cells    = new bool[25];

        public int                 GridSize => this.gridSize;
        public IReadOnlyList<bool> Cells    => this.cells;

        public bool GetCell(int x, int y)
        {
            if (x < 0 || x >= this.gridSize || y < 0 || y >= this.gridSize) return false;
            return this.cells[y * this.gridSize + x];
        }

        public void SetCell(int x, int y, bool value)
        {
            if (x < 0 || x >= this.gridSize || y < 0 || y >= this.gridSize) return;
            this.cells[y * this.gridSize + x] = value;
        }

        public void ResizeGrid(int newSize)
        {
            if (newSize == this.gridSize) return;

            var newCells = new bool[newSize * newSize];
            var minSize  = Mathf.Min(this.gridSize, newSize);

            for (var y = 0; y < minSize; y++)
            {
                for (var x = 0; x < minSize; x++)
                {
                    newCells[y * newSize + x] = this.cells[y * this.gridSize + x];
                }
            }

            this.cells    = newCells;
            this.gridSize = newSize;
        }
    }
}