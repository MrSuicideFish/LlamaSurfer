using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameScreenView : GameScreenView
{
    public override IEnumerator OnShow()
    {
        yield break;
    }

    public override void OnHide()
    {
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
