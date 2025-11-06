using TMPro;
using UnityEngine;

public class DevCard : MonoBehaviour
{
    public TextMeshProUGUI LevelName;
    public TextMeshProUGUI TimeTakenToComplete;
    public TextMeshProUGUI SuccessRate;
    public TextMeshProUGUI MovesCount;

    public void SetData(DevManager.TestTimes TestTimes)
    {
        LevelName.text = TestTimes.LevelName;
        TimeTakenToComplete.text = TestTimes.TimeInMinutes;
        SuccessRate.text = TestTimes.SuccessRate + "%";
        MovesCount.text = TestTimes.MovesCount.ToString();
    }
}
