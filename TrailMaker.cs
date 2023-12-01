using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailMaker : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Mesh mesh;
    public List<TrailPoint> points = new List<TrailPoint>();
    public int maxPoints = 50;
    public float emissionVelocity = .2f;
    public float distInterval = .5f;
    public float timeToLive = 1;
    public Vector3 scale = new Vector3(.5f,.5f,0f);
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
        vertices = new Vector3[maxPoints * 4];
        indices = new int[maxPoints * 24];
        uvs = new Vector2[maxPoints * 4];
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

	public void FixedUpdate()
	{
        Emit();
        Redraw();
        lastPosition = currentPosition;
        lastRotation = transform.rotation;
    }

	void Emit()
    {
		currentPosition = TerrainScript.carPosition + transform.position;
        float angularSpeed = Quaternion.Angle(transform.rotation, lastRotation);
        bool localEmit = emit && angularSpeed > emissionVelocity;
        //timeToLive = (angularSpeed / emissionVelocity) * .5f;
        if (points.Count >= 1)
        {
            points[points.Count - 1].ConnectWithNext(localEmit);
        }

        if (Vector3.SqrMagnitude(lastEmitPoint - currentPosition) > distInterval * distInterval)
        {
            //draw new point
            lastEmitPoint = currentPosition;
            points.Add(new TrailPoint(Time.time, timeToLive, transform, scale, localEmit));
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
            return;
        }

        int ind = 0;
        for (int i = 0; i < points.Count; i++)
        {
            vertices[i * 4 + 0] = transform.InverseTransformPoint(points[i].point1);
            vertices[i * 4 + 1] = transform.InverseTransformPoint(points[i].point2);
            vertices[i * 4 + 2] = transform.InverseTransformPoint(points[i].point3);
            vertices[i * 4 + 3] = transform.InverseTransformPoint(points[i].point4);

            float opacity = (Time.time - points[i].time) / points[i].timeToLive;
            uvs[i * 4 + 0] = new Vector2(opacity, 0);
            uvs[i * 4 + 1] = new Vector2(opacity, .5f);
            uvs[i * 4 + 2] = new Vector2(opacity, 1f);
            uvs[i * 4 + 3] = new Vector2(opacity, .5f);

            if (i > 0 && points[i-1].connectWithNext)
            {
                //bottom
                indices[ind++] = i * 4 + 0;
                indices[ind++] = i * 4 + 1;
                indices[ind++] = (i - 1) * 4;
                indices[ind++] = i * 4 + 1;
                indices[ind++] = (i - 1) * 4+1;
                indices[ind++] = (i - 1) * 4;

                //left
                indices[ind++] = i * 4 + 1;
                indices[ind++] = i * 4 + 2;
                indices[ind++] = (i - 1) * 4+1;
                indices[ind++] = i * 4 + 2;
                indices[ind++] = (i - 1) * 4 + 2;
                indices[ind++] = (i - 1) * 4+1;

                //top
                indices[ind++] = i * 4 + 2;
                indices[ind++] = i * 4 + 3;
                indices[ind++] = (i - 1) * 4 + 2;
                indices[ind++] = i * 4 + 3;
                indices[ind++] = (i - 1) * 4 + 3;
                indices[ind++] = (i - 1) * 4 + 2;

                //right
                indices[ind++] = i * 4 + 3;
                indices[ind++] = i * 4;
                indices[ind++] = (i - 1) * 4 + 3;
                indices[ind++] = i * 4;
                indices[ind++] = (i - 1) * 4;
                indices[ind++] = (i - 1) * 4 + 3;
            }
        }
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, 0,ind, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
    }
}

[System.Serializable]
public struct TrailPoint
{
    public float time;
    public float timeToLive;
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;
    public Vector3 point4;
    public Vector3 carPos;
    public bool connectWithNext;

    public TrailPoint(float time, float ttl, Transform t, Vector3 scale, bool connectWithNext)
	{
        this.time = time;
        this.timeToLive = ttl;
        this.connectWithNext = connectWithNext;
        point1 = t.TransformPoint(new Vector3(-1 * scale.x, 1 * scale.y, 1 * scale.z));
        point2 = t.TransformPoint(new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
        point3 = t.TransformPoint(new Vector3(1 * scale.x, -1 * scale.y, -1 * scale.z));
        point4 = t.TransformPoint(new Vector3(-1 * scale.x, -1 * scale.y, -1 * scale.z));
        carPos = TerrainScript.carPosition;
    }

    public void ConnectWithNext(bool connect)
	{
        connectWithNext = connect;
	}
}