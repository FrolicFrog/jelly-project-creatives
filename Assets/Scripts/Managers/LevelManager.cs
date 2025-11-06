using UnityEngine;
using UnityEngine.SceneManagement;
using BlockStackTypes;
using System.Data;
using System.Linq;
using System;
using UnityEngine.UI;
using PlayerPrefsManager;

public class LevelManager : MonoBehaviour
{
    [Header("REFERENCES")]
    public Transform CupParent;
    public Transform Table;

    [Header("SETTINGS")]

    public float YPosOffset = 5f;
    public float PreviewCupsDelayOffset = 0.5f;

    [Header("DEVELOPMENT")]
    [Space(10)]

    public bool TestMode = true;
    [HideInInspector] public int TestLevelLoad = 1;

    private LevelData CurrentLevelData;
    public LevelData CurrentLevelDataRef => CurrentLevelData;
    private int CurrentLevelNumber;
    public int CurrentLevel => CurrentLevelNumber;
    private BoxCollider EntryPointCollider;
    public bool EntryAllowed
    {
        get => !EntryPointCollider.enabled;
        set => EntryPointCollider.enabled = !value;
    }

    void Awake()
    {
#if !UNITY_EDITOR
        TestMode = false;   
#endif
        CurrentLevelNumber = TestMode ? TestLevelLoad : PlayerPrefs.GetInt("LastLevel", 1);
        CurrentLevelData = Resources.Load<LevelData>("Levels/" + CurrentLevelNumber.ToString());

        if ((CurrentLevelData == null || CurrentLevelNumber > 40) && !TestMode)
        {
            int LevelNumber = Utilities.GetRandomLevelNumber();
            CurrentLevelData = Resources.Load<LevelData>("Levels/" + LevelNumber.ToString());
        }
    }

    private void OnEnable()
    {
        SetupConveyer(CurrentLevelData.BeltIndex, CurrentLevelData.LayoutIndex);
        SetupLevel(Manager.Instance.ReferenceManagement.Cup);
        ShowFingerTapTutorial();
        ShowPowerupTutorial();

        if (!string.IsNullOrEmpty(CurrentLevelData.TutorialMessage))
            Manager.Instance.UIManagement.ShowTutorial(CurrentLevelData.TutorialMessage);

        GraphicsTheme ThemeMaterials = Manager.Instance.EffectsManagement.Themes.FirstOrDefault(t => t.Theme == CurrentLevelData.GetTheme());
        Manager.Instance.EffectsManagement.ApplyTheme(ThemeMaterials);

        if (CurrentLevelData.IsHardLevel)
            Manager.Instance.UIManagement.ShowHardLevelMessage();

        Transform EntryPoint = Manager.Instance.ReferenceManagement.Conveyor.EntryPoint;

        if (!EntryPoint.transform.GetChild(0).TryGetComponent(out EntryPointCollider))
            Debug.LogWarning("No Capsule Collider found for entrypoint :" + EntryPoint, EntryPoint.gameObject);
    }

    private void ShowPowerupTutorial()
    {
        ShowTutorialForPowerup(CurrentLevel == Manager.Instance.PowerupManagement.UnlockSwapPowerupLvl, PlayerPrefsKeyManager.TutorialShownAlreadySwapPowerup, Manager.Instance.UIManagement.SwapPowerupBtn, PowerupManager.Powerup.Swap);
        ShowTutorialForPowerup(CurrentLevel == Manager.Instance.PowerupManagement.UnlockShufflePowerupLvl, PlayerPrefsKeyManager.TutorialShownAlreadyShufflePowerup, Manager.Instance.UIManagement.ShufflePowerupBtn, PowerupManager.Powerup.Shuffle);
        ShowTutorialForPowerup(CurrentLevel == Manager.Instance.PowerupManagement.UnlockInfoPowerupLvl, PlayerPrefsKeyManager.TutorialShownAlreadyInfoPowerup, Manager.Instance.UIManagement.InfoPowerupBtn, PowerupManager.Powerup.Info);
    }

