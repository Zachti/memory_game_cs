﻿using Microsoft.Extensions.DependencyInjection;

    public class Program
    {
        public static void Main(string[] i_Args)
        {
        var ServiceProvider = new ServiceCollection()
        .AddSingleton<IMenu, Menu>()
        .AddTransient<GameUIManager>()
        .AddSingleton<IGameData, GameData>()
        .AddTransient<GameManager>()
        .AddSingleton<Player>(provider => new Player("Player One", ePlayerTypes.Human))
        .AddSingleton<Player>(provider => new Player("Player Two", ePlayerTypes.AI))
        .AddSingleton<Board>(provider => new Board(6, 6))
        .AddSingleton<IGameMode>(provider => new GameMode(eGameModes.singlePlayer))
        .BuildServiceProvider();

        var gameUiManager = ServiceProvider.GetRequiredService<GameUIManager>();

        gameUiManager.StartGame();
        }
    }