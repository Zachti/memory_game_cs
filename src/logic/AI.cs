namespace MemoryGame {
    internal class AI(int i_Difficulty) {
        public bool HasMatches { get; set; }
        public int DifficultyLevel { get; set; } = i_Difficulty;
        private Cell Selection { get; set; }
        private Dictionary<Cell, char> Memory { get; set; } = [];
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
   
        public void RememberCell(Cell i_CellToBeAdded, char i_Letter) {
            MemoryMutex.WaitOne();
            try {
                if (!isCellInMemory(i_CellToBeAdded)) {
                    Memory.Add(i_CellToBeAdded, i_Letter);
                }
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
    
        private bool isCellInMemory(Cell i_Cell) {
            MemoryMutex.WaitOne();
            try {
                return Memory.ContainsKey(i_Cell);
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }
    
        public bool IsMemoryEmpty() {
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
    
        public string MakeSelection(Card[,] i_Cards , bool i_IsFirstSelection) {

            bool isMemoryEmpty = IsMemoryEmpty();
            HasMatches = !isMemoryEmpty && HasMatches;

            MemoryMutex.WaitOne();

            try {
                return isMemoryEmpty
                    ? getRandomUnmemorizedCell(i_Cards)
                    : (i_IsFirstSelection ? getFirstSelection(i_Cards) : getSecondSelection(i_Cards));
            }
            finally {
                MemoryMutex.ReleaseMutex();
            }
        }

        private string getFirstSelection(Card[,] i_Cards)
        {
            string firstSelection = string.Empty;

            HasMatches = IsFoundMatch = findLetterMatch(ref firstSelection);
            return HasMatches ? firstSelection : getRandomUnmemorizedCell(i_Cards);
        }

        private string getSecondSelection(Card[,] i_Cards)
        {
            string secondSelection = IsFoundMatch ? Selection.ToString() : findLetterInMemory();
            HasMatches = secondSelection != "";
            return HasMatches ? secondSelection : getRandomUnmemorizedCell(i_Cards);
        }
    
        private string findLetterInMemory()
        {
            char Letter = Memory[Selection];
            return Memory
                .Where(memorizedLetter => 
                    !memorizedLetter.Key.Equals(Selection) &&
                    memorizedLetter.Value == Letter)
                .Select(memorizedLetter => memorizedLetter.Key.ToString())
                .FirstOrDefault() 
                ?? string.Empty;
        }

        private string getRandomUnmemorizedCell(Card[,] i_Cards)
        {
            List<Cell> cellsNotInMemory = Enumerable.Range(0, i_Cards.GetLength(0))
                .SelectMany(row => Enumerable.Range(0, i_Cards.GetLength(1)), (row, column) => new { row, column })
                .Where(cell => !(i_Cards[cell.row, cell.column].IsRevealed || Memory.ContainsKey(new Cell(cell.row, cell.column))))
                .Select(cell => new Cell(cell.row, cell.column))
                .ToList();
            int randomIndex = GameData.GetRandomNumber(0, cellsNotInMemory.Count);
            Selection = cellsNotInMemory[randomIndex];
            return Selection.ToString();
        }

        private bool findLetterMatch(ref string i_MemorizedMatchingLetter)
        {
            bool foundMatch = false;

            foreach (KeyValuePair<Cell, char> firstMemorizedLetter in Memory)
            {
                KeyValuePair<Cell, char> matchingLetter = Memory
                    .FirstOrDefault(secondMemorizedLetter =>
                        !firstMemorizedLetter.Key.Equals(secondMemorizedLetter.Key) &&
                        firstMemorizedLetter.Value == secondMemorizedLetter.Value);

                if (!matchingLetter.Equals(default(KeyValuePair<Cell, char>)))
                {
                    i_MemorizedMatchingLetter = firstMemorizedLetter.Key.ToString();
                    Selection = matchingLetter.Key;
                    foundMatch = true;
                    break;
                }
            }
            return foundMatch;
        }
    }
}