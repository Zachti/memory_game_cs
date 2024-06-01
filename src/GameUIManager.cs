
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
            string fPlayerName, sPlayerName;
            int height, width;
            int? difficulty;

            eGameModes desiredGameMode = IMenu.Start(out fPlayerName, out sPlayerName, out height, out width, out difficulty);
            Player fPlayer = new Player(fPlayerName, ePlayerTypes.Human);
            ePlayerTypes type = desiredGameMode == eGameModes.multiPlayer ? ePlayerTypes.Human : ePlayerTypes.AI;
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
            Console.WriteLine();
            Console.WriteLine($"Current Turn: {GameManager.CurrentPlayer.PlayerName}");
            Console.WriteLine();    
            drawTopLetterRow(GameManager.BoardWidth);
            string border = string.Format($"  {new string('=', 4 * GameManager.BoardWidth + 1)}");
            Console.WriteLine(border);
            for (int i = 0; i < GameManager.BoardHeight; i++)
            {
                drawRow(i);
                Console.WriteLine(border);
            }
        }

        private void drawTopLetterRow(int i_LengthOfRow)
        {
            StringBuilder topLettersRow = new StringBuilder(" ");

            for (int i = 0; i < i_LengthOfRow; i++)
            {
                topLettersRow.Append(string.Format($"   {(char)(i + 'A')}"));
            }

            Console.WriteLine(topLettersRow.ToString());
        }

        private void drawRow(int i_Index) {
            StringBuilder row = new StringBuilder();
            row.Append(string.Format($"{i_Index + 1} |"));
            for (int j = 0; j < GameManager.BoardWidth; j++)
            {
                BoardLetter currentBoardLetter = GameManager.Letters[i_Index, j];
                row.Append(string.Format(" {0} |", currentBoardLetter.IsRevealed ? currentBoardLetter.Letter : ' '));
            }
            Console.WriteLine(row.ToString());
        }

        private string getPlayerInput()
        {     
            string playerInput;
            if (GameManager.CurrentPlayer.Type == ePlayerTypes.Human)
            {
                playerInput = getHumanInput(GameManager.CurrentPlayer.PlayerName);
            }
            else
            {
                playerInput = GameManager.GetAiInput();
                showAIMessage();
            }
            return playerInput;
        }

        private string getHumanInput(string i_PlayerName)
        {
            string userInput = string.Empty;
            bool isValidInput = false;
            while (!isValidInput)
            {
                Console.WriteLine($"{i_PlayerName}, Please enter your selection: ");
                userInput = Console.ReadLine();
                isValidInput = GameManager.ValidatePlayerInput(userInput);
            }

            return userInput!;
        }

        private void updateUI(string i_PlayerInput)
        {
            if(i_PlayerInput == "Q")
            {
                exit();
            }
            GameManager.Update(Cell.Parse(i_PlayerInput));

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
            if(GameManager.AiHasMatches) {
                Console.WriteLine("AI has found a match!");
                System.Threading.Thread.Sleep(2000);
                return;
            }

            Console.WriteLine("AI is thinking...");
            System.Threading.Thread.Sleep(2000);
            System.Threading.Thread.Sleep(1000);
            Console.Write('.');
            System.Threading.Thread.Sleep(1000);
            Console.Write('.');
            System.Threading.Thread.Sleep(1000);
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

            if(checkForRestart())
            {
                clearWindow();
                restartGame();
            }
            else
            {
                exit();
            }
        }

        private bool checkForRestart()
        {
            Console.WriteLine("Would you like to play again? (Y/N)");
            string userInput = Console.ReadLine();
            return userInput?.ToUpper() == "Y";
        }

        private void restartGame()
        {
            IMenu.GetBoardSize(out int height, out int width);
            GameManager.ResetGame(height, width);
            StartGame();
        }   
    }
}