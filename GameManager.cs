using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//TODO
//FAIT              //1 - physique de la voiture + fun: mode conduite/mode drift
//FAIT              //2 - points drift
//FAIT              //3 - boost scaling terrain
//4 - musique/effets son
//FAIT            //5 - réparation des dérapages
//FAIT              //6 - décors sapins
//7 - interface
//FAIT              //8 - écran démarrage/fin
//9 - fin feu d'artifice sur le lac, écran "you are the drift king of the japanese mountain", feu d'artifice
//FAIT              //10 - effets lumière
public class GameManager : MonoBehaviour
{
    public TachymeterControl tach;
    public static GameState gameState = GameState.Running;
    public static GameManager instance;
    public CarScript2 carScript;
    public WheelScript[] wheels;
    public Rigidbody car;
    public TerrainScript terrain;

    public float timeToRespawn = 2.5f;
    float respawnCountdown = 2.5f;
    public GameObject respawnText;
    public TMP_Text countdownText;
    public TMP_Text fps;
    public TMP_Text timer;
    float raceTime = 0;
    bool respawnCar = false;

    public Slider nitroCounter;
    public float nitroIncreasePerPoint = 0.0005f;
    public float nitroValue = 0.0f;

    public float timeDrifting = 0;
    public float timeWithoutDrifting = 0;
    public int wheelsDrifting = 0;
    public TMP_Text drifting_Text;
    public TMP_Text drifting_Points;
    public string[] driftingDesc;
    public int driftingDescLevels = 500;

    public float driftingTextDisplayTime = .5f;

    public bool proxMultiplier = false;
    public int proxMultiplierValue = 2;
    public float tempScore = 0;
    public float totalScore = 0;
    public TMP_Text score_Points;
    public Material powerupMaterial;
    public AudioSource powerupSound;
    public Image startScreen;
    public Image endScreen;
    public TMP_Text endScore;
    public TMP_Text endTime;

	private void Awake()
    {
        gameState = GameState.Preparing;
        fps.gameObject.SetActive(false);
        startScreen.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        instance = this;
    }
	// Start is called before the first frame update
	void Start()
    {
        StartCoroutine("StartRace");
        StartCoroutine("UpdateInterface");
    }
    private IEnumerator StartRace()
    {
        countdownText.CrossFadeAlpha(0.0f, 0f, true);
        yield return new WaitForSeconds(1.0f);

        int time = 4;
        startScreen.CrossFadeAlpha(0.0f, 1, true);
        countdownText.CrossFadeAlpha(1.0f, .5f, true);

        while (time >0)
        {
            countdownText.text = time.ToString();
            time--;
            yield return new WaitForSeconds(1.0f);
        }
        gameState = GameState.Running;
        countdownText.text = "GO!";
        countdownText.CrossFadeAlpha(0.0f, 1, true);
    }

	private void FixedUpdate()
    {
        if (respawnCar)
        {
            //rb.position += Vector3.up;
            //car.MoveRotation(Quaternion.identity);
            //terrain.CenterToCurrentTile();
            respawnCar = false;
        }
    }

    private IEnumerator UpdateInterface()
	{
		while (Application.isPlaying)
        {
            fps.text = (1 / Time.smoothDeltaTime).ToString("F1");
            nitroCounter.value = Mathf.Lerp(nitroCounter.value, nitroValue, .25f);
            yield return new WaitForSecondsRealtime(.03f);
            timer.text = GetFormattedTime();
            tach.UpdateSpeed(carScript.speed * 5);
        }
    }

    string GetFormattedTime()
	{
        int mins = Mathf.FloorToInt(raceTime / 60);
        int secs = Mathf.FloorToInt(raceTime % 60);
        float dec = Mathf.FloorToInt((raceTime % 1) * 100);
        string s = mins.ToString() + ":" + secs.ToString().PadLeft(2, '0') + "." + dec.ToString().PadLeft(2, '0');
        return s;
    }

