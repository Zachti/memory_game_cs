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
                .AddSingleton(provider => new GameManagerInput(provider.GetRequiredService<IGameData>(), eGameModes.singlePlayer))
                .AddSingleton(provider => new GameDataInput(
                    new Player("Player One", ePlayerTypes.Human),
                    new Player("Player Two", ePlayerTypes.AI),
                    new Board(4, 4)))
                .BuildServiceProvider();
        }   
    }
}