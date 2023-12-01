using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileData data;
    public MeshFilter meshFilter_Road;
    public MeshCollider meshCollider_Road;
    public MeshFilter meshFilter_Fences;
    public MeshCollider meshCollider_Fences;
    public MeshFilter meshFilter_Markings;
    public List<PowerUp> powerups = new List<PowerUp>();

    public void SetData(TileData newData)
	{
        data = newData;
        data.ChangeOrientation(newData.orientation);
        transform.localRotation = Quaternion.Euler(0, data.orientation * 60, 0);

        meshFilter_Road.sharedMesh = data.scriptableTile.roadMesh;
        meshCollider_Road.sharedMesh = data.scriptableTile.roadMesh;
        meshFilter_Fences.sharedMesh = data.scriptableTile.fencesMesh;
        meshCollider_Fences.sharedMesh = data.scriptableTile.fencesMesh;
        if(data.scriptableTile.markingsMesh != null)
        {
            meshFilter_Markings.sharedMesh = data.scriptableTile.markingsMesh;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for(int i = powerups.Count-1;i>=0;i--)
		{
            if(powerups[i].t == null)
			{
                powerups.RemoveAt(i);
                continue;
			}
			powerups[i].t.localScale = new Vector3(
                powerups[i].scale.x / TerrainScript.currentScale.x,
                powerups[i].scale.y / TerrainScript.currentScale.y,
                powerups[i].scale.z / TerrainScript.currentScale.z
            );
		}
    }
}

[System.Serializable]
public class TileData
{
    public ScriptableTile scriptableTile;
    public int orientation = 0;
    public int[] connections;

    public TileData(ScriptableTile tileType, int orientation)
	{
        this.scriptableTile = tileType;
        this.orientation = orientation;
        ChangeOrientation(orientation);

    }
    public void ChangeOrientation(int orientation)
    {
        this.orientation = orientation;
        connections = new int[6];
        for (int i = 0; i < 6; i++)
        {
            connections[(i + orientation) % 6] = this.scriptableTile.connections[i];
        }
    }
	public override string ToString()
	{
		return scriptableTile.name + ":"+orientation;
	}
}


[System.Serializable]
public class PowerUp
{
    public Transform t;
    public Vector3 scale;

    public PowerUp(Transform t, Vector3 scale)
	{
        this.t = t;
        this.scale = scale;
	}
}