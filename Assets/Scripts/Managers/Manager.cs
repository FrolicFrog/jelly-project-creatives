using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using BlockStackTypes;
using PlayerPrefsManager;
using Managers;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    [Header("GAME SETTINGS")]

    [Range(1,10)]
    public int MinLevelNumber = 1;

    [Range(3,100)]
    public int MaxLevelNumber = 10;
    [Range(-1, 60)]
    public int TargetFPS = -1;

    [Header("MANAGERS")]
    public GameManager GameManagement;
    public UIManager UIManagement;
    public LevelManager LevelManagement;
    public ReferenceManager ReferenceManagement;
    public PrizeManager PrizeManagement;
    public EffectsManager EffectsManagement;
    public TutorialManager TutorialManagement;
    public HapticsManager HapticsManagement;
    public AudioManager AudioManagement;
    public DevManager DevManagement;
    public Managers.AnalyticsManager AnalyticsManagement;
    public PowerupManager PowerupManagement;

    [Header("TWEEN SETTINGS")]
    public UpdateType TweenUpdateType = UpdateType.Normal;
    public bool UseTimeScaleIndependentTweens = true;

    [Header("STATE BOOLS")]
    public bool IsLevelCompleted = false;

    public Action OnLevelCompleted;
    public Action OnLevelFailed;

    private bool _IsLevelFailed = false;
    public bool IsLevelFailed => _IsLevelFailed;

    public bool InputAllowedOnCups = true;
    public bool AlreadyRevived = false;

    void Awake()
    {
        Instance = this;
        GameManagement.OnPlacingCupsOnConveyer += CheckGameState;
        Application.targetFrameRate = TargetFPS;
        DOTween.SetTweensCapacity(1000, 100);
        DOTween.defaultTimeScaleIndependent = UseTimeScaleIndependentTweens;
        DOTween.defaultUpdateType = TweenUpdateType;
    }

    void Start()
    {
        string Key = PlayerPrefsKeyManager.LevelNumberName(LevelManagement.CurrentLevel);
        int CurrentAttempts = PlayerPrefs.GetInt(Key, 0);
        PlayerPrefs.SetInt(Key, CurrentAttempts + 1); //Including the current attempt...
        AnalyticsManagement.LevelStarted(LevelManagement.CurrentLevel);
    }

    public void CheckGameState()
    {
        List<CupStacks> CupStackList = GameManagement.CupStacksList;
        CupStackList.ForEach(x =>
        {
            if (x.Cups.TryPeek(out Transform Cup))
                Cup.GetComponent<Cup>().TakeInput = true;
        });

        WarnForLeftSpaces(GameManagement.CupsOnConveyer.Count, ReferenceManagement.Conveyor.MaximumCupsAllowed);
        if (GameManagement.AllTraySpawned)
        {
            LevelCompleted();
            OnLevelCompleted?.Invoke();
        }
        else if (GameManagement.ConveyorFilled && !GameManagement.NoCupsOnTable)
        {
            LevelFailed();
            OnLevelFailed?.Invoke();
        }
    }

    private void WarnForLeftSpaces(int count, int maximumCupsAllowed)
    {
        if (maximumCupsAllowed - count <= 6)
        {
            UIManagement.ShowInsufficientUI(maximumCupsAllowed - count);
        }
        else
        {
            UIManagement.HideInsufficientUI();
        }
    }

    public void LevelFailed()
    {
        if (_IsLevelFailed) return;

        _IsLevelFailed = true;
        AudioManagement.PlayAudioEffect("LevelFailed");
        SessionPrefs.Set("lost_last_level", true);
        UIManagement.FailedSequence();
        HapticsManagement.PlayHaptic(HapticsManagement.GameOver);
        AnalyticsManagement.LevelFailed(LevelManagement.CurrentLevel);
    }

    private CupStacks GetRandomFilledStack(List<CupStacks> StackList, HashSet<CupStacks> SelectedStacks)
    {
        CupStacks RandomStack = StackList.GetRandom();
        if (RandomStack.Cups.Count == 0 || SelectedStacks.Contains(RandomStack) || GameManagement.BlockedStackIdentifiers.Contains(RandomStack.Identifiers.Peek()))
            return GetRandomFilledStack(StackList, SelectedStacks);

        SelectedStacks.Add(RandomStack);
        return RandomStack;
    }

    private void LevelCompleted()
    {
        if (IsLevelCompleted)
        {
            Debug.LogWarning("Level already completed");
            return;
        }

        IsLevelCompleted = true;
        AudioManagement.PlayAudioEffect("Victory");
        UIManagement.VictorySequence();
        HapticsManagement.PlayHaptic(HapticsManagement.GameWin);
        AnalyticsManagement.LevelCompleted(LevelManagement.CurrentLevel);
    }
}