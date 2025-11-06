using UnityEngine;
using BlockStackTypes;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class TrayDispenser : MonoBehaviour, IPointerDownHandler
{
    [Header("REFERENCES")]
    public Transform FrontSpawnPoint;
    public Transform BackSpawnPoint;

    [Header("SETTINGS")]
    public TrayOrientation Orientation;
    public bool MatchTraysOrientation = true;
    public Tray[] TraysToSpawn;
    public float ScaleDownMultiplier => Manager.Instance.GameManagement.ScaleDownMultiplier;

    private int CurrentTrayIndex = 0;
    public Transform BackTrayTransform;
    private Transform _FrontTrayTransform;
    public Transform FrontTrayTransform => _FrontTrayTransform;
    private Vector3 BackTrayOriginalScale;

    private bool RotateTrays => Orientation == TrayOrientation.Up || Orientation == TrayOrientation.Down;
    public CupColors GetRequiredCup
    {
        get
        {
            if (_FrontTrayTransform == null) return CupColors.Blue;
            if (RequiredCups.TryGetValue(_FrontTrayTransform, out CupColors cupColor))
                return cupColor;
            return CupColors.Blue;
        }
    }

    private Sequence JiggleSequence;
    private TextMeshPro TraysLeftToSpawnLabel => GetComponentInChildren<TextMeshPro>();
    private RectTransform TraysLeftToSpawnLabelRect => TraysLeftToSpawnLabel.GetComponent<RectTransform>();
    private Dictionary<Transform, CupColors> RequiredCups = new Dictionary<Transform, CupColors>();
    public bool AllTraySpawned => CurrentTrayIndex >= TraysToSpawn.Length && FrontTrayTransform == null && BackTrayTransform == null;
    public bool TraysLeftToSpawn => CurrentTrayIndex < TraysToSpawn.Length;


    [Header("POWERUPS")]
    public bool SwitchModeEnabled = false;
    public bool InfoModeEnabled = false;
    private bool _IsSwitchingTrays = false;
    public bool IsSwitchingTrays => _IsSwitchingTrays;


    public Action OnSwitch;
    async void Start()
    {
        // SetupRotationByOrientation(Orientation);
        _FrontTrayTransform = await CreateTray(true);
        BackTrayTransform = await CreateTray();
    }

    private void SetupRotationByOrientation(TrayOrientation Orientation)
    {
        Quaternion originalChildRot = TraysLeftToSpawnLabelRect.rotation;

        if (Orientation == TrayOrientation.Left)
        {
            transform.localRotation = Quaternion.Euler(-90f, 90f, -90f);
        }
        else if (Orientation == TrayOrientation.Right)
        {
            transform.localRotation = Quaternion.Euler(-90f, -90f, -90f);
        }
        else if (Orientation == TrayOrientation.Up)
        {
            transform.localRotation = Quaternion.Euler(-90f, 0f, 90f);
        }
        else if (Orientation == TrayOrientation.Down)
        {
            transform.localRotation = Quaternion.Euler(-90f, 0f, -90f);
        }

        TraysLeftToSpawnLabelRect.rotation = originalChildRot;
    }

    [ContextMenu("Switch")]
    private void SwitchTrays(float WaitBeforeSwitch = 0f)
    {
        if (FrontTrayTransform == null || BackTrayTransform == null) return;

        Vector3 frontPos = FrontTrayTransform.position;
        Vector3 frontScale = FrontTrayTransform.localScale;
        Transform temp = FrontTrayTransform;
        float AnimationDuration = Manager.Instance.GameManagement.SwapAnimationDuration;

        _FrontTrayTransform = BackTrayTransform;
        BackTrayTransform = temp;

        Debug.Log("Wait before switch is " + WaitBeforeSwitch);
        Sequence seq = DOTween.Sequence();
        seq
        .Join(_FrontTrayTransform.DOMove(frontPos, AnimationDuration))
        .Join(_FrontTrayTransform.DOScale(frontScale, AnimationDuration))
        .Join(BackTrayTransform.DOMove(BackSpawnPoint.position, AnimationDuration))
        .Join(BackTrayTransform.DOScale(Manager.Instance.GameManagement.ScaleDownMultiplier * BackTrayOriginalScale, AnimationDuration))
        .OnComplete(() =>
        {
            _FrontTrayTransform.position = frontPos;
            _FrontTrayTransform.localScale = frontScale;
            BackTrayTransform.position = BackSpawnPoint.position;
            BackTrayTransform.localScale = Manager.Instance.GameManagement.ScaleDownMultiplier * BackTrayOriginalScale;
            _IsSwitchingTrays = false;
            Manager.Instance.GameManagement.PlayConveyer();
            Manager.Instance.InputAllowedOnCups = true;
        });

        seq.SetDelay(WaitBeforeSwitch).Play();
        _IsSwitchingTrays = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TrayDispenser[] Dispensers = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.parent.GetComponentsInChildren<TrayDispenser>(false);

        if (SwitchModeEnabled)
        {
            transform.DOScale(transform.localScale * 0.9f, Manager.Instance.GameManagement.SwapAnimationDuration).SetEase(Ease.InOutSine).SetLink(gameObject).SetLoops(2, LoopType.Yoyo);
            Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(0, 0.15f);
            SwitchTrays(0.35f);

            foreach (var Dispenser in Dispensers)
            {
                Dispenser.SwitchModeEnabled = false;
                Utilities.AssignLayerRecursively(Dispenser.transform, 0);//Back to the default layer

                if (Dispenser.FrontTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.FrontTrayTransform, 0);
                if (Dispenser.BackTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.BackTrayTransform, 0);
            }
        }
        else if (InfoModeEnabled)
        {
            Manager.Instance.UIManagement.ShowDispenserInformation(TraysToSpawn);

            foreach (var Dispenser in Dispensers)
            {
                Dispenser.InfoModeEnabled = false;
                Utilities.AssignLayerRecursively(Dispenser.transform, 0);//Back to the default layer

                if (Dispenser.FrontTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.FrontTrayTransform, 0);
                if (Dispenser.BackTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.BackTrayTransform, 0);
            }
        }

        Manager.Instance.PowerupManagement.EnableAllPowerupInputs();
        Manager.Instance.TutorialManagement.DestroyFingerAt();
        Manager.Instance.UIManagement.ClosePowerupTip();
        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(0, 0.15f);
        Manager.Instance.GameManagement.ClearPointerArrows();
        Manager.Instance.PowerupManagement.CurrentPowerup = PowerupManager.Powerup.None;
    }

    private async Task<Transform> CreateTray(bool AnimateToFront = false)
    {
        if (CurrentTrayIndex >= TraysToSpawn.Length) return null;

        Tray trayData = TraysToSpawn[CurrentTrayIndex++];
        GameObject TrayToSpawn = Resources.Load<GameObject>("Trays/" + trayData.Type.ToString());

        string BodyMaterialPath = "Trays/Materials/" + trayData.Material.ToString().Replace("Designer","") + "_Body";
        string SeatMaterialPath = "Trays/Materials/" + trayData.Material.ToString().Replace("Designer","") + "_Seat";

        Material MaterialToSetBody = Resources.Load<Material>(BodyMaterialPath);
        Material MaterialToSetSeat = Resources.Load<Material>(SeatMaterialPath);
        

        if (TrayToSpawn == null) return null;
        
        TrayObj CurrentTray = TrayToSpawn.GetComponent<TrayObj>();
        
        foreach(var Renderer in CurrentTray.Renderers)
        {
            int MaterialsCount = Renderer.sharedMaterials.Length;
            Material[] NewMats = new Material[MaterialsCount];

            for (int i = 0; i < MaterialsCount; i++)
            {
                if (i == 0)
                    NewMats[i] = MaterialToSetBody;
                else if (i == 1)
                    NewMats[i] = Manager.Instance.ReferenceManagement.WindowMaterial;
                else if(i == 2)
                    NewMats[i] = MaterialToSetSeat;
            }
                

            Renderer.sharedMaterials = NewMats;
        }

        GameObject TrayClone = Instantiate(TrayToSpawn, BackSpawnPoint.position, transform.rotation * Quaternion.Euler(90f,0f,0f));
        TrayObj TrayObjRef = TrayClone.GetComponentInChildren<TrayObj>();
        TrayObjRef.SetData(this, trayData.IsMysterious, MaterialToSetBody, trayData.IsStarredTray);
        TrayClone.transform.localScale *= ScaleDownMultiplier;
        Transform trayTransform = TrayClone.transform;

        if (AnimateToFront)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(trayTransform.DOMove(FrontSpawnPoint.position, 0.45f).SetEase(Ease.OutBack));
            sequence.Join(trayTransform.DOScale(TrayToSpawn.transform.localScale, 0.45f).SetEase(Ease.OutBack));
            if (trayTransform.TryGetComponent(out TrayObj tray)) tray.RevealMysteriousTray();
            await sequence.AsyncWaitForCompletion();
        }
        else
        {
            BackTrayOriginalScale = TrayToSpawn.transform.localScale;
        }
        Debug.Log(trayData.Material);
        RequiredCups.Add(trayTransform, GetCupTypeByTrayMaterial(trayData.Material, trayData.IsStarredTray));
        if (TraysToSpawn.Length - CurrentTrayIndex == 0)
        {
            TraysLeftToSpawnLabel.text = "<sprite name=check>";
        }
        else
        {
            TraysLeftToSpawnLabel.text = (TraysToSpawn.Length - CurrentTrayIndex).ToString();
        }
        return trayTransform;
    }

    private CupColors GetCupTypeByTrayMaterial(TrayMaterial material, bool isStarredTray)
    {
        return material switch
        {
            TrayMaterial.Blue => CupColors.Blue,
            TrayMaterial.Brown => CupColors.Brown,
            TrayMaterial.Cyan => CupColors.Cyan,
            TrayMaterial.Green => CupColors.Green,
            TrayMaterial.Orange => CupColors.Orange,
            TrayMaterial.Pink => CupColors.Pink,
            TrayMaterial.Purple => CupColors.Purple,
            TrayMaterial.Red => CupColors.Red,
            TrayMaterial.Yellow => CupColors.Yellow,
            TrayMaterial.Teal => CupColors.Teal,
            TrayMaterial.BlueDesigner => CupColors.BlueDesigner,
            TrayMaterial.BrownDesigner => CupColors.BrownDesigner,
            TrayMaterial.CyanDesigner => CupColors.CyanDesigner,
            TrayMaterial.GreenDesigner => CupColors.GreenDesigner,
            TrayMaterial.OrangeDesigner => CupColors.OrangeDesigner,
            TrayMaterial.PinkDesigner => CupColors.PinkDesigner,
            TrayMaterial.PurpleDesigner => CupColors.PurpleDesigner,
            TrayMaterial.RedDesigner => CupColors.RedDesigner,
            TrayMaterial.YellowDesigner => CupColors.YellowDesigner,
            TrayMaterial.TealDesigner => CupColors.Teal,
            _ => CupColors.Blue
        };
    }

    private async Task HandleNextTray()
    {
        if (_FrontTrayTransform != null)
        {
            TrayObj TrayObjRef = _FrontTrayTransform.GetComponentInChildren<TrayObj>();
            if (TrayObjRef != null)
            {
                TrayObjRef.FlyAway();
            }
            _FrontTrayTransform = null;
        }

        if (BackTrayTransform == null) return;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(BackTrayTransform.DOMove(FrontSpawnPoint.position, 0.45f).SetEase(Ease.OutBack));
        sequence.Join(DOVirtual.DelayedCall(0.2f, () =>
        {
            BackTrayTransform.GetComponentInChildren<TrayObj>().RevealMysteriousTray();
        }));
        sequence.Join(BackTrayTransform.DOScale(BackTrayOriginalScale, 0.45f).SetEase(Ease.OutBack));
        await sequence.AsyncWaitForCompletion();

        _FrontTrayTransform = BackTrayTransform;
        BackTrayTransform = await CreateTray();
    }

    public Vector3? GetPositionToLandCup()
    {
        if (_FrontTrayTransform != null && _FrontTrayTransform.TryGetComponent(out TrayObj tray))
        {
            if (IsSwitchingTrays)
                return null;
            Vector3? position = tray.GetPositionToLandCup();
            if (position.HasValue)
                return position.Value;
        }

        return null;
    }

    public void UpdateCurrentFrontTrayPositionIndex()
    {
        if (_FrontTrayTransform != null && _FrontTrayTransform.TryGetComponent(out TrayObj tray))
            tray.UpdatePositionIndex();
    }

    public void Jiggle()
    {
        if (JiggleSequence != null && JiggleSequence.IsActive() && JiggleSequence.IsPlaying()) return;

        Vector3 originalScale = FrontTrayTransform.localScale;
        Vector3 reducedScale = originalScale * Manager.Instance.GameManagement.TrayJiggleScale;

        JiggleSequence = DOTween.Sequence()
        .Append(FrontTrayTransform.DOScale(reducedScale, 0.1f).SetEase(Ease.OutQuad))
        .Append(FrontTrayTransform.DOScale(originalScale, 0.1f).SetEase(Ease.OutElastic));
    }

    [ContextMenu("Next Tray")]
    public void NextTray()
    {
        _ = HandleNextTray();
    }

    [ContextMenu("Set Random Data")]
    public void SetRandomData()
    {
        int count = UnityEngine.Random.Range(1, 5);
        for (int i = 1; i <= count; i++)
        {
            Tray tray = new()
            {
                Type = (TrayType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(TrayType)).Length),
                Material = (TrayMaterial)UnityEngine.Random.Range(0, Enum.GetValues(typeof(TrayMaterial)).Length)
            };
            Array.Resize(ref TraysToSpawn, TraysToSpawn.Length + 1);
            TraysToSpawn[TraysToSpawn.Length - 1] = tray;
        }
    }

#if UNITY_EDITOR
    public void UPDATE_DISPENSER_EDITOR(TrayDispenserData trayDispenserData)
    {
        SetupRotationByOrientation(trayDispenserData.Orientation);
        Vector2 Pos2D = trayDispenserData.Position;
        transform.localPosition = new Vector3(Pos2D.x, transform.position.y, Pos2D.y);
    }
#endif
}
