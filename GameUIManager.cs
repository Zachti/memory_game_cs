public class GameUIManager {
      private readonly Menu r_Menu;

    public GameUIManager()
    {
        r_Menu = new Menu();
    }

    public void StartGame()
    {
        // if(GameLogicManager.CurrentGameState == eGameStates.Menu)
        // {
            string o_fPlayerName, o_sPlayerName;
            eGameModes mode = r_Menu.Start(out o_fPlayerName, out o_sPlayerName);
        // }

        // runGame();
        // gameOver();
    }

}