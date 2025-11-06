using UnityEngine;
using BlockStackTypes;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Coffee Time/New Level")]
public class LevelData : ScriptableObject
{
    public ResolutionManager.CameraSettings CameraConfig = null;
    public CupGrid3D CupArrangement;
    public Themes Theme;
    public int BeltIndex;
    public int LayoutIndex;
    public string TutorialMessage = null;
    public bool IsHardLevel = false;
    public TrayDispenserData[] TrayDispensers;
    public Themes GetTheme() => IsHardLevel ? Themes.Red : Theme;
}
