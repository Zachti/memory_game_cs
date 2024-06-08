using Microsoft.Extensions.DependencyInjection;

namespace MemoryGame {
    internal class Program
    {
        public static void Main(string[] i_Args)
        {
            IServiceProvider ServiceProvider = configureServices();

            UIManager uiManager = ServiceProvider.GetRequiredService<UIManager>();

            uiManager.StartGame();
        }

        private static IServiceProvider configureServices()
        {
            return new ServiceCollection()
                .AddTransient<UIManager>()
                .AddTransient<GameManager>()
                .AddSingleton<IMenu, Menu>()
                .BuildServiceProvider();
        }   
    }
}