using UnityEngine;
using BlockStackTypes;
using System.Linq;
using PlayerPrefsManager;
using UnityEditor;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public string Path;
    public AudioEffect[] AudioEffects;
    public AudioSource BackgroundMusic;

    private bool IsSFXMuted = false;
    private bool IsBGMMuted = false;
    public bool BGMMuted => IsBGMMuted;
    public bool SFXMuted => IsSFXMuted;

    private void Awake()
    {
        UpdateFromPlayerPrefs();
        if (!IsBGMMuted && BackgroundMusic != null) BackgroundMusic.Play();
    }

    private void UpdateFromPlayerPrefs()
    {
        string SFXMutedKey = PlayerPrefsKeyManager.MutedSFX;
        string BGMMutedKey = PlayerPrefsKeyManager.MutedMusic;

        IsSFXMuted = PlayerPrefs.GetInt(SFXMutedKey, 0) == 1;
        IsBGMMuted = PlayerPrefs.GetInt(BGMMutedKey, 0) == 1;
    }

    public void PlayAudioEffect(string AudioEffectName, bool InterruptAlreadyPlaying = true, bool LayeredSound = false, Action OnComplete = null)
    {
        if (IsSFXMuted) return;

        AudioEffect TargetEffect = AudioEffects.Where(x => x.Name == AudioEffectName).FirstOrDefault();
        if (TargetEffect == null)
        {
            Debug.LogWarning("NO SUCH AUDIO EFFECT EXISTS");
            return;
        }

        if (!InterruptAlreadyPlaying && TargetEffect.AudioSource.isPlaying) return;
        if (LayeredSound)
        {
            CreateIndependentSound(TargetEffect.AudioSource.clip, TargetEffect.AudioSource.volume, TargetEffect.AudioSource.pitch);
            return;
        }

        TargetEffect.AudioSource.Play();
        if (OnComplete != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(TargetEffect.AudioSource.clip.length).OnComplete(() => OnComplete.Invoke());
        }
    }

    private void CreateIndependentSound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        AudioSource temp = gameObject.AddComponent<AudioSource>();
        temp.volume = volume;
        temp.pitch = pitch;
        temp.PlayOneShot(clip);
        Destroy(temp, clip.length + 0.1f);
    }


    public bool ToggleMusic()
    {
        if (IsBGMMuted) UnmuteMusic();
        else MuteMusic();

        return IsBGMMuted;
    }

    public bool ToggleSFX()
    {
        if (IsSFXMuted) UnmuteSFX();
        else MuteSFX();

        return IsSFXMuted;
    }

    private void MuteMusic()
    {
        IsBGMMuted = true;

        if(BackgroundMusic != null)
        BackgroundMusic.Stop();
        
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.MutedMusic, 1);
    }

    private void UnmuteMusic()
    {
        IsBGMMuted = false;

        if(BackgroundMusic != null)
        BackgroundMusic.Play();
        
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.MutedMusic, 0);
    }

    private void MuteSFX()
    {
        IsSFXMuted = true;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.MutedSFX, 1);
    }

    private void UnmuteSFX()
    {
        IsSFXMuted = false;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.MutedSFX, 0);
    }

    // [ContextMenu("Setup Audio Effects")]
    // public void SetupAudioEffects()
    // {
    //     AudioSource[] AudioSources = GetComponents<AudioSource>();
    //     Array.ForEach(AudioSources, x => DestroyImmediate(x));

    //     string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { Path });
    //     Debug.Log($"Found {guids.Length} audio clip GUIDs at path: {Path}");
    //     List<AudioEffect> audioEffectsList = new List<AudioEffect>();

    //     foreach (string guid in guids)
    //     {
    //         string assetPath = AssetDatabase.GUIDToAssetPath(guid);
    //         AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
    //         if (clip != null)
    //         {
    //             AudioSource audioSource = gameObject.AddComponent<AudioSource>();
    //             audioSource.clip = clip;
    //             audioSource.playOnAwake = false;
    //             audioSource.loop = false;
    //             audioEffectsList.Add(new AudioEffect(audioSource));
    //         }
    //     }

    //     Debug.Log($"Created {audioEffectsList.Count} audio effects from clips at path: {Path}");
    //     AudioEffects = audioEffectsList.ToArray();
    // }
}
