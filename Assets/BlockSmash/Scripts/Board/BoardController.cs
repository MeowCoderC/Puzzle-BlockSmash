namespace BlockSmash
{
    using UnityEngine;
    using System.Collections.Generic; 

    public class BoardController : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float     cellSize = 1f;
        [SerializeField] private float     spacing  = 0.1f;
        [SerializeField] private Transform cellInBoardParent; 
        
        [Header("Dependencies")]
        [SerializeField] private CellPool  cellPool; 

        [Header("Editor Preview & Alignment")]
        [SerializeField] private Transform originTransform;
        [SerializeField] private LevelData previewLevelData;

        private BoardModel model;
        private ThemeColor currentTheme;
        private float      startX;
        private float      startY;

        private Cell[,] placedBlocks;

        private int currentPreviewX = -1;
        private int currentPreviewY = -1;
        private List<Cell> ghostCells = new();
        private List<Cell> highlightedCells = new();
        private Dictionary<Cell, Sprite> originalSprites = new();
        private const float PREVIEW_ALPHA = 0.5f;
        public float CellSize => this.cellSize;
        public float Spacing  => this.spacing;

        public bool[,] GetBoardState()
        {
            if (this.model == null) return null;
            bool[,] state = new bool[this.model.Width, this.model.Height];
            for (int x = 0; x < this.model.Width; x++)
            {
                for (int y = 0; y < this.model.Height; y++)
                {
                    state[x, y] = this.model.IsOccupied(x, y);
                }
            }
            return state;
        }

        public void GenerateBoard(LevelData levelData, ThemeColor theme)
        {
            this.ClearBoard();

            this.model        = new BoardModel(levelData);
            this.currentTheme = theme;
            this.placedBlocks = new Cell[levelData.GridSize, levelData.GridSize];

            var centerPos  = this.originTransform != null ? this.originTransform.position : this.transform.position;
            var totalWidth = levelData.GridSize * (this.cellSize + this.spacing) - this.spacing;
            this.startX = centerPos.x - totalWidth / 2f + this.cellSize / 2f;
            this.startY = centerPos.y + totalWidth / 2f - this.cellSize / 2f;

            for (int y = 0; y < levelData.GridSize; y++)
            {
                for (int x = 0; x < levelData.GridSize; x++)
                {
                    int colorIndex = levelData.GetBlockColor(x, y);
                    if (colorIndex >= 0)
                    {
                        var cell = this.cellPool.Get();
                        cell.transform.SetParent(this.cellInBoardParent);
                        cell.transform.position   = this.GetWorldPosition(x, y);
                        cell.transform.localScale = Vector3.one;

                        Sprite sprite = (theme != null && colorIndex < theme.Sprites.Count) ? theme.Sprites[colorIndex] : null;
                        
                        cell.Init(sprite);
                        cell.SetType(CellType.InBoard);

                        this.placedBlocks[x, y] = cell;
                    }
                }
            }

            Debug.Log($"[BoardController] Initialized Logical Board. Size: {levelData.GridSize}x{levelData.GridSize}.");
        }

        public void ClearBoard()
        {
            this.ClearPreview(); 

            if (this.placedBlocks != null)
            {
                foreach (var cell in this.placedBlocks)
                {
                    if (cell != null && this.cellPool != null)
                    {
                        this.cellPool.Return(cell);
                    }
                }
            }

            this.placedBlocks = null;
            this.model        = null;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(
                this.startX + x * (this.cellSize + this.spacing), 
                this.startY - y * (this.cellSize + this.spacing), 
                0f
            );
        }

        public bool TryGetGridCoordinate(Vector3 worldPos, out int x, out int y)
        {
            x = Mathf.RoundToInt((worldPos.x - this.startX) / (this.cellSize + this.spacing));
            y = Mathf.RoundToInt((this.startY - worldPos.y) / (this.cellSize + this.spacing)); 

            if (this.model != null && x >= 0 && x < this.model.Width && y >= 0 && y < this.model.Height)
            {
                return true;
            }

            x = -1;
            y = -1;
            return false;
        }


        public void ShowPreview(Shape shape, int gridX, int gridY, Sprite blockSprite)
        {
            if (this.currentPreviewX == gridX && this.currentPreviewY == gridY) return;

            this.ClearPreview();

            if (this.model == null || !this.model.CanPlaceShape(shape, gridX, gridY)) return;

            this.currentPreviewX = gridX;
            this.currentPreviewY = gridY;

            for (var x = 0; x < shape.GridSize; x++)
            {
                for (var y = 0; y < shape.GridSize; y++)
                {
                    if (shape.GetCell(x, y))
                    {
                        var boardX = gridX + x;
                        var boardY = gridY + y;
                        
                        var cell = this.cellPool.Get();
                        cell.transform.SetParent(this.cellInBoardParent);
                        cell.transform.position = this.GetWorldPosition(boardX, boardY);
                        cell.transform.localScale = Vector3.one;

                        cell.Init(blockSprite);
                        cell.SetType(CellType.InBoard);
                        cell.SetAlpha(PREVIEW_ALPHA); 

                        this.ghostCells.Add(cell);
                    }
                }
            }

            this.GetSimulatedClears(shape, gridX, gridY, out List<int> clearRows, out List<int> clearCols);

            foreach (var y in clearRows)
            {
                for (var x = 0; x < this.model.Width; x++)
                {
                    var c = this.placedBlocks[x, y];
                    if (c != null && !this.highlightedCells.Contains(c))
                    {
                        this.originalSprites[c] = c.CurrentSprite;
                        c.Init(blockSprite); 
                        c.SetAlpha(PREVIEW_ALPHA); 
                        this.highlightedCells.Add(c);
                    }
                }
            }

            foreach (var x in clearCols)
            {
                for (var y = 0; y < this.model.Height; y++)
                {
                    var c = this.placedBlocks[x, y];
                    if (c != null && !this.highlightedCells.Contains(c))
                    {
                        this.originalSprites[c] = c.CurrentSprite;
                        c.Init(blockSprite);
                        c.SetAlpha(PREVIEW_ALPHA);
                        this.highlightedCells.Add(c);
                    }
                }
            }
        }

        public void ClearPreview()
        {
            if (this.currentPreviewX == -1 && this.currentPreviewY == -1) return;

            foreach (var cell in this.ghostCells)
            {
                cell.SetAlpha(1f); 
                this.cellPool.Return(cell);
            }
            this.ghostCells.Clear();

            foreach (var cell in this.highlightedCells)
            {
                if (this.originalSprites.TryGetValue(cell, out Sprite orig))
                {
                    cell.Init(orig);
                }
                cell.SetAlpha(1f);
            }
            this.highlightedCells.Clear();
            this.originalSprites.Clear();

            this.currentPreviewX = -1;
            this.currentPreviewY = -1;
        }

        private void GetSimulatedClears(Shape shape, int gridX, int gridY, out List<int> clearRows, out List<int> clearCols)
        {
            clearRows = new List<int>();
            clearCols = new List<int>();
            
            bool[,] tempOccupied = new bool[this.model.Width, this.model.Height];
            for(int x = 0; x < this.model.Width; x++)
                for(int y = 0; y < this.model.Height; y++)
                    tempOccupied[x, y] = this.model.IsOccupied(x, y);

            for (var x = 0; x < shape.GridSize; x++)
            {
                for (var y = 0; y < shape.GridSize; y++)
                {
                    if (shape.GetCell(x, y))
                    {
                        tempOccupied[gridX + x, gridY + y] = true;
                    }
                }
            }

            for (var y = 0; y < this.model.Height; y++)
            {
                bool full = true;
                bool hasPlayable = false;
                for (var x = 0; x < this.model.Width; x++)
                {
                    if (this.model.IsPlayable(x, y))
                    {
                        hasPlayable = true;
                        if (!tempOccupied[x, y]) { full = false; break; }
                    }
                }
                if (hasPlayable && full) clearRows.Add(y);
            }

            for (var x = 0; x < this.model.Width; x++)
            {
                bool full = true;
                bool hasPlayable = false;
                for (var y = 0; y < this.model.Height; y++)
                {
                    if (this.model.IsPlayable(x, y))
                    {
                        hasPlayable = true;
                        if (!tempOccupied[x, y]) { full = false; break; }
                    }
                }
                if (hasPlayable && full) clearCols.Add(x);
            }
        }


        public bool TryPlaceShape(Shape shape, int gridX, int gridY, Sprite blockSprite)
        {
            this.ClearPreview(); 

            if (this.model == null || this.cellPool == null) return false;

            if (this.model.CanPlaceShape(shape, gridX, gridY))
            {
                this.model.PlaceShape(shape, gridX, gridY);

                for (var x = 0; x < shape.GridSize; x++)
                {
                    for (var y = 0; y < shape.GridSize; y++)
                    {
                        if (shape.GetCell(x, y))
                        {
                            var boardX = gridX + x;
                            var boardY = gridY + y;
                            
                            var cell = this.cellPool.Get();
                            cell.transform.SetParent(this.cellInBoardParent);
                            cell.transform.position = this.GetWorldPosition(boardX, boardY);
                            cell.transform.localScale = Vector3.one;

                            cell.Init(blockSprite);
                            cell.SetType(CellType.InBoard);

                            this.placedBlocks[boardX, boardY] = cell;
                        }
                    }
                }

                var clearResult = this.model.CheckAndClearLines();
                if (clearResult.HasClears)
                {
                    this.HandleLinesCleared(clearResult);
                }

                return true;
            }

            return false;
        }

        private void HandleLinesCleared(ClearResult clearResult)
        {
            Debug.Log($"[BoardController] Combo! Cleared {clearResult.ClearedRows.Count} rows and {clearResult.ClearedCols.Count} columns.");

            foreach (var y in clearResult.ClearedRows)
            {
                for (var x = 0; x < this.model.Width; x++)
                {
                    if (this.placedBlocks[x, y] != null)
                    {
                        this.cellPool.Return(this.placedBlocks[x, y]); 
                        this.placedBlocks[x, y] = null;
                    }
                }
            }

            foreach (var x in clearResult.ClearedCols)
            {
                for (var y = 0; y < this.model.Height; y++)
                {
                    if (this.placedBlocks[x, y] != null)
                    {
                        this.cellPool.Return(this.placedBlocks[x, y]); 
                        this.placedBlocks[x, y] = null;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (this.previewLevelData == null) return;

            var centerPos  = this.originTransform != null ? this.originTransform.position : this.transform.position;
            var gridSize   = this.previewLevelData.GridSize;
            var totalWidth = gridSize * (this.cellSize + this.spacing) - this.spacing;
            var sX         = centerPos.x - totalWidth / 2f + this.cellSize / 2f;
            var sY         = centerPos.y + totalWidth / 2f - this.cellSize / 2f;

            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var pos = new Vector3(sX + x * (this.cellSize + this.spacing), sY - y * (this.cellSize + this.spacing), 0f);

                    if (this.previewLevelData.GetCell(x, y))
                    {
                        Gizmos.color = Color.green; 
                        Gizmos.DrawWireCube(pos, new Vector3(this.cellSize, this.cellSize, 0f));
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); 
                        Gizmos.DrawWireCube(pos, new Vector3(this.cellSize, this.cellSize, 0f));
                        Gizmos.DrawLine(pos - new Vector3(this.cellSize / 2f, this.cellSize / 2f, 0f), pos + new Vector3(this.cellSize / 2f, this.cellSize / 2f, 0f));
                    }
                }
            }
        }
    }
}