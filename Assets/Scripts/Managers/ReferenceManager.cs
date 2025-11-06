using System.Collections.Generic;
using UnityEngine;
using BlockStackTypes;
using System.Linq;

public class ReferenceManager : MonoBehaviour
{
    [Header("LEVEL PREFABS")]
    public GameObject Cup;
    public GameObject BlockerBoxPrefab;
    public DevCard DevTestTimesCardPrefab;
    public Transform ConfettiBlast;

    [Header("REFERENCES")]
    public LayerMask CupLayerMask;
    public LayerMask GhostCupLayerMask;
    public Transform ConveyersParent;
    public Transform BoxAnimationTarget;
    public Material MysteriousMaterial;
    public Material DesignerMysteriousMaterial;
    public ArrowPointer ArrowPointerPrefab;
    public Transform CupsRevertPos;
    public RectTransform DispenserInfoUIPrefab;
    public Material WindowMaterial;

    [Header("Sprites")]
    public Sprite MusicOn;
    public Sprite MusicOff;
    public Sprite AudioOn;
    public Sprite AudioOff;
    public Sprite VibrationOn;
    public Sprite VibrationOff;

    public Conveyer[] Conveyers;
    [HideInInspector] public ConveyorSystem Conveyor; //Conveyor being used in current level
    
    void Awake()
    {
        AutoAssignLayoutOptions();
    }

    [ContextMenu("Auto-Assign Layout Options")]
    public void AutoAssignLayoutOptions()
    {
        Conveyers = new Conveyer[ConveyersParent.childCount];

        for (int i = 0; i < ConveyersParent.childCount; i++)
        {
            Conveyers[i] = new Conveyer(ConveyersParent.GetChild(i));
        }
    }

    public string[] GetBeltOptions()
    {
        List<string> BeltOptions = new();
        foreach (Conveyer conveyer in Conveyers)
        {
            BeltOptions.Add(conveyer.ConveyerBelt.name);
        }

        return BeltOptions.ToArray();
    }

    public string[] GetLayoutOptions(int currentBeltIndex)
    {
        List<string> LayoutOptions = new();
        Conveyer TargetedConveyer = Conveyers[currentBeltIndex];

        foreach (Transform LayoutOption in TargetedConveyer.LayoutOptions)
        {
            LayoutOptions.Add(LayoutOption.name);
        }

        return LayoutOptions.ToArray();
    }

    public List<Vector3> GetDispensersPosition(int CurrentBeltIndex, int CurrentLayoutIndex)
    {
        List<Vector3> DispenserPositions = new();
        Transform SelectedLayoutOption = Conveyers[CurrentBeltIndex].LayoutOptions[CurrentLayoutIndex];
        TrayDispenser[] TrayDispensers = SelectedLayoutOption.GetComponentsInChildren<TrayDispenser>(true);
        TrayDispensers = TrayDispensers.OrderBy(td => td.transform.GetSiblingIndex()).ToArray();

        foreach (var TrayDispenser in TrayDispensers)
        {
            DispenserPositions.Add(TrayDispenser.transform.position);
        }
        return DispenserPositions;
    }

    #if UNITY_EDITOR
    public void ShowBelt(int currentBeltIndex, int currentLayoutIndex)
    {
        for (int i = 0; i < Conveyers.Length; i++)
        {
            Conveyers[i].ConveyerBelt.gameObject.SetActive(i == currentBeltIndex);
            for (int j = 0; j < Conveyers[i].LayoutOptions.Length; j++)
                Conveyers[i].LayoutOptions[j].gameObject.SetActive(j == currentLayoutIndex);

        }
    }

    public void UpdateDispenser(TrayDispenserData trayDispenserData, int currentBeltIndex, int currentLayoutIndex, int DispenserIndex)
    {
        Transform SelectedLayoutOption = Conveyers[currentBeltIndex].LayoutOptions[currentLayoutIndex];
        TrayDispenser[] TrayDispensers = SelectedLayoutOption.GetComponentsInChildren<TrayDispenser>(true);
        TrayDispensers = TrayDispensers.OrderBy(td => td.transform.GetSiblingIndex()).ToArray();

        TrayDispensers[DispenserIndex].UPDATE_DISPENSER_EDITOR(trayDispenserData);
    }
    #endif
}