    public void PowerUp(Transform powerup)
	{
        if (nitroValue < .5f) return;

        Vector3 scale = Vector3.zero;
		switch (powerup.tag)
		{
            case "X":
                scale = Vector3.right;
                break;
            case "Y":
                scale = Vector3.up;
                break;
            case "Z":
                scale = Vector3.forward;
                break;
		}
        nitroValue -= 0.5f;
        terrain.scaleVector -= scale;
        Destroy(powerup.gameObject);
        powerupSound.Play();

    }

    public void Reload()
	{
        SceneManager.LoadScene("MenuScreen");
	}

    public void Ending()
	{
        if (gameState == GameState.Ending) return;
        endScreen.gameObject.SetActive(true);
        endScreen.CrossFadeAlpha(0.0f,0,true);
        endScore.text = Mathf.RoundToInt(totalScore).ToString();
        endTime.text = GetFormattedTime();
        gameState = GameState.Ending;
    }

    //public static void Drifted()
    //{
    //    driftValue += driftIncreasePerWheel;
    //    wheelsDrifting++;
    //}

    // Update is called once per frame
    void Update()
    {
        
        switch(gameState)
		{
            case GameState.Running:
                raceTime += Time.deltaTime;
                break;
            case GameState.Ending:
            endScreen.CrossFadeAlpha(1.0f, 1.5f, true);
                break;

            //Time.timeScale = Mathf.Clamp(Time.timeScale - Time.unscaledDeltaTime * .5f, .75f, 1.0f);
		}
        powerupMaterial.SetFloat("nitro", nitroValue);
		//if (Input.GetKey(KeyCode.Space) && nitroValue >=.99f)
		//{
  //          terrain.scaleVector = Vector3.one;
  //          nitroValue = 0;
		//}
        wheelsDrifting = 0;
        foreach(WheelScript wheel in wheels)
		{
            wheelsDrifting += wheel.skidding ? 1 : 0;
		}
        
        //print(wheelsDrifting);
		if (carScript.carColliding)
		{
            respawnCountdown -= Time.deltaTime;
		}
		else
		{
            respawnCountdown = timeToRespawn;
        }
        respawnText.SetActive(respawnCountdown < 0);

		if (Input.GetKeyDown(KeyCode.F))
        {
            fps.gameObject.SetActive(!fps.gameObject.activeSelf);
		}
        if (Input.GetKeyDown(KeyCode.R) && respawnCountdown < 0)
        {
            respawnCountdown = timeToRespawn;
            terrain.respawnCar = true;
        }        

        //Drifting time calculation
        if (wheelsDrifting > 0 && car.velocity.sqrMagnitude>20)
        {
            timeDrifting += Time.deltaTime;
            tempScore += Mathf.Pow(wheelsDrifting, Mathf.Sqrt(timeDrifting * 2)) * (proxMultiplier?proxMultiplierValue:1);
            timeWithoutDrifting = 0;
        }
        else if (timeDrifting>0)
        {
            timeWithoutDrifting += Time.deltaTime;
        }

        //Drifting text display
        if (timeDrifting > driftingTextDisplayTime)
        {
            drifting_Text.gameObject.SetActive(true);
            drifting_Text.text = driftingDesc[Mathf.Clamp(Mathf.RoundToInt(tempScore/driftingDescLevels), 0, driftingDesc.Length - 1)];
            drifting_Points.text = timeDrifting.ToString("F1") + "s " + Mathf.RoundToInt(tempScore) + (proxMultiplier ? " x" + proxMultiplierValue : "   ");
        }
        if (timeWithoutDrifting > driftingTextDisplayTime *3)
        {
            timeDrifting = 0;
            totalScore += tempScore;
            nitroValue = Mathf.Clamp01(nitroValue + nitroIncreasePerPoint * tempScore);
            score_Points.text = Mathf.RoundToInt(totalScore).ToString().PadLeft(6,'0');
            tempScore = 0;
            proxMultiplier = false;
            drifting_Text.gameObject.SetActive(false);
        }
    }
}

public enum GameState
{
    Preparing,
    Running,
    Ending
}