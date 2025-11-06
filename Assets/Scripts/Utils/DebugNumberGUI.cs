#if UNITY_EDITOR
using UnityEngine;

[ExecuteAlways]
public class DebugNumberGUI : MonoBehaviour
{
    public Vector2 offset;

    GUIStyle style;

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label);
                style.fontSize = 45;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.black;
            }

            Camera gameCam = GetMainGameViewCamera();
            if (gameCam == null) return;

            int debugValue = GetTrayDispenserIndex();
            if (debugValue == -1) return;

            Vector3 screenPos = gameCam.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0)
            {
                Vector2 size = style.CalcSize(new GUIContent(debugValue.ToString()));
                Rect rect = new Rect(
                    screenPos.x - size.x / 2 + offset.x,
                    Screen.height - screenPos.y - size.y + offset.y,
                    size.x,
                    size.y
                );

                GUI.Label(rect, debugValue.ToString(), style);
            }
        }
    }

    int GetTrayDispenserIndex()
    {
        Transform parent = transform.parent;
        if (parent == null) return -1;

        var all = parent.GetComponentsInChildren<TrayDispenser>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].transform == transform)
                return i + 1;
        }
        return -1;
    }

    private Camera GetMainGameViewCamera()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.cameraType == CameraType.Game)
                return cam;
        }
        return null;
    }
}
#endif
