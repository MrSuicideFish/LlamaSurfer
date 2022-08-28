using System;
using System.Collections;
using UnityEngine;

public class GameScreenView : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(OnShow());
    }

    private void OnDisable()
    {
        OnHide();
    }

    public virtual IEnumerator OnShow()
    {
        yield break;
    }
    
    public virtual void OnHide(){}
}