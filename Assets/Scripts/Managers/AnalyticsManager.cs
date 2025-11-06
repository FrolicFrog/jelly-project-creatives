using PlayerPrefsManager;
using UnityEngine;

namespace Managers
{
    public class AnalyticsManager : MonoBehaviour
    {
        public void LevelStarted(int LevelNumber)
        {
            // TinySauce.OnGameStarted(LevelNumber);
        }

        public void LevelCompleted(int LevelNumber)
        {
            int CurrentCoins = PlayerPrefs.GetInt("coins", 0);
            // TinySauce.OnGameFinished(true, CurrentCoins, LevelNumber);
        }

        public void LevelFailed(int LevelNumber)
        {
            int CurrentCoins = PlayerPrefs.GetInt("coins", 0);
            // TinySauce.OnGameFinished(false, CurrentCoins, LevelNumber);
        }

        public void PowerupUsed(int LevelNumber, string PowerupName)
        {
            // TinySauce.OnPowerUpUsed(PowerupName, LevelNumber);
        }
    }
}
