using DG.Tweening;
using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    [Header("Animation Speed")]
    public float AnimationDuration = 2f;
    public Vector3 Offset;
    private Vector3 OriginalPos;

    void Awake()
    {
        OriginalPos = transform.GetChild(0).position;
        transform.GetChild(0).DOMove(OriginalPos + Offset, AnimationDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        Manager.Instance.GameManagement.RegisterPointerArrow(this);
    }
}
