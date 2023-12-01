using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarScript : MonoBehaviour
{
    public Slider torqueSlider;
    public Slider steeringSlider;
    public Transform terrain;
    public Transform terrainInner;
    public Transform scaleCube;
    public Rigidbody rb;
    public Transform carModel;
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque = 2000;
    public float motorTorqueChangeRate = 20;
    public float brakeTorque = 5000;
    public AnimationCurve maxSteeringAngle;
    public AnimationCurve steeringChangeRate;
    public TMP_Text speed;
    float steering = 0;
    float motor = 0;
    public float downForce = 10;
    public float differential = .5f;
    public Vector3 velocityVector = Vector3.zero;
    public float velocity = 0;

    public float shiftTime = 0;
    public Vector3 shiftVector = Vector3.one;

    // Start is called before the first frame update
    void Start()
    {
        //rb.centerOfMass += new Vector3(0, -0.20f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        torqueSlider.value = motor / (maxMotorTorque * 2.0f) + .5f;
        steeringSlider.value = steering / (MaxSteeringAngle(velocity) * 2.0f) + .5f;
        //terrain.position -= velocity * Time.deltaTime;
        //carModel.rotation = Quaternion.Euler(0, bearing, 0);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftTime += Time.deltaTime*2f;
        }
        else
        {
            shiftTime -= Time.deltaTime;
        }
        shiftTime = Mathf.Clamp(shiftTime, 1, 10);
        shiftVector = new Vector3(1, 1, 1 / shiftTime);
        terrain.localScale = shiftVector;
        scaleCube.localScale = shiftVector*1.5f;
    }

    public float MaxSteeringAngle(float speed)
	{
        return maxSteeringAngle.Evaluate(speed);
	}

    public void FixedUpdate()
    {

        if (Input.GetAxis("Vertical") != 0)
		{
            motor = Mathf.Clamp(motor + Input.GetAxis("Vertical") * motorTorqueChangeRate, maxMotorTorque  * - .75f, maxMotorTorque);
        }
		else
		{
            motor = Mathf.Clamp(motor - motor*.2f, maxMotorTorque * -.75f, maxMotorTorque);
        }
        if (Mathf.Abs(motor) < 1f)
        {
            motor = 0f;
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            steering = Mathf.Clamp(steering + Input.GetAxis("Horizontal") * steeringChangeRate.Evaluate(velocity), -MaxSteeringAngle(velocity), MaxSteeringAngle(velocity));
		}
		else
		{
            steering = Mathf.Clamp(steering - steering *.2f, -MaxSteeringAngle(velocity), MaxSteeringAngle(velocity));
        }
		if (Mathf.Abs(steering) < .1f)
		{
            steering = 0f;
        }
        bool braking = (Input.GetAxis("Vertical")<0);
        bool handbraking = (Input.GetKey(KeyCode.Space));
        //print(motor+ " "+steering);
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
                
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque =  Mathf.Clamp(motor * (1 + (steering / MaxSteeringAngle(velocity)) * differential), motor * .5f, motor);
                axleInfo.rightWheel.motorTorque = Mathf.Clamp(motor * (1 - (steering / MaxSteeringAngle(velocity)) * differential), motor * .5f, motor);
            }
            if ((braking && axleInfo.brakes) || (handbraking && axleInfo.handbrake))
            {
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
            }

            float travelL = 1.0f;
            float travelR = 1.0f;
            WheelHit hitL;
            WheelHit hitR;

            bool groundedL = axleInfo.leftWheel.GetGroundHit(out hitL);
            travelL = groundedL ? (-axleInfo.leftWheel.transform.InverseTransformPoint(hitL.point).y - axleInfo.leftWheel.radius) / axleInfo.leftWheel.suspensionDistance : 1.0f;
            bool groundedR = axleInfo.rightWheel.GetGroundHit(out hitR);
            travelR = groundedR ? (-axleInfo.rightWheel.transform.InverseTransformPoint(hitR.point).y - axleInfo.rightWheel.radius) / axleInfo.rightWheel.suspensionDistance : 1.0f;
            var antiRollForce = (travelL - travelR) * axleInfo.antiRoll;

            if (groundedL)
            {
                rb.AddForceAtPosition(axleInfo.leftWheel.transform.up * -antiRollForce, axleInfo.leftWheel.transform.position);
                axleInfo.leftSlipText.text = hitL.forwardSlip.ToString("F1") + " - " + hitL.sidewaysSlip.ToString("F1");
            }
            if (groundedR)
            {
                rb.AddForceAtPosition(axleInfo.rightWheel.transform.up * antiRollForce, axleInfo.rightWheel.transform.position);
                axleInfo.rightSlipText.text = hitR.forwardSlip.ToString("F1") + " - " + hitR.sidewaysSlip.ToString("F1");
            }

            axleInfo.leftWheel.GetWorldPose(out Vector3 Lpos, out Quaternion Lrot);
            axleInfo.leftWheelModel.position = Lpos;
            axleInfo.leftWheelModel.rotation = Lrot;

            axleInfo.rightWheel.GetWorldPose(out Vector3 Rpos, out Quaternion Rrot);
            axleInfo.rightWheelModel.position = Rpos;
            axleInfo.rightWheelModel.rotation = Rrot;
        }

        Vector3 downForceVector = -transform.up * rb.velocity.sqrMagnitude * downForce;
        rb.AddForceAtPosition(downForceVector, transform.position - transform.forward * .1f);

        //rb.MovePosition(Vector3.zero);
        speed.text = rb.velocity + " " + rb.velocity.magnitude.ToString("F1")+" " + (rb.velocity.magnitude*3.6).ToString("F1")+"KMH";
        if(Time.time>5 && Time.time < 5.1f)
		{
            //print((rb.velocity.magnitude * 3.6).ToString("F1") + "k/h");
		}
        //print(steering);
        velocityVector = rb.velocity;
        velocity = velocityVector.magnitude;

        //  With terrain movement
        terrainInner.localPosition -= new Vector3(rb.position.x / shiftVector.x, rb.position.y / shiftVector.y, rb.position.z / shiftVector.z);
        rb.position = Vector3.zero;

        //  With car movement
        //terrain.position = rb.position;
        //terrainInner.localPosition = -rb.position;
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public Transform leftWheelModel;
    public Transform rightWheelModel;
    public TMP_Text leftSlipText;
    public TMP_Text rightSlipText;
    public bool motor;
    public bool steering;
    public bool brakes;
    public bool handbrake;
    public TrailRenderer leftSkidmark;
    public TrailRenderer rightSkidmark;
    public float antiRoll = 5000;

}