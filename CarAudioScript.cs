using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAudioScript : MonoBehaviour
{
    public GameManager gameManager;
    public CarScript2 car;
    public AudioSource carEngine;
    public AudioSource carSkid;
    public AudioClip engineHigh;
    public AudioClip[] skids;
    public float pitchDelta = .1f;
    public AnimationCurve pitchPerRpm;
    int gear;
    bool changingGear = false;
    bool skidding = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void ChangeClip(AudioClip clip)
    {
    }

 //   IEnumerator ChangeGear()
	//{
 //       changingGear = true;
 //       AudioClip clip = gear > car.currentGear ? engineDown : engineUp;
 //       ChangeClip(clip);

 //       float time = clip.length;
	//	while (time > 0)
	//	{
 //           time -= Time.deltaTime;
 //           yield return null;
 //       }
 //       gear = car.currentGear;
 //       changingGear = false;
 //       ChangeClip(gear == 0? engineRest:engineHigh);
 //       yield return null;
 //   }
    // Update is called once per frame
    void Update()
    {
        if(gameManager.wheelsDrifting>0 && !skidding)
		{
            skidding = true;
            carSkid.clip = skids[Random.Range(0,skids.Length)];
            carSkid.Play();

        }
        carSkid.volume = .8f + gameManager.wheelsDrifting * .05f;
        if(gameManager.wheelsDrifting == 0)
		{
            skidding = false;
		}

        if (gear != car.currentGear && gear<=1)
		{
            gear = car.currentGear;
        }
		if (!changingGear)
        {
            float pitch = pitchPerRpm.Evaluate(car.currentRpm)-gear * .15f;
            carEngine.pitch = pitch;
            carEngine.volume = Mathf.Clamp(car.speed / 20f, 0.3f, .6f);
        }
        
    }
}
