using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
public class LevelTimer : MonoBehaviour
{
    public TMP_Text currentLevelTimeText;
    public TMP_Text sessionTotalText;
    public Image goldDot;
    public Image blueDot;
    public Image redDot;
    public float goldTimeThreshold = 60f;
    public float silverMultiplier = 1.1f;
    public float bronzeMultiplier = 1.2f;
    private float elapsedTime;
    private bool levelFinished = false;
    private string levelTimesFile;
    private string sessionTotalFile;
    private float sessionTotalTime;
    void Start()
    {
        levelTimesFile = Application.persistentDataPath + "/LevelTimes.txt";
        sessionTotalFile = Application.persistentDataPath + "/SessionTotal.txt";
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (File.Exists(levelTimesFile)) File.Delete(levelTimesFile);
            if (File.Exists(sessionTotalFile)) File.Delete(sessionTotalFile);
            sessionTotalTime = 0f;
            File.WriteAllText(sessionTotalFile, sessionTotalTime.ToString());
        }
        else
        {
            if (File.Exists(sessionTotalFile))
            {
                float.TryParse(File.ReadAllText(sessionTotalFile), out sessionTotalTime);
            }
            else
            {
                sessionTotalTime = 0f;
            }
        }
    }
    void Update()
    {
        if (!levelFinished)
        {
            elapsedTime += Time.deltaTime;
            UpdateDisplays();
        }
    }
    void UpdateDisplays()
    {
        int m = (int)(elapsedTime / 60);
        int s = (int)(elapsedTime % 60);
        int ms = (int)((elapsedTime * 1000) % 1000);
        string formatted = string.Format("{0:00}:{1:00}:{2:000}", m, s, ms);
        if (currentLevelTimeText != null) currentLevelTimeText.text = formatted;
        float total = sessionTotalTime + elapsedTime;
        int tm = (int)(total / 60);
        int ts = (int)(total % 60);
        int tms = (int)((total * 1000) % 1000);
        string formattedTotal = string.Format("{0:00}:{1:00}:{2:000}", tm, ts, tms);
        if (sessionTotalText != null) sessionTotalText.text = formattedTotal;
        float silverThreshold = goldTimeThreshold * silverMultiplier;
        float bronzeThreshold = goldTimeThreshold * bronzeMultiplier;
        if (elapsedTime <= goldTimeThreshold) SetMedals(true, true, true);
        else if (elapsedTime <= silverThreshold) SetMedals(false, true, true);
        else if (elapsedTime <= bronzeThreshold) SetMedals(false, false, true);
        else SetMedals(false, false, false);
    }
    public void FinishLevel()
    {
        levelFinished = true;
        float silverThreshold = goldTimeThreshold * silverMultiplier;
        float bronzeThreshold = goldTimeThreshold * bronzeMultiplier;
        string medal = "";
        if (elapsedTime <= goldTimeThreshold)
        {
            medal = "Gold";
            SetMedals(true, true, true);
        }
        else if (elapsedTime <= silverThreshold)
        {
            medal = "Blue";
            SetMedals(false, true, true);
        }
        else if (elapsedTime <= bronzeThreshold)
        {
            medal = "Red";
            SetMedals(false, false, true);
        }
        else
        {
            medal = "None";
            SetMedals(false, false, false);
        }
        sessionTotalTime += elapsedTime;
        File.WriteAllText(sessionTotalFile, sessionTotalTime.ToString());
        SaveLevelTime(medal);
    }
    void SetMedals(bool showGold, bool showBlue, bool showRed)
    {
        if (goldDot != null) goldDot.enabled = showGold;
        if (blueDot != null) blueDot.enabled = showBlue;
        if (redDot != null) redDot.enabled = showRed;
    }
    void SaveLevelTime(string medal)
    {
        string levelName = SceneManager.GetActiveScene().name;
        int m = (int)(elapsedTime / 60);
        int s = (int)(elapsedTime % 60);
        int ms = (int)((elapsedTime * 1000) % 1000);
        string formatted = string.Format("{0:00}:{1:00}:{2:000}", m, s, ms);
        string entry = levelName + "," + formatted + "," + medal + "\n";
        File.AppendAllText(levelTimesFile, entry);
    }
}
