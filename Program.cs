using Microsoft.Extensions.DependencyInjection;

    public class Program
    {
        public static void Main(string[] i_Args)
        {
        var services = new ServiceCollection();

        services.AddSingleton<IMenu, Menu>();
        services.AddTransient<GameUIManager>();

        var serviceProvider = services.BuildServiceProvider();

        var gameUiManager = serviceProvider.GetRequiredService<GameUIManager>();

        gameUiManager.StartGame();
        }
    }