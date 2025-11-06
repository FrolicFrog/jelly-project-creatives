using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CurvedPathGenerator;
using BlockStackTypes;
using System.Collections;
using UnityEngine.Events;
using CupStackManagement;
using System.Linq;
using UnityEditor;
using Unity.Collections;
public class GameManager : MonoBehaviour
{
    [Header("SETTINGS")]
    [Space(10)]

    [Header("Cup Spawning")]
    [Range(0.1f, 10f)] public float YTargetOffset = 4.5f;
    public float CompensationSpeedMultiplier = 1.2f;
    [Range(100f, 7500f)] public float CupSpeed = 500f;
    public List<CupStacks> CupStacksList = new();

    [Header("Curve Controls")]
    public AnimationCurve JumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(2, 20)] public int CurveSteps = 10;
    [Range(1f, 5f)] public float ArcHeight = 3f;

    [Range(0f, 5f)] public float ConveyerAnimArcHeight = 1.5f;

    [Header("Cup Stacking Animations")]
    [Range(0.001f, 0.5f)] public float TimeToGetStacked = 0.05f;
    public float SquishTweak = 1f;

    [Range(0f, 1f)] public float StackScaleAnimationDuration = 0.05f;

    [Header("Cup Conveyer Animations")]
    public float DistanceBtwCups = 1f;
    public float MinDistanceBtwCups = 0.5f;
    [Range(0.1f, 50f)] public float ConveyerAnimateScaleDownMultiplier = 1.2f;
    [Range(0.1f, 50f)] public float ConveyerAnimateScaleUpMultiplier = 1.2f;
    [Range(0.1f, 50f)] public float TimeToGetConveyer = 0.25f;
    [Range(0.1f, 50f)] public float ConveyerScaleAnimationDuration = 0.25f;
    [Range(0.1f, 50f)] public float ConveyerScaleNormalDuration = 0.25f;
    [Range(0.1f, 50f)] public float RotationAnimDuration = 0.25f;
    [Range(0.1f, 50f)] public float ElevationAnimDuration = 0.25f;
    [Range(-10f, 10f)] public float RequiredElevation = 4.5f;
    [Range(0f, 5f)] public float DelayInCups = 0.1f;

    [Header("Tray Dispenser Animations")]
    [Range(0.1f, 50f)] public float TraySlideAnimationDuration = 0.25f;

    [Header("POWERUPS")]
    public float SwapAnimationDuration = 0.12f;

    [Header("CUP TRAY COLLECTION")]
    public float DistanceForCollection = 0.5f;

    [Header("TRAY DISPENSERS")]
    public float ScaleDownMultiplier = 0.8f;
    public TrayTrailColors[] TrayTrailColors;

    [Header("BLOCKERS")]
    public float BlockerHeightPerCup = 0.55f;


    [Range(0.8f, 1f)]
    public float TrayJiggleScale = 0.95f;

    [Header("TRAY PACKING")]
    public float ScaleReduction = 0.8f;
    public float TrayPackingTime = 1f;
    public float TimeForCupsPlacement = 0.25f;
    public float CupOnTrayPackingAnimationArcHeight = 1f;
    public float CupOnTrayAnimCurveSteps = 10;
    public AnimationCurve CupOnTrayAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _CupsEnteringConveyer = false;
    private List<Transform> CurrentCups = new();
    private PathGenerator CurrentLevelConveyerPath => Manager.Instance.ReferenceManagement.Conveyor.Path;
    public Vector3 InitialScale => Vector3.one * 8f;
    public List<Transform> CupsOnConveyer = new();
    public bool CupsEnteringConveyer
    {
        set
        {
            _CupsEnteringConveyer = value;
        }

        get => _CupsEnteringConveyer;
    }

    private TrayDispenser[] _Dispensers;
    public TrayDispenser[] TrayDispensers
    {
        get => _Dispensers;
    }

    public Action OnPlacingCupsOnConveyer;
    public List<string> BlockedStackIdentifiers = new();
    private List<Transform> CupsOnTable = new();
    public List<Transform> CupsOnTableList => CupsOnTable;
    private List<Transform> GhostPreviewCups;
    public List<Transform> PreviewCups => GhostPreviewCups ??= new List<Transform>();

    // State Properties of Cups
    public float LevelProgressPercent => 100f - ((float)CupsOnTable.Count / CupsOnTableInitially * 100f);
    public int CupsOnTableInitially;
    public bool NoCupsOnTable => CupsOnTable.Count == 0;
    public bool NoCupsBeingAnimated => CurrentCups.Count == 0;
    public bool ConveyorFilled => Manager.Instance.ReferenceManagement.Conveyor.MaximumCupsAllowed == CupsOnConveyer.Count;
    public int MovesCount { private set; get; } = 0;

    public bool AllTraySpawned
    {
        get
        {
            foreach (TrayDispenser Dispenser in _Dispensers)
            {
                if (!Dispenser.AllTraySpawned) return false;
            }
            return true;
        }
    }
    public UnityEvent OnCupsEnteringConveyerEnd;
    private Queue<List<Transform>> conveyorQueue = new();
    private bool isAnimatingConveyor = false;
    private int MaximumCupsAllowed = -1;
    private int _SuccessRate = 100;
    public int SuccessRate => _SuccessRate;
    private List<ArrowPointer> ArrowPointers = new();
    public bool ConveyerPausedExplicitly = false;

    void Start()
    {
        _Dispensers = FindObjectsByType<TrayDispenser>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        CupsOnTableInitially = CupsOnTable.Count;
        MaximumCupsAllowed = Manager.Instance.ReferenceManagement.Conveyor.MaximumCupsAllowed;
    }

    private void Update()
    {
        UpdateSuccessRate();
    }

    public void ClearConveyerQueue()
    {
        conveyorQueue.Clear();
    }

    public void RegisterPointerArrow(ArrowPointer _ArrowPointer)
    {
        ArrowPointers.Add(_ArrowPointer);
    }
    public void ClearPointerArrows()
    {
        foreach (ArrowPointer arptr in ArrowPointers)
        {
            if (arptr == null) continue;
            Destroy(arptr.gameObject);
        }
    }

    private void UpdateSuccessRate()
    {
        int emptySpaces = MaximumCupsAllowed - CupsOnConveyer.Count;
        int NewRate = GetSuccessRate(emptySpaces);

        if (NewRate < _SuccessRate)
            _SuccessRate = NewRate;
    }

    private int GetSuccessRate(int emptySpaces)
    {
        if (emptySpaces <= 5) return 10;
        if (emptySpaces < 10) return 25;
        if (emptySpaces < 20) return 50;
        if (emptySpaces < 25) return UnityEngine.Random.Range(75, 81);
        if (emptySpaces >= 25) return 100;
        return 0;
    }

    public void ShuffleAllCupStacks(Action OnComplete)
    {
        List<CupStacks> CupStacksList = Manager.Instance.GameManagement.CupStacksList;
        Manager.Instance.InputAllowedOnCups = false;

        for (int i1 = 0; i1 < CupStacksList.Count; i1++)
        {
            CupStacks Target = CupStacksList[i1];
            Sequence seq = DOTween.Sequence();

            List<Transform> Cups = Target.Cups.ToList();
            List<string> Identifiers = Target.Identifiers.ToList();
            List<CupColors> Colors = Target.Colors.ToList();
            List<Vector3> Positions = Cups.Select(x => x.position).ToList();

            int count = Cups.Count;
            List<int> originalIndices = Enumerable.Range(0, count).ToList();
            List<int> shuffledIndices = originalIndices.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < count; i++)
            {
                int fromIndex = originalIndices[i];
                int toIndex = shuffledIndices[i];
                seq.Join(Cups[fromIndex].DOMove(Positions[toIndex], 0.25f).SetEase(Ease.InOutBack));
            }

            Transform[] NewCups = new Transform[count];
            string[] NewIdentifiers = new string[count];
            CupColors[] NewColors = new CupColors[count];

            for (int i = 0; i < count; i++)
            {
                int toIndex = shuffledIndices[i];
                NewCups[toIndex] = Cups[i];
                NewIdentifiers[toIndex] = Identifiers[i];
                NewColors[toIndex] = Colors[i];
            }

            int indexCopy = i1;
            seq.OnComplete(() =>
            {
                Target.Cups = new Stack<Transform>(NewCups.Reverse().ToList());
                Target.Identifiers = new Stack<string>(NewIdentifiers.Reverse().ToList());
                Target.Colors = new Stack<CupColors>(NewColors.Reverse().ToList());

                List<CupStacks> CupStacks = Manager.Instance.GameManagement.CupStacksList;
                CupStacks.ForEach((x) =>
                {
                    if (x.Cups.Count == 0) return;
                    List<Transform> Cups = x.Cups.ToList();
                    Cups.ForEach(cup => cup.GetComponent<Cup>().TakeInput = false);
                    Cups[0].GetComponent<Cup>().TakeInput = true;
                });

                if (indexCopy == CupStacksList.Count - 1) OnComplete?.Invoke();
            });
        }

        DOVirtual.DelayedCall(0.25f, () =>
        {
            List<CupStacks> StackList = Manager.Instance.GameManagement.CupStacksList;
            StackList.ForEach(x =>
            {
                if (x.Cups.Count == 0) return;
                x.Cups.Peek().GetComponent<Cup>().RevealMysteriousCup();
            });
        });
    }



    public void StackCupsIfAvailable(CupColors color, Vector3 target, string StackIdentifier, Action OnComplete)
    {
        if (ConveyorFilled) return;

        MovesCount++;
        Sequence seq = DOTween.Sequence();
        CurrentCups.Clear();
        List<Transform> StackedCups = new();

        bool hasCupsToMove = false;
        int CupIndex = 1;

        List<CupStacks> AdjacentCupStacksList = CupStackManager.GetAdjacentCupStacks(StackIdentifier, color, CupStacksList);

        if(AdjacentCupStacksList != null && AdjacentCupStacksList.Count > 0)
        AdjacentCupStacksList.RemoveAll(x =>
        {
            if (x == null || x.Identifiers == null || x.Identifiers.Count == 0) return false;
            string Identifier = x.Identifiers.Peek();

            if (BlockedStackIdentifiers != null && BlockedStackIdentifiers.Count > 0)
                return BlockedStackIdentifiers.Contains(Identifier);

            return false;
        });

        Cup CupToReveal = null;

        foreach (CupStacks stack in AdjacentCupStacksList)
        {
            while (stack.Colors.Count > 0 && stack.Colors.Peek() == color && !stack.Cups.Peek().GetComponent<Cup>().IsCupMysterious)
            {
                hasCupsToMove = true;
                CupsEnteringConveyer = true;
                Manager.Instance.DevManagement.StartLevelTimer();

                stack.Colors.Pop();
                Transform topCup = stack.Cups.Pop();

                if (stack.Cups.Count > 0)
                    CupToReveal = stack.Cups.Peek().GetComponent<Cup>();

                string CurrentIdentifier = stack.Identifiers.Pop();
                topCup.GetComponent<BoxCollider>().enabled = false;
                CupsOnTable.Remove(topCup);
                Manager.Instance.UIManagement.UpdateProgressBar(LevelProgressPercent);


                if (CurrentIdentifier != StackIdentifier)
                {
                    if (stack.Cups.Count > 0)
                    {
                        Transform Cup = stack.Cups.Peek();
                        Cup.GetComponent<Cup>().TakeInput = true;
                    }

                    topCup.gameObject.GetComponent<Cup>().TakeInput = false;

                    Vector3 finalPos = target + new Vector3(0, CupIndex * YTargetOffset, 0);
                    List<Vector3> pathPoints = AnimationManager.GenerateStackAnimationPath(target, topCup, finalPos, ArcHeight, CurveSteps, JumpCurve);

                    AnimationManager.CupStackAnimation(seq, topCup, pathPoints, TimeToGetStacked, SquishTweak, ++CupIndex, in StackedCups);
                }

                CurrentCups.Add(topCup);
                StackedCups.Add(topCup);
            }
        }

        if (hasCupsToMove) seq.OnComplete(() =>
        {
            OnComplete?.Invoke();
            AnimateToConveyorEntry(StackedCups);
        });
    }

    private void AnimateToConveyorEntry(List<Transform> CurrentCups)
    {
        conveyorQueue.Enqueue(new List<Transform>(CurrentCups));
        TryProcessConveyorQueue();
    }

    public void TryProcessConveyorQueue()
    {
        if (isAnimatingConveyor || conveyorQueue.Count == 0 || Manager.Instance.IsLevelFailed)
            return;

        isAnimatingConveyor = true;
        List<Transform> cupsToAnimate = conveyorQueue.Dequeue();

        Vector3 targetPosition = Manager.Instance.ReferenceManagement.Conveyor.GetEntryPosition;
        Sequence CupStackingAnimSeq = DOTween.Sequence();
        Utilities.SortByYPos(ref cupsToAnimate);

        foreach (Transform Cup in cupsToAnimate)
        {
            Vector3 startPos = Cup.transform.position;
            Vector3 midPos = new(targetPosition.x, Cup.position.y, targetPosition.z);
            Vector3 finalPos = targetPosition;

            List<Vector3> pathPoints = AnimationManager.GenerateConveyerAnimationPath(startPos, midPos, finalPos, CurveSteps, JumpCurve, ConveyerAnimArcHeight);
            Sequence CupSeq = DOTween.Sequence();

            AnimationManager.CupConveyerAnimation(Cup, pathPoints, CupSeq, TimeToGetConveyer, RotationAnimDuration, () => OnPathComplete(Cup), InitialScale, ConveyerAnimateScaleDownMultiplier, ConveyerAnimateScaleUpMultiplier, ConveyerScaleAnimationDuration, ConveyerScaleNormalDuration, RequiredElevation, ElevationAnimDuration, () => OnAnimationComplete(Cup), this);
            CupStackingAnimSeq.SetDelay(DelayInCups).Join(CupSeq);
        }

        CupsEnteringConveyer = true;
        Manager.Instance.DevManagement.StartLevelTimer();
        Manager.Instance.LevelManagement.EntryAllowed = false;

        CupStackingAnimSeq.Play().OnComplete(() =>
        {
            CupsEnteringConveyer = false;
            isAnimatingConveyor = false;
            Manager.Instance.LevelManagement.EntryAllowed = true;

            CupStackManager.RevealMysteriousCupIfAny(CupStacksList);
            OnPlacingCupsOnConveyer?.Invoke();

            TryProcessConveyorQueue();
        });
    }

    private void OnAnimationComplete(Transform Cup)
    {
        CupsOnConveyer.Add(Cup);
        CurrentCups.Remove(Cup);

        if (Cup.TryGetComponent(out Cup cup))
        {
            DOVirtual.DelayedCall(1f, () => cup.ReadyForCollection = true, true);
        }

        Manager.Instance.CheckGameState();
    }

    private void OnPathComplete(Transform Cup)
    {
        if (CurrentCups.Count == 1)
        {
            CupsEnteringConveyer = false;

            OnCupsEnteringConveyerEnd?.Invoke();
            OnCupsEnteringConveyerEnd = null;

            Manager.Instance.LevelManagement.EntryAllowed = true;
        }
        Manager.Instance.CheckGameState();
        Cup.GetComponent<Cup>().StartMovingOnConveyer(CurrentLevelConveyerPath, CupSpeed);
        Manager.Instance.HapticsManagement.PlayHaptic(Manager.Instance.HapticsManagement.CupsConveyerPlacement);
    }

    public void RegisterCupOnTable(Transform cup)
    {
        if (!CupsOnTable.Contains(cup)) CupsOnTable.Add(cup);
    }

    public void RegisterGhostPreviewCup(Transform cup)
    {
        GhostPreviewCups ??= new List<Transform>();
        if (!GhostPreviewCups.Contains(cup)) GhostPreviewCups.Add(cup);
    }
    public void AddBlockedStack(string stackIdentifier)
    {
        if (!BlockedStackIdentifiers.Contains(stackIdentifier)) BlockedStackIdentifiers.Add(stackIdentifier);
    }

    public void RemoveBlockedStack(string stackIdentifier)
    {
        if (BlockedStackIdentifiers.Contains(stackIdentifier)) BlockedStackIdentifiers.Remove(stackIdentifier);
    }
    public void PauseConveyer()
    {
        //Paused Cups
        ConveyerPausedExplicitly = true;
        foreach (Transform cup in CupsOnConveyer)
        {
            cup.GetComponent<PathFollower>().IsMove = false;
        }
    }
    public void PlayConveyer()
    {
        foreach (Transform cup in CupsOnConveyer)
        {
            cup.GetComponent<PathFollower>().IsMove = true;
        }
        ConveyerPausedExplicitly = false;
    }

}
