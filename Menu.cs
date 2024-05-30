public class Menu {
    public eGameModes Start(out string o_fPlayerName, out string o_sPlayerName) {
        Console.WriteLine("Welcome \n lets play some memory game! \n");
        getFirstPlayerName();
        eGameModes gameMode = selectGameMode(out string o_sPlayerName);
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

        if(gameMode == '2') {
            Console.WriteLine('Please enter the name of the secod player: ');
            o_sPlayerName = Console.ReadLine();
            return eGameModes.multiPlayer;
        }

        return eGameModes.singlePlayer;

    }

    private string validateGameMode() {
    
        string gameMode = Console.ReadLine();
  
        modeDescription = gameMode switch
        {
            "1" => "Single Player (Player vs. Computer)",
            "2" => "Multiplayer (1v1 or Head-to-Head)",
            _ => "Invalid choice. Please enter 1 or 2."
        };

        Console.WriteLine($"You have chosen {modeDescription}");
        return gameMode;
    }

}