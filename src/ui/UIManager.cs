
using System.Text;

namespace MemoryGame {
    internal class UIManager(IMenu i_Menu, GameManager i_GameManager)
    {
        private IMenu IMenu { get; } = i_Menu;
        private GameManager GameManager { get; } = i_GameManager;
        private Action<string> Display { get; } = Console.WriteLine;
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
            eGameModes desiredGameMode = IMenu.Start(out List<Player> players, out int height, out int width, out int? difficulty);
            GameManager.Initialize(players, new Board(height, width), difficulty);
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
            drawTopLetterRow(GameManager.BoardWidth);
            StringBuilder borderBuilder = new StringBuilder(" ").Append('=', 4 * GameManager.BoardWidth + 1);
            string border = borderBuilder.ToString();
            Display(border);
            foreach (int i in Enumerable.Range(0, GameManager.BoardHeight))
            {
                drawRow(i);
                Display(border);
            }
        }

        private void drawTopLetterRow(int i_LengthOfRow)
        {
            StringBuilder topLettersRow = new StringBuilder(" ");

            foreach (int i in Enumerable.Range(0, i_LengthOfRow))
            {
                topLettersRow.Append("   ").Append((char)(i + 'A'));
            }

            Display(topLettersRow.ToString());
        }

        private void drawRow(int i_Index) {
            StringBuilder row = new StringBuilder(i_Index + 1).Append(" |");
            foreach (int j in Enumerable.Range(0, GameManager.BoardWidth))
            {
                Card currentBoardLetter = GameManager.Board.Cards[i_Index, j];
                row.Append(' ')
                .Append( currentBoardLetter.IsRevealed ? currentBoardLetter.Letter : ' ')
                .Append(" |");
            }
            Display(row.ToString());
        }

        private string handleAiInput()
        {
            string aiInput = GameManager.GetAiInput();
            showAIMessage();
            return aiInput;
        }

        private string handleHumanInput()
        {
            string userInput;
            do
            {
                Display($"{GameManager.CurrentPlayer.Name}, Please enter your selection: ");
                userInput = getInput();
            }
            while (!ValidatePlayerInput(userInput));

            return userInput!;
        }

        private void updateUI(string i_PlayerInput)
        {

            (i_PlayerInput == "Q" ? (Action)exit : () => GameManager.Update(Cell.Parse(i_PlayerInput)))();
            

            if(GameManager.IsSelectionNotMatching)
            {
                drawBoard();
                Display("Mismatch, but try to remember!");

                Rest(eUiPauseInterval.TwoSeconds);

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
            Environment.Exit(0);
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
            return getInput().ToUpper() == "Y";
        }

        private void restartGame()
        {
            ClearUI();
            IMenu.GetBoardSize(out int height, out int width);
            GameManager.ResetGame(height, width);
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
            return !isInvalid && checkIfLetterInRange(i_UserInput[0]) && checkIfDigitInRange(i_UserInput[1]);
        }

        private bool checkIfLetterInRange(char i_Letter) {
            char lastLetter = (char)('A' + GameManager.BoardWidth - 1);
            bool isInvalid = i_Letter < 'A' || i_Letter > lastLetter;
            if (isInvalid) {
                Console.WriteLine($"Invalid input, letter must be between A and {lastLetter}");
            }
            return !isInvalid;
        }

        private bool checkIfDigitInRange(char i_Digit) {
            char lastDigit = (char)('0' + GameManager.BoardHeight);
            bool isInvalid = i_Digit < '1' || i_Digit > lastDigit;
            if (isInvalid) {
                Console.WriteLine($"Invalid input, digit must be between 1 and {lastDigit}");
            }
            return !isInvalid;
        }
        
        private bool validateCellIsHidden(string i_UserInput) {
            Cell cell = Cell.Parse(i_UserInput);
            bool isInvalid = GameManager.IsCellRevealed(cell);
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

        private string getInput() => Console.ReadLine() ?? string.Empty;
   }
}