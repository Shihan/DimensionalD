using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TerrainMenuScript : MonoBehaviour
{
    public CanvasGroup button;
    public CanvasGroup title;
    public CanvasGroup fade;
    public Rigidbody car;
    public GameObject terrain1;
    public GameObject terrain2;
    public float speed = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Go()
	{
        StartCoroutine("Accelerate");
    }
    public IEnumerator Accelerate()
	{
        float time = 0;

        while (time < 2)
        {
            fade.alpha = Mathf.Clamp(time-1, 0, 1);
            title.alpha = Mathf.Clamp(1 - time, 0, 1);
            button.alpha = Mathf.Clamp(1 - time, 0, 1);
            car.AddForce(-Vector3.forward * 10000);
            time += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadSceneAsync("Driving", LoadSceneMode.Single);
        yield return 0; // wait a frame, so it can finish loading
        while (SceneManager.loadedSceneCount == 1)
        {
            car.AddForce(-Vector3.forward * 5000);
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Driving"));
        yield return null;
	}

    // Update is called once per frame
    void Update()
    {
        terrain1.transform.Translate(Vector3.forward * Time.deltaTime*speed);
        terrain2.transform.Translate(Vector3.forward * Time.deltaTime*speed);
        if (terrain1.transform.position.z > 100) terrain1.transform.Translate(Vector3.back * 129.9038f*2);
        if (terrain2.transform.position.z > 100) terrain2.transform.Translate(Vector3.back * 129.9038f*2);
    }
}
