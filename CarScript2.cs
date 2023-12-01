using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarScript2 : MonoBehaviour
{
    GameManager gameManager;
    public Camera cam;
    public Rigidbody rb;
    public float motorForce = 10000;
    public float speedPerGear = 6;
    public float speed;
    public int currentGear = 0;
    public float currentRpm = 0;
    public float steeringForce = 10000;
    public float steeringAngle = 0;
    public float steeringSpeed = 2;
    public AnimationCurve steeringMaxAngle;
    public float baseDrag = 0.25f;
    public float angularDragPerWheel = .25f;

    public TMP_Text velocityText;

    public List<AxleInfo2> axleInfos;

    public TrailMaker[] backlightTrails;
    public bool carColliding = false;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
    }

	private void FixedUpdate()
    {
        speed = rb.velocity.magnitude;
        float direction = Vector3.Dot(rb.velocity.normalized, transform.forward);

        float wheelForce = motorForce * Mathf.Clamp(Input.GetAxis("Vertical"),-.5f,1f);
        currentGear = Mathf.Clamp(Mathf.FloorToInt((speed - 1) / speedPerGear),0,6);
        currentRpm = speed / speedPerGear - currentGear;
        Vector3 worldForward = transform.TransformVector(Vector3.forward);
        worldForward.Scale(new Vector3(1, 0.01f, 1));
        worldForward = worldForward.normalized * wheelForce;

        float wheelsOnGround = 0;
        if(GameManager.gameState == GameState.Running)
		{
            if (Mathf.Abs(steeringAngle) < 1f) steeringAngle = 0;
            steeringAngle = Mathf.Clamp(steeringAngle + Input.GetAxis("Horizontal") * steeringSpeed, -steeringMaxAngle.Evaluate(speed), steeringMaxAngle.Evaluate(speed));
            if (Input.GetAxis("Horizontal") == 0)
            {
                steeringAngle *= .5f;
            }
            foreach (AxleInfo2 axle in axleInfos)
            {
                Vector3 dragWheelForce = rb.velocity * -axle.dragPerWheel * (axle.normalDragPerWheel - (axle.normalDragPerWheel - 1) * Mathf.Abs(Vector3.Dot(rb.velocity.normalized, axle.leftWheelModel.transform.forward)));
                //print((Mathf.Abs(Vector3.Dot(rb.velocity.normalized, axle.leftWheelModel.transform.forward))));
                if (axle.leftWheel.hit)
                {
                    rb.AddForceAtPosition(dragWheelForce, axle.leftWheel.transform.position); //Drag
                    if (axle.motor) rb.AddForceAtPosition(worldForward, axle.leftWheel.transform.position); //Motor
                    if (axle.steering) rb.AddForceAtPosition(axle.leftWheel.transform.right * steeringAngle * speed * 10, axle.leftWheel.transform.position); //Steering
                    if (axle.steering) rb.AddRelativeTorque(new Vector3(0, steeringAngle * steeringForce * Mathf.Clamp01(speed / 6f) * direction, 0), ForceMode.Force);
                    wheelsOnGround++;
                }
                if (axle.rightWheel.hit)
                {
                    rb.AddForceAtPosition(dragWheelForce, axle.rightWheel.transform.position); //Drag
                    if (axle.motor) rb.AddForceAtPosition(worldForward, axle.rightWheel.transform.position); //Motor
                    if (axle.steering) rb.AddForceAtPosition(axle.rightWheel.transform.right * steeringAngle * speed * 10, axle.rightWheel.transform.position); //Steering
                    if (axle.steering) rb.AddRelativeTorque(new Vector3(0, steeringAngle * steeringForce * Mathf.Clamp01(speed / 6f) * direction, 0), ForceMode.Force);

                    wheelsOnGround++;
                }

                if (axle.steering)
                {
                    axle.leftWheelModel.localRotation = Quaternion.Euler(steeringAngle, 0, 90);
                    axle.rightWheelModel.localRotation = Quaternion.Euler(steeringAngle, 0, 90);
                }
            }
        }
        
        rb.drag = baseDrag * (2-Mathf.Abs(Vector3.Dot(rb.velocity.normalized,transform.forward)));
        rb.angularDrag = baseDrag + wheelsOnGround * angularDragPerWheel;

        //velocityText.text = (speed*3.6).ToString("F1")+"kmh";
        //angularVel.text = rb.angularVelocity.ToString("F2");

        foreach (TrailMaker trail in backlightTrails)
        {
            if (Vector3.Dot(trail.transform.forward, cam.transform.forward) < -0.0f)
            {
                trail.emit = true;
            }
			else
			{
                trail.emit = false;
            }
        }
        
    }

    private void OnCollisionStay(Collision collision)
    {
        carColliding = true;
        if (collision.collider.gameObject.layer == 8)
        {
            gameManager.proxMultiplier = false;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        carColliding = false;
    }

	private void OnTriggerStay(Collider other)
	{
        switch (other.gameObject.layer)
        {
            case 8: //The car proximity is touching a fence
                gameManager.proxMultiplier = true;
                break;
            case 9: //Got a PowerUp
                gameManager.PowerUp(other.transform);
                break;
            case 10: //Got an ending
                gameManager.Ending();
                break;
        }
	}
	//private void OnTriggerExit(Collider other)
	//{
 //       if (other.gameObject.layer == 8)
 //       {
 //           GameManager.proxMultiplier = false;
 //       }
 //   }
}

[System.Serializable]
public class AxleInfo2
{
    public WheelScript leftWheel;
    public WheelScript rightWheel;
    public Transform leftWheelModel;
    public Transform rightWheelModel;
    public bool motor;
    public bool steering;
    public bool brakes;
    public bool handbrake;
    public float dragPerWheel = .1f;
    public float normalDragPerWheel = 50f;
}