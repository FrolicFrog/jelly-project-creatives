using UnityEngine;
using BlockStackTypes;
using PlayerPrefsManager;

public class HapticsManager : MonoBehaviour
{
    [Header("Haptics")]
    public HapticType CupTrayLanding;
    public HapticType CupsStacking;
    public HapticType CupsConveyerPlacement;
    public HapticType GameOver;
    public HapticType GameWin;
    public HapticType CoinCollection;

    private bool IsHapticsMuted = false;
    public bool HapticsMuted => IsHapticsMuted;

    void Awake()
    {
        string HapticsMutedKey = PlayerPrefsKeyManager.MutedHaptics;
        IsHapticsMuted = PlayerPrefs.GetInt(HapticsMutedKey, 0) == 1;
    }

    public bool ToggleHaptics()
    {
        IsHapticsMuted = !IsHapticsMuted;
        string HapticsMutedKey = PlayerPrefsKeyManager.MutedHaptics;
        PlayerPrefs.SetInt(HapticsMutedKey, IsHapticsMuted ? 1 : 0);
        return IsHapticsMuted;
    }

    public void PlayHaptic(HapticType hapticType)
    {
        if (IsHapticsMuted) return;

        switch (hapticType)
        {
            case HapticType.Heavy:
                Taptic.Heavy();
                return;
            case HapticType.Medium:
                Taptic.Medium();
                return;
            case HapticType.Light:
                Taptic.Light();
                return;
            case HapticType.Success:
                Taptic.Success();
                return;
            case HapticType.Warning:
                Taptic.Warning();
                return;
            case HapticType.Failure:
                Taptic.Failure();
                return;
            case HapticType.Default:
                Taptic.Default();
                return;
            case HapticType.Vibrate:
                Taptic.Vibrate();
                return;
            case HapticType.Selection:
                Taptic.Selection();
                return;
            default:
                break;
        }
    }
}
