using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
public class LevelTimesPanel : MonoBehaviour {
    public Transform entriesContainer;
    public GameObject entryPrefab;
    public TMP_Text totalTimeText;
    private string levelTimesFile;
    private string sessionTotalFile;
    void OnEnable() {
        levelTimesFile = Application.persistentDataPath + "/LevelTimes.txt";
        sessionTotalFile = Application.persistentDataPath + "/SessionTotal.txt";
        string[] lines = new string[0];
        if (File.Exists(levelTimesFile)) {
            lines = File.ReadAllLines(levelTimesFile);
        }
        foreach (Transform child in entriesContainer) {
            Destroy(child.gameObject);
        }
        foreach (string line in lines) {
            if (line.Trim() == "") continue;
            string[] parts = line.Split(',');
            if (parts.Length >= 3) {
                GameObject entry = Instantiate(entryPrefab, entriesContainer);
                LevelRecordEntry record = entry.GetComponent<LevelRecordEntry>();
                if (record != null) {
                    record.SetData(parts[0], parts[1], parts[2]);
                }
            }
        }
        float total = 0f;
        if (File.Exists(sessionTotalFile)) {
            float.TryParse(File.ReadAllText(sessionTotalFile), out total);
        }
        int tm = (int)(total / 60);
        int ts = (int)(total % 60);
        int tms = (int)((total * 1000) % 1000);
        string formattedTotal = string.Format("{0:00}:{1:00}:{2:000}", tm, ts, tms);
        if (totalTimeText != null) {
            totalTimeText.text = "Total Time: " + formattedTotal;
        }
    }
}
