using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMaker : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float trackLength = 400;
    public float legMinDistance = 10;
    public float legMaxDistance = 50;
    public float legMaxAngle = 180;
    public float legMinRaise = 1;
    public float legMaxRaise = 5;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> positions = new List<Vector3>();
        float distance = 0;
        Vector3 lastPoint = Vector3.zero;
        positions.Add(lastPoint);
        while (distance < trackLength)
        {
            float currentLegLength = Random.Range(legMinDistance, legMaxDistance);
            float currentAngle = Random.Range(-legMaxAngle, legMaxAngle) * Mathf.Deg2Rad; //Bell Curve
            Vector3 legDelta = new Vector3(currentLegLength * Mathf.Cos(currentAngle), Random.Range(legMinRaise, legMaxRaise), currentLegLength * Mathf.Sin(currentAngle));
            Vector3 currentPoint = lastPoint + legDelta;
            positions.Add(currentPoint);
            distance += currentLegLength;
            lastPoint = currentPoint;
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
