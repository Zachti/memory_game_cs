
using System.Text;

public class GameUIManager {
      private readonly Menu r_Menu;
      private GameManager m_GameManager;

    public GameUIManager()
    {
        r_Menu = new Menu();
    }

    public void StartGame()
    {
        if(GameManager.CurrentGameState == eGameStates.Menu)
        {

            RunMenu(); 
        }

        RunGame();
        // gameOver();
    }

    private void RunMenu()
    {
        string fPlayerName, sPlayerName;
        int height, width;

        eGameModes desiredGameMode = r_Menu.Start(out fPlayerName, out sPlayerName, out height, out width);
        Player fPlayer = new Player(fPlayerName, ePlayerTypes.Human);
        ePlayerTypes type = desiredGameMode == eGameModes.multiPlayer ? ePlayerTypes.Human : ePlayerTypes.Computer;
        Player sPlayer = new Player(sPlayerName, type);

        m_GameManager = new GameManager(fPlayer, sPlayer, height, width, desiredGameMode);
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
        //drawComputerMessage();
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
            isValidInput = m_GameManager.validatePlayerInput(userInput);
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
            //stopGame();
        }
        m_GameManager.Update(Cell.Parse(i_PlayerInput));

        if(m_GameManager.SelectionNotMatching)
        {
            DrawBoard();
            Console.WriteLine("Mismatch, remember this!");

            System.Threading.Thread.Sleep(2000);

            m_GameManager.ChangeTurn();
        }
    }
}