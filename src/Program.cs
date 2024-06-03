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
                .AddTransient<GameUIManager>()
                .AddTransient<GameManager>()
                .AddSingleton<IMenu, Menu>()
                .AddSingleton<IGameData, GameData>()
                .AddSingleton(provider => new GameManagerInput(provider.GetRequiredService<IGameData>(), eGameModes.singlePlayer))
                .AddSingleton(provider => new GameDataInput([new Player("AI", ePlayerTypes.AI)], new Board(4, 4)))
                .BuildServiceProvider();
        }   
    }
}