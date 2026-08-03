namespace SokobanGame.Services
{
    public class GameEngine
    {
        // Symbols used to represent objects in the level
        public const char Wall = '#';
        public const char Player = '@';
        public const char Box = '$';
        public const char Target = '.';
        public const char BoxOnTarget = '*';
        public const char PlayerOnTarget = '+';
        public const char Floor = ' ';

        // Possible movement directions
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        // Converts the level string into a grid that the game can use
        public char[,] ParseLevel(string levelString)
        {
            string[] rows = levelString.Split('\n');

            int height = rows.Length;
            int width = rows.Max(r => r.Length);

            var grid = new char[height, width];

            // Go through each row and add it into the grid
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    // Empty spaces are added if a row is shorter
                    grid[r, c] = c < rows[r].Length ? rows[r][c] : Floor;
                }
            }

            return grid;
        }

        // Creates a copy of the grid so it can be used for undo moves
        public char[,] CopyGrid(char[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            var copy = new char[rows, cols];

            Array.Copy(grid, copy, grid.Length);

            return copy;
        }

        // Finds where the player currently is on the map
        public (int row, int col) FindPlayer(char[,] grid)
        {
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    if (grid[r, c] == Player || grid[r, c] == PlayerOnTarget)
                    {
                        return (r, c);
                    }
                }
            }

            // Returns this if the player cannot be found
            return (-1, -1);
        }

        // Tries to move the player and returns true if the move works
        public bool TryMove(char[,] grid, Direction dir)
        {
            var (pr, pc) = FindPlayer(grid);

            // Convert the direction into row and column movement
            var (dr, dc) = dir switch
            {
                Direction.Up => (-1, 0),
                Direction.Down => (1, 0),
                Direction.Left => (0, -1),
                Direction.Right => (0, 1),
                _ => (0, 0)
            };

            int nr = pr + dr;
            int nc = pc + dc;

            // The square after a box when pushing
            int br = nr + dr;
            int bc = nc + dc;

            if (!InBounds(grid, nr, nc))
                return false;

            char next = grid[nr, nc];

            // Player cannot move through walls
            if (next == Wall)
                return false;

            // If there is a box, try to push it
            if (next == Box || next == BoxOnTarget)
            {
                if (!InBounds(grid, br, bc))
                    return false;

                char beyond = grid[br, bc];

                // Cannot push a box into another box or wall
                if (beyond == Wall || beyond == Box || beyond == BoxOnTarget)
                    return false;

                // Move the box forward
                grid[br, bc] = beyond == Target ? BoxOnTarget : Box;

                // Remove the old box position
                grid[nr, nc] = next == BoxOnTarget ? Target : Floor;
            }

            // Remove player from old position
            char current = grid[pr, pc];

            grid[pr, pc] = current == PlayerOnTarget ? Target : Floor;

            // Move player into the new position
            grid[nr, nc] = (next == Target || next == BoxOnTarget)
                ? PlayerOnTarget
                : Player;

            return true;
        }

        // Checks if all boxes are placed on targets
        public bool IsComplete(char[,] grid)
        {
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    // If there is still a normal box, the level is not finished
                    if (grid[r, c] == Box)
                        return false;
                }
            }

            return true;
        }

        // Checks that a position is inside the level boundaries
        private bool InBounds(char[,] grid, int r, int c)
        {
            return r >= 0 &&
                   r < grid.GetLength(0) &&
                   c >= 0 &&
                   c < grid.GetLength(1);
        }
    }
}