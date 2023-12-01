using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    //public float distance = 25;
    public Rigidbody rb;
    public float aspectRatio = .5f;
    public float transformForwardRatio = 2;
    public float velocityRatio = 2;
    public float speedFactor = 2;
    public float targetFOV;
    public float stopFOV;
    public float FOVSpeedMult;
    public float maxFOV;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 flatForward = new Vector3(-rb.transform.forward.x, 1, -rb.transform.forward.z).normalized;
        //Camera.main.transform.localPosition = flatForward * distance;
        //Camera.main.transform.LookAt(rb.transform);
        Vector3 newPosition = new Vector3(
            rb.transform.forward.x * transformForwardRatio + rb.velocity.x * velocityRatio,
            0,
            (rb.transform.forward.z * transformForwardRatio + rb.velocity.z * velocityRatio)*aspectRatio);
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * speedFactor);
        targetFOV = Mathf.Clamp(stopFOV + rb.velocity.magnitude * FOVSpeedMult, stopFOV, maxFOV);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime);
    }
}
