using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aCrossSceneManager : MonoBehaviour
{
    public Toggle qualityToggle;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeQuality(bool high)
	{
        print(qualityToggle.isOn);
        QualitySettings.SetQualityLevel(qualityToggle.isOn? 1:0, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
