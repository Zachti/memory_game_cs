
using System.Text;

namespace MemoryGame {
    internal class UIManager(IMenu i_Menu, GameManager i_GameManager)
    {
        private bool IsSelectionNotMatching => Board[PreviousUserSelection].Symbol != Board[CurrentUserSelection].Symbol;       
        private IMenu IMenu { get; } = i_Menu;
        private GameManager GameManager { get; } = i_GameManager;
        private Board Board { get; set; } = new Board(0, 0);
        private int BoardWidth => Board.Width;
        private int BoardHeight => Board.Height;
        private bool IsFirstSelection { get; set; } = true;
        private Action<string> Display { get; } = Console.WriteLine;
        private Cell CurrentUserSelection { get; set; } = new Cell (-1,-1);
        private Cell PreviousUserSelection{ get; set; } = new Cell (-1,-1);
        private Action<eUiPauseInterval> Rest { get; } = interval => Thread.Sleep((int)interval);
        private Action ClearUI { get; } = Ex02.ConsoleUtils.Screen.Clear;    

        public void StartGame()
        {
            if(GameManager.CurrentGameState == eGameStates.Menu)
            {
                runMenu();
            }
            runGame();
            gameOver();
        }

        private void runMenu()
        {
            IMenu.Start(out List<Player> players, out int height, out int width, out int? difficulty);
            Board = new Board(height, width);
            GameManager.Initialize(players, height, width, difficulty);
            initializeBoardSymbols();
        }

        private void runGame()
            {
                while(GameManager.CurrentGameState == eGameStates.OnGoing)
                {
                    drawBoard();
                    updateUI(getPlayerInput().ToUpper());
                }
            }
        
        private void drawBoard()
        {
            ClearUI();
            Display($"\nCurrent Turn: {GameManager.CurrentPlayer.Name}\n");
            drawTopSymbolsRow(BoardWidth);
            StringBuilder borderBuilder = new StringBuilder(" ").Append('=', 4 * BoardWidth + 1);
            string border = borderBuilder.ToString();
            Display(border);
            foreach (int i in Enumerable.Range(0, BoardHeight))
            {
                drawRow(i);
                Display(border);
            }
        }

        private void drawTopSymbolsRow(int i_LengthOfRow)
        {
            StringBuilder topSymbolsRow = new StringBuilder(" ");

            foreach (int i in Enumerable.Range(0, i_LengthOfRow))
            {
                topSymbolsRow.Append("   ").Append((char)(i + 'A'));
            }

            Display(topSymbolsRow.ToString());
        }

        private void drawRow(int i_Index) {
            StringBuilder row = new StringBuilder(i_Index + 1).Append(" |");
            foreach (int j in Enumerable.Range(0, BoardWidth))
            {
                Card currentBoardSymbol = Board.Cards[i_Index, j];
                row.Append(' ')
                .Append( currentBoardSymbol.IsRevealed ? currentBoardSymbol.Symbol : ' ')
                .Append(" |");
            }
            Display(row.ToString());
        }

        private string handleAiInput()
        {
            Cell aiSelection = GameManager.GetAiInput(IsFirstSelection);
            showAIMessage();
            return aiSelection.ToString();
        }

        private string handleHumanInput()
        {
            string userInput;
            do
            {
                Display($"{GameManager.CurrentPlayer.Name}, Please enter your selection: ");
                userInput = getInoputOrEmpty();
            }
            while (!ValidatePlayerInput(userInput));

            return userInput!;
        }

        private void updateUI(string i_PlayerInput)
        {
            if(i_PlayerInput == "Q")
            {
                exit();
            }
            
            updateTurn(Cell.Parse(i_PlayerInput));

            if(IsSelectionNotMatching)
            {
                drawBoard();

                Display("Mismatch, but try to remember!");

                Rest(eUiPauseInterval.TwoSeconds);

                Board[CurrentUserSelection].Flip();
                Board[PreviousUserSelection].Flip();

                GameManager.ChangeTurn();
            }
        }
        
        private void showAIMessage()
        {
            string message = GameManager.IsAiHasMatches ? "AI has found a match!" : "AI is thinking...";
            Display(message);
            
            if (!GameManager.IsAiHasMatches)
            {
                Rest(eUiPauseInterval.OneSecond);
                for (int i = 0; i < 2; i++)
                {
                    Rest(eUiPauseInterval.OneSecond);
                    Console.Write(".");
                }
            }
            
            Rest(eUiPauseInterval.TwoSeconds);
        }

        private void exit()
        {
            Display("Goodbye!");
            Display("We hope to see you soon again!");
            Rest(eUiPauseInterval.TwoSeconds);
        }

        private void gameOver()
        {

            drawBoard();

            Display(getGameResult());

            (checkForRestart() ?  (Action)restartGame :  (Action)exit).Invoke();
  
        }

        private bool checkForRestart()
        {
            Display("Press 'Y' to play again, or any other key to exit.");
            return getInoputOrEmpty().Equals("Y", StringComparison.CurrentCultureIgnoreCase);
        }

        private void restartGame()
        {
            ClearUI();
            IMenu.GetBoardSize(out int height, out int width);
            Board = new Board(height, width);
            GameManager.ResetGame(height, width);
            initializeBoardSymbols();
            IsFirstSelection = true;
            StartGame();
        } 

