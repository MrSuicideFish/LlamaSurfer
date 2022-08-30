using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

[EditorTool("Level Editor", typeof(TrackController))]
public class LevelEditor : EditorTool
{
    public enum EPlatformType
    {
        Single,
        Straight,
        TurnRight,
        TurnLeft
    }

    public enum EPaintMode
    {
        Paint,
        Erase
    }

    private readonly Dictionary<EPlatformType, string> PlatformNameDictionary = new Dictionary<EPlatformType, string>()
    {
        {EPlatformType.Single, "Platform_1x1"},
        {EPlatformType.Straight, "Platform_2x1"},
        {EPlatformType.TurnLeft, "Platform_turn_left"},
        {EPlatformType.TurnRight, "Platform_turn_right"},
    };

    private TrackController _track;
    private Platform _platforms;
    private Transform _platformContainer;

    private Vector3 paintPosition;
    private WorldObjectBase[] objectRepo;
    private int objectToPaint;
    private Texture2D[] objectThumbnails;

    private EPaintMode paintMode;
    private Texture[] paintModeIcons;

    private WorldObjectLayer _objectLayer;
    private GameObject sceneCamObj;

    public float trackPosition
    {
        get { return _track.TrackTime; }

        set { _track.TrackTime = value; }
    }

    public int TrackCount
    {
        get { return _platforms.Count; }
    }

    public void OnEnable()
    {
        Transform platformContainer = GetPlatformContainer();
        if (platformContainer.childCount > 0)
        {
            _platforms = platformContainer.GetChild(0).GetComponent<Platform>();
        }
        else
        {
            BuildNext(EPlatformType.Single);
        }

        RebuildCameraTrack();
        ReloadObjectRepo();
    }

    public void OnDisable()
    {
    }

