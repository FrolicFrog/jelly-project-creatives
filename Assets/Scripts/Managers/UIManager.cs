using System;
using System.Collections;
using System.Collections.Generic;
using BlockStackTypes;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [Header("REFERENCES")]
    public GameObject CoinUIPrefab;
    public RectTransform CoinSpawnPoint;

    [Header("SETTINGS")]
    public float FadeInDuration = 0.5f;
    public float ScaleUpDuration = 0.5f;
    public float DelayInSubLabelAnim = 0.15f;
    public float DelayInCoinSpawn = 1f;
    public float YOffset = 100f;
    public float FailedYOffset = 100f;
    public float YRiseAnimationDuration = 0.5f;
    public float CoinSpawnSpace = 100f;
    public float CoinAnimateToLabelDuration = 0.8f;
    public float CoinNumScaleUp = 1.2f;
    public float CoinNumScaleUpDuration = 0.1f;
    public float YOffsetHardLevelMessage = 400f;

    [Header("Main UI")]
    public TextMeshProUGUI LevelNumLabel;
    public TextMeshProUGUI CoinNumLabel;
    public Image LevelProgressBar;
    public Button SettingsBtn;

    [Header("Level Complete UI")]
    public GameObject LevelCompleteUI;
    public RectTransform VictoryTitle;
    public RectTransform LevelNumSubLabel;
    public TextMeshProUGUI LevelNumSubLabelTxt => LevelNumSubLabel.GetComponent<TextMeshProUGUI>();
    public RectTransform PrizeHolder;
    public Image VictoryBackgroundImage;
    public Image PrizeImage;
    public Image PrizeFill;
    public Button VictoryContinueBtn;
    public TextMeshProUGUI PrizeImageLabel;

    [Header("Level Failed UI")]
    public GameObject LevelFailedUI;
    public Image FailedBackgroundImage;
    public RectTransform FailedTitle;
    public Button FailedContinueBtn;
    // public Button FailedReviveBtn;

    [Header("Tutorial UI")]
    public GameObject TutorialUI;
    public Image TutorialBackgroundImage;
    public RectTransform TutorialMessage;
    public TextMeshProUGUI MessageLabel;
    public Button TutorialContinueBtn;

    [Header("Hard Level Message UI")]
    public GameObject HardLevelMessageUI;
    public Image HardLevelMessageBackgroundImage;
    public RectTransform HardLevelMessage;

    [Header("Finger Animation UI")]
    public GameObject FingerAnimationBackdrop;

    [Header("Settings UI")]
    public GameObject DevTestTimesUI;
    public RectTransform DevTestTimesContent;
    public Button CloseDevTestTimesBtn;
    public TextMeshProUGUI TotalTimeTxt;
    public Button MusicToggleBtn;
    public Button SoundToggleBtn;
    public Button VibrationToggleBtn;

    [Header("Insufficient Space UI")]
    public GameObject InsufficientSpaceUI;
    public RectTransform InsufficientSpaceMessage;
    public TextMeshProUGUI InsufficientSpaceLabel;

    [Header("Powerups UI")]
    public Button SwapPowerupBtn;
    public GameObject SwapPowerupAdIcon;
    public GameObject SwapPowerupLockedIcon;
    public GameObject SwapPowerupLabel;
    public TextMeshProUGUI SwapPowerupLockedTxt;

    [Space(10)]
    public Button InfoPowerupBtn;
    public GameObject InfoPowerupAdIcon;
    public GameObject InfoPowerupLockedIcon;
    public GameObject InfoPowerupLabel;
    public GameObject InfoPowerupDialogUI;
    public RectTransform InfoPowerupDialog;
    public TextMeshProUGUI InfoPowerupLockedTxt;

    [Space(10)]
    public Button ShufflePowerupBtn;
    public GameObject ShufflePowerupAdIcon;
    public GameObject ShufflePowerupLockedIcon;
    public GameObject ShufflePowerupLabel;
    public TextMeshProUGUI ShufflePowerupLockedTxt;

    [Space(10)]
    public GameObject PowerupTipUI;
    public TextMeshProUGUI PowerupTipTxt;
    public Transform PowerupTipMessage;
    public Button PowerupTipCloseBtn;

    [Space(10)]
    [Range(0f, 10f)]
    public float DurationBtwAdIconShake = 5f;
    private Tween TapToSelectGrowShrinkTween;
    private List<RectTransform> ValidPowerups;


    [Header("PURCHASE POWERUPS")]
    public GameObject PurchasePowerupsUI;
    public Image PurchasePowerupsBackdrop;
    public Button PurchasePowerupBackdropButton;
    public Image PurchasePowerupsImage;
    public Button PurchasePowerupsButton;
    public Button PurchasePowerupCloseButton;
    public Sprite SwapPurchasePowerupUISprite;
    public Sprite InfoPurchasePowerupUISprite;
    public Sprite ShufflePurchasePowerupUISprite;

    [Header("Powerup Cost Capsule Animation")]
    public float ExpandedWidth = 155f;
    public float NormalWidth = 55f;
    public float NormalY = 31.1f;
    public float ExpandedY = 37.9f;
    public float NormalX = 0f;
    public float ExpandedX = 0f;


    void Awake()
    {
        InitialSetup();
        UpdateCoins();
        LevelNumSubLabelTxt.text = "LEVEL " + Manager.Instance.LevelManagement.CurrentLevel.ToString();
        LevelProgressBar.fillAmount = 0f;
        ShakeAdIcons();
    }

    private void ShakeAdIcons()
    {
        StartCoroutine(ShakeAdIconRoutine(SwapPowerupAdIcon.transform.parent, SwapPowerupAdIcon.transform, 2f, 3f));
        StartCoroutine(ShakeAdIconRoutine(InfoPowerupAdIcon.transform.parent, InfoPowerupAdIcon.transform, 2f, 3f));
        StartCoroutine(ShakeAdIconRoutine(ShufflePowerupAdIcon.transform.parent, ShufflePowerupAdIcon.transform, 2f, 3f));
    }

    IEnumerator ShakeAdIconRoutine(Transform Target, Transform Original, float duration, float interval)
    {
        WaitForSeconds wait = new WaitForSeconds(interval);
        Sequence seq = null;

        while (true)
        {
            if (Original.gameObject.activeSelf)
            {
                if (seq == null || !seq.IsActive() || !seq.IsPlaying())
                {
                    seq = DOTween.Sequence();
                    seq
                    .Append(Target.DOScale(1.2f, 0.25f))
                    .Append(Target.DOScale(1f, 0.25f))
                    .SetLoops(Mathf.FloorToInt(duration / 0.5f), LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetLink(Target.gameObject);
                }
            }
            else if (seq != null && seq.IsActive())
            {
                seq.Kill();
                seq = null;
            }

            yield return wait;
        }
    }

    private void InitialSetup()
    {
        // LionAds.OnRewardedStatusChanged += UpdateAdIconStatus;

        //Main UI
        SettingsBtn.onClick.AddListener(() => DevTestTimesUI.SetActive(true));

        //Level Complete Screen UI
        Color c = VictoryBackgroundImage.color;
        VictoryBackgroundImage.color = new Color(c.r, c.g, c.b, 0f);
        VictoryTitle.localScale = Vector3.zero;
        LevelNumSubLabel.localScale = Vector3.zero;
        PrizeHolder.transform.localScale = Vector3.zero;
        VictoryContinueBtn.transform.localScale = Vector3.zero;
        LevelCompleteUI.SetActive(false);

        //Level Failed Screen UI
        LevelFailedScreenInitialSetup();

        //Tutorial UI


        //Main UI
        LevelNumLabel.text = "LEVEL " + Manager.Instance.LevelManagement.CurrentLevel.ToString();

        //Settings UI
        DevTestTimesUI.SetActive(false);
        CloseDevTestTimesBtn.onClick.AddListener(() => DevTestTimesUI.SetActive(false));
        MusicToggleBtn.onClick.AddListener(() =>
        {
            bool IsMusicMuted = Manager.Instance.AudioManagement.ToggleMusic();

            MusicToggleBtn.GetComponent<Image>().color = IsMusicMuted ? Color.white : TryParseColor("#3D51BE");
            MusicToggleBtn.transform.GetChild(1).GetComponent<Image>().sprite = IsMusicMuted ? Manager.Instance.ReferenceManagement.MusicOff : Manager.Instance.ReferenceManagement.MusicOn;
            MusicToggleBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = IsMusicMuted ? TryParseColor("#3D51BE") : Color.white;
            MusicToggleBtn.transform.GetChild(1).GetComponent<Image>().color = IsMusicMuted ? TryParseColor("#3D51BE") : Color.white;
        });

        SoundToggleBtn.onClick.AddListener(() =>
        {
            bool IsSFXMuted = Manager.Instance.AudioManagement.ToggleSFX();

            SoundToggleBtn.GetComponent<Image>().color = IsSFXMuted ? Color.white : TryParseColor("#3D51BE");
            SoundToggleBtn.transform.GetChild(1).GetComponent<Image>().sprite = IsSFXMuted ? Manager.Instance.ReferenceManagement.AudioOff : Manager.Instance.ReferenceManagement.AudioOn;
            SoundToggleBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = IsSFXMuted ? TryParseColor("#3D51BE") : Color.white;
            SoundToggleBtn.transform.GetChild(1).GetComponent<Image>().color = IsSFXMuted ? TryParseColor("#3D51BE") : Color.white;
        });

        VibrationToggleBtn.onClick.AddListener(() =>
        {
            bool IsHapticsMuted = Manager.Instance.HapticsManagement.ToggleHaptics();

            VibrationToggleBtn.GetComponent<Image>().color = IsHapticsMuted ? Color.white : TryParseColor("#3D51BE");
            VibrationToggleBtn.transform.GetChild(1).GetComponent<Image>().sprite = IsHapticsMuted ? Manager.Instance.ReferenceManagement.VibrationOff : Manager.Instance.ReferenceManagement.VibrationOn;
            VibrationToggleBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = IsHapticsMuted ? TryParseColor("#3D51BE") : Color.white;
            VibrationToggleBtn.transform.GetChild(1).GetComponent<Image>().color = IsHapticsMuted ? TryParseColor("#3D51BE") : Color.white;
        });

        bool IsMusicMuted = Manager.Instance.AudioManagement.BGMMuted;
        bool IsSFXMuted = Manager.Instance.AudioManagement.SFXMuted;
        bool IsHapticsMuted = Manager.Instance.HapticsManagement.HapticsMuted;

        MusicToggleBtn.GetComponent<Image>().color = IsMusicMuted ? Color.white : TryParseColor("#3D51BE");
        MusicToggleBtn.transform.GetChild(1).GetComponent<Image>().sprite = IsMusicMuted ? Manager.Instance.ReferenceManagement.MusicOff : Manager.Instance.ReferenceManagement.MusicOn;
        MusicToggleBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = IsMusicMuted ? TryParseColor("#3D51BE") : Color.white;
        MusicToggleBtn.transform.GetChild(1).GetComponent<Image>().color = IsMusicMuted ? TryParseColor("#3D51BE") : Color.white;

        SoundToggleBtn.GetComponent<Image>().color = IsSFXMuted ? Color.white : TryParseColor("#3D51BE");
        SoundToggleBtn.transform.GetChild(1).GetComponent<Image>().sprite = IsSFXMuted ? Manager.Instance.ReferenceManagement.AudioOff : Manager.Instance.ReferenceManagement.AudioOn;
        SoundToggleBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = IsSFXMuted ? TryParseColor("#3D51BE") : Color.white;
        SoundToggleBtn.transform.GetChild(1).GetComponent<Image>().color = IsSFXMuted ? TryParseColor("#3D51BE") : Color.white;

        VibrationToggleBtn.GetComponent<Image>().color = IsHapticsMuted ? Color.white : TryParseColor("#3D51BE");
        VibrationToggleBtn.transform.GetChild(1).GetComponent<Image>().sprite = IsHapticsMuted ? Manager.Instance.ReferenceManagement.VibrationOff : Manager.Instance.ReferenceManagement.VibrationOn;
        VibrationToggleBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = IsHapticsMuted ? TryParseColor("#3D51BE") : Color.white;
        VibrationToggleBtn.transform.GetChild(1).GetComponent<Image>().color = IsHapticsMuted ? TryParseColor("#3D51BE") : Color.white;

        //Insufficient Space UI
        InsufficientSpaceUI.SetActive(false);
        InsufficientSpaceMessage.localScale = Vector3.zero;

        //Powerup UI
        PowerupTipUI.SetActive(false);
        PowerupTipMessage.localScale = Vector3.zero;
        PowerupTipCloseBtn.onClick.AddListener(ClosePowerupTip);

        //SWAP POWERUP
        SwapPowerupBtn.onClick.AddListener(() => Manager.Instance.PowerupManagement.UseSwapPowerup());
        SwapPowerupAdIcon.SetActive(false);
        SwapPowerupLabel.SetActive(false);

        //INFO POWERUP
        InfoPowerupBtn.onClick.AddListener(() => Manager.Instance.PowerupManagement.UseInfoPowerup());
        InfoPowerupAdIcon.SetActive(false);
        InfoPowerupLabel.SetActive(false);
        InfoPowerupDialogUI.GetComponent<Button>().onClick.AddListener(() => EndInfoPowerup());

        //SHUFFLE POWERUP
        ShufflePowerupBtn.onClick.AddListener(() => Manager.Instance.PowerupManagement.UseShufflePowerup());
        ShufflePowerupAdIcon.SetActive(false);
        ShufflePowerupLabel.SetActive(false);

        //PURCHASE POWERUPS UI
        PurchasePowerupsUI.SetActive(false);
        PurchasePowerupsBackdrop.color = new Color(PurchasePowerupsBackdrop.color.r, PurchasePowerupsBackdrop.color.g, PurchasePowerupsBackdrop.color.b, 0f);
        PurchasePowerupsImage.GetComponent<RectTransform>().localScale = Vector3.zero;
        PurchasePowerupBackdropButton.onClick.AddListener(() => ClosePowerupsPurchaseDialog());
        PurchasePowerupCloseButton.onClick.AddListener(() => ClosePowerupsPurchaseDialog());
    }

    public void LevelFailedScreenInitialSetup()
    {
        Manager.Instance.EffectsManagement.StopEffect("sad");
        FailedTitle.localScale = Vector3.zero;
        FailedContinueBtn.transform.localScale = Vector3.zero;
        // FailedReviveBtn.transform.localScale = Vector3.zero;
        Color c2 = FailedBackgroundImage.color;
        FailedBackgroundImage.color = new Color(c2.r, c2.g, c2.b, 0f);
        LevelFailedUI.SetActive(false);
    }

    void Start()
    {
        UpdatePowerupLabels();

        if (SessionPrefs.Get("lost_last_level", false))
        {
            HighlightRandomPowerup();
            SessionPrefs.DeleteKey("lost_last_level");
        }
    }

    private void HighlightRandomPowerup()
    {
        PowerupManager _PowerupManager = Manager.Instance.PowerupManagement;
        int CurrentLevel = Manager.Instance.LevelManagement.CurrentLevel;
        ValidPowerups = new();

        if (CurrentLevel >= _PowerupManager.UnlockSwapPowerupLvl)
            ValidPowerups.Add(SwapPowerupBtn.GetComponent<RectTransform>());
        if (CurrentLevel >= _PowerupManager.UnlockInfoPowerupLvl)
            ValidPowerups.Add(InfoPowerupBtn.GetComponent<RectTransform>());
        if (CurrentLevel >= _PowerupManager.UnlockShufflePowerupLvl)
            ValidPowerups.Add(ShufflePowerupBtn.GetComponent<RectTransform>());

        InvokeRepeating(nameof(AnimatePowerupButton), 0f, 10f);
    }

    private void AnimatePowerupButton()
    {
        if (ValidPowerups == null || ValidPowerups.Count == 0) return;
        RectTransform ToSelectMinimally = InfoPowerupBtn.GetComponent<RectTransform>();
        RectTransform Target = ValidPowerups.GetRandomWithBias(ToSelectMinimally, 0.33f);

        Target.DOScale(1.2f, 0.25f)
        .SetEase(Ease.InOutSine)
        .SetLink(Target.gameObject)
        .SetLoops(6, LoopType.Yoyo);
    }

    public void UpdateCoinsLabels()
    {
        CoinNumLabel.text = PlayerPrefs.GetInt("coins", 0).ToString();
    }

    public void UpdatePowerupLabels()
    {
        RectTransform CurrentTarget = null;

        bool IsSwapPowerupAvailable = Manager.Instance.PowerupManagement.IsSwapPowerupAvailable;
        bool IsInfoPowerupAvailable = Manager.Instance.PowerupManagement.IsInfoPowerupAvailable;
        bool IsShufflePowerupAvailable = Manager.Instance.PowerupManagement.IsShufflePowerupAvailable;

        bool IsSwapPowerupUnlocked = Manager.Instance.PowerupManagement.UnlockSwapPowerupLvl <= Manager.Instance.LevelManagement.CurrentLevel;
        bool IsInfoPowerupUnlocked = Manager.Instance.PowerupManagement.UnlockInfoPowerupLvl <= Manager.Instance.LevelManagement.CurrentLevel;
        bool IsShufflePowerupUnlocked = Manager.Instance.PowerupManagement.UnlockShufflePowerupLvl <= Manager.Instance.LevelManagement.CurrentLevel;

        SwapPowerupAdIcon.SetActive(!IsSwapPowerupAvailable && IsSwapPowerupUnlocked);
        CurrentTarget = SwapPowerupAdIcon.transform.parent as RectTransform;

        if (!IsSwapPowerupAvailable && IsSwapPowerupUnlocked)
        {
            ExpandCapsulePowerup(CurrentTarget);
        }
        else
        {
            ContractCapsulePowerup(CurrentTarget);
        }

        SwapPowerupLabel.SetActive(IsSwapPowerupAvailable && IsSwapPowerupUnlocked);
        SwapPowerupLabel.GetComponent<TextMeshProUGUI>().text = Manager.Instance.PowerupManagement.AvailableSwaps.ToString();
        SwapPowerupLockedIcon.SetActive(!IsSwapPowerupUnlocked);
        SwapPowerupBtn.interactable = IsSwapPowerupUnlocked;
        SwapPowerupAdIcon.transform.parent.gameObject.SetActive(IsSwapPowerupUnlocked);
        SwapPowerupBtn.GetComponent<Shadow>().enabled = IsSwapPowerupUnlocked;
        SwapPowerupLockedTxt.text = $"LVL {Manager.Instance.PowerupManagement.UnlockSwapPowerupLvl}";
        SwapPowerupLockedTxt.gameObject.SetActive(!IsSwapPowerupUnlocked);

        InfoPowerupAdIcon.SetActive(!IsInfoPowerupAvailable && IsInfoPowerupUnlocked);
        CurrentTarget = InfoPowerupAdIcon.transform.parent as RectTransform;

        if (!IsInfoPowerupAvailable && IsInfoPowerupUnlocked)
        {
            ExpandCapsulePowerup(CurrentTarget);
        }
        else
        {
            ContractCapsulePowerup(CurrentTarget);
        }

        InfoPowerupLabel.SetActive(IsInfoPowerupAvailable && IsInfoPowerupUnlocked);
        InfoPowerupLabel.GetComponent<TextMeshProUGUI>().text = Manager.Instance.PowerupManagement.AvailableInfo.ToString();
        InfoPowerupLockedIcon.SetActive(!IsInfoPowerupUnlocked);
        InfoPowerupBtn.interactable = IsInfoPowerupUnlocked;
        InfoPowerupAdIcon.transform.parent.gameObject.SetActive(IsInfoPowerupUnlocked);
        InfoPowerupBtn.GetComponent<Shadow>().enabled = IsInfoPowerupUnlocked;
        InfoPowerupLockedTxt.text = $"LVL {Manager.Instance.PowerupManagement.UnlockInfoPowerupLvl}";
        InfoPowerupLockedTxt.gameObject.SetActive(!IsInfoPowerupUnlocked);

        ShufflePowerupAdIcon.SetActive(!IsShufflePowerupAvailable && IsShufflePowerupUnlocked);
        CurrentTarget = ShufflePowerupAdIcon.transform.parent as RectTransform;

        if (!IsShufflePowerupAvailable && IsShufflePowerupUnlocked)
        {
            ExpandCapsulePowerup(CurrentTarget);
        }
        else
        {
            ContractCapsulePowerup(CurrentTarget);
        }

        ShufflePowerupLabel.SetActive(IsShufflePowerupAvailable && IsShufflePowerupUnlocked);
        ShufflePowerupLabel.GetComponent<TextMeshProUGUI>().text = Manager.Instance.PowerupManagement.AvailableShuffle.ToString();
        ShufflePowerupLockedIcon.SetActive(!IsShufflePowerupUnlocked);
        ShufflePowerupBtn.interactable = IsShufflePowerupUnlocked;
        ShufflePowerupAdIcon.transform.parent.gameObject.SetActive(IsShufflePowerupUnlocked);
        ShufflePowerupBtn.GetComponent<Shadow>().enabled = IsShufflePowerupUnlocked;
        ShufflePowerupLockedTxt.text = $"LVL {Manager.Instance.PowerupManagement.UnlockShufflePowerupLvl}";
        ShufflePowerupLockedTxt.gameObject.SetActive(!IsShufflePowerupUnlocked);
    }

    private void ExpandCapsulePowerup(RectTransform target)
    {
        if (target == null) return;

        target.DOSizeDelta(new Vector2(ExpandedWidth, target.sizeDelta.y), 0.4f).SetEase(Ease.OutBack);
        target.DOAnchorPosY(ExpandedY, 0.4f).SetEase(Ease.OutBack);
        target.DOAnchorPosX(ExpandedX, 0.4f).SetEase(Ease.OutBack);
    }

    private void ContractCapsulePowerup(RectTransform target)
    {
        if (target == null) return;

        target.DOSizeDelta(new Vector2(NormalWidth, target.sizeDelta.y), 0.4f).SetEase(Ease.OutBack);
        target.DOAnchorPosY(NormalY, 0.4f).SetEase(Ease.OutBack);
        target.DOAnchorPosX(NormalX, 0.4f).SetEase(Ease.OutBack);
    }

    private Color TryParseColor(string HexCode)
    {
        if (ColorUtility.TryParseHtmlString(HexCode, out Color color))
        {
            return color;
        }
        else
        {
            Debug.LogError("Invalid HTML color code: " + HexCode);
            return Color.white;
        }
    }

    private void UpdateCoins()
    {
        int Coins = PlayerPrefs.GetInt("coins", 0);
        CoinNumLabel.text = Coins.ToString();
    }

    public void VictorySequence()
    {
        (Sprite Graphic, int PreviousProgress, int Progress, int LevelsToPlayToUnlock) = Manager.Instance.PrizeManagement.GetCurrentPrize();

        if (Graphic != null && PreviousProgress != -1 && Progress != -1)
        {
            if (Progress < PreviousProgress)
                PreviousProgress = Progress;

            float PreviousProgressPercent = PreviousProgress / LevelsToPlayToUnlock * 100f;
            PrizeFill.fillAmount = PreviousProgressPercent / 100f;
            PrizeImage.sprite = Graphic;
            PrizeImage.SetNativeSize();
            PrizeImageLabel.text = "";
        }

        VictoryContinueBtn.onClick.AddListener(() =>
        {
            Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton", OnComplete: () => Manager.Instance.LevelManagement.LoadNextLevel());
            VictoryContinueBtn.interactable = false;
        });

        Manager.Instance.IsLevelCompleted = true;
        if (LevelNumLabel.TryGetComponent(out TextMeshProUGUI levelNumLabel))
            levelNumLabel.text = "LEVEL " + Manager.Instance.LevelManagement.CurrentLevel.ToString();

        LevelCompleteUI.SetActive(true);
        Sequence sequence = DOTween.Sequence();

        float Duration = Manager.Instance.EffectsManagement.GetDuration("confetti");
        sequence.AppendCallback(() => Manager.Instance.EffectsManagement.PlayEffect("firework1"));
        sequence.AppendCallback(() => Manager.Instance.EffectsManagement.PlayEffect("firework2"));
        sequence.AppendCallback(() => Manager.Instance.EffectsManagement.PlayEffect("confetti"));
        sequence.AppendInterval(Duration / 2f);

        sequence.Append(VictoryBackgroundImage.DOFade(1f, FadeInDuration).SetEase(Ease.OutSine));
        sequence.Join(VictoryTitle.DOScale(Vector3.one, ScaleUpDuration).SetEase(Ease.OutBack));
        sequence.AppendCallback(() => Manager.Instance.EffectsManagement.PlayEffect("firework3"));
        sequence.AppendCallback(() => Manager.Instance.EffectsManagement.PlayEffect("firework4"));
        sequence.SetDelay(DelayInSubLabelAnim);

        sequence.Append(LevelNumSubLabel.transform.DOScale(Vector3.one, ScaleUpDuration).SetEase(Ease.OutBack));

        sequence.SetDelay(DelayInCoinSpawn);
        sequence.Append(VictoryTitle.DOAnchorPosY(VictoryTitle.anchoredPosition.y - YOffset, YRiseAnimationDuration).SetEase(Ease.OutBack));
        sequence.Join(LevelNumSubLabel.DOAnchorPosY(LevelNumSubLabel.anchoredPosition.y - YOffset, YRiseAnimationDuration).SetEase(Ease.OutBack));
        sequence.Append(CoinSequence(10)).AppendCallback(() =>
        {
            if (Graphic != null)
            {
                sequence.Append(PrizeHolder.DOScale(Vector3.one, ScaleUpDuration));
                float ProgressPercent = (float)Progress / LevelsToPlayToUnlock * 100f;
                PrizeFill.DOFillAmount(ProgressPercent / 100f, 0.5f);
                PrizeImageLabel.text = $"{Progress}/{LevelsToPlayToUnlock}";
            }
        })
        .AppendInterval(1.35f)
        .Append(VictoryContinueBtn.transform.DOScale(Vector3.one, ScaleUpDuration).SetEase(Ease.InOutBack));
    }

    private Tween CoinSequence(int Count)
    {
        Sequence InitialSpawnSeq = DOTween.Sequence();
        RectTransform[] Coins = new RectTransform[Count];

        for (int i = 0; i < Count; i++)
        {
            RectTransform CoinClone = Instantiate(CoinUIPrefab, LevelCompleteUI.transform).GetComponent<RectTransform>();
            CoinClone.anchoredPosition = CoinSpawnPoint.anchoredPosition + UnityEngine.Random.insideUnitCircle * CoinSpawnSpace;
            Vector3 RandomScale = UnityEngine.Random.Range(0.6f, 1f) * Vector3.one;
            Coins[i] = CoinClone;
            InitialSpawnSeq.SetDelay(DelayInCoinSpawn).Join(CoinClone.DOScale(RandomScale, 0.2f).SetEase(Ease.InOutBack));
        }

        InitialSpawnSeq.AppendCallback(() =>
        {
            Sequence seq = DOTween.Sequence();
            Manager.Instance.AudioManagement.PlayAudioEffect("CoinCollection");

            for (int i = 0; i < Coins.Length; i++)
            {
                RectTransform Coin = Coins[i];
                Vector3 worldTargetPos = CoinNumLabel.GetComponent<RectTransform>().position;
                Vector2 TargetPos = Coin.parent.InverseTransformPoint(worldTargetPos);
                seq.Insert(i * 0.1f,
                    Coin.DOAnchorPos(TargetPos, CoinAnimateToLabelDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLink(Coin.gameObject)
                    .OnComplete(() =>
                    {
                        int Coins = PlayerPrefs.GetInt("coins", 0) + 10;
                        PlayerPrefs.SetInt("coins", Coins);
                        UpdateCoins();
                        Manager.Instance.HapticsManagement.PlayHaptic(Manager.Instance.HapticsManagement.CoinCollection);

                        if (!DOTween.IsTweening(CoinNumLabel.transform.parent))
                        {
                            CoinNumLabel.transform.parent.DOScale(CoinNumScaleUp, CoinNumScaleUpDuration).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo);
                            Manager.Instance.EffectsManagement.PlayEffect("star", false);
                        }

                        Destroy(Coin.gameObject);
                    })
                );

                seq.Join(Coin.DOScale(Vector3.one * 1.5f, CoinAnimateToLabelDuration * 0.5f));
            }

            seq.Play();
        });

        return InitialSpawnSeq;
    }

    public void FailedSequence()
    {
        if (Manager.Instance.IsLevelCompleted) return;

        FailedContinueBtn.onClick.AddListener(() =>
        {
            Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton", OnComplete: () => Manager.Instance.LevelManagement.RestartLevel());
            FailedContinueBtn.interactable = false;
        });
        // FailedReviveBtn.onClick.AddListener(() =>
        // {
        //     Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton", OnComplete: () => Manager.Instance.InitiateRevive());
        //     FailedReviveBtn.interactable = false;
        // });

        LevelFailedUI.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        FailedBackgroundImage.DOFade(1f, FadeInDuration).SetEase(Ease.OutSine);
        sequence.Append(FailedTitle.DOScale(Vector3.one, ScaleUpDuration).SetEase(Ease.OutBack));
        sequence.Append(FailedTitle.DOAnchorPosY(VictoryTitle.anchoredPosition.y - FailedYOffset, YRiseAnimationDuration).SetEase(Ease.OutBack));
        float Delay = Manager.Instance.EffectsManagement.GetDuration("sad");
        sequence.AppendCallback(() => Manager.Instance.EffectsManagement.PlayEffect("sad"));
        sequence.AppendInterval(Delay);
        sequence.Append(FailedContinueBtn.transform.DOScale(Vector3.one, ScaleUpDuration).SetEase(Ease.InOutBack));

        // if(!Manager.Instance.AlreadyRevived)
        // sequence.Join(FailedReviveBtn.transform.DOScale(Vector3.one, ScaleUpDuration).SetEase(Ease.InOutBack));
    }


    public void ShowTutorial(string Message)
    {
        if (Manager.Instance.IsLevelCompleted) return;

        TutorialUI.SetActive(false);
        Color c3 = TutorialBackgroundImage.color;
        TutorialBackgroundImage.color = new Color(c3.r, c3.g, c3.b, 0f);
        TutorialMessage.localScale = Vector3.zero;

        TutorialContinueBtn.onClick.AddListener(() =>
        {
            Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton", OnComplete: () => TutorialUI.SetActive(false));
            TutorialContinueBtn.interactable = false;
        });
        TutorialUI.SetActive(true);
        MessageLabel.text = Message;

        Sequence sequence = DOTween.Sequence();
        Color c = TutorialBackgroundImage.color;
        sequence.Append(TutorialBackgroundImage.DOFade(c3.a, FadeInDuration).SetEase(Ease.OutSine));
        sequence.Append(TutorialMessage.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack));
        sequence.Play();
    }

    public void ShowHardLevelMessage()
    {
        if (Manager.Instance.IsLevelCompleted) return;

        HardLevelMessageUI.SetActive(false);

        Color bgColor = HardLevelMessageBackgroundImage.color;
        HardLevelMessageBackgroundImage.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0f);
        HardLevelMessage.localScale = Vector3.zero;
        HardLevelMessage.anchoredPosition = new Vector2(HardLevelMessage.anchoredPosition.x, -Screen.height);
        HardLevelMessageUI.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.Join(HardLevelMessageBackgroundImage.DOFade(bgColor.a, FadeInDuration).SetEase(Ease.OutSine));
        seq.AppendInterval(FadeInDuration / 2f);
        seq.Append(
            DOTween.Sequence()
            .Join(HardLevelMessage.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutBack))
            .Join(HardLevelMessage.DOScale(Vector3.one * 2, 0.4f).SetEase(Ease.OutBack))
            .OnComplete(() =>
            {
                HardLevelMessage.DOAnchorPosY(HardLevelMessage.anchoredPosition.y + 20f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetId("Float");
            })
        );

        seq.AppendInterval(1f);
        seq.AppendCallback(() => DOTween.Kill("Float"));

        seq.Append(
            DOTween.Sequence()
            .Join(HardLevelMessage.DOAnchorPosY(Screen.height, 0.4f).SetEase(Ease.OutBack))
            .Join(HardLevelMessage.DOScale(Vector3.zero, 0.4f).SetEase(Ease.OutBack))
            .Join(HardLevelMessageBackgroundImage.DOFade(0f, 0.4f).SetEase(Ease.OutSine))
        );

        seq.OnComplete(() => HardLevelMessageUI.SetActive(false));
    }

    public void UpdateProgressBar(float levelProgressPercent)
    {
        if (DOTween.IsTweening(LevelProgressBar))
        {
            LevelProgressBar.DOKill();
        }

        float FillAmount = levelProgressPercent / 100f;
        LevelProgressBar.DOFillAmount(FillAmount, 0.15f);
    }

    public void SetFingerAnimationHolePosition(Material mat, Vector3 worldPos, Camera cam, RectTransform uiRect)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRect, screenPos, cam, out localPoint);

        Vector2 normalizedPos = new Vector2(
            (localPoint.x / uiRect.rect.width) + 0.5f,
            (localPoint.y / uiRect.rect.height) + 0.5f
        );

        mat.SetVector("_HoleCenter", new Vector4(normalizedPos.x, normalizedPos.y, 0, 0));
        FingerAnimationBackdrop.gameObject.SetActive(true);
    }

    public void UpdateDevLevelProgress(List<DevManager.TestTimes> allTestTimes)
    {
        DevCard Card = Manager.Instance.ReferenceManagement.DevTestTimesCardPrefab;
        float TotalTime = 0f;

        foreach (DevManager.TestTimes TestTimes in allTestTimes)
        {
            DevCard CardClone = Instantiate(Card, DevTestTimesContent);
            TotalTime += TestTimes.TimeInSeconds;
            CardClone.SetData(TestTimes);
        }

        TotalTimeTxt.text = $"{(int)(TotalTime / 60f)}:{(int)(TotalTime % 60f)}";
    }

    private int CurrentSpaceLeft = -1;
    public void ShowInsufficientUI(int SpaceLeft)
    {
        if (SpaceLeft == 0)
        {
            if (!Manager.Instance.IsLevelFailed)
                Manager.Instance.LevelFailed();
            return;
        }

        if (SpaceLeft > 6)
        {
            HideInsufficientUI();
            return;
        }

        InsufficientSpaceUI.SetActive(true);
        InsufficientSpaceMessage.DOScale(1, 0.1f).SetEase(Ease.OutBack).SetDelay(0.1f);

        if (CurrentSpaceLeft == SpaceLeft) return;

        InsufficientSpaceLabel.text = $"{Mathf.Max(0, SpaceLeft) + " Space" + (SpaceLeft <= 1 ? " " : "s ")}Left";
        Manager.Instance.AudioManagement.PlayAudioEffect("SpaceLeft");
        CurrentSpaceLeft = SpaceLeft;
    }

    public void ShowPowerupTip(string Message)
    {
        PowerupTipMessage.DOKill();
        PowerupTipUI.SetActive(true);
        PowerupTipTxt.text = Message;
        PowerupTipMessage.DOScale(1, 0.1f).SetEase(Ease.OutBack).SetDelay(0.1f);
    }

    public void ClosePowerupTip()
    {
        if (!PowerupTipMessage.gameObject.activeSelf) return;
        PowerupTipMessage.DOScale(0, 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            PowerupTipMessage.localScale = Vector3.zero;
            PowerupTipUI.SetActive(false);
        });
    }

    public void HideInsufficientUI()
    {
        InsufficientSpaceUI.SetActive(false);
        InsufficientSpaceMessage.localScale = Vector3.zero;
    }

    public void DisableFingerAnimationBackdrop()
    {
        TapToSelectGrowShrinkTween?.Kill();
        FingerAnimationBackdrop.GetComponentInChildren<TextMeshProUGUI>().transform.DOScale(1.3f, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetLink(FingerAnimationBackdrop);
        FingerAnimationBackdrop.SetActive(false);
    }
    public void EnableFingerAnimationBackdrop(string Message = "Tap To Play")
    {
        FingerAnimationBackdrop.GetComponentInChildren<TextMeshProUGUI>().text = Message;
        TapToSelectGrowShrinkTween ??= FingerAnimationBackdrop.GetComponentInChildren<TextMeshProUGUI>().transform.DOScale(1.3f, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetLink(FingerAnimationBackdrop);
        FingerAnimationBackdrop.SetActive(true);
    }

    public void ShowDispenserInformation(Tray[] traysToSpawn)
    {
        RectTransform DispenserUIPrefab = Manager.Instance.ReferenceManagement.DispenserInfoUIPrefab;
        foreach (Tray tray in traysToSpawn)
        {
            RectTransform UIPrefabClone = Instantiate(DispenserUIPrefab, InfoPowerupDialog);

            Image Img = UIPrefabClone.GetChild(0).GetComponent<Image>();

            Img.color = Manager.Instance.PowerupManagement.GetTrayColor(tray.Material);
            Img.transform.GetChild(0).gameObject.SetActive(tray.IsStarredTray);
        }

        InfoPowerupDialogUI.SetActive(true);
        InfoPowerupDialog.DOSizeDelta(new Vector2(160f, InfoPowerupDialog.sizeDelta.y), 0.15f).SetEase(Ease.InOutBounce);
    }
    private void EndInfoPowerup()
    {
        Manager.Instance.GameManagement.PlayConveyer();

        InfoPowerupDialog.DOSizeDelta(new Vector2(0f, InfoPowerupDialog.sizeDelta.y), 0.15f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            InfoPowerupDialogUI.SetActive(false);
            foreach (Transform child in InfoPowerupDialog)
                Destroy(child.gameObject);
        });

        Manager.Instance.InputAllowedOnCups = true;
        UpdatePowerupLabels();
        Manager.Instance.PowerupManagement.CurrentPowerup = PowerupManager.Powerup.None;
    }

    public void PurchasePowerup(PowerupManager.Powerup powerup)
    {
        if (powerup == PowerupManager.Powerup.None) return;
        Sprite PowerupSpriteUI = null;

        switch (powerup)
        {
            case PowerupManager.Powerup.Info:
                PowerupSpriteUI = InfoPurchasePowerupUISprite;
                break;
            case PowerupManager.Powerup.Shuffle:
                PowerupSpriteUI = ShufflePurchasePowerupUISprite;
                break;
            case PowerupManager.Powerup.Swap:
                PowerupSpriteUI = SwapPurchasePowerupUISprite;
                break;
        }

        PurchasePowerupsButton.onClick.RemoveAllListeners();
        if (Utilities.CanPurchasePowerup())
        {
            PurchasePowerupsButton.interactable = true;
            PurchasePowerupsButton.onClick.AddListener(() =>
            {
                Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton");
                int CurrentCoins = PlayerPrefs.GetInt("coins", 0);
                PlayerPrefs.SetInt("coins", Mathf.Max(CurrentCoins - 500, 0));
                Manager.Instance.UIManagement.UpdateCoinsLabels();
                Manager.Instance.PowerupManagement.IncrementPowerupCount(powerup);

                ClosePowerupsPurchaseDialog();
            });
        }
        else
        {
            PurchasePowerupsButton.interactable = false;
        }

        PurchasePowerupsImage.sprite = PowerupSpriteUI;
        PurchasePowerupsUI.SetActive(true);
        PurchasePowerupsBackdrop.DOColor(new Color(PurchasePowerupsBackdrop.color.r, PurchasePowerupsBackdrop.color.g, PurchasePowerupsBackdrop.color.b, 0.75f), 0.15f).OnComplete(() =>
        {
            PurchasePowerupsImage.transform.DOScale(1, 0.15f).SetEase(Ease.OutBack).SetDelay(0.1f);
        });

    }

    private void ClosePowerupsPurchaseDialog()
    {
        PurchasePowerupsBackdrop.DOColor(new Color(PurchasePowerupsBackdrop.color.r, PurchasePowerupsBackdrop.color.g, PurchasePowerupsBackdrop.color.b, 0f), 0.15f).OnComplete(() =>
        {
            PurchasePowerupsImage.transform.DOScale(0, 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                PurchasePowerupsUI.SetActive(false);
                PurchasePowerupsBackdrop.color = new Color(PurchasePowerupsBackdrop.color.r, PurchasePowerupsBackdrop.color.g, PurchasePowerupsBackdrop.color.b, 0f);
                PurchasePowerupsImage.transform.localScale = Vector3.zero;
            });
        });
    }
}