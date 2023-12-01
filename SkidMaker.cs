using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMaker : MonoBehaviour
{
    public MeshFilter meshFilter;
    Mesh mesh;
    public List<SkidPoint> points = new List<SkidPoint>();
    public int maxPoints = 50;
    public float distInterval = .5f;
    public float timeToLive = 1;
    public Vector3 scale = new Vector3(.5f, .5f, .5f);
    Vector3 lastPosition = Vector3.negativeInfinity;
    Vector3 currentPosition = Vector3.negativeInfinity;
    Vector3 lastEmitPoint = Vector3.negativeInfinity;
    Quaternion lastRotation = Quaternion.identity;
    public bool emit = false;

    Vector3[] vertices;
    int[] indices;
    Vector2[] uvs;

    // Start is called before the first frame update
    void Start()
    {
        vertices = new Vector3[maxPoints * 3];
        indices = new int[maxPoints * 6];
        uvs = new Vector2[maxPoints * 3];
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    public void FixedUpdate()
    {
        
        Emit();
        lastPosition = currentPosition;
        lastRotation = transform.rotation;
        transform.localScale = 0.5f*(Vector3.one + TerrainScript.currentScale);
    }

    public void Update()
    {
        Redraw();
    }

	void Emit()
    {
        currentPosition = TerrainScript.carPosition + transform.position;
        if (points.Count >= 1)
        {
            points[points.Count - 1].ConnectWithNext(emit);
        }

        if (Vector3.SqrMagnitude(lastEmitPoint - currentPosition) > distInterval * distInterval)
        {
            //if too much points
            if (points.Count >= maxPoints - 1)
            {
                points.RemoveAt(0);
            }

            //draw new point
            lastEmitPoint = currentPosition;			
            points.Add(new SkidPoint(Time.time, timeToLive, transform, scale, emit));
        }
        //check points age
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (points[i].time + points[i].timeToLive < Time.time)
            {
                points.RemoveAt(i);
            }
        }
    }

    void Redraw()
    {
        if (points.Count <= 1)
        {
            mesh.Clear();
            return;
        }
        int ind = 0; //print(TerrainScript.carPosition - points[0].carPos);
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pos1 = transform.InverseTransformPoint(TerrainScript.carPosition - points[i].point1);
            Vector3 pos2 = transform.InverseTransformPoint(TerrainScript.carPosition - points[i].point2);
            vertices[i * 2 + 0] = pos1;// new Vector3(pos1.x * TerrainScript.currentScale.x,pos1.y * TerrainScript.currentScale.y, pos1.z * TerrainScript.currentScale.z);
            vertices[i * 2 + 1] = pos2;// new Vector3(pos2.x * TerrainScript.currentScale.x, pos2.y * TerrainScript.currentScale.y, pos2.z * TerrainScript.currentScale.z);

            //vertices[i * 4 + 2] = transform.InverseTransformPoint(points[i].point3 + (TerrainScript.carPosition - points[i].carPos));
            //vertices[i * 4 + 3] = transform.InverseTransformPoint(points[i].point4 + (TerrainScript.carPosition - points[i].carPos));

            float opacity = (Time.time - points[i].time) / points[i].timeToLive;
            uvs[i * 2 + 0] = new Vector2(opacity, 0);
            uvs[i * 2 + 1] = new Vector2(opacity, .5f);
            //uvs[i * 4 + 2] = new Vector2(opacity, .10f);
            //uvs[i * 4 + 3] = new Vector2(opacity, .15f);

            if (i > 0 && points[i - 1].connectWithNext)
            {
				////bottom
				indices[ind++] = i * 2 + 0;
				indices[ind++] = i * 2 + 1;
				indices[ind++] = (i - 1) * 2;
				indices[ind++] = i * 2 + 1;
				indices[ind++] = (i - 1) * 2 + 1;
				indices[ind++] = (i - 1) * 2;

				//left
				//indices[ind++] = i * 4 + 1;
				//indices[ind++] = i * 4 + 2;
				//indices[ind++] = (i - 1) * 4 + 1;
				//indices[ind++] = i * 4 + 2;
				//indices[ind++] = (i - 1) * 4 + 2;
				//indices[ind++] = (i - 1) * 4 + 1;

				////top
				//indices[ind++] = i * 4 + 2;
				//indices[ind++] = i * 4 + 3;
				//indices[ind++] = (i - 1) * 4 + 2;
				//indices[ind++] = i * 4 + 3;
				//indices[ind++] = (i - 1) * 4 + 3;
				//indices[ind++] = (i - 1) * 4 + 2;

				////right
				//indices[ind++] = i * 4 + 3;
				//indices[ind++] = i * 4;
				//indices[ind++] = (i - 1) * 4 + 3;
				//indices[ind++] = i * 4;
				//indices[ind++] = (i - 1) * 4;
				//indices[ind++] = (i - 1) * 4 + 3;
			}
        }
        mesh.Clear();
        mesh.SetVertices(vertices);//,0,points.Count*2);
        mesh.SetUVs(0, uvs);///, 0, points.Count * 2);
        mesh.SetIndices(indices, 0,ind, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
    }
}


[System.Serializable]
public struct SkidPoint
{
    public float time;
    public float timeToLive;
    public Vector3 point1;
    public Vector3 point2;
    public bool connectWithNext;

    public SkidPoint(float time, float ttl, Transform t, Vector3 scale, bool connectWithNext)
    {
        this.time = time;
        this.timeToLive = ttl;
        this.connectWithNext = connectWithNext;
        point1 = TerrainScript.carPosition + t.TransformPoint(new Vector3(0 * scale.x, 1 * scale.y, -1 * scale.z));
        point2 = TerrainScript.carPosition + t.TransformPoint(new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
    }

    public void ConnectWithNext(bool connect)
    {
        connectWithNext = connect;
    }
}