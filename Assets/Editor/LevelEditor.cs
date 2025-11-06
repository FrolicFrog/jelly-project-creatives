using BlockStackTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelEditor : EditorWindow
{
    private bool LoadInRealTime = false;
    private int Width = 3;
    private int Height = 3;
    private CupGrid3D Grid;
    private int CurrentLevelNumber = 1;
    private Themes GraphicsTheme = Themes.Blue;
    private int _CurrentBeltIndex;
    private int CurrentBeltIndex
    {
        get => _CurrentBeltIndex;
        set
        {
            _CurrentBeltIndex = value;
            _ReferenceManager.ShowBelt(_CurrentBeltIndex, _CurrentLayoutIndex);
        } 
    }

    private int _CurrentLayoutIndex;
    private int CurrentLayoutIndex
    {
        get => _CurrentLayoutIndex;
        set
        {
            _CurrentLayoutIndex = value;
            _ReferenceManager.ShowBelt(_CurrentBeltIndex, _CurrentLayoutIndex);
        }
    }

    public List<TrayDispenserData> TrayDispensers;
    private bool ShowTutorialMessage = false;
    private string TutorialMessage;
    private bool IsHardLevel = false;

    private static readonly int MAX_GRID_WIDTH = 50;
    private static readonly int MAX_GRID_HEIGHT = 50;
    private static readonly int MIN_GRID_WIDTH = 1;
    private static readonly int MIN_GRID_HEIGHT = 1;

    
    private int _prevBeltIndex = -1;
    private int _prevLayoutIndex = -1;
    private int _prevLevelNumber = -1;

    private Vector2 ScrollPosition;
    private ReferenceManager _ReferenceManager;


    //Camera Settings
    private bool UseLevelCameraSettings = true;
    private Vector3 CameraPosition = Vector3.zero;
    private Vector3 CameraRotation = Vector3.zero;
    private float CamFOV = 5f;
    private Transform CameraSystem;
    private Camera MainCamera;
    private Camera UICamera;


    [MenuItem("Coffee Time/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditor>("Level Editor");
    }

    private void OnEnable()
    {
        UpdateGrid();
        Grid = new CupGrid3D(Width, Height);
        TrayDispensers = new();

        CameraSystem = FindAnyObjectByType<Camera>().transform.parent;
        MainCamera = CameraSystem.GetChild(0).GetComponent<Camera>();
        UICamera = CameraSystem.GetChild(1).GetComponent<Camera>();

        CameraPosition = CameraSystem.position;
        CameraRotation = CameraSystem.eulerAngles;
        CamFOV = MainCamera.orthographicSize;
    }
    private void OnGUI()
    {
        DrawHeader("🥤 Boba Time", "Bubble Tea Craze");
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
        DrawLevelActions();
        DrawLevelSelector();
        DrawCameraSettings();
        DrawConveyerSelector();
        DrawHeader("Grid", "Click to add cup and Ctrl + Click to remove cup, Right Click to change color", false);
        DrawGridControls();
        DrawGrid();
        DrawTrayDispensers();
        GUILayout.EndScrollView();
    }

    private void DrawCameraSettings()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Camera Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);
        UseLevelCameraSettings = EditorGUILayout.Toggle("Use Level Camera Settings", UseLevelCameraSettings);
        CameraPosition = EditorGUILayout.Vector3Field("Camera Position", CameraPosition);
        CameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", CameraRotation);
        GUILayout.Space(5);
        CamFOV = EditorGUILayout.FloatField("Camera Field of View", CamFOV);
        GUILayout.EndVertical();
        GUILayout.Space(20);


        if (Application.isPlaying) return;
        CameraSystem.SetPositionAndRotation(CameraPosition, Quaternion.Euler(CameraRotation));
        MainCamera.orthographicSize = CamFOV;
        UICamera.orthographicSize = CamFOV;
    }

    private void DrawTrayDispensers()
    {
        if (_ReferenceManager == null) _ReferenceManager = FindFirstObjectByType<ReferenceManager>();

        EditorGUILayout.Space(10);
        DrawHeader("Tray Dispensers", "Setup properties of tray dispensers like orientation, type of trays to be dispensed, etc.");

        List<Vector3> DispenserPositions = _ReferenceManager.GetDispensersPosition(CurrentBeltIndex, CurrentLayoutIndex);
        int DispenserCount = DispenserPositions.Count;

        if (DispenserCount == 0) return;

        TrayDispensers ??= new List<TrayDispenserData>(DispenserCount);

        if (TrayDispensers.Count < DispenserCount)
        {
            for (int i = TrayDispensers.Count; i < DispenserCount; i++)
            {
                TrayDispensers.Add(new TrayDispenserData(TrayOrientation.Down, true, DispenserPositions[i]));
            }
        }
        else if (TrayDispensers.Count > DispenserCount)
        {
            TrayDispensers.RemoveRange(DispenserCount, TrayDispensers.Count - DispenserCount);
        }

        GUILayout.Space(10);
        GUILayout.BeginVertical();
        for(int i = 0; i < DispenserCount; i++)
        {
            GUILayout.BeginVertical("Box");
            EditorGUI.BeginChangeCheck();
            TrayDispensers[i].Orientation = (TrayOrientation)EditorGUILayout.EnumPopup($"Orientation", TrayDispensers[i].Orientation);
            TrayDispensers[i].Active = EditorGUILayout.Toggle("Active", TrayDispensers[i].Active);
            TrayDispensers[i].Position = EditorGUILayout.Vector2Field("Position", TrayDispensers[i].Position);

            //Check if any of the three values are changed
            if (EditorGUI.EndChangeCheck())
            {
                _ReferenceManager.UpdateDispenser(TrayDispensers[i], CurrentBeltIndex, CurrentLayoutIndex, i);
            }

            TraysToSpawnList(i);
            GUILayout.EndVertical();
            GUILayout.Space(1);
        }
        GUILayout.EndVertical();
    }

    private void TraysToSpawnList(int i)
    {
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Trays To Spawn", EditorStyles.boldLabel);
        if (GUILayout.Button("➕", GUILayout.Width(25), GUILayout.Height(20)))
        {
            TrayDispensers[i].TraysToSpawn.Add(new Tray());
        }
        if (GUILayout.Button("🗑️", GUILayout.Width(25), GUILayout.Height(20)))
        {
            TrayDispensers[i].TraysToSpawn.Clear();
        }
        GUILayout.EndHorizontal();

        if (TrayDispensers[i].TraysToSpawn == null) TrayDispensers[i].TraysToSpawn = new List<Tray>();
        for (int j = 0; j < TrayDispensers[i].TraysToSpawn.Count; j++)
        {
            TrayDispensers[i].TraysToSpawn[j].Material = (TrayMaterial)EditorGUILayout.EnumPopup("Material", TrayDispensers[i].TraysToSpawn[j].Material);
            TrayDispensers[i].TraysToSpawn[j].Type = (TrayType)EditorGUILayout.EnumPopup("Type", TrayDispensers[i].TraysToSpawn[j].Type);
            TrayDispensers[i].TraysToSpawn[j].IsMysterious = EditorGUILayout.Toggle("Is Mysterious", TrayDispensers[i].TraysToSpawn[j].IsMysterious);
            if (GUILayout.Button("Remove Tray", GUILayout.Width(100), GUILayout.Height(20)))
            {
                TrayDispensers[i].TraysToSpawn.RemoveAt(j);
                j--;
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawConveyerSelector()
    {
        if (_ReferenceManager == null) _ReferenceManager = FindFirstObjectByType<ReferenceManager>();

        // IsHardLevel = EditorGUILayout.Toggle("Hard Level", IsHardLevel);
        GraphicsTheme = (Themes)EditorGUILayout.EnumPopup($"Graphics Theme", GraphicsTheme);
        

        ShowTutorialMessage = EditorGUILayout.Toggle("Show Tutorial Message", ShowTutorialMessage);
        if (ShowTutorialMessage) TutorialMessage = EditorGUILayout.TextField("Tutorial Message", TutorialMessage);
    
        int newBeltIndex = EditorGUILayout.Popup("Belt", CurrentBeltIndex, _ReferenceManager.GetBeltOptions());
        int newLayoutIndex = EditorGUILayout.Popup("Layout", CurrentLayoutIndex, _ReferenceManager.GetLayoutOptions(newBeltIndex));
    
        if (newBeltIndex != _prevBeltIndex || newLayoutIndex != _prevLayoutIndex)
        {
            TrayDispensers.Clear();
            _prevBeltIndex = newBeltIndex;
            _prevLayoutIndex = newLayoutIndex;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("POWERUPS", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        CurrentBeltIndex = newBeltIndex;
        CurrentLayoutIndex = newLayoutIndex;
    }

    private void DrawHeader(string Text, string SubText = "", bool ShowLoadInRealTimeToggle = true)
    {
        GUIStyle HeadingStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        GUIStyle SubHeadingStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(Text, HeadingStyle);
        EditorGUILayout.LabelField(SubText, SubHeadingStyle);
        
        if (ShowLoadInRealTimeToggle)
            LoadInRealTime = EditorGUILayout.Toggle("Load In Real Time", LoadInRealTime);

        EditorGUILayout.EndVertical();
    }

    private void DrawLevelSelector()
    {
        EditorGUILayout.BeginHorizontal();
        CurrentLevelNumber = EditorGUILayout.IntSlider("Level Number", CurrentLevelNumber, 1, 100);
        if (CurrentLevelNumber != _prevLevelNumber && LoadInRealTime)
        {
            LoadSelectedLevel();
            _prevLevelNumber = CurrentLevelNumber;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(15);
    }

    private void DrawGridControls()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.Space(1);

        if (GUILayout.Button("➕ Row", GUILayout.Width(80), GUILayout.Height(35)))
        {
            if (Width < MAX_GRID_WIDTH) Width++;
            UpdateGrid();
        }
        if (GUILayout.Button("➖ Row", GUILayout.Width(80), GUILayout.Height(35)))
        {
            if (Width > MIN_GRID_WIDTH) Width--;
            UpdateGrid();
        }
        if (GUILayout.Button("➕ Column", GUILayout.Width(80), GUILayout.Height(35)))
        {
            if (Height < MAX_GRID_HEIGHT) Height++;
            UpdateGrid();
        }
        if (GUILayout.Button("➖ Column", GUILayout.Width(80), GUILayout.Height(35)))
        {
            if (Height > MIN_GRID_HEIGHT) Height--;
            UpdateGrid();
        }
        if (GUILayout.Button("🔄 Reset", GUILayout.Width(80), GUILayout.Height(35)))
        {
            Grid = new CupGrid3D(5, 5);
            UpdateGrid();
        }

        EditorGUILayout.Space(1);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    private void DrawGrid()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        for (int i = 0; i < Height; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (i % 2 == 0) EditorGUILayout.Space(20);
            
            for (int j = Width - 1; j >= 0; j--)
            {
                GUIStyle buttonStyle = new(GUI.skin.button);
                int StackHeight = Grid.GetStackHeight(j, i);
                string SuffixName = "";
                bool IsStackLocked = Grid.GetStack(j, i).LockedStackUntil > 0;
                
                if (StackHeight > 0)
                {
                    var CupDetails = Grid.GetCupColor(j, i, StackHeight - 1);
                    GUI.color = CupDetails.Item1;
                    SuffixName += CupDetails.Item2 ? "⭐" : "";
                    SuffixName += Grid.IsCupMysterious(j, i, StackHeight - 1) ? "❓" : "";
                    SuffixName += IsStackLocked ? "🔒" : "";
                }

                if (GUILayout.Button(StackHeight.ToString() + SuffixName, buttonStyle, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    if (Event.current.button == 0)
                    {
                        if (IsStackLocked)
                        {
                            EditorUtility.DisplayDialog("Stack Locked", "This stack is locked, please unlock it to add or remove cups.", "OK");
                            EditorGUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            return;
                        }

                        if (Event.current.control)
                        {
                            Grid.RemoveCup(j, i);
                        }
                        else
                        {
                            Grid.AddCup(j, i, CupColors.Blue, Event.current.alt);
                        }
                    }
                    else if (Event.current.button == 1 && StackHeight > 0)
                    {
                        ShowContextMenu(j, i);
                    }
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void ShowContextMenu(int j, int i)
    {
        GenericMenu menu = new GenericMenu();

        foreach (CupColors color in Enum.GetValues(typeof(CupColors)))
        {
            CupColors selectedColor = color;
            menu.AddItem(new GUIContent("Set Color/" + color.ToString()), false, () => Grid.SetCupColor(j, i, selectedColor));
        }

        for(int ind = 1; ind <= 15; ind++)
        {
            int turns = ind;
            menu.AddItem(new GUIContent("Lock Stack/" + turns + " Turns"), false, () =>
            {
                Grid.GetStack(j, i).LockedStackUntil = turns;
            });
        }


        if(Grid.GetStack(j, i).LockedStackUntil > 0)
        {
            menu.AddItem(new GUIContent("Unlock Stack"), false, () =>
            {
                Grid.GetStack(j, i).LockedStackUntil = -1;
            });
        }

        menu.ShowAsContext();
    }

    private void DrawLevelActions()
    {
        EditorGUILayout.BeginHorizontal("box");
        string Label = LevelExists(CurrentLevelNumber) ? "🧩 Update Level " + CurrentLevelNumber : "✅ Create Level" + CurrentLevelNumber;
        if (GUILayout.Button(Label, GUILayout.Height(30)))
        {
            CreateNewLevel();
        }
        if (GUILayout.Button("📂 Load Level", GUILayout.Height(30)))
        {
            LoadSelectedLevel();
        }
        if (GUILayout.Button("▶ Play Level " + CurrentLevelNumber, GUILayout.Height(30)))
        {
            PlayLevel();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(15);
    }

    private bool LevelExists(int currentLevelNumber)
    {
        return File.Exists("Assets/Resources/Levels/" + currentLevelNumber + ".asset");
    }

    private void CreateNewLevel()
    {
        string path = $"Assets/Resources/Levels/{CurrentLevelNumber}.asset";
        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        if (levelData == null)
        {
            levelData = CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(levelData, path);
        }

        levelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (ShowTutorialMessage)
        {
            levelData.TutorialMessage = TutorialMessage;
        }
        else
        {
            levelData.TutorialMessage = null;
        }

        levelData.CameraConfig = UseLevelCameraSettings ? new ResolutionManager.CameraSettings(CameraPosition, CameraRotation, CamFOV) : null;
        levelData.IsHardLevel = GraphicsTheme == Themes.Red;
        levelData.Theme = GraphicsTheme;
        levelData.CupArrangement = Grid;
        levelData.BeltIndex = CurrentBeltIndex;
        levelData.LayoutIndex = CurrentLayoutIndex;
        levelData.TrayDispensers = TrayDispensers.ToArray();
        EditorUtility.SetDirty(levelData);
        Debug.Log("New Level Data Created");
    }


    private void LoadSelectedLevel()
    {
        LevelData LevelData = Resources.Load<LevelData>($"Levels/{CurrentLevelNumber}");
        if (LevelData == null)
        {
            EditorUtility.DisplayDialog("Level Not Found", "No level found with the selected index!", "OK");
            return;
        }

        if (!string.IsNullOrEmpty(LevelData.TutorialMessage))
        {
            ShowTutorialMessage = true;
            TutorialMessage = LevelData.TutorialMessage;
        }

        ShowTutorialMessage = !string.IsNullOrEmpty(LevelData.TutorialMessage);
        CameraPosition = LevelData.CameraConfig != null ? LevelData.CameraConfig.Position : Vector3.zero;
        CameraRotation = LevelData.CameraConfig != null ? LevelData.CameraConfig.Rotation : Vector3.zero;
        CamFOV = LevelData.CameraConfig != null ? LevelData.CameraConfig.OrthographicSize : 5f;
        GraphicsTheme = LevelData.Theme;
        IsHardLevel = LevelData.IsHardLevel;
        CurrentBeltIndex = LevelData.BeltIndex;
        CurrentLayoutIndex = LevelData.LayoutIndex;
        Width = LevelData.CupArrangement.Width;
        Height = LevelData.CupArrangement.Height;
        Grid = LevelData.CupArrangement;
        TrayDispensers = LevelData.TrayDispensers.ToList();
        Repaint();
        DrawGrid();
    }

    private void PlayLevel()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogWarning("No LevelManager found in the scene!");
            return;
        }

        LevelData levelData = Resources.Load<LevelData>($"Levels/{CurrentLevelNumber}");
        if (levelData == null)
        {
            EditorUtility.DisplayDialog("Level Not Found", $"Level {CurrentLevelNumber} Not Found, please create the level for playing it.", "OK");
            return;
        }

        levelManager.TestLevelLoad = CurrentLevelNumber;
        EditorApplication.isPlaying = true;
    }

    private void UpdateGrid()
    {
        Grid = new CupGrid3D(Width, Height);
    }
}