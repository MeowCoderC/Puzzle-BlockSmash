namespace BlockSmash
{
    using System.Collections.Generic;

    public class ClearResult
    {
        public List<int> ClearedRows = new();
        public List<int> ClearedCols = new();
        public bool HasClears => this.ClearedRows.Count > 0 || this.ClearedCols.Count > 0;
    }

    public class BoardModel
    {
        public readonly int Width;
        public readonly int Height;
        
        private readonly bool[,] isPlayable; 
        private readonly bool[,] isOccupied; 

        public BoardModel(LevelData levelData)
        {
            this.Width      = levelData.GridSize;
            this.Height     = levelData.GridSize;
            this.isPlayable = new bool[this.Width, this.Height];
            this.isOccupied = new bool[this.Width, this.Height];

            for (var y = 0; y < this.Height; y++)
            {
                for (var x = 0; x < this.Width; x++)
                {
                    this.isPlayable[x, y] = levelData.GetCell(x, y);
                    this.isOccupied[x, y] = levelData.GetBlockColor(x, y) >= 0;
                }
            }
        }

        public bool CanPlaceShape(Shape shape, int startX, int startY)
        {
            for (var x = 0; x < shape.GridSize; x++)
            {
                for (var y = 0; y < shape.GridSize; y++)
                {
                    if (shape.GetCell(x, y))
                    {
                        var boardX = startX + x;
                        var boardY = startY + y;

                        if (boardX < 0 || boardX >= this.Width || boardY < 0 || boardY >= this.Height) return false;
                        if (!this.isPlayable[boardX, boardY]) return false;
                        if (this.isOccupied[boardX, boardY]) return false;
                    }
                }
            }
            return true;
        }

        public void PlaceShape(Shape shape, int startX, int startY)
        {
            for (var x = 0; x < shape.GridSize; x++)
            {
                for (var y = 0; y < shape.GridSize; y++)
                {
                    if (shape.GetCell(x, y))
                    {
                        this.isOccupied[startX + x, startY + y] = true;
                    }
                }
            }
        }

        public ClearResult CheckAndClearLines()
        {
            var result = new ClearResult();

            for (var y = 0; y < this.Height; y++)
            {
                var isRowFull        = true;
                var hasPlayableCells = false;

                for (var x = 0; x < this.Width; x++)
                {
                    if (this.isPlayable[x, y])
                    {
                        hasPlayableCells = true;
                        if (!this.isOccupied[x, y])
                        {
                            isRowFull = false;
                            break;
                        }
                    }
                }

                if (hasPlayableCells && isRowFull) result.ClearedRows.Add(y);
            }

            for (var x = 0; x < this.Width; x++)
            {
                var isColFull        = true;
                var hasPlayableCells = false;

                for (var y = 0; y < this.Height; y++)
                {
                    if (this.isPlayable[x, y])
                    {
                        hasPlayableCells = true;
                        if (!this.isOccupied[x, y])
                        {
                            isColFull = false;
                            break;
                        }
                    }
                }

                if (hasPlayableCells && isColFull) result.ClearedCols.Add(x);
            }

            foreach (var y in result.ClearedRows)
            {
                for (var x = 0; x < this.Width; x++) this.isOccupied[x, y] = false;
            }

            foreach (var x in result.ClearedCols)
            {
                for (var y = 0; y < this.Height; y++) this.isOccupied[x, y] = false;
            }

            return result;
        }

        public bool IsOccupied(int x, int y) { return this.isOccupied[x, y]; }
        public bool IsPlayable(int x, int y) { return this.isPlayable[x, y]; }
    }
}