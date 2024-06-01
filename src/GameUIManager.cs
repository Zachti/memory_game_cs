
using System.Text;

namespace MemoryGame {
    internal class GameUIManager(IMenu i_Menu, GameManager i_GameManager)
    {
        private IMenu IMenu { get; } = i_Menu;
        private GameManager GameManager { get; } = i_GameManager;
        private Action<string> Display { get; } = Console.WriteLine;
        private Action<int> Rest { get; } =  System.Threading.Thread.Sleep;
        private Action ClearUI { get; } = Ex02.ConsoleUtils.Screen.Clear;    
        private Func<string> Read { get; } = () => Console.ReadLine() ?? string.Empty;

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
            eGameModes desiredGameMode = IMenu.Start(out string fPlayerName, out string sPlayerName, out int height, out int width, out int? difficulty);
            ePlayerTypes type = desiredGameMode == eGameModes.multiPlayer ? ePlayerTypes.Human : ePlayerTypes.AI;
            Player fPlayer = new Player(fPlayerName, ePlayerTypes.Human);
            Player sPlayer = new Player(sPlayerName, type);
            GameManager.Initialize(fPlayer, sPlayer, new Board(height, width), desiredGameMode, difficulty);
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
            string border = string.Format($"  {new string('=', 4 * GameManager.BoardWidth + 1)}");
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
                topLettersRow.Append(string.Format($"   {(char)(i + 'A')}"));
            }

            Display(topLettersRow.ToString());
        }

        private void drawRow(int i_Index) {
            StringBuilder row = new StringBuilder();
            row.Append(string.Format($"{i_Index + 1} |"));
            foreach (int j in Enumerable.Range(0, GameManager.BoardWidth))
            {
                BoardLetter currentBoardLetter = GameManager.Letters[i_Index, j];
                row.Append(string.Format(" {0} |", currentBoardLetter.IsRevealed ? currentBoardLetter.Letter : ' '));
            }
            Display(row.ToString());
        }

        private string getPlayerInput()
        {     
            return GameManager.IsCurrentPlayerHuman ? handleHumanInput() : handleAiInput();
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
                userInput = Read();
            }
            while (!GameManager.ValidatePlayerInput(userInput));

            return userInput!;
        }

        private void updateUI(string i_PlayerInput)
        {

            (i_PlayerInput == "Q" ? (Action)exit : () => GameManager.Update(Cell.Parse(i_PlayerInput)))();
            

            if(GameManager.IsSelectionNotMatching)
            {
                drawBoard();
                Display("Mismatch, but try to remember!");

                Rest(2000);

                GameManager.ChangeTurn();
            }
        }
        
        private void showAIMessage()
        {
            string message = GameManager.IsAiHasMatches ? "AI has found a match!" : "AI is thinking...";
            Display(message);
            
            if (!GameManager.IsAiHasMatches)
            {
                Rest(1000);
                for (int i = 0; i < 2; i++)
                {
                    Rest(1000);
                    Console.Write(".");
                }
            }
            
            Rest(2000);
        }

        private void exit()
        {
            Display("Goodbye!");
            Display("We hope to see you soon again!");
            Rest(2000);
            Environment.Exit(0);
        }

        private void gameOver()
        {

            drawBoard();

            Display(GameManager.GetGameOverStatus());

            (checkForRestart() ?  (Action)restartGame :  (Action)exit).Invoke();
  
        }

        private bool checkForRestart()
        {
            Display("Would you like to play again? (Y/N)");
            string userInput = Read();
            return userInput?.ToUpper() == "Y";
        }

        private void restartGame()
        {
            ClearUI();
            IMenu.GetBoardSize(out int height, out int width);
            GameManager.ResetGame(height, width);
            StartGame();
        }   
   }
}