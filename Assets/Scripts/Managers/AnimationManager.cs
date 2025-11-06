
using System;
using System.Collections.Generic;
using System.Linq;
using BlockStackTypes;
using DG.Tweening;
using UnityEngine;

public class AnimationManager
{
    public static List<Vector3> GenerateStackAnimationPath(Vector3 target, Transform topCup, Vector3 finalPos, float ArcHeight, int CurveSteps, AnimationCurve JumpCurve)
    {
        int minSteps = 20;
        int maxSteps = 100;
        float distance = Vector3.Distance(topCup.position, finalPos);
        int dynamicSteps = Mathf.Clamp((int)(distance * 5f), minSteps, maxSteps);
        CurveSteps = Mathf.Max(CurveSteps, dynamicSteps);

        List<Vector3> pathPoints = new();
        float adjustedArcHeight = Mathf.Max(ArcHeight, Mathf.Abs(finalPos.y - topCup.position.y) + 1f);

        for (int i = 0; i <= CurveSteps; i++)
        {
            float t = i / (float)CurveSteps;

            Vector3 parabolicPoint = Vector3.Lerp(topCup.position, target, t);
            parabolicPoint.y += JumpCurve.Evaluate(t) * adjustedArcHeight;
            if (t == 1f) parabolicPoint = finalPos;
            pathPoints.Add(parabolicPoint);
        }

        return pathPoints;
    }

    public static List<Vector3> GenerateConveyerAnimationPath(Vector3 startPos, Vector3 midPos, Vector3 finalPos, int CurveSteps, AnimationCurve JumpCurve, float ConveyerAnimArcHeight)
    {
        List<Vector3> pathPoints = new();
        for (int i = 0; i <= CurveSteps; i++)
        {
            float t = i / (float)CurveSteps;

            Vector3 parabolicPoint = Vector3.Lerp(startPos, midPos, t);
            parabolicPoint.y += JumpCurve.Evaluate(t) * ConveyerAnimArcHeight;

            if (t == 1f)
            {
                parabolicPoint = Vector3.Lerp(midPos, finalPos, t);
                parabolicPoint.y += JumpCurve.Evaluate(t) * ConveyerAnimArcHeight;
            }

            pathPoints.Add(parabolicPoint);
        }

        return pathPoints;
    }

    // public static void CupStackAnimation(Sequence seq, Transform topCup, List<Vector3> pathPoints, float TimeToGetStacked, float SquishTweak, int SequenceIdx, in List<Transform> CupStackBeneath)
    // {
    //     if (!topCup.TryGetComponent(out Cup TopCup))
    //     {
    //         Debug.LogWarning("Cup Component not founded");
    //         return;
    //     }

    //     float OriginalY = topCup.localScale.y;
    //     Sequence localSeq = DOTween.Sequence();

    //     float OriginalEyeY = TopCup.Eyes.localScale.y;

    //     localSeq

    //     //Arc Path Follow
    //     .Join(topCup.DOPath(pathPoints.ToArray(), TimeToGetStacked, PathType.CatmullRom).SetEase(Ease.Linear))

    //     //Stretching
    //     .Join(topCup.DOScaleY(OriginalY * 2f, TimeToGetStacked / 4).SetDelay(TimeToGetStacked / 8f)).OnComplete(() => topCup.DOScaleY(OriginalY, TimeToGetStacked / 4))

    //     //Squashing
    //     .Join(topCup.DOScaleY(OriginalY * 0.4f, 0.1f).SetDelay(TimeToGetStacked * 0.75f).SetEase(Ease.InOutBack))
    //     .Join(TopCup.Eyes.DOScaleX(OriginalEyeY * 4f, 0.005f).SetDelay(TimeToGetStacked * 0.75f))

    //     //Restoration
    //     .Join(topCup.DOScaleY(OriginalY, 0.1f).SetDelay(TimeToGetStacked * 0.80f).SetEase(Ease.InOutBack))
    //     .Join(TopCup.Eyes.DOScaleX(OriginalEyeY, 0.005f).SetDelay(TimeToGetStacked * 0.80f))

    //     //Effects and Feedback
    //     .JoinCallback(() => Manager.Instance.AudioManagement.PlayAudioEffect("CupStacking", LayeredSound: true))
    //     .JoinCallback(() => Manager.Instance.HapticsManagement.PlayHaptic(Manager.Instance.HapticsManagement.CupsStacking))
    //     .SetLink(topCup.gameObject);

    //     seq.Insert(SequenceIdx * 0.05f, localSeq);
    // }

