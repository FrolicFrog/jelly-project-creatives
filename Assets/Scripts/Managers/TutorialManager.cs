using DG.Tweening;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public Transform FingerAnimation;
    public Transform FingerAnimationUI;
    public Vector3 FingerOffset;
    public RectTransform Powerups;

    private Transform FingerCurrentClone;
    private readonly Quaternion Rotation = Quaternion.Euler(29.9999504f, 0f, 0f);
    private readonly Vector3 Scale = Vector3.one * 7.8f;
    public RectTransform PowerupTarget;

    void Update()
    {
        if (Manager.Instance.GameManagement.CupsEnteringConveyer)
            DestroyFingerAt();
    }

    public void ShowFingerAt(Vector3 Position)
    {
        FingerCurrentClone = Instantiate(FingerAnimation, Position + FingerOffset, Rotation);
        FingerCurrentClone.localScale = Scale;
    }

    public void DestroyFingerAt()
    {
        if (FingerCurrentClone == null) return;
        
        Destroy(FingerCurrentClone.gameObject);
        Manager.Instance.UIManagement.DisableFingerAnimationBackdrop();
        FingerCurrentClone = null;
    }

    public void ShowFingerOnPowerup(RectTransform Target)
    {
        Manager.Instance.InputAllowedOnCups = false;
        Sequence seq = DOTween.Sequence();
        seq.Append(Powerups.DOAnchorPosY(Powerups.anchoredPosition.y + 105f, 0.5f).SetEase(Ease.OutCubic));
        seq.AppendCallback(() =>
        {
            Transform FingerAnimation = Target.GetChild(Target.childCount - 1);
            FingerAnimation.gameObject.SetActive(true);
        });

        PowerupTarget = Target;
        seq.Play();
    }

    public void HidePowerupTutorialUI()
    {
        Manager.Instance.UIManagement.DisableFingerAnimationBackdrop();
        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(0, 0.15f);
        Manager.Instance.InputAllowedOnCups = true;

        if (PowerupTarget == null) return;
        Transform FingerAnimation = PowerupTarget.GetChild(PowerupTarget.childCount - 1);
        FingerAnimation.gameObject.SetActive(false);
        PowerupTarget = null;
        Powerups.DOAnchorPosY(Powerups.anchoredPosition.y - 105f, 0.5f).SetEase(Ease.OutCubic);
    }
}