    private void ShowTutorialForPowerup(bool ToShowTutorial, string PlayerPrefKey,Button PowerupButton, PowerupManager.Powerup PowerupType)
    {
        if (!ToShowTutorial || PlayerPrefs.GetInt(PlayerPrefKey, 0) == 1) return;

        if (PowerupType == PowerupManager.Powerup.Swap) 
            Manager.Instance.PowerupManagement.ShowHandForSwapTutorial = true;
            
        else if (PowerupType == PowerupManager.Powerup.Info) 
            Manager.Instance.PowerupManagement.ShowHandForInfoTutorial = true;
            
        Manager.Instance.PowerupManagement.AllowInputOnlyFor(PowerupType);
        RectTransform Target = PowerupButton.GetComponent<RectTransform>();
        Manager.Instance.TutorialManagement.ShowFingerOnPowerup(Target);
        Manager.Instance.InputAllowedOnCups = false;
        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(-4.5f, 0.2f);
        Manager.Instance.UIManagement.EnableFingerAnimationBackdrop("Tap to use powerup");
        PlayerPrefs.SetInt(PlayerPrefKey, 1);
    }

    private void ShowFingerTapTutorial()
    {
        if (CurrentLevelNumber != 1) return;

        Transform CupOnTable = Manager.Instance.GameManagement.CupsOnTableList.GetRandom(false);
        Manager.Instance.TutorialManagement.ShowFingerAt(CupOnTable.position);
        Manager.Instance.UIManagement.EnableFingerAnimationBackdrop();
    }