    public static void CupStackAnimation(Sequence seq, Transform topCup, List<Vector3> pathPoints, float TimeToGetStacked, float SquishTweak, int SequenceIdx, in List<Transform> CupStackBeneath)
    {
        if (!topCup.TryGetComponent(out Cup TopCup))
        {
            Debug.LogWarning("Cup Component not founded");
            return;
        }

        Vector3 OriginalScale = topCup.localScale;
        float OriginalX = OriginalScale.x;
        float OriginalY = OriginalScale.y;
        float OriginalZ = OriginalScale.z;
        float OriginalEyeX = TopCup.Eyes.localScale.x;

        Sequence localSeq = DOTween.Sequence();

        // Move along path
        localSeq.Append(topCup.DOPath(pathPoints.ToArray(), TimeToGetStacked, PathType.CatmullRom).SetEase(Ease.InOutSine));

        // Stretch on upward movement (first half of travel)
        localSeq.Insert(0f, topCup.DOScaleY(OriginalY * 1.3f, TimeToGetStacked * 0.5f).SetEase(Ease.OutQuad)
        .OnUpdate(() =>
        {
            float newY = topCup.localScale.y;
            float factor = Mathf.Sqrt(OriginalY / newY);
            topCup.localScale = new Vector3(OriginalX * factor, newY, OriginalZ * factor);
        }));

        // Squash/stretch on landing (last part of travel)
        float ImpactTime = TimeToGetStacked * 0.85f;
        Vector3 SquashScale = new(OriginalX * Mathf.Sqrt(OriginalY / (OriginalY * 0.7f)), OriginalY * 0.7f, OriginalZ * Mathf.Sqrt(OriginalY / (OriginalY * 0.7f)));
        
        localSeq.Insert(ImpactTime, topCup.DOScale(SquashScale, 0.10f).SetEase(Ease.OutQuad));
        localSeq.Insert(ImpactTime, TopCup.Eyes.DOScaleX(OriginalEyeX * 4f, 0.10f).SetEase(Ease.OutQuad));

        // Restoring Eyes Size
        localSeq.Insert(ImpactTime + 0.17f, TopCup.Eyes.DOScaleX(OriginalEyeX, 0.2f).SetEase(Ease.OutBack));

        // Restoring Squash Scaled down
        localSeq.Insert(ImpactTime + 0.17f, topCup.DOScale(OriginalScale, 0.2f).SetEase(Ease.OutBack));

        List<Transform> CupStackBeneathCopy = new(CupStackBeneath);
        localSeq.InsertCallback(ImpactTime + 0.17f, () =>
        {
            foreach(Transform cup in CupStackBeneathCopy)
            {
                if(cup.TryGetComponent(out Cup cupComponent))
                {
                    cupComponent.Eyes.DOKill();
                    cupComponent.Eyes.DOScaleZ(25f, 0.1f).SetEase(Ease.OutBack);
                }
            }
        });

        // Feedback
        localSeq.InsertCallback(ImpactTime + 0.02f, () => Manager.Instance.AudioManagement.PlayAudioEffect("CupStacking", LayeredSound: true));
        localSeq.InsertCallback(ImpactTime + 0.02f, () => Manager.Instance.HapticsManagement.PlayHaptic(Manager.Instance.HapticsManagement.CupsStacking));

        localSeq.SetLink(topCup.gameObject);
        seq.Insert(SequenceIdx * 0.05f, localSeq);
    }

    public static void CupConveyerAnimation(Transform Cup, List<Vector3> pathPoints, Sequence cupSeq, float TimeToGetConveyer, float RotationAnimDuration, Action OnPathComplete, Vector3 InitialScale, float ConveyerAnimateScaleDownMultiplier, float ConveyerAnimateScaleUpMultiplier, float ConveyerScaleAnimationDuration, float ConveyerScaleNormalDuration, float RequiredElevation, float ElevationAnimDuration, Action OnAnimationComplete, MonoBehaviour mb = null)
    {
        Vector3 FinalScale = InitialScale * ConveyerAnimateScaleDownMultiplier;

        cupSeq
        .Join(Cup.DOPath(pathPoints.ToArray(), TimeToGetConveyer, PathType.Linear)
        .OnStart(() =>
        {
            if(Cup.TryGetComponent(out Cup CupCmp))
                CupCmp.Eyes.DOScale(100,0.1f).SetEase(Ease.OutBack);
        })
        .OnComplete(() =>
        {
            Manager.Instance.AudioManagement.PlayAudioEffect("CupGoingOnBelt", LayeredSound: true);
            Cup.GetComponent<Cup>().PausedExplicitly = false;
            OnPathComplete?.Invoke();
        }))
        .Join(Cup.transform.GetChild(0).DOScale(InitialScale * ConveyerAnimateScaleUpMultiplier, ConveyerScaleNormalDuration))
        .Join(Cup.transform.GetChild(0).DOScale(FinalScale, ConveyerScaleNormalDuration).SetDelay(ConveyerScaleAnimationDuration))
        // .Append(Cup.transform.GetChild(0).DOScaleY(FinalScale.y * 0.3f, 0.1f).SetEase(Ease.InOutBack))
        // .Append(Cup.transform.GetChild(0).DOScale(FinalScale, 0.1f).SetEase(Ease.InOutBack))
        // .Append(Cup.transform.GetChild(0).DOLocalMoveY(RequiredElevation, ElevationAnimDuration).SetEase(Ease.OutSine))
        .OnComplete(() => OnAnimationComplete?.Invoke())
        .SetLink(Cup.gameObject);
    }
}