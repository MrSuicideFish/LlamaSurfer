
using System;
using System.Collections;

public class LevelSelectScreenView : GameScreenView
{
    public override IEnumerator OnShow()
    {
        yield break;
    }

    public override void OnHide()
    {
    }

    public void GoToLevel(int level)
    {
        
    }

    public void Back()
    {
        if (GameManager.Instance.gameHasStarted 
            && GameManager.Instance.gameHasEnded)
        {
            if (GameManager.Instance.playerHasFailed)
            {
                GameUIManager.ShowLevelFailCanvas();
            }
            else
            {
                GameUIManager.ShowLevelCompleteCanvas();
            }
        }
    }
}