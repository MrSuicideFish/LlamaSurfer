using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager instance;
    public static GameSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("_SCENE_MANAGER");
                instance = obj.AddComponent<GameSceneManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    public static void GoToNextLevel()
    {
        
    }

    public static void RestartLevel()
    {
        
    }
}