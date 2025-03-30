using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LevelRecordEntry : MonoBehaviour
{
    public TMP_Text levelInfoText;
    public Image goldDot;
    public Image blueDot;
    public Image redDot;
    public void SetData(string levelName, string time, string medal)
    {
        levelInfoText.text = levelName + ": " + time;
        if (medal == "Gold")
        {
            if (goldDot != null) goldDot.enabled = true;
            if (blueDot != null) blueDot.enabled = true;
            if (redDot != null) redDot.enabled = true;
        }
        else if (medal == "Blue")
        {
            if (goldDot != null) goldDot.enabled = false;
            if (blueDot != null) blueDot.enabled = true;
            if (redDot != null) redDot.enabled = true;
        }
        else if (medal == "Red")
        {
            if (goldDot != null) goldDot.enabled = false;
            if (blueDot != null) blueDot.enabled = false;
            if (redDot != null) redDot.enabled = true;
        }
        else
        {
            if (goldDot != null) goldDot.enabled = false;
            if (blueDot != null) blueDot.enabled = false;
            if (redDot != null) redDot.enabled = false;
        }
    }
}
