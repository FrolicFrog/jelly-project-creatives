using System;
using UnityEditor;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Serializable]
    public class Resolution
    {
        public float Width;
        public float Height;

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }

        public Resolution(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public float AspectRatio => Width / Height;
    }

    [Serializable]
    public class CameraSettings
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float OrthographicSize;

        public CameraSettings(Vector3 position, Vector3 rotation, float fieldOfView)
        {
            Position = position;
            Rotation = rotation;
            OrthographicSize = fieldOfView;
        }
    }

    [Serializable]
    public class ResolutionSetting
    {
        public Resolution Resolution;
        public CameraSettings CameraSettings;
    }

    [Header("REFERENCES")]
    public Transform CameraSystem;

    [Header("RESOLUTION SETTINGS")]
    public ResolutionSetting[] ResolutionSettings;

    private void OnEnable()
    {
        if (Manager.Instance.LevelManagement.CurrentLevelDataRef != null && Manager.Instance.LevelManagement.CurrentLevelDataRef.CameraConfig != null)
        {
            ApplyCameraSettings(Manager.Instance.LevelManagement.CurrentLevelDataRef.CameraConfig);
            return;
        }
        
        ApplyResolutionSettings();
    }

    private void ApplyResolutionSettings()
    {
        if (ResolutionSettings.Length == 0) return;

        Resolution current = new(Screen.width, Screen.height);
        ResolutionSetting bestMatch = ResolutionSettings[0];
        float closestDiff = Mathf.Abs(current.AspectRatio - bestMatch.Resolution.AspectRatio);

        foreach (var setting in ResolutionSettings)
        {
            float diff = Mathf.Abs(current.AspectRatio - setting.Resolution.AspectRatio);
            if (diff < closestDiff)
            {
                closestDiff = diff;
                bestMatch = setting;
            }
        }

        ApplyCameraSettings(bestMatch.CameraSettings);
    }

    private void ApplyCameraSettings(CameraSettings settings)
    {
        CameraSystem.SetPositionAndRotation(settings.Position, Quaternion.Euler(settings.Rotation));

        Transform MainCamera = CameraSystem.GetChild(0);
        Transform UICamera = CameraSystem.GetChild(1);


        MainCamera.GetComponent<Camera>().orthographicSize = settings.OrthographicSize;
        UICamera.GetComponent<Camera>().orthographicSize = settings.OrthographicSize;
    }

    [ContextMenu("Auto-Assign Camera Settings")]
    public void AutoAssignCameraSettings()
    {
        ResolutionSettings ??= new ResolutionSetting[1];
        int LastIndex = ResolutionSettings.Length - 1;

        Vector3 Position = CameraSystem.position;
        Vector3 Rotation = CameraSystem.rotation.eulerAngles;
        float OrthographicSize = CameraSystem.transform.GetChild(0).GetComponent<Camera>().orthographicSize;

        CameraSettings cameraSetting = new(Position, Rotation, OrthographicSize);
        ResolutionSettings[LastIndex].CameraSettings = cameraSetting;
        // Undo.RecordObject(this, "Auto-Assign Camera Settings");
    }

    [ContextMenu("Load Camera Settings")]
    public void LoadCameraSettings()
    {
        if (ResolutionSettings.Length == 0) return;
        ResolutionSettings ??= new ResolutionSetting[1];
        int LastIndex = ResolutionSettings.Length - 1;

        CameraSystem.SetPositionAndRotation(ResolutionSettings[LastIndex].CameraSettings.Position, Quaternion.Euler(ResolutionSettings[LastIndex].CameraSettings.Rotation));
        CameraSystem.GetChild(0).GetComponent<Camera>().orthographicSize = ResolutionSettings[LastIndex].CameraSettings.OrthographicSize;

        // Undo.RecordObject(this, "Auto-Assign Camera Settings");
    }
}
