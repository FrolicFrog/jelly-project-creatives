using UnityEngine;
using UnityEngine.EventSystems;
using CurvedPathGenerator;
using BlockStackTypes;
using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using CupStackManagement;

public class Cup : MonoBehaviour, IPointerDownHandler
{
    [Serializable]
    public class CupData
    {
        public CupColors CupColor;
        public GameObject CupGbj;

        public override string ToString()
        {
            return CupColor.ToString();
        }
    }
    [Header("SETTINGS")]
    public bool IsEntryPointBlocker = false;
    public float ArcWidth = 25f;

    [Header("REFERENCES")]
    public CupData[] CupParts;
    public Animator AnimationSystem;
    public Transform EyesTransform;

    [Header("DETECTION")]
    public LayerMask CupLayer;
    public Cup CupInFront;
    public Vector3 RaycastOffset;
    public float RaycastDistance;

    private bool CanTakeInput = false;
    public PathFollower ObjectPathFollower;
    private CupColors _CurrentCupColor = CupColors.Blue;
    public CupColors CurrentCupColor => _CurrentCupColor;
    public string StackIdentifier;
    public bool TakeInput
    {
        get => CanTakeInput;
        set => CanTakeInput = value;
    }

    private bool _isCupHighlighted = false;
    public bool HighlightCup
    {
        set
        {
            _isCupHighlighted = value;
            UpdateVisuals();
        }

        get => _isCupHighlighted;
    }

    public TrayDispenser[] CurrentTrayDispensers => Manager.Instance.GameManagement.TrayDispensers;
    public float DistanceForCollection => Manager.Instance.GameManagement.DistanceForCollection;
    private Material OriginalMaterial;
    private bool IsBeingCollected = false;
    public bool CupIsBeingCollected => IsBeingCollected;
    public bool ReadyForCollection = false;
    private bool IsMysterious = false;
    public bool IsCupMysterious => IsMysterious;
    private bool InputAllowed => CanTakeInput && Manager.Instance.InputAllowedOnCups;
    private Coroutine _lerpCoroutine;
    private bool IsDesigner
    {
        get => _CurrentCupColor == CupColors.BlueDesigner || _CurrentCupColor == CupColors.BrownDesigner || _CurrentCupColor == CupColors.CyanDesigner || _CurrentCupColor == CupColors.GreenDesigner || _CurrentCupColor == CupColors.OrangeDesigner || _CurrentCupColor == CupColors.PinkDesigner || _CurrentCupColor == CupColors.PurpleDesigner || _CurrentCupColor == CupColors.RedDesigner || _CurrentCupColor == CupColors.TealDesigner || _CurrentCupColor == CupColors.YellowDesigner;
    }
    private int FrameCount = 0;
    private int ColorA = Shader.PropertyToID("_Color_A");
    private int ColorB = Shader.PropertyToID("_Color_B");
    private int AO = Shader.PropertyToID("_AO");
    [HideInInspector] public GameObject FillingGbj;
    public bool PausedExplicitly = false;
    public Transform Eyes;
    internal bool IsSquashed;

    private void Awake()
    {
        ObjectPathFollower = GetComponent<PathFollower>();
        if (IsEntryPointBlocker) return;

        InvokeRepeating(nameof(AnimateEyes), UnityEngine.Random.Range(0f, 5f), 3f);
    }

    void Update()
    {
        FrameCount++;
        if (FrameCount % 2 != 0) return;
        if (IsEntryPointBlocker) return;
        if (IsBeingCollected) return;
        if (ObjectPathFollower == null) return;
        if (ReadyForCollection) CheckTrayDispensersForCupPlacement();
        if (ObjectPathFollower.Generator != null) HandleFrontCupDetection();
        if (Manager.Instance.GameManagement.ConveyerPausedExplicitly) return;

        if (CupInFront == null)
        {
            if (PausedExplicitly == false)
                ObjectPathFollower.IsMove = true;
            return;
        }

        if (CupInFront.IsEntryPointBlocker && Manager.Instance.GameManagement.CupsEnteringConveyer)
        {
            if (Manager.Instance.GameManagement.OnCupsEnteringConveyerEnd == null)
            {
                Manager.Instance.GameManagement.OnCupsEnteringConveyerEnd = new UnityEngine.Events.UnityEvent();
                Manager.Instance.GameManagement.OnCupsEnteringConveyerEnd.AddListener(() => ObjectPathFollower.IsMove = true);
            }

            ObjectPathFollower.IsMove = false;
            return;
        }

        if (Vector3.Distance(transform.position, CupInFront.transform.position) > Manager.Instance.GameManagement.DistanceBtwCups && !Manager.Instance.GameManagement.ConveyerPausedExplicitly)
        {
            ObjectPathFollower.IsMove = true;
            ObjectPathFollower.Speed = Manager.Instance.GameManagement.CompensationSpeedMultiplier * Manager.Instance.GameManagement.CupSpeed;
        }
        else
        {
            ObjectPathFollower.IsMove = CupInFront.ObjectPathFollower.IsMove;
            ObjectPathFollower.Speed = Manager.Instance.GameManagement.CupSpeed;
        }
    }

