using System;
using System.Collections;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LevelCompleteView : GameScreenView
{
    public TextMeshProUGUI pointCountText;
    public Image playButton;
    public RectTransform contentWindow;
    public Image[] stars;

    private int currentPoints;
    private int maxPoints;

    private Tween DoCountPoints()
    {
        // do points counting
        currentPoints = 0;
        int targetPoints = GameManager.Instance.points;
        maxPoints = LevelCfgDb.GetCurrentLevel().maxPoints;

        return DOTween.To(
                () => currentPoints, x => currentPoints = x, targetPoints, 3).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                pointCountText.rectTransform.DOPunchScale(Vector3.one * 1.2f, 0.3f, 1, 0.1f);
            });
    }

    private Tween DoPresentStars(int count)
    {
        Sequence seq = DOTween.Sequence();
        
        // move all stars to top of screen
        foreach(Image star in stars)
        {
            star.rectTransform.anchoredPosition = contentWindow.rect.position;
            star.color = new Color(1, 1, 1, 0.0f);
        }

        for (int i = 0; i < count; i++)
        {
            Sequence starSeq = DOTween.Sequence();
            starSeq.Join(stars[i].DOColor(new Color(1, 1, 1, 1), 0.1f));
            starSeq.Join(stars[i].rectTransform.DOAnchorPos(new Vector2(0,0), 0.1f));
            starSeq.Append(stars[i].rectTransform.DOPunchScale(Vector3.one * 1.2f, 0.1f, vibrato: 1, elasticity: 0.1f));
            seq.Append(starSeq);
        }
        
        return seq;
    }

    private void HidePlayButton()
    {
        foreach (Image img in playButton.GetComponentsInChildren<Image>())
        {
            img.DOColor(new Color(1, 1, 1, 0), 0.0f);
        }
    }

    private void ShowPlayButton()
    {
        foreach (Image img in playButton.GetComponentsInChildren<Image>())
        {
            img.DOColor(new Color(1, 1, 1, 1), 0.5f);
        }

        playButton.rectTransform.DOPunchScale(Vector3.one * 2.0f, 0.3f, vibrato: 1, elasticity: 0.1f);
    }

    public override IEnumerator OnShow()
    {
        HidePlayButton();
        contentWindow.anchoredPosition = new Vector2(0, -3000);

        Sequence lvlCompleteSeq = DOTween.Sequence();
        lvlCompleteSeq.Append(contentWindow.DOAnchorPos(Vector2.zero, 1.4f, false).SetEase(Ease.OutBounce));
        lvlCompleteSeq.Append(DoCountPoints());
        
        lvlCompleteSeq.Append(DoPresentStars(
            Mathf.RoundToInt((float)
                    Math.Round(RoundDown(GameManager.Instance.points / (float) maxPoints, 1), 1)*10.0f)));
        lvlCompleteSeq.OnComplete(ShowPlayButton).Play();
        yield break;
    }
    
    public double RoundDown(double number, int decimalPlaces)
    {
        return Math.Floor(number * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
    }

    public override void OnHide()
    {
    }

    private void Update()
    {
        pointCountText.SetText("{0} / {1}", currentPoints, maxPoints);
    }
}