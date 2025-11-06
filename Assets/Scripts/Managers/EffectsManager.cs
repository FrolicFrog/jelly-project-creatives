using UnityEngine;
using System;
using UnityEngine.UI;
using BlockStackTypes;
using System.Collections;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectsManager : MonoBehaviour
{
    [Serializable]
    public class Effect
    {
        public string EffectName;
        public ParticleSystem ParticleSystem;
        public float Duration;
    }

    [Serializable]
    public class UIVariant
    {
        public Themes TargetTheme = BlockStackTypes.Themes.Red;
        public Image image;
        public Sprite sprite;
        public float DelayDuration = 0f;

        public override string ToString()
        {
            return $"{TargetTheme} - {image.name}";
        }
    }

    [Serializable]
    public class SpriteVariant
    {
        public Themes TargetTheme = BlockStackTypes.Themes.Red;
        public Sprite sprite;
        public SpriteRenderer spriteRenderer;

        public override string ToString()
        {
            return $"{TargetTheme} - {sprite.name}";
        }
    }   

    public Effect[] Effects;

    [Header("References")]
    public MeshRenderer[] BaseMaterialTargets;
    private MeshRenderer[] BeltMaterialTargets;
    private MeshRenderer[] BeltWallMaterialTargets;
    public MeshRenderer[] DividerMaterialTargets;
    private MeshRenderer[] TrayDispenserMaterialTargets;
    public MeshRenderer[] TableMaterialTargets;
    public MeshRenderer[] TableBaseMaterialTargets;


    [Header("Graphic Themes")]
    public GraphicsTheme[] Themes;

    [Header("Hard Level")]
    public UIVariant[] UIVariants;
    public SpriteVariant[] SpriteVariants;

    [Header("Post-Processing")]
    public Volume PostProcessingVolume;

    public void PlayEffect(string EffectName, bool RestartIfPlaying = true)
    {
        foreach (Effect effect in Effects)
        {
            if (effect.EffectName == EffectName)
            {
                if (RestartIfPlaying)
                {
                    effect.ParticleSystem.gameObject.SetActive(true);
                    if (effect.ParticleSystem.isPlaying)
                    {
                        effect.ParticleSystem.Stop();
                        effect.ParticleSystem.Play();
                    }
                    else
                        effect.ParticleSystem.Play();
                }
                else
                    effect.ParticleSystem.Play();

                break;
            }
        }
    }

    public void StopEffect(string EffectName)
    {
        foreach (Effect effect in Effects)
        {
            if (effect.EffectName == EffectName)
            {
                effect.ParticleSystem.gameObject.SetActive(false);
                effect.ParticleSystem.Stop();
                break;
            }
        }
    }

    public float GetDuration(string EffectName)
    {
        foreach (Effect effect in Effects)
        {
            if (effect.EffectName == EffectName)
            {
                return effect.Duration;
            }
        }

        return 0f;
    }

    public void ApplyTheme(GraphicsTheme Theme)
    {
        // MeshRenderer Belt = Manager.Instance.ReferenceManagement.Conveyor.BeltMeshRenderer;
        // BeltMaterialTargets = new MeshRenderer[1] { Belt };

        // MeshRenderer BeltWall = Manager.Instance.ReferenceManagement.Conveyor.BeltWallMeshRenderer;
        // BeltWallMaterialTargets = new MeshRenderer[1] { BeltWall };

        // TrayDispenser[] Dispensers = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.parent.GetComponentsInChildren<TrayDispenser>(true);
        // TrayDispenserMaterialTargets = new MeshRenderer[Dispensers.Length];

        // MeshRenderer Path = Manager.Instance.ReferenceManagement.Conveyor.Path.GetComponent<MeshRenderer>();
        // Path.material.SetColor("_Color", Theme.ArrowColor.Color);

        // Transform TargetShadow = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.GetChild(1);
        // MeshRenderer Shadow = TargetShadow.GetComponent<MeshRenderer>();
        // Shadow.material = Theme.ShadowMaterial;

        // for (int i = 0; i < Dispensers.Length; i++)
        //     TrayDispenserMaterialTargets[i] = Dispensers[i].GetComponent<MeshRenderer>();

        // foreach (MeshRenderer target in BaseMaterialTargets)
        // {
        //     if (target != null && Theme.BaseMaterial != null)
        //         target.material = Theme.BaseMaterial;
        // }

        // foreach (MeshRenderer target in BeltMaterialTargets)
        // {
        //     if (target != null && Theme.BeltMaterial != null)
        //         target.material = Theme.BeltMaterial;
        // }

        // foreach (MeshRenderer target in BeltWallMaterialTargets)
        // {
        //     if (target != null && Theme.BeltWallMaterial != null)
        //         target.material = Theme.BeltWallMaterial;
        // }

        // foreach (MeshRenderer target in DividerMaterialTargets)
        // {
        //     if (target != null && Theme.DividerMaterial != null)
        //         target.material = Theme.DividerMaterial;
        // }

        // foreach (MeshRenderer target in TrayDispenserMaterialTargets)
        // {
        //     if (target == null) continue;

        //     if (Theme.TrayDispenserMaterial != null)
        //     {
        //         target.material = Theme.TrayDispenserMaterial;
        //     }

        //     if (Theme.TrayDispenserBaseColor != null)
        //     {
        //         Color BaseColor = Theme.TrayDispenserBaseColor.Color;
        //         target.transform.GetChild(0).GetComponent<SpriteRenderer>().color = BaseColor;
        //     }
        // }

        // foreach (MeshRenderer target in TableMaterialTargets)
        // {
        //     if (target != null && Theme.TableMaterial != null)
        //         target.material = Theme.TableMaterial;
        // }

        // foreach (MeshRenderer target in TableBaseMaterialTargets)
        // {
        //     if (target != null && Theme.TableBaseMaterial != null)
        //         target.material = Theme.TableBaseMaterial;
        // }

        // foreach (UIVariant variant in UIVariants)
        // {
        //     if (variant.TargetTheme != Theme.Theme) continue;

        //     if (variant.DelayDuration <= 0f)
        //         variant.image.sprite = variant.sprite;
        //     else
        //         StartCoroutine(DelayedUIChange(variant));
        // }

        // foreach(SpriteVariant spriteVariant in SpriteVariants)
        // {
        //     if (spriteVariant.TargetTheme != Theme.Theme) continue;

        //     spriteVariant.spriteRenderer.sprite = spriteVariant.sprite;
        // }
    }

    public void AnimateExposureByPostProcess(float Value, float Duration)
    {
        if (!PostProcessingVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments)) return;

        DOTween
        .To(() => colorAdjustments.postExposure.value, x => colorAdjustments.postExposure.value = x, Value, Duration)
        .OnUpdate(() => Camera.main.UpdateVolumeStack())
        .SetEase(Ease.Linear);
    }

    IEnumerator DelayedUIChange(UIVariant variant)
    {
        yield return new WaitForSeconds(variant.DelayDuration);
        RectTransform LevelBarRectTransform = variant.image.GetComponent<RectTransform>();
        Vector2 InitialScale = LevelBarRectTransform.localScale;
        LevelBarRectTransform.DOScale(0f, 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            variant.image.sprite = variant.sprite;
            LevelBarRectTransform.DOScale(InitialScale, 0.1f).SetEase(Ease.InOutBack);
        });
    }
}
