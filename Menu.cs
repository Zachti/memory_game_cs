public class Menu {
    public eGameModes Start(out string o_fPlayerName, out string o_sPlayerName, out int o_Height, out int o_Width) {
        Console.WriteLine("Welcome \n lets play some memory game! \n");
        getFirstPlayerName(out o_fPlayerName);
        eGameModes gameMode = selectGameMode(out o_sPlayerName);
        getBoardSize(out o_Height, out o_Width);
        return gameMode;
    }

    private void getFirstPlayerName(out string o_fPlayerName) {
        Console.WriteLine("Please enter your name: ");
        o_fPlayerName = Console.ReadLine();
        Console.WriteLine("Hi {0}, \n Welcome Aboard! \nPlease choose a game mode: ", o_fPlayerName);
    }

    private eGameModes selectGameMode(out string o_sPlayerName) {
        string gameMode = "0";
         while (gameMode != "1" && gameMode != "2")
    {
        Console.WriteLine("1. Single Player (Player vs. Computer)");
        Console.WriteLine("2. Multiplayer (Head-to-Head)");
        Console.WriteLine("Please enter 1 or 2 to choose the game mode: ");
        gameMode = validateGameMode();
    }

        if(gameMode == "2") {
            Console.WriteLine("Please enter the name of the secod player: ");
            o_sPlayerName = Console.ReadLine();
            return eGameModes.multiPlayer;
        }

        o_sPlayerName = "Computer";
        return eGameModes.singlePlayer;

    }

    private string validateGameMode() { 
    
        string gameMode = Console.ReadLine();
  
        string modeDescription = gameMode switch
        {
            "1" => "Single Player (Player vs. Computer)",
            "2" => "Multiplayer (1v1 or Head-to-Head)",
            _ => "Invalid choice. Please enter 1 or 2."
        };

        Console.WriteLine($"You have chosen {modeDescription}");
        return gameMode;
    }
 
    public void getBoardSize(out int o_Height, out int o_Width) {
        bool isEven = false;
        o_Height = o_Width = 1;
        while(!isEven) {
            o_Height = validateNumberInRange();
            o_Width = validateNumberInRange();
            isEven = (o_Height * o_Width) % 2 == 0;
            if (!isEven) {
                Console.WriteLine("Invalid input. Please enter an even number.");
            }
        }
    }

    private int validateNumberInRange() {
        int userInput = 0;
        bool isNumber = false;
        bool isWithinRange = false;
        while (!isNumber || !isWithinRange) {
            Console.WriteLine("Please enter a value (must be between 4 and 6): ");
            isNumber = int.TryParse(Console.ReadLine(), out userInput); 
            isWithinRange = userInput >= 4 && userInput <= 6;
            if (!isNumber || !isWithinRange) {
                Console.WriteLine("Invalid input. Please enter a number between 4 and 6.");
        }
    }
    return userInput;
  }
}