    private void SetupConveyer(int beltIndex, int layoutIndex)
    {
        Conveyer[] Conveyers = Manager.Instance.ReferenceManagement.Conveyers;
        for (int i = 0; i < Conveyers.Length; i++)
        {
            if (i == beltIndex)
            {
                Conveyers[i].ConveyerBelt.gameObject.SetActive(true);

                for (int j = 0; j < Conveyers[i].LayoutOptions.Length; j++)
                {
                    if (j == layoutIndex)
                    {
                        Conveyers[i].LayoutOptions[j].gameObject.SetActive(true);
                        Manager.Instance.ReferenceManagement.Conveyor = Conveyers[i].LayoutOptions[j].GetComponentInChildren<ConveyorSystem>();
                        TrayDispenser[] TrayDispensers = Conveyers[i].LayoutOptions[j].GetComponentsInChildren<TrayDispenser>();
                        TrayDispensers = TrayDispensers.OrderBy(td => td.transform.GetSiblingIndex()).ToArray();

                        for (int k = 0; k < TrayDispensers.Length; k++)
                        {
                            bool isActive = CurrentLevelData.TrayDispensers[k].Active;
                            TrayDispensers[k].gameObject.SetActive(isActive);
                            TrayDispensers[k].TraysToSpawn = CurrentLevelData.TrayDispensers[k].TraysToSpawn.ToArray();

                            if (isActive)
                            {
                                TrayDispensers[k].Orientation = CurrentLevelData.TrayDispensers[k].Orientation;
                                Vector2 Pos2D = CurrentLevelData.TrayDispensers[k].Position;
                                // TrayDispensers[k].transform.localPosition = new Vector3(Pos2D.x, TrayDispensers[k].transform.position.y, Pos2D.y);
                            }
                        }

                    }
                    else
                    {
                        Conveyers[i].LayoutOptions[j].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Conveyers[i].ConveyerBelt.gameObject.SetActive(false);
                foreach (Transform LayoutOption in Conveyers[i].LayoutOptions)
                    LayoutOption.gameObject.SetActive(false);

                // Destroy(Conveyers[i].ConveyerBelt.gameObject);
            }
        }
    }

    public void SetupLevel(GameObject Cup)
    {
        for (int i = 0; i < CurrentLevelData.CupArrangement.Width; i++)
        {
            for (int j = 0; j < CurrentLevelData.CupArrangement.Height; j++)
            {
                CupStack Stack = CurrentLevelData.CupArrangement.GetStack(i, j);
                CupStacks cupStacks = new();
                int StackLockedUntil = Stack.LockedStackUntil;
                Transform BaseCupCloneTransform = null;
                Transform HighestCupCloneTransform = null;

                for (int k = 0; k < Stack.Count; k++)
                {
                    string StackIdentifier = j.ToString() + "_" + i.ToString();
                    CupColors cupColor = Stack.Cups[k].Color;
                    bool IsMysterious = Stack.Cups[k].IsMysterious;
                    bool CanTakeInput = k == Stack.Count - 1;

                    GameObject CupClone = Instantiate(Cup, Vector3.zero, Quaternion.identity, CupParent);
                    CupClone.name = "Cup_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString();
                    Manager.Instance.GameManagement.RegisterCupOnTable(CupClone.transform);

                    Vector3 position = CupClone.GetComponent<Grid>().GetCellCenterLocal(new Vector3Int(i, j, k));

                    if (k == 0) BaseCupCloneTransform = CupClone.transform;
                    if (k == Stack.Count - 1) HighestCupCloneTransform = CupClone.transform;

                    CupClone.transform.position = position;

                    CupClone.GetComponent<Cup>().SetData(cupColor, StackIdentifier, CanTakeInput, IsMysterious);

                    cupStacks.Identifiers.Push(StackIdentifier);
                    cupStacks.Cups.Push(CupClone.transform);
                    cupStacks.Colors.Push(cupColor);
                }

                if (StackLockedUntil > 0) LockStack(StackLockedUntil, cupStacks, BaseCupCloneTransform, HighestCupCloneTransform);
                Manager.Instance.GameManagement.CupStacksList.Add(cupStacks);
            }
        }

        SetParentCenterTo(CupParent.gameObject, Table.gameObject);
        CupParent.transform.position = new Vector3(CupParent.transform.position.x, YPosOffset, CupParent.transform.position.z);

        RotateFromCenter(180f, CupParent);
    }

    private void LockStack(int stackLockedUntil, CupStacks cupStacks, Transform BaseCupCloneTransform, Transform HighestCupCloneTransform)
    {
        Vector3 PositionBaseOffset = new(0f, 5f, 0f);
        GameObject BlockerBoxPrefab = Manager.Instance.ReferenceManagement.BlockerBoxPrefab;
        GameObject SpawnedObj = Instantiate(BlockerBoxPrefab, BaseCupCloneTransform.position - PositionBaseOffset, Quaternion.identity, CupParent);
        if (SpawnedObj.TryGetComponent(out Blocker BlockerRef))
        {
            BlockerRef.TurnsToBreak = stackLockedUntil;
            BlockerRef.BlockedStackIdentifier = cupStacks.Identifiers.Peek();
        }
        float YScale = cupStacks.Cups.Count * Manager.Instance.GameManagement.BlockerHeightPerCup;
        SpawnedObj.transform.localScale = new Vector3(SpawnedObj.transform.localScale.x, YScale, SpawnedObj.transform.localScale.z);
        Manager.Instance.GameManagement.AddBlockedStack(cupStacks.Identifiers.Peek());
    }

    public void LoadNextLevel()
    {
        if (TestMode)
        {
            RestartLevel();
            return;
        }

        PlayerPrefs.SetInt("LastLevel", CurrentLevelNumber + 1);
        RestartLevel();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TestLevel(int LevelNumber)
    {
        TestLevelLoad = LevelNumber;
    }

    private void SetParentCenterTo(GameObject parent, GameObject target)
    {
        if (parent.transform.childCount == 0) return;
        Bounds bounds = new(parent.transform.GetChild(0).position, Vector3.zero);

        foreach (Transform child in parent.transform)
            bounds.Encapsulate(child.position);

        Vector3 targetCenter = target.transform.position;
        Vector3 offset = parent.transform.position - bounds.center;
        parent.transform.position = targetCenter + offset;
    }

    private void RotateFromCenter(float Amount, Transform Target)
    {
        if (Target.transform.childCount == 0) return;
        Bounds bounds = new(Target.transform.GetChild(0).position, Vector3.zero);

        foreach (Transform child in Target.transform)
            bounds.Encapsulate(child.position);

        Target.transform.RotateAround(bounds.center, Vector3.up, Amount);
    }
}
