namespace BlockSmash
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    [RequireComponent(typeof(BoxCollider2D))]
    public class DraggableShape : MonoBehaviour
    {
        public Shape   ShapeData     { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public Sprite  CurrentSprite { get; private set; } 
        
        [SerializeField, HideInInspector] 
        private BoxCollider2D col;

        private float cellSize;
        private float spacing;
        
        private readonly float slotScale = 0.6f; 
        private readonly float dragScale = 1.0f;
        
        //(1.2f = 20%) ---
        private readonly float colliderMultiplier = 1.5f; 

        private List<Cell> activeCells = new();

        private void Reset()
        {
            this.col = this.GetComponent<BoxCollider2D>();
            this.col.isTrigger = true; 
        }

        private void Awake()
        {
            if (this.col == null) this.col = this.GetComponent<BoxCollider2D>();
        }

        public void Initialize(Shape shape, ThemeColor theme, Vector3 startPos, float cellSize, float spacing, Func<Cell> getCellFunc)
        {
            this.ShapeData     = shape;
            this.StartPosition = startPos;
            this.cellSize      = cellSize;
            this.spacing       = spacing;

            this.transform.position   = startPos;
            this.transform.localScale = Vector3.one * this.slotScale;

            if (theme != null)
            {
                this.CurrentSprite = theme.GetRandomSprite();
            }
            else
            {
                this.CurrentSprite = null;
            }

            float totalWidth = shape.GridSize * (cellSize + spacing) - spacing;
            float startX     = -totalWidth / 2f + cellSize / 2f;
            float startY     = totalWidth / 2f - cellSize / 2f;

            this.activeCells.Clear();

            for (int x = 0; x < shape.GridSize; x++)
            {
                for (int y = 0; y < shape.GridSize; y++)
                {
                    if (shape.GetCell(x, y))
                    {
                        var cell = getCellFunc();
                        cell.transform.SetParent(this.transform);
                        cell.transform.localPosition = new Vector3(startX + x * (cellSize + spacing), startY - y * (cellSize + spacing), 0f);
                        cell.transform.localScale = Vector3.one;
                        
                        cell.Init(this.CurrentSprite);
                        cell.SetType(CellType.InBlock); 

                        this.activeCells.Add(cell);
                    }
                }
            }

            if (this.col != null)
            {
                this.col.size = new Vector2(totalWidth * this.colliderMultiplier, totalWidth * this.colliderMultiplier);
                this.col.offset = Vector2.zero;
            }
        }

        public void ClearCells(Action<Cell> returnCellAction)
        {
            foreach (var cell in this.activeCells)
            {
                if (cell != null)
                {
                    returnCellAction(cell);
                }
            }
            this.activeCells.Clear();
        }

        public void Pickup()
        {
            this.transform.localScale = Vector3.one * this.dragScale;
            
            foreach (var cell in this.activeCells)
            {
                if (cell != null) cell.SetType(CellType.Selected);
            }
        }

        public void ReturnToStart()
        {
            this.transform.position   = this.StartPosition;
            this.transform.localScale = Vector3.one * this.slotScale;
            
            foreach (var cell in this.activeCells)
            {
                if (cell != null) cell.SetType(CellType.InBlock);
            }
        }

        public Vector3 GetTopLeftCellWorldPosition()
        {
            float totalWidth = this.ShapeData.GridSize * (this.cellSize + this.spacing) - this.spacing;
            float startX     = this.transform.position.x - totalWidth / 2f + this.cellSize / 2f;
            float startY     = this.transform.position.y + totalWidth / 2f - this.cellSize / 2f;
            
            return new Vector3(startX, startY, 0f);
        }
    }
}