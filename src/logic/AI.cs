namespace MemoryGame {
    internal class AI(int i_Difficulty) {
        public bool HasMatches { get; private set; }
        public int DifficultyLevel { get; } = i_Difficulty;
        private Cell Selection { get; set; } = new Cell (-1,-1);
        private List<Cell> Memory { get; set; } = [];
        private Mutex MemoryMutex { get; } = new Mutex();
        private bool IsFoundMatch { get; set; }

        public void ForgetCell(Cell i_Selection) {
            MemoryMutex.WaitOne();
            try {
                Memory.Remove(i_Selection);
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
   
        public void RememberCell(Cell i_CellToBeAdded) {

            MemoryMutex.WaitOne();
            try {
                if (!isCellInMemory(i_CellToBeAdded)) {
                    Memory.Add(i_CellToBeAdded);
                }
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
    
        private bool isCellInMemory(Cell i_Cell) {
            MemoryMutex.WaitOne();
            try {
                return Memory.Contains(i_Cell);
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
    
        private bool isMemoryEmpty() {
            MemoryMutex.WaitOne();
            try {
                return Memory.Count == 0;
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
    
        public void ResetMemory() {
            MemoryMutex.WaitOne();
            try {
                IsFoundMatch = HasMatches = false;  
                Memory.Clear();
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
    
        public Cell MakeSelection(List<Cell> i_Cells , bool i_IsFirstSelection) {

            bool isMemoryEmpty = this.isMemoryEmpty();
            HasMatches = !isMemoryEmpty && HasMatches;

            MemoryMutex.WaitOne();

            try {
                return isMemoryEmpty
                    ? getRandomUnmemorizedCell(i_Cells)
                    : (i_IsFirstSelection ? getFirstSelection(i_Cells) : getSecondSelection(i_Cells));
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }

        private Cell getFirstSelection(List<Cell> i_Cells)
        {
            Cell? firstSelection = null;

            HasMatches = IsFoundMatch = findCellMatch(ref firstSelection);
            return HasMatches ? firstSelection! : getRandomUnmemorizedCell(i_Cells);
        }

        private Cell getSecondSelection(List<Cell> i_Cells)
        {
            Cell? secondSelection = IsFoundMatch ? Selection : findCellInMemory();
            HasMatches = secondSelection != null;
            return HasMatches ? secondSelection! : getRandomUnmemorizedCell(i_Cells);
        }
    
        private Cell? findCellInMemory() => 
            Memory.FirstOrDefault(memorizedSymbol => memorizedSymbol.MatchCell!.Equals(Selection));

        private Cell getRandomUnmemorizedCell(List<Cell> i_Cells)
        {
           List<Cell> cellsNotInMemory = i_Cells.Where(cell => !Memory.Contains(cell)).ToList();
            int randomIndex = GameManager.GetRandomNumber(0, cellsNotInMemory.Count);
            Selection = cellsNotInMemory[randomIndex];
            return Selection;
        }

        private bool findCellMatch(ref Cell? i_MemorizedMatchingSymbol)
        {
            bool foundMatch = false;

            foreach (Cell firstMemorizedCell in Memory)
            {
                Cell? matchingCell = Memory
                    .FirstOrDefault(secondMemorizedCell =>
                        !firstMemorizedCell.Equals(secondMemorizedCell) &&
                        firstMemorizedCell.Equals(secondMemorizedCell.MatchCell));

                if (matchingCell != null)
                {
                    i_MemorizedMatchingSymbol = firstMemorizedCell;
                    Selection = matchingCell;
                    foundMatch = true;
                    break;
                }
            }
            return foundMatch;
        }
    }
}