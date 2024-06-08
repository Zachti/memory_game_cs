using System.Text;

namespace MemoryGame {

    internal record DifficultyOptions(eSinglePlayerDifficulty Difficulty, int Index);
    
    internal interface IMenu
    {
        void Start(out List<Player> io_Players, out int o_Height, out int o_Width, out int? o_Difficulty);
        void GetBoardSize(out int o_Height, out int o_Width);
    }

    internal class Menu(GameManager i_GameManager) : IMenu
    {
        private static int MinBoardDimension { get; } = (int)eBoardDimensionRange.Min;
        private static int MaxBoardDimension { get; } = (int)eBoardDimensionRange.Max;
        private GameManager GameManager { get; } = i_GameManager;
        private Action<string> Display { get; } = Console.WriteLine;

        public void Start(out List<Player> io_Players, out int o_Height, out int o_Width, out int? o_Difficulty) {
            io_Players = [];
            Display("\nWelcome \nLet's play a memory game! \n");
            getFirstPlayerName(io_Players);
            createGameMode(io_Players, out o_Difficulty);
            GetBoardSize(out o_Height, out o_Width);
        }

        private void getFirstPlayerName(List<Player> i_Players) {
            Display("Please enter your name: ");
            string playerName = GetInputOrEmpty();
            playerName = getValidName(playerName);
            i_Players.Add(new Player(playerName, ePlayerTypes.Human));
            Display($"\nHi {playerName}, \nWelcome Aboard!");
        }

        private void createGameMode(List<Player> i_Players, out int? o_Difficulty) {

            getGameMode(out bool isMultiPlayer);

            if (isMultiPlayer) {
                getAllPlayers(i_Players);
                o_Difficulty = null;
            }

            else {
                i_Players.Add(new Player("AI", ePlayerTypes.AI));
                getDifficultyLevel(out o_Difficulty);
            }
        }

        private void getGameMode(out bool o_IsMultiPlayer) {

            eGameModes gameMode;
            do 
            {
                gameMode = EnumMenuToEnumChoice<eGameModes>("\nPlease choose a game mode: ");
                validateGameMode(gameMode);
            } while (gameMode != eGameModes.Single_Player && gameMode != eGameModes.Multi_Player);
            o_IsMultiPlayer = gameMode == eGameModes.Multi_Player;
        }

        private void addPlayer(int i_Index, List<Player> i_Players) {
            Display($"Please enter the name of player {i_Index + 2}: ");
            string playerName = GetInputOrEmpty();
            playerName = getValidName(playerName);
            i_Players.Add(new Player(playerName, ePlayerTypes.Human));
        }

        private string getValidName(string i_Name) {
            while(string.IsNullOrEmpty(i_Name) || !i_Name.All(char.IsLetter)) {
                Display("Invalid input. Please enter a valid name: ");
                i_Name = GetInputOrEmpty();
            }
            return $"{char.ToUpper(i_Name[0])}{i_Name.Substring(1).ToLower()}";
        }
        
        private void validateGameMode(eGameModes i_GameMode) { 
            
            string modeDescription = i_GameMode switch
            {
                eGameModes.Single_Player => "single Player (Player vs. AI)",
                eGameModes.Multi_Player => "multiplayer",
                _ => "invalid choice. Please enter 1 or 2."
            };

            Display($"\nYou have chosen {modeDescription}\n");
        }
    
        public void GetBoardSize(out int o_Height, out int o_Width) {
            bool isValid;
            do {
                o_Height = validateNumberInRange();
                o_Width = validateNumberInRange();
                isValid = GameManager.IsBoardValid(o_Width, o_Height);
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
                isNumber = int.TryParse(GetInputOrEmpty(), out userInput); 
                isWithinRange = userInput >= MinBoardDimension && userInput <= MaxBoardDimension;
                if (!isNumber || !isWithinRange) {
                    Display($"Invalid input. Please enter a number between {MinBoardDimension} and {MaxBoardDimension}.");
            } 
        } while (!isNumber || !isWithinRange);
        return userInput;
    }

        private void getDifficultyLevel(out int? o_Difficulty) {
            
            eSinglePlayerDifficulty difficulty;

            do {
                difficulty = EnumMenuToEnumChoice<eSinglePlayerDifficulty>("\nPlease choose a difficulty level: ");
                o_Difficulty = difficulty switch {
                    eSinglePlayerDifficulty.Baginner => 0,
                    eSinglePlayerDifficulty.Medium => 40,
                    eSinglePlayerDifficulty.Expert => 70,
                    eSinglePlayerDifficulty.Impossible => 100,
                    _ => null
                };
            } while (o_Difficulty == null);
        }
    
        private void getAllPlayers(List<Player> i_Players) {
            int numOfPlayers;
            bool isNumber;

            do {
                Display("Please enter the number of players: ");
                isNumber = int.TryParse(GetInputOrEmpty(), out numOfPlayers); 
                if (!isNumber || numOfPlayers < 2) {
                Display("Invalid input. Please enter a number greater than 2.");
                }
            } while (!isNumber || numOfPlayers < 2);

            foreach (int i in Enumerable.Range(0, numOfPlayers - 1)) 
            {
                addPlayer(i, i_Players);
            }
        }
    
        private void enumToMenu<TEnum>(string? i_OpenMessage) where TEnum : Enum
        {
            StringBuilder menu = new StringBuilder(i_OpenMessage ?? "").AppendLine();
            int index = 1;
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                menu.AppendLine($"{index}. {value.ToString().Replace("_", " ")}");
                index++;
            }
            Console.WriteLine(menu.ToString());
        }
        
        private T EnumMenuToEnumChoice<T>(string i_Message) where T : Enum {
            enumToMenu<T>(i_Message);
            return (T)(object)GetSingleDigit();
        }

        private int GetSingleDigit() 
        {

            string input = GetInputOrEmpty();

            return int.Parse(input);
        }
        
        private string GetInputOrEmpty() => Console.ReadLine() ?? string.Empty;
    }
}