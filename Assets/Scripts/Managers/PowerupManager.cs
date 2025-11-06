using System;
using BlockStackTypes;
// using LionStudios.Suite.Ads;
using PlayerPrefsManager;
using Unity.Collections;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public enum Powerup
    {
        None,
        Swap,
        Info,
        Shuffle
    }
    [Serializable]
    public class TrayColor
    {
        public TrayMaterial trayMaterial;
        public Color color;
    }
    public TrayColor[] TrayColors;
    public Powerup CurrentPowerup = Powerup.None;
    public bool IsSwapPowerupAvailable => AvailableSwaps > 0;
    public bool IsInfoPowerupAvailable => AvailableInfo > 0;
    public bool IsShufflePowerupAvailable => AvailableShuffle > 0;
    public readonly string SwapPowerupTip = "Tap on dispenser\n <size=35/>to swap trays";
    public readonly string InfoPowerupTip = "Tap on dispenser\n <size=35/>to see incoming trays";
    public readonly string ShufflePowerupTip = "Tap on cups stack\n <size=35/>to shuffle cups";

    [Header("Powerup Unlocks")]
    public int UnlockSwapPowerupLvl = 6;
    public int UnlockInfoPowerupLvl = 10;
    public int UnlockShufflePowerupLvl = 12;

    public int AvailableSwaps => PlayerPrefs.GetInt(PlayerPrefsKeyManager.AvailableSwaps, 2);
    public int AvailableInfo => PlayerPrefs.GetInt(PlayerPrefsKeyManager.AvailableInfo, 2);
    public int AvailableShuffle => PlayerPrefs.GetInt(PlayerPrefsKeyManager.AvailableShuffle, 2);
    public (bool SwapInputAllowed, bool InfoInputAllowed, bool ShuffleInputAllowed) InputsAllowed = (true, true, true);

    public bool ShowHandForSwapTutorial = false;
    public bool ShowHandForInfoTutorial = false;

    public void AllowInputOnlyFor(Powerup powerup)
    {
        InputsAllowed.SwapInputAllowed = false;
        InputsAllowed.InfoInputAllowed = false;
        InputsAllowed.ShuffleInputAllowed = false;

        switch (powerup)
        {
            case Powerup.Swap:
                InputsAllowed.SwapInputAllowed = true;
                break;
            case Powerup.Info:
                InputsAllowed.InfoInputAllowed = true;
                break;
            case Powerup.Shuffle:
                InputsAllowed.ShuffleInputAllowed = true;
                break;
        }
    }

    public void EnableAllPowerupInputs()
    {
        InputsAllowed.SwapInputAllowed = true;
        InputsAllowed.InfoInputAllowed = true;
        InputsAllowed.ShuffleInputAllowed = true;
    }

    public void UseSwapPowerup()
    {
        if (!InputsAllowed.SwapInputAllowed)
        {
            if (InputsAllowed.InfoInputAllowed)
                OffInfoPowerup();
            else if (InputsAllowed.ShuffleInputAllowed)
                OffShufflePowerup();
            
            EnableAllPowerupInputs();
            UseSwapPowerup();
            return;
        }

        if (CurrentPowerup == Powerup.Swap)
        {
            if (IsATutorialFor(Powerup.Swap)) return;

            OffSwapPowerup();
            EnableAllPowerupInputs();
            return;
        }

        Manager.Instance.TutorialManagement.HidePowerupTutorialUI();
        if (Manager.Instance.GameManagement.CupsEnteringConveyer || CurrentPowerup != Powerup.None) return;

        if (IsSwapPowerupAvailable)
        {
            OnSwapPowerup();
            AllowInputOnlyFor(Powerup.Swap);
            return;
        }
        else
        {
            // LionAds.TryShowRewarded("placement", () => IncrementPowerupCount(Powerup.Swap));
            ShowPowerupPurchaseDialog(Powerup.Swap);
        }
    }

    private bool IsATutorialFor(Powerup powerupType)
    {
        int CurrentLevel = Manager.Instance.LevelManagement.CurrentLevel;

        if (powerupType == Powerup.Swap)
            return CurrentLevel == UnlockSwapPowerupLvl;
        else if (powerupType == Powerup.Info)
            return CurrentLevel == UnlockInfoPowerupLvl;
        else if (powerupType == Powerup.Shuffle)
            return CurrentLevel == UnlockShufflePowerupLvl;

        return false;
    }

    private void ShowPowerupPurchaseDialog(Powerup swap)
    {
        Manager.Instance.UIManagement.PurchasePowerup(swap);
    }

    public void UseInfoPowerup()
    {
        if (!InputsAllowed.InfoInputAllowed)
        {   
            if(InputsAllowed.SwapInputAllowed)
                OffSwapPowerup();
            else if(InputsAllowed.ShuffleInputAllowed)
                OffShufflePowerup();

            EnableAllPowerupInputs();
            UseInfoPowerup();
            return;
        }   
        if (CurrentPowerup == Powerup.Info)
        {
            if (IsATutorialFor(Powerup.Info)) return;

            OffInfoPowerup();
            EnableAllPowerupInputs();
            return;
        }

        Manager.Instance.TutorialManagement.HidePowerupTutorialUI();
        if (Manager.Instance.GameManagement.CupsEnteringConveyer || CurrentPowerup != Powerup.None) return;

        if (IsInfoPowerupAvailable)
        {
            OnInfoPowerup();
            AllowInputOnlyFor(Powerup.Info);
            return;
        }
        else
        {
            // LionAds.TryShowRewarded("placement", () => IncrementPowerupCount(Powerup.Info));
            ShowPowerupPurchaseDialog(Powerup.Info);
        }
    }

    public void UseShufflePowerup()
    {
        if (!InputsAllowed.ShuffleInputAllowed)
        {
            if(InputsAllowed.InfoInputAllowed)
                OffInfoPowerup();
            else if(InputsAllowed.SwapInputAllowed)
                OffSwapPowerup();

            EnableAllPowerupInputs();
            UseShufflePowerup();
            return;
        }
        if (CurrentPowerup == Powerup.Shuffle)
        {
            if (IsATutorialFor(Powerup.Shuffle)) return;
            
            OffShufflePowerup();
            return;
        }

        Manager.Instance.TutorialManagement.HidePowerupTutorialUI();
        if (Manager.Instance.GameManagement.CupsEnteringConveyer || CurrentPowerup != Powerup.None) return;
        if (IsShufflePowerupAvailable)
        {
            OnShufflePowerup();
            return;
        }
        else
        {
            // LionAds.TryShowRewarded("placement", () => IncrementPowerupCount(Powerup.Shuffle));
            ShowPowerupPurchaseDialog(Powerup.Shuffle);
        }
    }

    private void OnInfoPowerup()
    {
        Manager.Instance.AnalyticsManagement.PowerupUsed(Manager.Instance.LevelManagement.CurrentLevel, "tray_info");
        TrayDispenser[] Dispensers = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.parent.GetComponentsInChildren<TrayDispenser>(false);
        ArrowPointer ArrowPointerPrefab = Manager.Instance.ReferenceManagement.ArrowPointerPrefab;
        int ArrowsGenerated = 0;
        TrayDispenser PointedToDispenser = null;
        foreach (TrayDispenser Dispenser in Dispensers)
        {
            if (Dispenser == null) continue;
            ArrowPointer Arrows = Instantiate(ArrowPointerPrefab, Dispenser.transform.position, Quaternion.Euler(0f, 180f, 0f));
            ArrowsGenerated++;
            Dispenser.InfoModeEnabled = true;

            if (Dispenser.FrontTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.FrontTrayTransform, 5);
            if (Dispenser.BackTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.BackTrayTransform, 5);

            Utilities.AssignLayerRecursively(Arrows.transform, 5);
            Utilities.AssignLayerRecursively(Dispenser.transform, 5);
            if (PointedToDispenser == null && ShowHandForInfoTutorial)
            {
                PointedToDispenser = Dispenser;
                Manager.Instance.TutorialManagement.ShowFingerAt(Dispenser.transform.position);
                ShowHandForInfoTutorial = false;
            }
        }

        if (ArrowsGenerated == 0) return;

        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(-4.5f, 0.2f);
        Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton");
        Manager.Instance.GameManagement.PauseConveyer();
        Manager.Instance.UIManagement.ShowPowerupTip(InfoPowerupTip);
        Manager.Instance.InputAllowedOnCups = false;
        CurrentPowerup = Powerup.Info;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableInfo, Mathf.Max(AvailableInfo - 1,0));
        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }

    private void OffInfoPowerup()
    {
        if (ShowHandForInfoTutorial) return;
        Manager.Instance.TutorialManagement.HidePowerupTutorialUI();
        Manager.Instance.UIManagement.ClosePowerupTip();
        Manager.Instance.TutorialManagement.DestroyFingerAt();
        TrayDispenser[] Dispensers = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.parent.GetComponentsInChildren<TrayDispenser>(false);
        foreach (TrayDispenser Dispenser in Dispensers)
        {
            if (Dispenser == null) continue;
            Dispenser.InfoModeEnabled = false;

            Utilities.AssignLayerRecursively(Dispenser.transform, 0);
            if (Dispenser.FrontTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.FrontTrayTransform, 0);
            if (Dispenser.BackTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.BackTrayTransform, 0);
        }

        EnableAllPowerupInputs();
        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(0, 0.15f);
        Manager.Instance.GameManagement.ClearPointerArrows();
        CurrentPowerup = Powerup.None;
        Manager.Instance.GameManagement.PlayConveyer();
        Manager.Instance.InputAllowedOnCups = true;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableInfo, AvailableInfo + 1);
        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }

    private void OnShufflePowerup()
    {
        Manager.Instance.AnalyticsManagement.PowerupUsed(Manager.Instance.LevelManagement.CurrentLevel, "shuffle_cupstacks");
        Manager.Instance.GameManagement.ShuffleAllCupStacks(() =>
        {
            Manager.Instance.UIManagement.ClosePowerupTip();
            CurrentPowerup = Powerup.None;
            Manager.Instance.InputAllowedOnCups = true;
        });

        Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton");
        Manager.Instance.UIManagement.ShowPowerupTip(ShufflePowerupTip);
        Manager.Instance.InputAllowedOnCups = false;
        CurrentPowerup = Powerup.Shuffle;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableShuffle, Mathf.Max(AvailableShuffle - 1, 0));
        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }

    private void OffShufflePowerup()
    {
        if (Manager.Instance.TutorialManagement.PowerupTarget != null) return;
        Manager.Instance.TutorialManagement.DestroyFingerAt();
        Manager.Instance.TutorialManagement.HidePowerupTutorialUI();
        Manager.Instance.UIManagement.ClosePowerupTip();
        CurrentPowerup = Powerup.None;
        Manager.Instance.GameManagement.PlayConveyer();
        Manager.Instance.InputAllowedOnCups = true;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableShuffle, AvailableShuffle + 1);
        EnableAllPowerupInputs();
        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }

    private void OnSwapPowerup()
    {
        Manager.Instance.AnalyticsManagement.PowerupUsed(Manager.Instance.LevelManagement.CurrentLevel, "swap_trays");
        TrayDispenser[] Dispensers = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.parent.GetComponentsInChildren<TrayDispenser>(false);
        ArrowPointer ArrowPointerPrefab = Manager.Instance.ReferenceManagement.ArrowPointerPrefab;
        int ArrowsGenerated = 0;
        TrayDispenser PointedToDispenser = null;

        foreach (TrayDispenser Dispenser in Dispensers)
        {
            if (Dispenser == null) continue;
            if (!Dispenser.TraysLeftToSpawn && Dispenser.BackTrayTransform == null) continue; //Check if there are two trays to swap?


            ArrowPointer Arrows = Instantiate(ArrowPointerPrefab, Dispenser.transform.position, Quaternion.Euler(0f, 180f, 0f));

            ArrowsGenerated++;
            Dispenser.SwitchModeEnabled = true;

            if (Dispenser.FrontTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.FrontTrayTransform, 5);
            if (Dispenser.BackTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.BackTrayTransform, 5);

            Utilities.AssignLayerRecursively(Arrows.transform, 5);
            Utilities.AssignLayerRecursively(Dispenser.transform, 5);
            if (PointedToDispenser == null && ShowHandForSwapTutorial)
            {
                PointedToDispenser = Dispenser;
                Manager.Instance.TutorialManagement.ShowFingerAt(Dispenser.transform.position);
                ShowHandForSwapTutorial = false;
            }
        }

        if (ArrowsGenerated == 0) return;

        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(-4.5f, 0.2f);
        Manager.Instance.AudioManagement.PlayAudioEffect("ContinueButton");
        Manager.Instance.GameManagement.PauseConveyer();
        Manager.Instance.UIManagement.ShowPowerupTip(SwapPowerupTip);
        Manager.Instance.InputAllowedOnCups = false;
        CurrentPowerup = Powerup.Swap;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableSwaps, Mathf.Max(AvailableSwaps - 1, 0));
        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }

    private void OffSwapPowerup()
    {
        if (ShowHandForSwapTutorial) return;
        Manager.Instance.TutorialManagement.HidePowerupTutorialUI();
        Manager.Instance.UIManagement.ClosePowerupTip();
        Manager.Instance.TutorialManagement.DestroyFingerAt();
        TrayDispenser[] Dispensers = Manager.Instance.ReferenceManagement.Conveyor.transform.parent.parent.parent.GetComponentsInChildren<TrayDispenser>(false);
        foreach (TrayDispenser Dispenser in Dispensers)
        {
            if (Dispenser == null) continue;
            Dispenser.SwitchModeEnabled = false;

            Utilities.AssignLayerRecursively(Dispenser.transform, 0);
            if (Dispenser.FrontTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.FrontTrayTransform, 0);
            if (Dispenser.BackTrayTransform != null) Utilities.AssignLayerRecursively(Dispenser.BackTrayTransform, 0);
        }

        EnableAllPowerupInputs();
        Manager.Instance.EffectsManagement.AnimateExposureByPostProcess(0, 0.15f);
        Manager.Instance.GameManagement.ClearPointerArrows();
        CurrentPowerup = Powerup.None;
        PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableSwaps, AvailableSwaps + 1);
        Manager.Instance.GameManagement.PlayConveyer();
        Manager.Instance.InputAllowedOnCups = true;
        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }

    public Color GetTrayColor(TrayMaterial Material)
    {
        string MaterialName = Material.ToString();
        MaterialName = MaterialName.Replace("Designer", "");
        foreach (var Color in TrayColors)
        {
            if (Color.trayMaterial.ToString() == MaterialName)
            {
                return Color.color;
            }
        }

        return Color.black;
    }

    public void IncrementPowerupCount(Powerup powerup)
    {
        if (powerup == Powerup.Swap) PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableSwaps, AvailableSwaps + 1);
        else if (powerup == Powerup.Info) PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableInfo, AvailableInfo + 1);
        else if (powerup == Powerup.Shuffle) PlayerPrefs.SetInt(PlayerPrefsKeyManager.AvailableShuffle, AvailableShuffle + 1);

        Manager.Instance.UIManagement.UpdatePowerupLabels();
    }
}