using System.Text;

namespace MemoryGame {

    internal record DifficultyOptions(eSinglePlayerDifficulty Difficulty, int Index);
    
    internal interface IMenu
    {
        eGameModes Start(out List<Player> io_Players, out int o_Height, out int o_Width, out int? o_Difficulty);
        void GetBoardSize(out int o_Height, out int o_Width);
    }

    internal class Menu : IMenu
    {
        private static int MinBoardDimension { get; } = (int)eBoardDimensionRange.Min;
        private static int MaxBoardDimension { get; } = (int)eBoardDimensionRange.Max;
        private Action<string> Display { get; } = Console.WriteLine;

        public eGameModes Start(out List<Player> io_Players, out int o_Height, out int o_Width, out int? o_Difficulty) {
            io_Players = [];
            Display("\nWelcome \nLet's play a memory game! \n");
            getFirstPlayerName(io_Players);
            createGameMode(io_Players, out eGameModes gameMode, out o_Difficulty);
            GetBoardSize(out o_Height, out o_Width);
            return gameMode;
        }

        private void getFirstPlayerName(List<Player> i_Players) {
            Display("Please enter your name: ");
            string playerName = getInput();
            playerName = getValidName(playerName);
            i_Players.Add(new Player(playerName, ePlayerTypes.Human));
            Display($"\nHi {playerName}, \nWelcome Aboard!");
        }

        private void createGameMode(List<Player> i_Players, out eGameModes o_GameMode, out int? o_Difficulty) {

            getGameMode(out bool isMultiPlayer);

            if (isMultiPlayer) {
                getAllPlayers(i_Players);
                o_Difficulty = null;
            }

            else {
                i_Players.Add(new Player("AI", ePlayerTypes.AI));
                getDifficultyLevel(out o_Difficulty);
            }

            o_GameMode = isMultiPlayer ? eGameModes.multiPlayer : eGameModes.singlePlayer;  
        }

        private void getGameMode(out bool o_IsMultiPlayer) {

            string gameMode = String.Empty;
            while (gameMode != "1" && gameMode != "2") 
            {
            Display("\nPlease choose a game mode: ");
            Display("1. Single Player (Player vs. AI)");
            Display("2. Multiplayer");
            gameMode = validateGameMode();
            }
            o_IsMultiPlayer = gameMode == "2";
        }

        private void addPlayer(int i_Index, List<Player> i_Players) {
            Display($"Please enter the name of player {i_Index + 2}: ");
            string playerName = getInput();
            playerName = getValidName(playerName);
            i_Players.Add(new Player(playerName, ePlayerTypes.Human));
        }

        private string getValidName(string i_Name) {
            while(string.IsNullOrEmpty(i_Name) || !i_Name.All(char.IsLetter)) {
                Display("Invalid input. Please enter a valid name: ");
                i_Name = getInput();
            }
            return $"{char.ToUpper(i_Name[0])}{i_Name.Substring(1).ToLower()}";
        }
        
        private string validateGameMode() { 
        
            string gameMode = getInput();
    
            string modeDescription = gameMode switch
            {
                "1" => "single Player (Player vs. AI)",
                "2" => "multiplayer",
                _ => "invalid choice. Please enter 1 or 2."
            };

            Display($"\nYou have chosen {modeDescription}\n");
            return gameMode!;
        }
    
        public void GetBoardSize(out int o_Height, out int o_Width) {
            bool isValid;
            do {
                o_Height = validateNumberInRange();
                o_Width = validateNumberInRange();
                isValid = Board.IsValid(o_Width, o_Height);
                if (!isValid) {
                    Display("Invalid input. Please enter an even number.");
                }
            } while(!isValid);
        }

        private int validateNumberInRange() {
            bool isNumber, isWithinRange;
            int userInput;
            do {
                Display($"Please enter a value (must be between {MinBoardDimension} and {MaxBoardDimension}): ");
                isNumber = int.TryParse(getInput(), out userInput); 
                isWithinRange = userInput >= MinBoardDimension && userInput <= MaxBoardDimension;
                if (!isNumber || !isWithinRange) {
                    Display($"Invalid input. Please enter a number between {MinBoardDimension} and {MaxBoardDimension}.");
            } 
        } while (!isNumber || !isWithinRange);
        return userInput;
    }

        private void getDifficultyLevel(out int? o_Difficulty) {
            
            displayDifficultyOptions(out DifficultyOptions[] difficultyOptions);

            bool isValidInput;

            do {
                Display("\nEnter the number corresponding to your choice: ");
                isValidInput = int.TryParse(getInput(), out int selectedOption)
                    && difficultyOptions.Any(option => option.Index == selectedOption);

                o_Difficulty = isValidInput ? 
                (int)difficultyOptions.First(option => option.Index == selectedOption).Difficulty 
                : null;
                
                if (!isValidInput) {
                    Display("Invalid input. Please enter a valid option.");
                }
            } while (!isValidInput);
        }

        private void displayDifficultyOptions(out DifficultyOptions[] io_DifficultyOptions) {

            io_DifficultyOptions = Enum.GetValues(typeof(eSinglePlayerDifficulty))
                                        .Cast<eSinglePlayerDifficulty>()
                                        .Select((value, index) => new DifficultyOptions(value, index + 1))
                                        .ToArray();

            StringBuilder difficultyOptionsMessage = new StringBuilder("\nPlease choose a difficulty level:\n");
            Array.ForEach(io_DifficultyOptions, option => 
            difficultyOptionsMessage.Append($"{option.Index}. {option.Difficulty}\n"));
            Display(difficultyOptionsMessage.ToString());
        }
    
        private void getAllPlayers(List<Player> i_Players) {
            int numOfPlayers;
            bool isNumber;

            do {
                Display("Please enter the number of players: ");
                isNumber = int.TryParse(getInput(), out numOfPlayers); 
                if (!isNumber || numOfPlayers < 2) {
                Display("Invalid input. Please enter a number greater than 2.");
                }
            } while (!isNumber || numOfPlayers < 2);

            foreach (int i in Enumerable.Range(0, numOfPlayers - 1)) 
            {
                addPlayer(i, i_Players);
            }
        }
    
        private string getInput() => Console.ReadLine() ?? string.Empty;
    }
}