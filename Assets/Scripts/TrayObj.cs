using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockStackTypes;
using DG.Tweening;
using UnityEngine;

public class TrayObj: MonoBehaviour
{
    public GameObject PackingTrailGbj;
    public MeshRenderer[] Renderers;
    public SpriteRenderer[] Stars;

    [HideInInspector] public int Capacity = 1;
    [HideInInspector] public TrayDispenser DispensedBy = null;
    private int PositionIndex = 1;
    private Vector3 InitialScale;
    private bool TrayIsFull = false;
    private int LastPositionIndex = -1;
    private bool IsMysteriousTray = false;
    private Material[] OriginalMaterials;
    private Material TrayMaterial;

    void Awake()
    {
        //One for Mesh and One for the Trail
        Capacity = transform.childCount - 2;
        OriginalMaterials = Renderers[0].materials;
        PackingTrailGbj.SetActive(false);
    }

    private void SetupTrailColors()
    {
        Color TrayColor = GetTrayColor(TrayMaterial);

        ParticleSystem Stars = PackingTrailGbj.GetComponent<ParticleSystem>();
        ParticleSystem Sparks = PackingTrailGbj.transform.GetChild(0).GetComponent<ParticleSystem>();
        ParticleSystem Glow = PackingTrailGbj.transform.GetChild(1).GetComponent<ParticleSystem>();

        ParticleSystem.MainModule a = Stars.main;
        ParticleSystem.MainModule b = Sparks.main;
        ParticleSystem.MainModule c = Glow.main;

        a.startColor = TrayColor;
        b.startColor = TrayColor;

        float cAlpha = c.startColor.color.a;
        c.startColor = new Color(TrayColor.r, TrayColor.g, TrayColor.b, cAlpha);
    }

    private Color GetTrayColor(Material trayMaterial)
    {
        string MaterialName = trayMaterial.name;

        if (trayMaterial.name.EndsWith("Designer"))
            MaterialName = trayMaterial.name.Replace("Designer", "");

        TrayTrailColors[] TrayColors = Manager.Instance.GameManagement.TrayTrailColors;
        List<TrayTrailColors> MatchingColor = TrayColors.Where(x => x.MaterialName == MaterialName).ToList();

        return MatchingColor.Count > 0 ? MatchingColor[0].MaterialColor : trayMaterial.GetColor("_Color_A");
    }

    public void ShowStars()
    {
        foreach (var star in Stars)
        {
            star.gameObject.SetActive(true);
        }
    }
    
    public void HideStars()
    {
        foreach (var star in Stars)
        {
            star.gameObject.SetActive(false);
        }
    }
    
    public Vector3? GetPositionToLandCup()
    {
        if(TrayIsFull || transform.childCount <= PositionIndex) return null;
        if(LastPositionIndex == PositionIndex) return null; //Preventing the same position to be used again

        Vector3 LandPos = transform.GetChild(PositionIndex).position;
        LastPositionIndex = PositionIndex;

        return LandPos;
    }

    public void UpdatePositionIndex()
    {
        PositionIndex++;

        if (PositionIndex > Capacity && DispensedBy != null && !Manager.Instance.IsLevelFailed)
        {
            TrayIsFull = true;
            DispensedBy.NextTray();
        }
    }

    public void FlyAway()
    {
        PackingTrailGbj.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y + 13f, 1f).SetEase(Ease.OutBack).SetLink(gameObject))
        .Join(transform.DORotate(new Vector3(0f, 0f, 360f), 1f).SetEase(Ease.OutBack).SetLink(gameObject))
        .Append(transform.DOMove(Manager.Instance.ReferenceManagement.BoxAnimationTarget.position, 0.6f).SetEase(Ease.Linear).SetLink(gameObject))
        .JoinCallback(() => Manager.Instance.AudioManagement.PlayAudioEffect("BoxFlying", LayeredSound: true))
        .Join(transform.DOScale(transform.localScale * 0.5f, 0.6f).SetEase(Ease.Linear).SetLink(gameObject))
        .OnComplete(() =>
        {
            Manager.Instance.CheckGameState();
            Destroy(gameObject);
        });
    }
    
    public void RevealMysteriousTray()
    {
        IsMysteriousTray = false;
        UpdateVisuals();
    }

    public void SetData(TrayDispenser trayDispenser, bool isMysterious, Material TrayMaterial, bool IsStarredTray)
    {
        DispensedBy = trayDispenser;
        IsMysteriousTray = isMysterious;
        this.TrayMaterial = TrayMaterial;

        if(IsStarredTray)
            ShowStars();
        else
            HideStars();
        
        UpdateVisuals();
        SetupTrailColors();
    }

    private void UpdateVisuals()
    {
        foreach(var Renderer in Renderers)
        Renderer.materials = IsMysteriousTray ? GetArrayOf(Manager.Instance.ReferenceManagement.MysteriousMaterial,Renderer.materials.Length) : OriginalMaterials;
    }

    private Material[] GetArrayOf(Material mysteriousMaterial, int Length)
    {
        Material[] materials = new Material[Length];

        for (int i = 0; i < Length; i++)
        {
            materials[i] = mysteriousMaterial;
        }

        return materials;
    }
}