    public void UnassignPathFollower()
    {
        ObjectPathFollower.Generator = null;
    }

    private void HandleFrontCupDetection()
    {
        CupInFront = null;
        Vector3 origin = transform.position + RaycastOffset;
        Vector3[] directions = new Vector3[]
        {
            transform.forward,
            Quaternion.Euler(0, -ArcWidth, 0) * transform.forward,
            Quaternion.Euler(0, ArcWidth, 0) * transform.forward,
        };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, RaycastDistance, CupLayer))
            {
                CupInFront = hit.transform.GetComponent<Cup>();
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 origin = transform.position + RaycastOffset;

        Vector3[] directions = new Vector3[]
        {
            transform.forward,
            Quaternion.Euler(0, -ArcWidth, 0) * transform.forward,
            Quaternion.Euler(0, ArcWidth, 0) * transform.forward,
        };

        foreach (var dir in directions)
        {
            Gizmos.DrawLine(origin, origin + dir * RaycastDistance);
        }

        if (CanTakeInput)
            Gizmos.DrawIcon(transform.position + new Vector3(0f, 5f, 0f), "CupInput", true);
    }

    private void CheckTrayDispensersForCupPlacement()
    {
        if (!ObjectPathFollower.IsMove || Manager.Instance.IsLevelFailed) return;
        TrayDispenser NearestDispenser = null;
        float nearestDistance = float.MaxValue;

        foreach (TrayDispenser Dispenser in CurrentTrayDispensers)
        {
            if (Dispenser.GetRequiredCup == _CurrentCupColor)
            {
                float dist = Vector3.Distance(transform.position, Dispenser.FrontSpawnPoint.position);
                if (dist <= DistanceForCollection && dist < nearestDistance)
                {
                    nearestDistance = dist;
                    NearestDispenser = Dispenser;
                }
            }
        }

        if (NearestDispenser != null)
        {
            Vector3? Position = NearestDispenser.GetPositionToLandCup();
            if (!Position.HasValue) return;

            Manager.Instance.GameManagement.CupsOnConveyer.Remove(transform);
            GetPlacedOnTray(Position.Value, NearestDispenser);
        }
    }

    private void GetPlacedOnTray(Vector3 position, TrayDispenser nearestDispenser)
    {
        IsBeingCollected = true;
        ObjectPathFollower.IsMove = false;
        ObjectPathFollower.Generator = null;

        List<Vector3> pathPoints = new List<Vector3>();
        Sequence seq = DOTween.Sequence();
        float adjustedArcHeight = Mathf.Max(Manager.Instance.GameManagement.CupOnTrayPackingAnimationArcHeight, Mathf.Abs(position.y - transform.position.y) + 1f);

        for (int i = 0; i <= Manager.Instance.GameManagement.CupOnTrayAnimCurveSteps; i++)
        {
            float t = i / Manager.Instance.GameManagement.CupOnTrayAnimCurveSteps;

            Vector3 parabolicPoint = Vector3.Lerp(transform.position, position, t);
            parabolicPoint.y += Manager.Instance.GameManagement.CupOnTrayAnimCurve.Evaluate(t) * adjustedArcHeight;
            if (t == 1f) parabolicPoint = position;
            pathPoints.Add(parabolicPoint);
        }

        transform.SetParent(nearestDispenser.FrontTrayTransform, true);

        seq.Join(transform.DOPath(pathPoints.ToArray(), Manager.Instance.GameManagement.TimeForCupsPlacement, PathType.CatmullRom)
        .SetLink(transform.gameObject)
        .SetEase(Ease.Linear))
        .Join(transform.DOScale(transform.localScale * Manager.Instance.GameManagement.ScaleReduction, Manager.Instance.GameManagement.TimeForCupsPlacement))
        .OnComplete(() =>
        {
            TrayDispenser TargetDispenser = nearestDispenser;
            Manager.Instance.AudioManagement.PlayAudioEffect("CupOnTray2", LayeredSound: true);
            TargetDispenser.Jiggle();
            AfterPlacementSequence();
            nearestDispenser.UpdateCurrentFrontTrayPositionIndex();
        });
    }

    public float GetTravelledPath()
    {
        if (ObjectPathFollower != null && ObjectPathFollower.Generator != null)
            return ObjectPathFollower.GetPassedLength() / ObjectPathFollower.Generator.GetLength() * 100f;

        return 0f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Manager.Instance.AudioManagement.PlayAudioEffect("CupTapSound", LayeredSound: true);
        if (!InputAllowed)
        {
            Debug.LogWarning("Input is not allowed yet for this cup");
            return;
        }

        CanTakeInput = false;
        Manager.Instance.GameManagement.StackCupsIfAvailable(_CurrentCupColor, transform.position, StackIdentifier, () => { });
    }

    public void SetData(CupColors cupColor, string stackIdentifier, bool canTakeInput = true, bool IsMysterious = false)
    {
        _CurrentCupColor = cupColor;
        StackIdentifier = stackIdentifier;
        CanTakeInput = canTakeInput;
        this.IsMysterious = IsMysterious;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        foreach (CupData cupData in CupParts)
        {
            if (cupData.CupColor == _CurrentCupColor)
            {
                cupData.CupGbj.SetActive(true);
                Transform CharacterMesh = cupData.CupGbj.transform.GetChild(0);
                Eyes = CharacterMesh.GetChild(1);

                ApplyBreathingTween(cupData.CupGbj.transform);
                MeshRenderer meshRenderer = cupData.CupGbj.transform.GetChild(0).transform.GetChild(0).GetComponent<MeshRenderer>();

                if (_isCupHighlighted)
                {
                    meshRenderer.material = null;
                }
                else if (IsMysterious)
                {
                    OriginalMaterial = meshRenderer.material;
                    meshRenderer.material = IsDesigner ? Manager.Instance.ReferenceManagement.DesignerMysteriousMaterial : Manager.Instance.ReferenceManagement.MysteriousMaterial;
                }
                else if (OriginalMaterial != null && meshRenderer.material != OriginalMaterial)
                {
                    if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
                    _lerpCoroutine = StartCoroutine(LerpMaterialRoutine(meshRenderer, meshRenderer.material, OriginalMaterial, 0.1f));
                }
            }
            else
            {
                Destroy(cupData.CupGbj);
            }
        }
    }

    private void ApplyBreathingTween(Transform transform)
    {
        if (transform == null) return;
        transform.DOScaleY(transform.localScale.y * 1.1f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(UnityEngine.Random.Range(0f, 3f)).SetLink(gameObject);
    }

    private void AnimateEyes()
    {
        if (Eyes == null) return;
        Eyes.DOScaleZ(0f, 0.15f).SetEase(Ease.InOutBack).SetLoops(2, LoopType.Yoyo).SetLink(Eyes.gameObject);
    }

    private IEnumerator LerpMaterialRoutine(MeshRenderer renderer, Material fromMat, Material toMat, float duration)
    {
        Color fromColorA = fromMat.GetColor(ColorA);
        Color fromColorB = fromMat.GetColor(ColorB);
        Color fromAO = fromMat.GetColor(AO);

        Color toColorA = toMat.GetColor(ColorA);
        Color toColorB = toMat.GetColor(ColorB);
        Color toAO = toMat.GetColor(AO);

        float time = 0f;
        Material mat = renderer.material;

        while (time < duration)
        {
            float t = time / duration;

            mat.SetColor(ColorA, Color.Lerp(fromColorA, toColorA, t));
            mat.SetColor(ColorB, Color.Lerp(fromColorB, toColorB, t));
            mat.SetColor(AO, Color.Lerp(fromAO, toAO, t));

            time += Time.deltaTime;
            yield return null;
        }

        mat.SetColor(ColorA, toColorA);
        mat.SetColor(ColorB, toColorB);
        mat.SetColor(AO, toAO);

        _lerpCoroutine = null;
    }

    public void StartMovingOnConveyer(PathGenerator Path, float Speed)
    {
        ObjectPathFollower.Speed = Speed;
        ObjectPathFollower.Generator = Path;
        ObjectPathFollower.StartFollow();
    }
    public void AfterPlacementSequence()
    {
        Manager.Instance.HapticsManagement.PlayHaptic(Manager.Instance.HapticsManagement.CupTrayLanding);
    }

    public void RevealMysteriousCup()
    {
        CanTakeInput = true;
        if (!IsMysterious) return;
        IsMysterious = false;
        UpdateVisuals();
    }
}
