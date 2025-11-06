using System;
using UnityEngine;
using PlayerPrefsManager;

public class PrizeManager : MonoBehaviour
{
    [Serializable]
    public class Prize
    {
        public Sprite Graphic;
        [Range(1,100)]
        public int LevelsRequiredToUnlock;
    }

    [Header("PRIZES")]
    public Prize[] Prizes;

    public (Sprite,int, int, int) GetCurrentPrize()
    {
        if (Prizes.Length == 0) return (null,-1,-1,-1);

        int CurrentPrizeIndex = PlayerPrefs.GetInt(PlayerPrefsKeyManager.CurrentPrizeIndex, 0);
        int Progress = PlayerPrefs.GetInt(PlayerPrefsKeyManager.CurrentPrizeProgress, 0);
        int PreviousProgess = Progress;

        if (Progress == Prizes[CurrentPrizeIndex].LevelsRequiredToUnlock)
        {
            if (CurrentPrizeIndex >= Prizes.Length - 1)
                return (null,-1,-1,-1);

            CurrentPrizeIndex++;
            Progress = 1;
        }
        else
        {
            Progress += 1;
            Progress = Mathf.Clamp(Progress,0,Prizes[CurrentPrizeIndex].LevelsRequiredToUnlock);
        }

        PlayerPrefs.SetInt(PlayerPrefsKeyManager.CurrentPrizeIndex, CurrentPrizeIndex);
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.CurrentPrizeProgress, Progress);

        return (Prizes[CurrentPrizeIndex].Graphic,PreviousProgess, Progress,Prizes[CurrentPrizeIndex].LevelsRequiredToUnlock);
    }
}
