using UnityEngine;

public class GameUIManager
{
    public enum UIScreen
    {
        LevelSelect,
        Options,
        PreGame,
        Game,
        GameFail,
        GameSuccess,
    }
    
    private GameUIManager(GameUIView view)
    {
        _view = view;
    }

    private static GameUIManager instance;
    private GameUIView _view;
    private static GameUIManager GetInstance()
    {
        if (instance == null)
        {
            GameUIView view = Object.FindObjectOfType<GameUIView>();
            if (view == null)
            {
                GameUIView viewRes = Resources.Load<GameUIView>("UI/GameUIView");
                if (viewRes != null)
                {
                    view = Object.Instantiate(viewRes, GameSceneManager.Instance.transform, true);
                }
            }
            
            instance = new GameUIManager(view);
        }
        return instance;
    }

    public static void UpdatePointsUI()
    {
        GameplayScreenController controller = (GameplayScreenController) instance._view.GameplayScreen.controller;
        if (controller != null)
        {
            controller.UpdatePoints();
        }
    }

    public static void ResetPoints()
    {
        
    }
    
    public static void ShowPregameCanvas()
    {
        GetInstance()._view.GoToScreen(UIScreen.PreGame);
    }

    public static void ShowGameplayCanvas()
    {
        GetInstance()._view.GoToScreen(UIScreen.Game);
    }

    public static void ShowLevelFailCanvas()
    {
        GetInstance()._view.GoToScreen(UIScreen.GameFail);
    }
    
    public static void ShowLevelCompleteCanvas()
    {
        GetInstance()._view.GoToScreen(UIScreen.GameSuccess);
    }

    public static void ShowOptionsCanvas()
    {
        GetInstance()._view.GoToScreen(UIScreen.Options);
    }

    public static void ShowLevelsCanvas()
    {
        GetInstance()._view.GoToScreen(UIScreen.LevelSelect);
    }
}