    public override void OnActivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Level Editor"), .1f);
        ReloadObjectRepo();
        ReloadGraphicIcons();
    }

    private void ReloadGraphicIcons()
    {
        paintModeIcons = new Texture[]
        {
            EditorGUIUtility.IconContent("d_Grid.PaintTool@2x").image,
            EditorGUIUtility.IconContent("d_Grid.EraserTool@2x").image
        };
    }

    private void ReloadObjectRepo()
    {
        objectRepo = Resources.LoadAll<WorldObjectBase>("Level/Objects");
        objectThumbnails = new Texture2D[objectRepo.Length];
        for (int i = 0; i < objectRepo.Length; i++)
        {
            objectThumbnails[i] = AssetPreview.GetAssetPreview(objectRepo[i].gameObject);
        }
    }

    // The second "context" argument accepts an EditorWindow type.
    [Shortcut("Open Level Editor", typeof(SceneView), KeyCode.P)]
    static void LevelEditorShortcut()
    {
        if (Selection.GetFiltered<TrackController>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<LevelEditor>();
        else
            Debug.Log("No track selected!");
    }

    private void DrawTrackWindow()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Save Level"))
            {
                Save();
            }

            EditorGUI.BeginChangeCheck();
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("1x1")) BuildNext(LevelEditor.EPlatformType.Single);
                if (GUILayout.Button("2x1")) BuildNext(LevelEditor.EPlatformType.Straight);
                if (GUILayout.Button("Right")) BuildNext(LevelEditor.EPlatformType.TurnRight);
                if (GUILayout.Button("Left")) BuildNext(LevelEditor.EPlatformType.TurnLeft);
            }

            if (TrackCount > 1)
            {
                if (GUILayout.Button("Delete")) RemoveLast();
            }

            GUILayout.Label($"TrackPosition ({trackPosition})");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("<<")) _track.Step(-(1.0f / GetCameraTrack().spline.Spline.GetLength()));
                if (GUILayout.Button(">>")) _track.Step((1.0f / GetCameraTrack().spline.Spline.GetLength()));
            }

            if (GUILayout.Button("Rebuild"))
            {
                RebuildCameraTrack();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_platformContainer, "change platforms");
                Undo.RecordObject(_track, "change track");
            }

            GUILayout.Space(10);
        }
    }

    private Vector2 _scrollPosition;

    private void DrawObjectWindow()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new GUILayout.HorizontalScope())
            {
                int selection = GUILayout.SelectionGrid(paintMode.GetHashCode(), paintModeIcons, 2);
                if (selection != paintMode.GetHashCode())
                {
                    paintMode = (EPaintMode) selection;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Trash"))))
                {
                    GetObjectLayer().EraseAll();
                }
            }

            GUILayout.Space(10);
            using (new GUILayout.ScrollViewScope(_scrollPosition))
            {
                objectToPaint = GUILayout.SelectionGrid(objectToPaint, objectThumbnails, 10);
            }

            GUILayout.Space(10);
        }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;

        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

        if (_track != null)
        {
            sceneView.sceneViewState.alwaysRefresh = true;
            TeleportSceneCamera(Camera.main.transform.position, Camera.main.transform.forward);
            HandleMouseControl(sceneView);
            
            Handles.BeginGUI();

            using (new GUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    DrawTrackWindow();
                    GUILayout.FlexibleSpace();
                    DrawObjectWindow();
                }

                GUILayout.Space(10);
            }

            Handles.EndGUI();
            CameraController.Instance.UpdatePositionToPath();
        }
    }

    private TrackController GetCameraTrack()
    {
        if (_track == null)
        {
            _track = Object.FindObjectOfType<TrackController>();
            if (_track == null)
            {
                CreateNewTrack();
            }
        }

        return _track;
    }

    private Transform GetPlatformContainer()
    {
        if (_platformContainer == null)
        {
            GameObject existing = GameObject.FindGameObjectWithTag("PlatformContainer");
            if (existing != null)
            {
                _platformContainer = existing.transform;
            }
            else
            {
                _platformContainer = new GameObject("_PLATFORMS").transform;
                _platformContainer.gameObject.tag = "PlatformContainer";
            }
        }

        return _platformContainer;
    }

    private WorldObjectLayer GetObjectLayer()
    {
        if (_objectLayer == null)
        {
            GameObject existing = GameObject.FindGameObjectWithTag("WorldObjectLayer");
            if (existing != null)
            {
                _objectLayer = existing.GetComponent<WorldObjectLayer>();
            }
            else
            {
                _objectLayer = new GameObject("_OBJECTS").AddComponent<WorldObjectLayer>();
                _objectLayer.gameObject.tag = "WorldObjectLayer";
            }
        }

        return _objectLayer;
    }

    public void ResetLevel()
    {
        var lastPlatform = _platforms.Last();
        while (lastPlatform.previous != null)
        {
            Platform platform = lastPlatform;
            GameObject.DestroyImmediate(platform.gameObject);
            lastPlatform = lastPlatform.previous;
        }

        if (_platforms != null)
        {
            GameObject.DestroyImmediate(((Platform) _platforms).gameObject);
            _platforms = null;
        }

        _track.Clear();
        BuildNext(EPlatformType.Single);
    }

    private void CreateNewTrack()
    {
        GameObject splineObj = new GameObject("_LEVEL_TRACK");
        splineObj.tag = "TrackController";
        _track = splineObj.AddComponent<TrackController>();
        _track.spline.Spline.Add(new BezierKnot(new float3(0, 0, 0)));
    }

    public void BuildNext(EPlatformType platform)
    {
        Platform newPlatform = SpawnNewPlatform(platform);

        if (_platforms == null)
        {
            newPlatform.transform.position = Vector3.zero;
            newPlatform.transform.forward = Vector3.forward;
            newPlatform.transform.SetParent(GetPlatformContainer());
            _platforms = newPlatform;
        }
        else
        {
            Platform lastPlatform = _platforms.Last();
            newPlatform.transform.position = lastPlatform.platformExit.position;
            newPlatform.transform.forward = lastPlatform.platformExit.forward;

            lastPlatform.next = newPlatform;
            newPlatform.previous = lastPlatform;

            newPlatform.transform.SetParent(GetPlatformContainer());
        }

        RebuildCameraTrack();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Save();
    }

    private Platform SpawnNewPlatform(EPlatformType platformType)
    {
        Platform newPlatform = Resources.Load<Platform>($"Level/Platforms/{PlatformNameDictionary[platformType]}");
        return Object.Instantiate(newPlatform);
    }

    public void RemoveLast()
    {
        if (_platforms.next == null)
        {
            return;
        }

        Platform lastPlatform = _platforms.Last();
        {
            Platform prevPlatform = lastPlatform.previous;
            if (prevPlatform != null)
            {
                prevPlatform.next = null;
            }

            Object.DestroyImmediate(lastPlatform.gameObject);
            RebuildCameraTrack();
            Save();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void RebuildCameraTrack()
    {
        GetCameraTrack().Clear();
        var current = _platforms.First();
        while (current != null)
        {
            Platform platform = current;
            foreach (Transform trackPoint in platform.trackPoints)
            {
                _track.AddTrackNode(trackPoint.position,
                    new float3(),
                    new float3(),
                    trackPoint.rotation);
            }

            current = current.next;
        }
    }

    public void Save()
    {
    }

    private void HandleMouseControl(SceneView sceneView)
    {
        float mouseY = Event.current.mousePosition.y;
        float screenHeight = sceneView.camera.pixelRect.height;
        if (mouseY < screenHeight - 220)
        {
            var worldSamplePos = SampleMousePosition();

            float size = 0.5f;
            DrawPaintPreviewGridRect(worldSamplePos, size,
                paintMode == EPaintMode.Paint ? Color.blue : Color.red,
                Color.black);
        
            // paint / erase
            if (Event.current.type == EventType.MouseDown 
                || Event.current.type == EventType.MouseDrag)
            {
                if (paintMode == EPaintMode.Paint)
                {
                    if (!ObjectExistsAtPosition(worldSamplePos))
                    {
                        AddWorldObject(objectToPaint, worldSamplePos, Vector3.forward);
                    }
                    else if(GetPaintObject().canStack)
                    {
                        StackWorldObject(objectToPaint, worldSamplePos, Vector3.forward);
                    }
                }
                else if (paintMode == EPaintMode.Erase)
                {
                    RemoveWorldObject(worldSamplePos);
                }
            }
        }
    }

    private void DrawPaintPreviewGridRect(Vector3 position, float size, Color faceColor, Color outlineColor)
    {
        PaintGridRect(position, size, faceColor, outlineColor);
    }

    private void PaintGridRect(Vector3 position, float size, Color faceColor, Color outlineColor)
    {
        Vector3[] verts = new Vector3[]
        {
            new Vector3(position.x - size, position.y, position.z - size),
            new Vector3(position.x - size, position.y, position.z + size),
            new Vector3(position.x + size, position.y, position.z + size),
            new Vector3(position.x + size, position.y, position.z - size)
        };
        
        Handles.DrawSolidRectangleWithOutline(verts, faceColor, outlineColor);
    }

    private Vector3 SampleMousePosition()
    {
        if (_track == null || _track._grid == null)
            return Vector3.zero;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane drawPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        drawPlane.Raycast(ray, out float enter);
        var point = ray.GetPoint(enter);
        Vector3Int samplePosInt =
            new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));

        return _track._grid.GetCellCenterWorld(samplePosInt);
    }

    private void AddWorldObject(int objIndex, Vector3 position, Vector3 rotation)
    {
        if (objIndex < 0 || objIndex >= objectRepo.Length)
        {
            return;
        }

        WorldObjectBase obj = objectRepo[objIndex];
        if (obj != null)
        {
            string id = obj.gameObject.name;
            obj = PrefabUtility.InstantiatePrefab(obj) as WorldObjectBase;
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.eulerAngles = rotation;
                GetObjectLayer().AddObject(position, obj);
            }
        }
    }

    private void StackWorldObject(int objIndex, Vector3 position, Vector3 rotation)
    {
        if (objIndex < 0 || objIndex >= objectRepo.Length)
        {
            return;
        }
        
        List<WorldObjectBase> objs = new List<WorldObjectBase>();
        GetObjectLayer().GetObjectsAtPosition(position, ref objs);

        if (objs.Count == 0)
        {
            AddWorldObject(objIndex, position, rotation);
            return;
        }
        
        WorldObjectBase obj = objectRepo[objIndex];
        if (obj != null)
        {
            string id = obj.gameObject.name;
            obj = PrefabUtility.InstantiatePrefab(obj) as WorldObjectBase;
            if (obj != null)
            {
                var pos = position;
                pos.y += objs.Count;
                obj.transform.position = pos;
                obj.transform.eulerAngles = rotation;
                GetObjectLayer().AddObject(position, obj);
            }
        }
    }

    private void RemoveWorldObject(Vector3 position)
    {
        GetObjectLayer().RemoveObject(position);
    }

    private bool ObjectExistsAtPosition(Vector3 position)
    {
        List<WorldObjectBase> objsAtPos = null;
        GetObjectLayer().GetObjectsAtPosition(position, ref objsAtPos);
        return objsAtPos != null && objsAtPos.Count > 0;
    }

    private WorldObjectBase GetPaintObject()
    {
        if (objectToPaint < 0 || objectToPaint >= objectRepo.Length)
            return null;
        return objectRepo[objectToPaint];
    }

    public static void TeleportSceneCamera(Vector3 cam_position, Vector3 cam_forward)
    {
        //UnityEditor.EditorApplication.ExecuteMenuItem("Window/Scene");

        // Can't set transform of camera :(
        // It internally updates every frame:
        //      cam.position = pivot + rotation * new Vector3(0, 0, -cameraDistance)
        // Info: https://forum.unity.com/threads/moving-scene-scene_view-camera-from-editor-script.64920/#post-3388397
        // But we can align it to an object! Source: http://answers.unity.com/answers/256969/scene_view.html
        var scene_view = UnityEditor.SceneView.lastActiveSceneView;
        if (scene_view != null)
        {
            scene_view.orthographic = false;

            var target = scene_view.camera;
            target.transform.position = cam_position;
            target.transform.rotation = Quaternion.LookRotation(cam_forward);
            scene_view.AlignViewToObject(target.transform);
        }
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }

        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}