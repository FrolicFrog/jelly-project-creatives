using TMPro;
using UnityEngine;
using DG.Tweening;

public class Blocker : MonoBehaviour
{
    public ParticleSystem ExplosionEffect;
    public ParticleSystem ReductionEffect; //Reduction of the label number
    public float JiggleScale = 0.9f;

    private int _TurnsToBreak = -1;
    private TextMeshPro Label => GetComponentInChildren<TextMeshPro>();
    public int TurnsToBreak
    {
        get => _TurnsToBreak;
        set
        {
            _TurnsToBreak = value;
            Label.text = _TurnsToBreak.ToString();
        }
    }

    private Sequence JiggleSequence;


    [HideInInspector] public string BlockedStackIdentifier;

    void Awake()
    {
        Manager.Instance.GameManagement.OnPlacingCupsOnConveyer += HandleBreaking;
    }

    private void HandleBreaking()
    {
        if (TurnsToBreak > 0)
        {
            TurnsToBreak--;

            if (TurnsToBreak == 0)
            {
                Manager.Instance.GameManagement.OnPlacingCupsOnConveyer -= HandleBreaking;
                Manager.Instance.GameManagement.RemoveBlockedStack(BlockedStackIdentifier);
                BlastBlocker();
            }
            else
            {
                Jiggle();
                ReductionEffect.Play();
            }
        }
    }

    public void Jiggle()
    {
        if (JiggleSequence != null && JiggleSequence.IsActive() && JiggleSequence.IsPlaying()) return;

        Vector3 originalScale = transform.localScale;
        Vector3 reducedScale = originalScale * JiggleScale;

        JiggleSequence = DOTween.Sequence()
        .Append(transform.DOScale(reducedScale, 0.1f).SetEase(Ease.OutQuad))
        .Append(transform.DOScale(originalScale, 0.1f).SetEase(Ease.OutElastic));
    }

    private void BlastBlocker()
    {
        ExplosionEffect.Play();
        Destroy(Label.gameObject);
        Animator Anim = GetComponentInChildren<Animator>();
        if (Anim != null) Anim.enabled = true;
        Destroy(gameObject, 3f);
    }
}
