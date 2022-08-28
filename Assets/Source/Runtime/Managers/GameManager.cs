using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController playerController;
    public int points;

    public bool gameHasStarted { get; private set; }
    public bool gameHasEnded { get; private set; }
    public bool playerHasFailed { get; private set; }

    public UnityEvent OnPlayerBlockAdded = new UnityEvent();
    public UnityEvent OnPlayerBlockRemoved = new UnityEvent();

    private void OnEnable()
    {
        Instance = this;
        TrackController.Instance.Restart();
    }

    private void Start()
    {
        TrackController.Instance.OnTrackEnd.RemoveListener(OnTrackEnded);
        TrackController.Instance.OnTrackEnd.AddListener(OnTrackEnded);
        TrackController.Instance.Restart();
        GameUIManager.ShowPregameCanvas();
    }

    private void OnTrackEnded()
    {
        EndGame(isFail: false);
    }

    public void StartGame()
    {
        TrackController.Instance.SetTrackTime(0.0f);
        TrackController.Instance.Play();
        GameUIManager.ShowGameplayCanvas();
        gameHasStarted = true;
    }

    public void EndGame(bool isFail)
    {
        gameHasEnded = true;
        TrackController.Instance.Pause();
        if (isFail)
        {
            playerHasFailed = true;
            GameUIManager.ShowLevelFailCanvas();
        }
        else
        {
            playerHasFailed = false;
            GameUIManager.ShowLevelCompleteCanvas();
        }
    }

    public void AddPoints(int amount)
    {
        if (amount > 0)
        {
            points += amount;
            GameUIManager.UpdatePointsUI();
        }
    }

    public void GivePlayerBlock()
    {
        SurfBlockView newViewRes = playerController.surfBlockParent.GetChild(0).GetComponent<SurfBlockView>();
        if (newViewRes != null)
        {
            SurfBlockView newView = GameObject.Instantiate(newViewRes, playerController.surfBlockParent);
            newView.transform.localPosition = new Vector3(0, playerController.BlockCount()-1, 0);
            newView.transform.localEulerAngles = Vector3.zero;
            
            OnPlayerBlockAdded?.Invoke();
        }
    }

    public void RemovePlayerBlock(SurfBlockView view)
    {
        SurfBlockView[] blocks = playerController.surfBlockParent.gameObject.GetComponentsInChildren<SurfBlockView>();
        if (blocks.Length <= 1)
        {
            GameManager.Instance.EndGame(true);
            return;
        }
        
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].GetInstanceID() == view.GetInstanceID())
            {
                blocks[i].Detatch();
                break;
            }
        }
        
        OnPlayerBlockRemoved?.Invoke();
    }
}