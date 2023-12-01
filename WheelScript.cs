using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelScript : MonoBehaviour
{
    public Rigidbody rb;
    public bool hit;
    public bool skidding = false;
    public SkidMaker skid;
    public ParticleSystem smoke;
    public float skidMinVelocity = 1;
    ParticleSystem.VelocityOverLifetimeModule velocityModule;
    ParticleSystem.EmissionModule emissionModule;
    // Start is called before the first frame update
    void Start()
    {
        velocityModule = smoke.velocityOverLifetime;
        velocityModule.radial = new ParticleSystem.MinMaxCurve(.1f);
        emissionModule = smoke.emission;
        //velocityModule.space = ParticleSystemSimulationSpace.World;
    }

	private void OnCollisionStay(Collision collision)
	{
        hit = true;
        float dot = Vector3.Dot(rb.velocity.normalized, transform.forward);
        skidding = (Mathf.Abs(dot) < .8f && rb.velocity.sqrMagnitude > skidMinVelocity * skidMinVelocity);
        skid.emit = skidding;
    }

    private void OnCollisionExit(Collision collision)
    {
        hit = false;
        skidding = false;
        skid.emit = false;
    }

    // Update is called once per frame
    void Update()
    {
        velocityModule.x = .1f;
        velocityModule.y = -rb.velocity.magnitude/3f;
        velocityModule.z = Mathf.Sign(transform.localPosition.x)/10f;
        //smoke.velocityOverLifetime = velocityModule;

        emissionModule.rateOverTimeMultiplier = skid.emit?30:0;        
    }
}
