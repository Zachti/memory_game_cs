
using System.Text;

public class GameUIManager {
      private readonly IMenu r_Menu;
      private GameManager m_GameManager;

   public GameUIManager(IMenu i_Menu, GameManager i_GameManager)
    {
        r_Menu = i_Menu;
        m_GameManager = i_GameManager;
    }

    public void StartGame()
    {
        RunMenu(); 
        RunGame();
        GameOver();
    }

    private void RunMenu()
    {
        string fPlayerName, sPlayerName;
        int height, width;
        int? difficulty;

        eGameModes desiredGameMode = r_Menu.Start(out fPlayerName, out sPlayerName, out height, out width, out difficulty);
        Player fPlayer = new Player(fPlayerName, ePlayerTypes.Human);
        ePlayerTypes type = desiredGameMode == eGameModes.multiPlayer ? ePlayerTypes.Human : ePlayerTypes.AI;
        Player sPlayer = new Player(sPlayerName, type);
        m_GameManager.Initializae(fPlayer, sPlayer, new Board(height, width), desiredGameMode, difficulty);
    }

    private void RunGame()
        {
            while(GameManager.CurrentGameState == eGameStates.Running)
            {
                DrawBoard();
                string playerInput = GetPlayerInput();
                UpdateUI(playerInput);
            }
        }
    
    private void ClearWindow()
    {
       Console.Clear();
    }

    private void DrawBoard()
    {
        ClearWindow();
        Console.WriteLine();
        Console.WriteLine($"Current Turn: {m_GameManager.CurrentPlayer.PlayerName}");
        Console.WriteLine();    
        DrawTopLetterRow(m_GameManager.BoardWidth);
        string border = string.Format("  {0}", new string('=', 4 * m_GameManager.BoardWidth + 1));
        Console.WriteLine(border);
        for (int i = 0; i < m_GameManager.BoardHeight; i++)
        {
            DrawRow(i);
            Console.WriteLine(border);
        }
    }

    private void DrawTopLetterRow(int i_LengthOfRow)
    {
        StringBuilder topLettersRow = new StringBuilder(" ");

        for (int i = 0; i < i_LengthOfRow; i++)
        {
            topLettersRow.Append(string.Format("   {0}", (char)(i + 'A')));
        }

        Console.WriteLine(topLettersRow.ToString());
    }

    private void DrawRow(int i_Index) {
        StringBuilder row = new StringBuilder();
        row.Append(string.Format("{0} |", i_Index + 1));
        for (int j = 0; j < m_GameManager.BoardWidth; j++)
        {
            BoardLetter currentBoardLetter = m_GameManager.Letters[i_Index, j];
            row.Append(string.Format(" {0} |", currentBoardLetter.IsRevealed ? currentBoardLetter.Letter : ' '));
        }
        Console.WriteLine(row.ToString());
    }

    private string GetPlayerInput()
    {     
        if (m_GameManager.CurrentPlayer.Type == ePlayerTypes.Human)
        {
            return GetHumanInput(m_GameManager.CurrentPlayer.PlayerName);
        }
        string aiInput = m_GameManager.GetAiInput();
        ShowAIMessage();
        return aiInput;
    }

    private string GetHumanInput(string i_PlayerName)
    {
        string userInput = string.Empty;
        bool isValidInput = false;
        while (!isValidInput)
        {
            Console.WriteLine("{0}, Please enter your selection: ", i_PlayerName);
            userInput = Console.ReadLine();
            isValidInput = m_GameManager.ValidatePlayerInput(userInput);
            if (!isValidInput)
            {
                Console.WriteLine("\nInvalid input. Please enter a valid selection.");
            }
        }

        return userInput!;
    }

    private void UpdateUI(string i_PlayerInput)
    {
        if(i_PlayerInput == "Q")
        {
            Exit();
        }
        m_GameManager.Update(Cell.Parse(i_PlayerInput));

        if(m_GameManager.SelectionNotMatching)
        {
            DrawBoard();
            Console.WriteLine("Mismatch, but try to remember!");

            System.Threading.Thread.Sleep(2000);

            m_GameManager.ChangeTurn();
        }
    }
    
    private void ShowAIMessage()
    {
        if(m_GameManager.AiHasMatches) {
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

    private void Exit()
    {
        Console.WriteLine("Goodbye!");
        Console.WriteLine("We hope to see you soon again!");
        System.Threading.Thread.Sleep(2000);
        Environment.Exit(0);
    }

    private void GameOver()
    {

        DrawBoard();

        Console.WriteLine(m_GameManager.GetGameOverStatus());

        if(CheckForRestart())
        {
            ClearWindow();
            RestartGame();
        }
        else
        {
            Exit();
        }
    }

    private bool CheckForRestart()
    {
        Console.WriteLine("Would you like to play again? (Y/N)");
        string userInput = Console.ReadLine();
        return userInput?.ToUpper() == "Y";
    }

     private void RestartGame()
    {
        r_Menu.GetBoardSize(out int height, out int width);
        m_GameManager.ResetGame(height, width);
        StartGame();
    }   
}