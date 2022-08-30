using System;
using Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    
    public CinemachineVirtualCamera mainTrackCamera;
    public CinemachinePath cameraPath;
    
    private CinemachineTrackedDolly _cameraDolly;
    
    private float targetTrackPos;
    private float currentVelocity;
    public float moveSmoothTime;

    private void OnEnable()
    {
        Instance = this;
    }

    private void Start()
    {
        Instance = this;
#if UNITY_EDITOR
        cameraPath = FindObjectOfType<CinemachinePath>();
        if (cameraPath == null)
        {
            GameObject cameraPathObj = new GameObject("Camera_Path");
            cameraPath = cameraPathObj.AddComponent<CinemachinePath>();
        }
#endif
    }

    private void Update()
    {
        targetTrackPos = cameraPath.MaxUnit(CinemachinePathBase.PositionUnits.Distance) * (TrackController.Instance.TrackTime + TrackController.Instance.pathTargetLead);
        UpdatePositionToPath();
    }

    public void FixedUpdate()
    {

    }

    public void UpdatePositionToPath()
    {
        /*
        GetCameraDolly().m_PathPosition = Mathf.SmoothDamp(GetCameraDolly().m_PathPosition, targetTrackPos,
            ref currentVelocity, moveSmoothTime * Time.deltaTime);
            */
        GetCameraDolly().m_PathPosition = targetTrackPos;
    }

    private CinemachineTrackedDolly GetCameraDolly()
    {
        if (mainTrackCamera == null)
        {
            return null;
        }

        return mainTrackCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
    }
}