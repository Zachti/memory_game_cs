using System.Dynamic;
using System.Runtime.CompilerServices;

namespace MemoryGame {
    internal interface IMenu
    {
        eGameModes Start(out string o_fPlayerName, out string o_sPlayerName, out int o_Height, out int o_Width, out int? o_Difficulty);
        void GetBoardSize(out int o_Height, out int o_Width);
    }

    internal class Menu : IMenu
    {
        private static int MinBoardSize { get; } = 4;
        private static int MaxBoardSize { get; } = 6;

        public eGameModes Start(out string o_fPlayerName, out string o_sPlayerName, out int o_Height, out int o_Width, out int? o_Difficulty) {
            Console.WriteLine("\nWelcome \nLet's play a memory game! \n");
            getFirstPlayerName(out o_fPlayerName);
            createGameMode(out o_sPlayerName, out eGameModes gameMode, out o_Difficulty);
            GetBoardSize(out o_Height, out o_Width);
            return gameMode;
        }

        private void getFirstPlayerName(out string o_fPlayerName) {
            Console.WriteLine("Please enter your name: ");
            o_fPlayerName = Console.ReadLine();
            o_fPlayerName = getValidName(o_fPlayerName);
            Console.WriteLine($"\nHi {o_fPlayerName}, \nWelcome Aboard! \nPlease choose a game mode: ");
        }

        private void createGameMode(out string o_sPlayerName, out eGameModes o_GameMode, out int? o_Difficulty) {

            getGameMode(out bool isMultiPlayer);

            o_GameMode = isMultiPlayer ? eGameModes.multiPlayer : eGameModes.singlePlayer;
            o_sPlayerName = isMultiPlayer ? getSecondPlayerName() : "AI";
            o_Difficulty = isMultiPlayer ? null : getDifficultyLevel();

        }

        private void getGameMode(out bool o_IsMultiPlayer) {

            string gameMode = String.Empty;
            while (gameMode != "1" && gameMode != "2") {
            Console.WriteLine("Please choose a game mode: ");
            Console.WriteLine("1. Single Player (Player vs. AI)");
            Console.WriteLine("2. Multiplayer (Head-to-Head)");
            gameMode = validateGameMode();
            }
            o_IsMultiPlayer = gameMode == "2";
        }

        private string getSecondPlayerName() {
            Console.WriteLine("Please enter the name of the second player: ");
            string? sPlayerName = Console.ReadLine();
            return getValidName(sPlayerName);
        }

        private string getValidName(string? i_name) {
            while(string.IsNullOrEmpty(i_name) || !i_name.All(char.IsLetter)) {
                Console.WriteLine("Invalid input. Please enter a valid name: ");
                i_name = Console.ReadLine();
            }
            return $"{char.ToUpper(i_name[0])}{i_name.Substring(1).ToLower()}";
        }
        
        private string validateGameMode() { 
        
            string? gameMode = Console.ReadLine();
    
            string modeDescription = gameMode switch
            {
                "1" => "single Player (Player vs. AI)",
                "2" => "multiplayer (1v1 or Head-to-Head)",
                _ => "invalid choice. Please enter 1 or 2."
            };

            Console.WriteLine($"\nYou have chosen {modeDescription}\n");
            return gameMode!;
        }
    
        public void GetBoardSize(out int o_Height, out int o_Width) {
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
                isWithinRange = userInput >= MinBoardSize && userInput <= MaxBoardSize;
                if (!isNumber || !isWithinRange) {
                    Console.WriteLine("Invalid input. Please enter a number between 4 and 6.");
            }
        }
        return userInput;
    }

        private int getDifficultyLevel() {
            
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
}