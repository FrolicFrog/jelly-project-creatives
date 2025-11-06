using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DevManager : MonoBehaviour
{
    [System.Serializable]
    public class TestTimes
    {
        public int LevelNumber;
        public float TimeInSeconds;
        public float SuccessRate;
        public int MovesCount;

        public string LevelName => $"Lvl {LevelNumber}";
        public string TimeInMinutes => $"{(int)(TimeInSeconds / 60)}:{(int)(TimeInSeconds % 60):00}";

        public TestTimes(int levelNumber, float timeInSeconds, float successRate, int movesCount)
        {
            LevelNumber = levelNumber;
            TimeInSeconds = timeInSeconds;
            SuccessRate = successRate;
            MovesCount = movesCount;
        }
    }

    [System.Serializable]
    public class TestTimesList
    {
        public List<TestTimes> allTestTimes = new List<TestTimes>();
    }

    private bool LevelTimerStarted = false;
    private float LevelTimer = 0f;
    private TestTimesList savedTimes = new();
    private string SavePath => Path.Combine(Application.persistentDataPath, "leveltimes.json");

    void Start()
    {
        LoadTestTimes();
        Manager.Instance.OnLevelCompleted += CreateDevLevelProgressData;
    }

    void Update()
    {
        if (LevelTimerStarted)
        {
            LevelTimer += Time.deltaTime;
        }
    }

    public void CreateDevLevelProgressData()
    {
        if (!LevelTimerStarted) return;

        LevelTimerStarted = false;
        int CurrentLevel = Manager.Instance.LevelManagement.CurrentLevel;

        int MovesCount = Manager.Instance.GameManagement.MovesCount;
        int SuccessRate = Manager.Instance.GameManagement.SuccessRate;

        TestTimes CurrentLevelTestTimes = new TestTimes(CurrentLevel,LevelTimer,SuccessRate,MovesCount);

        foreach (TestTimes testTime in savedTimes.allTestTimes)
        {
            if (testTime.LevelNumber == CurrentLevel)
            {
                testTime.TimeInSeconds = LevelTimer;
                return;
            }
        }

        savedTimes.allTestTimes.Add(CurrentLevelTestTimes);
        SaveTestTimes();
    }

    public void StartLevelTimer()
    {
        if (LevelTimerStarted) return;
        LevelTimerStarted = true;
        LevelTimer = 0f;
    }

    void SaveTestTimes()
    {
        string json = JsonUtility.ToJson(savedTimes, true);
        File.WriteAllText(SavePath, json);
    }

    void LoadTestTimes()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            savedTimes = JsonUtility.FromJson<TestTimesList>(json);
        }
        else
        {
            savedTimes = new TestTimesList();
        }

        Manager.Instance.UIManagement.UpdateDevLevelProgress(savedTimes.allTestTimes);
    }
}
