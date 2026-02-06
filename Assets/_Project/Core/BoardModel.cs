using System;
using System.Collections.Generic;

namespace Minesweeper.Core
{
    public enum CellState { Hidden, Revealed, Flagged }

    public sealed class Cell
    {
        public bool IsMine;
        public int AdjacentMines;
        public CellState State = CellState.Hidden;
    }

    public enum GameState { Ready, Playing, Won, Lost }

    public sealed class Board
    {
        public int Width { get; }
        public int Height { get; }
        public int MineCount { get; }

        public Cell[] Cells { get; }
        public bool IsGenerated { get; private set; }

        public Board(int width, int height, int mines)
        {
            Width = width;
            Height = height;
            MineCount = mines;

            Cells = new Cell[width * height];
            for (int i = 0; i < Cells.Length; i++) Cells[i] = new Cell();
        }

        public int Index(int x, int y) => y * Width + x;

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public IEnumerable<(int nx, int ny)> Neighbors(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (InBounds(nx, ny)) yield return (nx, ny);
                }
        }

        public void Reset()
        {
            foreach (var c in Cells)
            {
                c.IsMine = false;
                c.AdjacentMines = 0;
                c.State = CellState.Hidden;
            }
            IsGenerated = false;
        }

        /// <summary>
        /// Generate mines after first click so first click (and its neighbors) are always safe.
        /// </summary>
        public void Generate(int firstX, int firstY, int? seed = null)
        {
            if (IsGenerated) return;
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();

            // Safe zone: first cell + its neighbors
            var banned = new HashSet<int>();
            banned.Add(Index(firstX, firstY));
            foreach (var (nx, ny) in Neighbors(firstX, firstY))
                banned.Add(Index(nx, ny));

            // Build candidate list
            var candidates = new List<int>(Cells.Length);
            for (int i = 0; i < Cells.Length; i++)
                if (!banned.Contains(i)) candidates.Add(i);

            if (MineCount > candidates.Count)
                throw new InvalidOperationException("Too many mines for this board size (consider reducing mines).");

            // Place mines
            for (int m = 0; m < MineCount; m++)
            {
                int pick = rng.Next(candidates.Count);
                int idx = candidates[pick];
                candidates.RemoveAt(pick);
                Cells[idx].IsMine = true;
            }

            // Compute adjacency numbers
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    int idx = Index(x, y);
                    if (Cells[idx].IsMine) continue;

                    int count = 0;
                    foreach (var (nx, ny) in Neighbors(x, y))
                        if (Cells[Index(nx, ny)].IsMine) count++;

                    Cells[idx].AdjacentMines = count;
                }

            IsGenerated = true;
        }

        public int CountFlags()
        {
            int f = 0;
            foreach (var c in Cells) if (c.State == CellState.Flagged) f++;
            return f;
        }

        public bool ToggleFlag(int x, int y)
        {
            int idx = Index(x, y);
            var c = Cells[idx];
            if (c.State == CellState.Revealed) return false;

            c.State = (c.State == CellState.Flagged) ? CellState.Hidden : CellState.Flagged;
            return true;
        }

        public bool Reveal(int x, int y, out bool hitMine)
        {
            hitMine = false;
            int idx = Index(x, y);
            var c = Cells[idx];

            if (c.State == CellState.Revealed || c.State == CellState.Flagged) return false;

            c.State = CellState.Revealed;

            if (c.IsMine)
            {
                hitMine = true;
                return true;
            }

            // Flood fill if zero
            if (c.AdjacentMines == 0)
                FloodRevealZeros(x, y);

            return true;
        }

        private void FloodRevealZeros(int startX, int startY)
        {
            var q = new Queue<(int x, int y)>();
            q.Enqueue((startX, startY));

            while (q.Count > 0)
            {
                var (x, y) = q.Dequeue();
                foreach (var (nx, ny) in Neighbors(x, y))
                {
                    var nc = Cells[Index(nx, ny)];
                    if (nc.State == CellState.Revealed || nc.State == CellState.Flagged) continue;
                    if (nc.IsMine) continue;

                    nc.State = CellState.Revealed;

                    if (nc.AdjacentMines == 0)
                        q.Enqueue((nx, ny));
                }
            }
        }

        public void RevealAllMines()
        {
            foreach (var c in Cells)
                if (c.IsMine) c.State = CellState.Revealed;
        }

        public bool CheckWin()
        {
            // Win when all non-mine cells are revealed
            foreach (var c in Cells)
            {
                if (!c.IsMine && c.State != CellState.Revealed)
                    return false;
            }
            return true;
        }
    }
}
