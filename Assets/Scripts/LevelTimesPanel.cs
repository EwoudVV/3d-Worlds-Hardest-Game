using UnityEngine;
using TMPro;
using System.IO;

public class LevelTimesPanel : MonoBehaviour
{
    public TMP_Text listText;
    public TMP_SpriteAsset goldMedalAsset;
    public TMP_SpriteAsset blueMedalAsset;
    public TMP_SpriteAsset redMedalAsset;
    public string goldSpriteName = "gold";
    public string blueSpriteName = "blue";
    public string redSpriteName = "red";
    private string levelTimesFile;
    private string sessionTotalFile;

    void OnEnable()
    {
        levelTimesFile = Application.persistentDataPath + "/LevelTimes.txt";
        sessionTotalFile = Application.persistentDataPath + "/SessionTotal.txt";

        if (listText != null && goldMedalAsset != null)
        {
            listText.spriteAsset = goldMedalAsset;
        }

        string output = "";
        if (File.Exists(levelTimesFile))
        {
            string[] lines = File.ReadAllLines(levelTimesFile);
            int levelIndex = 1;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string[] parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    string time = parts[1].Trim();
                    string medal = parts[2].Trim();
                    string medalTag = "";
                    if (medal == "Gold")
                    {
                        medalTag = "<sprite name=\"" + goldSpriteName + "\">";
                    }
                    else if (medal == "Blue")
                    {
                        medalTag = "<sprite name=\"" + blueSpriteName + "\">";
                    }
                    else if (medal == "Red")
                    {
                        medalTag = "<sprite name=\"" + redSpriteName + "\">";
                    }
                    output += levelIndex + ": " + time + " " + medalTag + "\n";
                    levelIndex++;
                }
            }
        }
        else
        {
            output = "-----\n";
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
        output += "\n+: " + formattedTotal;
        listText.text = output;
    }
}
