using System;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[ExecuteAlways]
public class TrackController : MonoBehaviour
{
    public static TrackController Instance;
    
    public SplineContainer spline;
    public Grid _grid { get; private set; }
    
    private const float MIN_TRACK_POSITION = 0.0001f;
    private const float MAX_TRACK_POSITION = 0.9999f;
    public float trackSpeed = 1.0f;

    public UnityEvent OnTrackEnd;
    
    // track playback
    private float _trackTime;
    public float TrackTime
    {
        get
        {
            return _trackTime;
        }

        set
        {
            _trackTime = Mathf.Clamp(value, MIN_TRACK_POSITION, MAX_TRACK_POSITION);
        }
    }

    public float StartTime { get; private set; }
    
    // path targeting
    private GameObject _pathPositionTarget;
    private CinemachineTargetGroup _pathTargetCineGrp;
    public float pathTargetLead;

    private bool _isPlaying;
    public bool IsPlaying
    {
        get { return _isPlaying; }
    }

    public bool HasEnded
    {
        get { return TrackTime >= 1.0f; }
    }

    public void OnEnable()
    {
        Instance = this;
        _grid = this.GetComponent<Grid>();
        if (_grid == null)
        {
            _grid = this.AddComponent<Grid>();
        }

        if (spline == null)
        {
            spline = this.AddComponent<SplineContainer>();
        }
        
        if (_pathPositionTarget == null)
        {
            _pathPositionTarget = GameObject.FindGameObjectWithTag("PathTarget");
            if (_pathPositionTarget == null)
            {
                _pathPositionTarget = new GameObject("_PATH_TARGET");
                _pathPositionTarget.gameObject.tag = "PathTarget";
            }
        }

        _pathTargetCineGrp = _pathPositionTarget.GetComponent<CinemachineTargetGroup>();
        if (!_pathTargetCineGrp)
        {
            _pathTargetCineGrp = _pathPositionTarget.AddComponent<CinemachineTargetGroup>();
        }
        
#if UNITY_EDITOR
        RefreshCameraWaypoints();
#endif
    }

    public void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerBlockAdded?.AddListener(() =>
            {
                RefreshCameraGroupTarget(true);
            });
        
            GameManager.Instance.OnPlayerBlockRemoved?.AddListener(() =>
            {
                RefreshCameraGroupTarget(true);
            });
        }
    }

    public void Clear()
    {
        spline.Spline.Clear();
        RefreshCameraWaypoints();
    }

    public void AddTrackNode(Vector3 position, float3 tangentIn, float3 tangentOut, Quaternion rotation)
    {
        spline.Spline.Add(new BezierKnot()
        {
            Position = position,
            Rotation = rotation,
            TangentIn = tangentIn,
            TangentOut = tangentOut
        });
        RefreshCameraWaypoints();
    }
    
    private Vector3 _pathTargetMoveVel;
    private void Update()
    {
        if (_isPlaying)
        {
            Step();
        }
    }

    public void Play()
    {
        _isPlaying = true;
        StartTime = Time.time;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Restart()
    {
        SetTrackTime(0.0f);
    }

    public void Step(float amount)
    {
        SetTrackTime(TrackTime + amount);
    }

    private void Step()
    {
        TrackTime += (1.0f / spline.Spline.GetLength()) * trackSpeed * Time.deltaTime;

        // track end
        if (TrackTime >= MAX_TRACK_POSITION)
        {
            TrackTime = MAX_TRACK_POSITION;
            Pause();
            OnTrackEnd?.Invoke();
        }

        GetPathTarget().DOMove(GetTrackPositionAt(TrackTime + pathTargetLead), 1 * Time.deltaTime);
    }

    public void SetTrackTime(float time, bool fireTrackEvents = true)
    {
        TrackTime = time;

        // track end
        if (TrackTime >= MAX_TRACK_POSITION)
        {
            TrackTime = MAX_TRACK_POSITION;
            Pause();
            
            if (fireTrackEvents)
            {
                OnTrackEnd?.Invoke();
            }
        }
        
        GetPathTarget().position = spline.EvaluatePosition(TrackTime + pathTargetLead);
    }

    private Transform GetPathTarget()
    {
        return _pathPositionTarget.transform;
    }

    [ContextMenu("Refresh CameraWaypoints")]
    public void RefreshCameraWaypoints()
    {
        CinemachinePath _cameraPath = FindObjectOfType<CinemachinePath>();
        if (_cameraPath == null)
        {
            GameObject cameraPathObj = new GameObject("Camera_Path");
            _cameraPath = cameraPathObj.AddComponent<CinemachinePath>();
        }

        _cameraPath.m_Waypoints = new CinemachinePath.Waypoint[spline.Spline.Count];
        for (int i = 0; i < _cameraPath.m_Waypoints.Length; i++)
        {
            _cameraPath.m_Waypoints[i].position = spline.Spline[i].Position;
            _cameraPath.m_Waypoints[i].tangent = spline.Spline[i].TangentOut;
            _cameraPath.m_Waypoints[i].roll = 0.0f;
        }
    }

    public Vector3 GetTrackPosition()
    {
        return spline.EvaluatePosition(TrackTime);
    }

    public Vector3 GetTrackPositionAt(float time)
    {
        return spline.EvaluatePosition(time);
    }

    public Vector3 GetTrackTangent()
    {
        return ((Vector3) spline.EvaluateTangent(TrackTime)).normalized;
    }

    public Vector3 GetTrackWorldUp()
    {
        return spline.EvaluateUpVector(TrackTime);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }

    private void RefreshCameraGroupTarget(bool force = false)
    {
        const int CountToRefresh = 4;
        Transform parent = GameManager.Instance.playerController.surfBlockParent;
        if (parent.childCount % CountToRefresh == 0 || force)
        {
            int count = parent.childCount / CountToRefresh;
            _pathTargetCineGrp.m_Targets =
                new CinemachineTargetGroup.Target[count];
            for (int i = 0; i < count; i++)
            {
                _pathTargetCineGrp.m_Targets[i] = new CinemachineTargetGroup.Target()
                {
                    target = parent.GetChild(i).transform,
                    radius = 0.2f,
                    weight = 0.1f
                };
            }
        }
    }
}