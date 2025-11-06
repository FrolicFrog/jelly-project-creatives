namespace PlayerPrefsManager
{
    public class PlayerPrefsKeyManager
    {
        //Current Prize Index Key
        public static string CurrentPrizeIndex = "currentprizeindex";

        //Current Prize Progress Key
        public static string CurrentPrizeProgress = "currentprizeprogress";

        //Muted SFX Key
        public static string MutedSFX = "mutedsfx";

        //Muted Music Key
        public static string MutedMusic = "mutedmusic";

        //Muted Haptics Key
        public static string MutedHaptics = "mutedhaptics";

        //Level Id for Attempts Tracking
        public static string LevelNumberName(int CurrentLevel) => $"Level_{CurrentLevel}";

        public static string AvailableSwaps = "swapused";
        public static string AvailableInfo = "infoused";
        public static string AvailableShuffle = "shuffleused";
        public static string TutorialShownAlreadySwapPowerup = "tutorial_swap_powerup";
        public static string TutorialShownAlreadyInfoPowerup = "tutorial_info_powerup";
        public static string TutorialShownAlreadyShufflePowerup = "tutorial_shuffle_powerup";
    }
}