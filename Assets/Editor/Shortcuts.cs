using UnityEngine;
using UnityEditor;
using System.IO;
using CurvedPathGenerator;

public class Shortcuts : MonoBehaviour
{
    [MenuItem("Tools/Activate Selected GameObject(s) %#x")]
    static void Activate()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            obj.SetActive(!obj.activeSelf);
            EditorUtility.SetDirty(obj);
        }
    }

    [MenuItem("Tools/Setup Tray Dispenser Labels")]
    static void SetupTrayDispenserLabels()
    {
        GameObject ToBeCreated = Selection.activeGameObject;
        TrayDispenser[] dispensers = FindObjectsByType<TrayDispenser>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TrayDispenser dispenser in dispensers)
        {
            dispenser.transform.GetChild(0).name = "OldBase";
            GameObject Clone = Instantiate(ToBeCreated, dispenser.transform);
            Clone.transform.localPosition = new Vector3(-0.102742605f, -0.00141842593f, 0.00567350816f);
            Clone.transform.rotation = Quaternion.Euler(new Vector3(270, 270, 0));
            Clone.transform.localScale = new Vector3(0.0813987032f, 0.115911223f, 0.0202632435f);
            Clone.name = "Base";
            Clone.transform.SetSiblingIndex(0);

            Undo.RegisterCompleteObjectUndo(dispenser, "Setup Tray Dispenser Labels");
            EditorUtility.SetDirty(dispenser);
        }
    }

    [MenuItem("Tools/Clear Persisent Data")]
    static void ClearSavedTestTimes()
    {
        string path = Path.Combine(Application.persistentDataPath, "leveltimes.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("TestTimes data cleared.");
        }
    }

    [MenuItem("Tools/Modify PathFollower")]
    static void ModifyPathFollower()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {

            if (!obj.TryGetComponent(out PathGenerator pg)) continue;
            pg.PathDensity = 2;
            EditorUtility.SetDirty(pg);
        }
    }
}
