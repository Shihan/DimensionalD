using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TachymeterControl : MonoBehaviour
{
    public RectTransform needle;
    public TMP_Text speedText;
    public float maxSpeed = 180;
    float zeroPoint = 160;
    float maxPoint = 240;
    // Start is called before the first frame update

    public void UpdateSpeed(float speed)
	{
        speedText.text = speed.ToString("F0").PadLeft(3,'0');
        float rotation = zeroPoint - (speed / maxSpeed) * maxPoint;
        needle.rotation = Quaternion.Euler(0, 0, rotation);
	}
}
