using System.Dynamic;
using System.Runtime.CompilerServices;

public interface IMenu
{
    eGameModes Start(out string o_fPlayerName, out string o_sPlayerName, out int o_Height, out int o_Width, out int? o_Difficulty);
    void GetBoardSize(out int o_Height, out int o_Width);
}

public class Menu : IMenu
{
    public eGameModes Start(out string o_fPlayerName, out string o_sPlayerName, out int o_Height, out int o_Width, out int? o_Difficulty) {
        Console.WriteLine("\nWelcome \nLet's play a memory game! \n");
        GetFirstPlayerName(out o_fPlayerName);
        eGameModes gameMode = SelectGameMode(out o_sPlayerName, out o_Difficulty);
        GetBoardSize(out o_Height, out o_Width);
        return gameMode;
    }

    private void GetFirstPlayerName(out string o_fPlayerName) {
        Console.WriteLine("Please enter your name: ");
        o_fPlayerName = Console.ReadLine();
        o_fPlayerName = GetValidName(o_fPlayerName);
        Console.WriteLine("\nHi {0}, \nWelcome Aboard! \nPlease choose a game mode: ", o_fPlayerName);
    }

    private eGameModes SelectGameMode(out string o_sPlayerName, out int? o_Difficulty) {
        string gameMode = String.Empty;
        o_sPlayerName = "AI";
        o_Difficulty = null;
        while (gameMode != "1" && gameMode != "2")
        {
            Console.WriteLine("1. Single Player (Player vs. AI)");
            Console.WriteLine("2. Multiplayer (Head-to-Head)");
            Console.WriteLine("\nPlease enter 1 or 2 to choose the game mode: ");
            gameMode = ValidateGameMode();
        }

        if (gameMode == "2") {
            o_sPlayerName = GetSecondPlayerName();
            return eGameModes.multiPlayer;
        }

        o_Difficulty = GetDifficultyLevel();

        return eGameModes.singlePlayer;
    }

    private string GetSecondPlayerName() {
        Console.WriteLine("Please enter the name of the second player: ");
        string? sPlayerName = Console.ReadLine();
        return GetValidName(sPlayerName);
    }

    private string GetValidName(string? i_name) {
        while(string.IsNullOrEmpty(i_name)) {
            Console.WriteLine("Invalid input. Please enter a valid name: ");
            i_name = Console.ReadLine();
        }
        return i_name;
    }
    
    private string ValidateGameMode() { 
    
        string? gameMode = Console.ReadLine();
  
        string modeDescription = gameMode switch
        {
            "1" => "Single Player (Player vs. AI)",
            "2" => "Multiplayer (1v1 or Head-to-Head)",
            _ => "Invalid choice. Please enter 1 or 2."
        };

        Console.WriteLine($"\nYou have chosen {modeDescription}\n");
        return gameMode!;
    }
 
    public void GetBoardSize(out int o_Height, out int o_Width) {
        bool isEven = false;
        o_Height = o_Width = 1;
        while(!isEven) {
            o_Height = ValidateNumberInRange();
            o_Width = ValidateNumberInRange();
            isEven = (o_Height * o_Width) % 2 == 0;
            if (!isEven) {
                Console.WriteLine("Invalid input. Please enter an even number.");
            }
        }
    }

    private int ValidateNumberInRange() {
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

    private int GetDifficultyLevel() {
        
        int difficultyLevel = 1;
        Console.WriteLine("\nPlease choose a difficulty level (between 1 to 100): ");
        bool isValidInput = false;
        while (!isValidInput) {
            isValidInput = int.TryParse(Console.ReadLine(), out difficultyLevel);
            if (!isValidInput || difficultyLevel < 1 || difficultyLevel > 100) {
                Console.WriteLine("Invalid input. Please enter a number between 1 and 100.");
                isValidInput = false;
            }
        }
        return difficultyLevel;
    }
}