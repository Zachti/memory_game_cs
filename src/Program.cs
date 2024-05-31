using Microsoft.Extensions.DependencyInjection;

namespace MemoryGame {
    internal class Program
    {
        public static void Main(string[] i_Args)
        {
            IServiceProvider ServiceProvider = configureServices();

            GameUIManager gameUiManager = ServiceProvider.GetRequiredService<GameUIManager>();

            gameUiManager.StartGame();
        }

        private static IServiceProvider configureServices()
        {
            return new ServiceCollection()
                .AddSingleton<IMenu, Menu>()
                .AddTransient<GameUIManager>()
                .AddSingleton<IGameData, GameData>()
                .AddTransient<GameManager>()
                .AddSingleton<Player>(provider => new Player("Player One", ePlayerTypes.Human))
                .AddSingleton<Player>(provider => new Player("Player Two", ePlayerTypes.AI))
                .AddSingleton<Board>(provider => new Board(6, 6))
                .AddSingleton<IGameMode>(provider => new GameMode(eGameModes.singlePlayer))
                .BuildServiceProvider();
        }   
    }
}