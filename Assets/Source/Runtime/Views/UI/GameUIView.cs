using System;
using System.Collections.Generic;
using UnityEngine;

public class GameUIView : MonoBehaviour
{
    [Serializable]
    public class GameUIViewScreen
    {
        public GameUIManager.UIScreen id;
        public GameObject gameObject;
        public GameScreenControllerBase controller;

        public GameUIViewScreen(GameUIManager.UIScreen identifier)
        {
            id = identifier;
        }

        public void SetView(GameScreenControllerBase controller, GameScreenView view)
        {
            this.controller = controller;
            if (this.controller != null)
            {
                controller.SetView(view);
            }
        }

        public void TrySetActive(GameUIManager.UIScreen activeScreen)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(id == activeScreen);
            }
        }
    }

    public GameUIViewScreen LevelScreen =
        new GameUIViewScreen(GameUIManager.UIScreen.LevelSelect);

    public GameUIViewScreen OptionsScreen =
        new GameUIViewScreen(GameUIManager.UIScreen.Options);

    public GameUIViewScreen PreGameScreen =
        new GameUIViewScreen(GameUIManager.UIScreen.PreGame);

    public GameUIViewScreen GameplayScreen =
        new GameUIViewScreen(GameUIManager.UIScreen.Game);

    public GameUIViewScreen LevelFailScreen =
        new GameUIViewScreen(GameUIManager.UIScreen.GameFail);

    public GameUIViewScreen LevelCompleteScreen =
        new GameUIViewScreen(GameUIManager.UIScreen.GameSuccess);

    public void Start()
    {
        LevelScreen.SetView(new LevelSelectScreenController(), LevelScreen.gameObject.GetComponent<GameScreenView>());
        OptionsScreen.SetView(new OptionsScreenController(), OptionsScreen.gameObject.GetComponent<GameScreenView>());
        PreGameScreen.SetView(new PreGameScreenController(), PreGameScreen.gameObject.GetComponent<GameScreenView>());
        GameplayScreen.SetView(new GameplayScreenController(), GameplayScreen.gameObject.GetComponent<GameScreenView>());
        LevelFailScreen.SetView(new LevelFailedScreenController(), LevelFailScreen.gameObject.GetComponent<GameScreenView>());
        LevelCompleteScreen.SetView(new LevelCompleteScreenController(),LevelCompleteScreen.gameObject.GetComponent<GameScreenView>());
    }

    public void DoStartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void DoNextLevel()
    {
        GameSceneManager.GoToNextLevel();
    }

    public void DoReplayLevel()
    {
        GameSceneManager.RestartLevel();
    }

    public void DoOptions()
    {
        GoToScreen(GameUIManager.UIScreen.Options);
    }

    public void DoLevels()
    {
        GoToScreen(GameUIManager.UIScreen.LevelSelect);
    }

    public void GoToScreen(GameUIManager.UIScreen screen)
    {
        LevelScreen.TrySetActive(screen);
        OptionsScreen.TrySetActive(screen);
        PreGameScreen.TrySetActive(screen);
        GameplayScreen.TrySetActive(screen);
        LevelFailScreen.TrySetActive(screen);
        LevelCompleteScreen.TrySetActive(screen);
    }
}