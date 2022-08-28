using UnityEngine;

public class GameScreenControllerBase
{
    protected GameScreenView _view;
    public void SetView(GameScreenView view)
    {
        _view = view;
    }

    public void Show()
    {
        _view.StartCoroutine(_view.OnShow());
    }

    public void Hide()
    {
        _view.OnHide();
    }
}