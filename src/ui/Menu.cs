namespace MemoryGame {
    internal interface IMenu
    {
        eGameModes Start(out List<Player> io_Players, out int o_Height, out int o_Width, out int? o_Difficulty);
        void GetBoardSize(out int o_Height, out int o_Width);
    }

    internal class Menu : IMenu
    {
        private static int MinBoardSize { get; } = 4;
        private static int MaxBoardSize { get; } = 6;
        private Action<string> Display { get; } = Console.WriteLine;
        private Func<string> Read { get; } = () => Console.ReadLine() ?? string.Empty;

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
            string playerName = Read();
            playerName = getValidName(playerName);
            i_Players.Add(new Player(playerName, ePlayerTypes.Human));
            Display($"\nHi {playerName}, \nWelcome Aboard! \nPlease choose a game mode: ");
        }

        private void createGameMode(List<Player> i_Players, out eGameModes o_GameMode, out int? o_Difficulty) {

            getGameMode(out bool isMultiPlayer);

            if (isMultiPlayer) {
                getAllPlayers(i_Players);
                o_Difficulty = null;
            }

            else {
                i_Players.Add(new Player("AI", ePlayerTypes.AI));
                o_Difficulty = getDifficultyLevel();
            }

            o_GameMode = isMultiPlayer ? eGameModes.multiPlayer : eGameModes.singlePlayer;  
        }

        private void getGameMode(out bool o_IsMultiPlayer) {

            string gameMode = String.Empty;
            while (gameMode != "1" && gameMode != "2") 
            {
            Display("Please choose a game mode: ");
            Display("1. Single Player (Player vs. AI)");
            Display("2. Multiplayer (Head-to-Head)");
            gameMode = validateGameMode();
            }
            o_IsMultiPlayer = gameMode == "2";
        }

        private void addPlayer(int i_index, List<Player> i_Players) {
            Display($"Please enter the name of player {i_index + 2}: ");
            string playerName = Read();
            playerName = getValidName(playerName);
            i_Players.Add(new Player(playerName, ePlayerTypes.Human));
        }

        private string getValidName(string i_name) {
            while(string.IsNullOrEmpty(i_name) || !i_name.All(char.IsLetter)) {
                Display("Invalid input. Please enter a valid name: ");
                i_name = Read();
            }
            return $"{char.ToUpper(i_name[0])}{i_name.Substring(1).ToLower()}";
        }
        
        private string validateGameMode() { 
        
            string gameMode = Read();
    
            string modeDescription = gameMode switch
            {
                "1" => "single Player (Player vs. AI)",
                "2" => "multiplayer (1v1 or Head-to-Head)",
                _ => "invalid choice. Please enter 1 or 2."
            };

            Display($"\nYou have chosen {modeDescription}\n");
            return gameMode!;
        }
    
        public void GetBoardSize(out int o_Height, out int o_Width) {
            bool isEven;
            do {
                o_Height = validateNumberInRange();
                o_Width = validateNumberInRange();
                isEven = o_Height * o_Width % 2 == 0;
                if (!isEven) {
                    Display("Invalid input. Please enter an even number.");
                }
            } while(!isEven);
        }

        private int validateNumberInRange() {
            bool isNumber, isWithinRange;
            int userInput;
            do {
                Display("Please enter a value (must be between 4 and 6): ");
                isNumber = int.TryParse(Read(), out userInput); 
                isWithinRange = userInput >= MinBoardSize && userInput <= MaxBoardSize;
                if (!isNumber || !isWithinRange) {
                    Display("Invalid input. Please enter a number between 4 and 6.");
            } 
        } while (!isNumber || !isWithinRange);
        return userInput;
    }

        private int getDifficultyLevel() {
            
            int difficultyLevel;
            bool isValidInput;
            Display("\nPlease choose a difficulty level (between 1 to 100): ");
            do {
                isValidInput = int.TryParse(Read(), out difficultyLevel);
                if (!isValidInput || difficultyLevel < 1 || difficultyLevel > 100) {
                    Display("Invalid input. Please enter a number between 1 and 100.");
                    isValidInput = false;
                }
            } while (!isValidInput);
            return difficultyLevel;
        }
    
        private void getAllPlayers(List<Player> i_Players) {
            int numOfPlayers;
            bool isNumber;

            do {
                Display("Please enter the number of players: ");
                isNumber = int.TryParse(Read(), out numOfPlayers); 
                if (!isNumber || numOfPlayers < 2) {
                Display("Invalid input. Please enter a number greater than 2.");
                }
            } while (!isNumber || numOfPlayers < 2);

            foreach (int i in Enumerable.Range(0, numOfPlayers - 1)) 
            {
                addPlayer(i, i_Players);
            }
        }
    }
}