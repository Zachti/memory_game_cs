
using System.Text;

namespace MemoryGame {
    internal class GameUIManager(IMenu i_Menu, GameManager i_GameManager)
    {
        private IMenu IMenu { get; } = i_Menu;
        private GameManager GameManager { get; } = i_GameManager;

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
                    string playerInput = getPlayerInput();
                    updateUI(playerInput.ToUpper());
                }
            }
        
        private void clearWindow()
        {
        Console.Clear();
        }

        private void drawBoard()
        {
            clearWindow();
            Console.WriteLine($"\nCurrent Turn: {GameManager.CurrentPlayer.PlayerName}\n");
            drawTopLetterRow(GameManager.BoardWidth);
            string border = string.Format($"  {new string('=', 4 * GameManager.BoardWidth + 1)}");
            Console.WriteLine(border);
            foreach (int i in Enumerable.Range(0, GameManager.BoardHeight))
            {
                drawRow(i);
                Console.WriteLine(border);
            }
        }

        private void drawTopLetterRow(int i_LengthOfRow)
        {
            StringBuilder topLettersRow = new StringBuilder(" ");

            foreach (int i in Enumerable.Range(0, i_LengthOfRow))
            {
                topLettersRow.Append(string.Format($"   {(char)(i + 'A')}"));
            }

            Console.WriteLine(topLettersRow.ToString());
        }

        private void drawRow(int i_Index) {
            StringBuilder row = new StringBuilder();
            row.Append(string.Format($"{i_Index + 1} |"));
            foreach (int j in Enumerable.Range(0, GameManager.BoardWidth))
            {
                BoardLetter currentBoardLetter = GameManager.Letters[i_Index, j];
                row.Append(string.Format(" {0} |", currentBoardLetter.IsRevealed ? currentBoardLetter.Letter : ' '));
            }
            Console.WriteLine(row.ToString());
        }

        private string getPlayerInput()
        {     
            return GameManager.CurrentPlayer.Type == ePlayerTypes.Human
                ? handleHumanInput(GameManager.CurrentPlayer.PlayerName)
                : handleAiInput();
        }

        private string handleAiInput()
        {
            string aiInput = GameManager.GetAiInput();
            showAIMessage();
            return aiInput;
        }

        private string handleHumanInput(string i_PlayerName)
        {
            string userInput;
            do
            {
                Console.WriteLine($"{i_PlayerName}, Please enter your selection: ");
                userInput = Console.ReadLine();
            }
            while (!GameManager.ValidatePlayerInput(userInput));

            return userInput!;
        }

        private void updateUI(string i_PlayerInput)
        {

            (i_PlayerInput == "Q" ? (Action)exit : () => GameManager.Update(Cell.Parse(i_PlayerInput)))();
            

            if(GameManager.SelectionNotMatching)
            {
                drawBoard();
                Console.WriteLine("Mismatch, but try to remember!");

                System.Threading.Thread.Sleep(2000);

                GameManager.ChangeTurn();
            }
        }
        
        private void showAIMessage()
        {
            if(GameManager.AiHasMatches) 
            {
                Console.WriteLine("AI has found a match!");
            }
            else 
            {
                Console.WriteLine("AI is thinking...");
                System.Threading.Thread.Sleep(1000);
                for (int i = 0; i < 2; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.Write(".");
                }
            }
            System.Threading.Thread.Sleep(2000);
        }

        private void exit()
        {
            Console.WriteLine("Goodbye!");
            Console.WriteLine("We hope to see you soon again!");
            System.Threading.Thread.Sleep(2000);
            Environment.Exit(0);
        }

        private void gameOver()
        {

            drawBoard();

            Console.WriteLine(GameManager.GetGameOverStatus());

            (checkForRestart() ? new Action(restartGame) : new Action(exit)).Invoke();
  
        }

        private bool checkForRestart()
        {
            Console.WriteLine("Would you like to play again? (Y/N)");
            string userInput = Console.ReadLine();
            return userInput?.ToUpper() == "Y";
        }

        private void restartGame()
        {
            clearWindow();
            IMenu.GetBoardSize(out int height, out int width);
            GameManager.ResetGame(height, width);
            StartGame();
        }   
    }
}