        private bool ValidatePlayerInput(string i_UserInput) {
            bool isInvalid = i_UserInput == string.Empty;
            if (isInvalid) {
                Console.WriteLine("Input must not be empty");
            }
            i_UserInput = i_UserInput.ToUpper();
            return !isInvalid && ( i_UserInput == "Q" || ( validateCellSelection(i_UserInput) && validateCellIsHidden(i_UserInput) ) );
        }

        private bool validateCellSelection(string i_UserInput) {
            bool isInvalid = i_UserInput.Length != 2;
            if(isInvalid)
            {
                Console.WriteLine("Input must have exactly 2 characters");
            }
            return !isInvalid && checkIfSymbolInRange(i_UserInput[0]) && checkIfDigitInRange(i_UserInput[1]);
        }

        private bool checkIfSymbolInRange(char i_Symbol) {
            char lastSymbol = (char)('A' + BoardWidth - 1);
            bool isInvalid = i_Symbol < 'A' || i_Symbol > lastSymbol;
            if (isInvalid) {
                Console.WriteLine($"Invalid input, Symbol must be between A and {lastSymbol}");
            }
            return !isInvalid;
        }

        private bool checkIfDigitInRange(char i_Digit) {
            char lastDigit = (char)('0' + BoardHeight);
            bool isInvalid = i_Digit < '1' || i_Digit > lastDigit;
            if (isInvalid) {
                Console.WriteLine($"Invalid input, digit must be between 1 and {lastDigit}");
            }
            return !isInvalid;
        }
        
        private bool validateCellIsHidden(string i_UserInput) {
            Cell cell = Cell.Parse(i_UserInput);
            bool isInvalid = Board[cell].IsRevealed;
            if (isInvalid)
            {
                Display($"Cell {i_UserInput} is already revealed!\nPlease choose a hidden cell.");
            }
            return !isInvalid;
        }
        
        private string drawScoreboard(List<Player> i_Players)
        {
            generateScoreBoardHeaders(out StringBuilder scoreboard, out string[] scoreBoardHeaders);

            foreach (Player player in i_Players)
            {
                scoreboard.Append("| ");
                foreach (string header in scoreBoardHeaders)
                {
                    scoreboard.Append(header switch
                    {
                        nameof(eScoreBoardHeaders.PlayerName) => player.Name.PadRight(header.Length),
                        nameof(eScoreBoardHeaders.Score) => player.Score.ToString().PadRight(header.Length),
                        _ => string.Empty
                    }).Append(" | ");
                }
                scoreboard.AppendLine();
            }
            return scoreboard.ToString();
        }

        private string getGameResult()
        {
            List<Player> players = GameManager.GetPlayersOrderByScore();
            StringBuilder gameResult = new(players[1].Score == players[0].Score ? "It's a tie!" : $"{players[0].Name} wins!");
            gameResult.AppendLine().Append(drawScoreboard(players));
            return gameResult.ToString();
            }
        
        private void generateScoreBoardHeaders(out StringBuilder io_Scoreboard, out string[] io_ScoreBoardHeaders) {
            io_ScoreBoardHeaders = Enum.GetNames(typeof(eScoreBoardHeaders));
            io_Scoreboard = new StringBuilder("\nScoreboard:\n\n").Append("| ");
            foreach (string header in io_ScoreBoardHeaders) {
                io_Scoreboard.Append(header.PadRight(header.Length)).Append(" | ");
            }
            io_Scoreboard.AppendLine().Append('+');
            foreach (string header in io_ScoreBoardHeaders) {
                io_Scoreboard.Append('-', header.Length + 2).Append('+');
            }
            io_Scoreboard.AppendLine();
        }
                
        private string getPlayerInput() => GameManager.IsCurrentPlayerHuman ? handleHumanInput() : handleAiInput();

        private string getInoputOrEmpty() => Console.ReadLine() ?? string.Empty;

        private void checkAndHandleMatch()
        {
            if (!IsSelectionNotMatching)
            {
                GameManager.Update(CurrentUserSelection, true);
                GameManager.Update(PreviousUserSelection, true);

                GameManager.CurrentPlayer.Score++;
            }
        }

        private void revealCurrentSelection()
        {
            Board[CurrentUserSelection].Flip();
            if (IsFirstSelection)
            {
                PreviousUserSelection = CurrentUserSelection;
            }
            IsFirstSelection = !IsFirstSelection;
        }
   
        private void updateTurn(Cell i_UserSelection)
        {
            GameManager.Update(i_UserSelection, false);

            CurrentUserSelection = i_UserSelection;

            if (!IsFirstSelection)
            {
                checkAndHandleMatch();
            }

            revealCurrentSelection();
        }   
   
        private void initializeBoardSymbols()
        {
            char[] Symbols = Enumerable.Range(0, Board.Height * Board.Width / 2)
                    .Select(i => (char)('A' + i))
                    .ToArray();

            List<Cell> freeCells = new List<Cell>(GameManager.Choices);

            foreach (char currentSymbol in Symbols)
            {
                Cell firstCell = freeCells[0];
                Cell secondCell = firstCell.MatchCell!;
      
                insertSymbolAndRemoveCell(currentSymbol, firstCell, freeCells);
                insertSymbolAndRemoveCell(currentSymbol, secondCell, freeCells);
            }
        }

        private void insertSymbolAndRemoveCell(char i_Symbol, Cell i_Cell, List<Cell> i_FreeCells)
        {
            Board.InsertSymbolToBoard(i_Symbol, i_Cell);
            i_FreeCells.Remove(i_Cell);
        }
   }
}