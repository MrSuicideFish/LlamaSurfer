using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways]
public class PlayerController : MonoBehaviour
{
    public CharacterController charController;

    public Transform surfBlockParent;

    // locomotion
    public float moveSpeed;
    public float moveWidth;
    
    public float moveX { get; private set; }

    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    public float moveSmoothTime;

    public void Move(float position)
    {
        if (GameManager.Instance.gameHasStarted && !GameManager.Instance.gameHasEnded)
        {
            moveX = Remap(position, 0.0f, 1.0f, -1.0f, 1.0f);
        }
    }
    
    public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax)
    {
        var fromAbs  =  from - fromMin;
        var fromMaxAbs = fromMax - fromMin;      
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }
    
    private void Update()
    {
        UpdatePosition();
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdatePosition();
        }
        #endif
        charController.transform.position = targetPosition;
    }

    private void FixedUpdate()
    {
        if (TrackController.Instance != null)
        {
            /*
            charController.transform.position = Vector3.SmoothDamp(
                charController.transform.position,
                targetPosition,
                ref currentVelocity,
                moveSmoothTime * Time.deltaTime);
                */
        }
    }

    private void UpdatePosition()
    {
        Vector3 trackPosition = TrackController.Instance.GetTrackPosition();
        Vector3 trackTangent = TrackController.Instance.GetTrackTangent();
        Vector3 cross = Vector3.Cross(Vector3.up, trackTangent);
        targetPosition = trackPosition + (cross * (moveX * moveWidth));
    }

    public int BlockCount()
    {
        return surfBlockParent.childCount;
    }
}