using UnityEngine;
using TMPro;
using System.IO;
public class LevelTimesPanel : MonoBehaviour
{
    public TMP_Text listText;
    private string levelTimesFile;
    private string sessionTotalFile;
    void OnEnable()
    {
        levelTimesFile = Application.persistentDataPath + "/LevelTimes.txt";
        sessionTotalFile = Application.persistentDataPath + "/SessionTotal.txt";
        string output = "";
        if (File.Exists(levelTimesFile))
        {
            string[] lines = File.ReadAllLines(levelTimesFile);
            foreach (string line in lines)
            {
                if (line.Trim() == "") continue;
                string[] parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    output += parts[0] + ": " + parts[1] + " - Medal: " + parts[2] + "\n";
                }
            }
        }
        else
        {
            output = "No level times recorded.\n";
        }
        float total = 0f;
        if (File.Exists(sessionTotalFile))
        {
            float.TryParse(File.ReadAllText(sessionTotalFile), out total);
        }
        int tm = (int)(total / 60);
        int ts = (int)(total % 60);
        int tms = (int)((total * 1000) % 1000);
        string formattedTotal = string.Format("{0:00}:{1:00}:{2:000}", tm, ts, tms);
        output += "\nTotal Time: " + formattedTotal;
        listText.text = output;
    }
}
