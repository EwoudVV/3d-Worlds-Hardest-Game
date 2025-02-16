using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElapsedTimeDisplay : MonoBehaviour
{
    public TMP_Text tmpText;
    
    private float elapsedTime;

    void Update()
    {
        elapsedTime = Time.timeSinceLevelLoad;

        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);
        int milliseconds = (int)((elapsedTime * 1000) % 1000);

        string formattedTime = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

        if (tmpText != null)
        {
            tmpText.text = formattedTime;
        }
    }
}
