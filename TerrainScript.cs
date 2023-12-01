using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainScript : MonoBehaviour
{
    public int seed = 1;
    public Vector2Int terrainSize = new Vector2Int(10, 30);
    public Rect bounds;
    public static Vector3 carPosition;
    public GameObject tilePrefab;
    public GameObject propPrefab;
    public GameObject powerUpPrefab;
    public GameObject endingPrefab;
    public ScriptableTile[] tileTypes;
    public ScriptableTile[] specialTileTypes;
    public ScriptableTile tunnelEnding;
    public float tileSize = 25;
    public Dictionary<Vector2Int,Tile> tiles;
    public Transform terrain;
    public Transform terrainInner;
    public Transform scaleCube;
    Vector3 initialCubeScale;
    public Rigidbody rb;
    public Mesh[] propMeshes;

    public Material redMat;
    public Material whiteMat;

    public static Vector3 movementDelta = Vector3.zero;

    public float rescaleInterval = 5f;
    float scaleTime = 0;
    public Vector3 previousScaleVector = new Vector3(.5f,.5f,.5f);
    public Vector3 scaleVector = new Vector3(.5f, .5f, .5f);
    public static Vector3 currentScale = Vector3.one;
    public float scaleIncrease = .5f;

    float SIN60 = Mathf.Sin(Mathf.PI / 3);
    public Vector2Int currentTileCoords = new Vector2Int(-100,-100);
    public bool respawnCar = true;
    public bool occlusion;
    public float occlusionDistance = 2500;
    public Vector3 occlusionOffset = new Vector3(25, 0, 0);

    public TMP_Text X_Scale;
    public TMP_Text Y_Scale;
    public TMP_Text Z_Scale;

    //        0
    //     ---------
    //    /         \
    //  5/           \1
    //  /             \
    //  \             /
    //  4\           /2
    //    \         /
    //     ---------
    //        3

    Vector2Int[] coordsOffsetEven = new Vector2Int[]
            {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(1,-1),
            new Vector2Int(0,-1),
            new Vector2Int(-1,-1),
            new Vector2Int(-1,0)
            };
    Vector2Int[] coordsOffsetOdd = new Vector2Int[]
            {
            new Vector2Int(0,1),
            new Vector2Int(1,1),
            new Vector2Int(1,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1,0),
            new Vector2Int(-1,1)
            };

    // Start is called before the first frame update
    void Start()
    {
        GenerateTrack();
        carPosition = terrainInner.localPosition;
        initialCubeScale = scaleCube.localScale;
        //StartCoroutine("ScaleTerrain");
    }

    private IEnumerator ScaleTerrain()
	{
		while (Application.isPlaying)
		{
            scaleTime = 0;
            previousScaleVector = scaleVector;
            scaleVector = Random.onUnitSphere;
            yield return new WaitForSeconds(rescaleInterval);
		}
	}

    void GenerateTrack()
    {
        Random.InitState(seed);
        print("Seed "+seed);
        bounds = new Rect(-terrainSize.x / 2, 0, terrainSize.x, terrainSize.y);
        tiles = new Dictionary<Vector2Int, Tile>();
        Dictionary<Vector2Int, List<TileData>> tilesToGenerate = new Dictionary<Vector2Int, List<TileData>>();


        //Adding flat bounds
        for (int x = -terrainSize.x / 2; x < terrainSize.x / 2; x++)
        {
			for (int y = -1; y < terrainSize.y; y++)
			{
                if(x == -terrainSize.x / 2 || x == terrainSize.x / 2 - 1 || y == -1 || y == terrainSize.y-1)
                AddTile(new Vector2Int(x, y), new TileData(tileTypes[0], 0));
                //AddTile(new Vector2Int(x, y), Random.Range(0,5)); //TEMP
            }
        }
        //Adding start and end
        AddTile(new Vector2Int(0, 0), new TileData(tileTypes[1], 0));
        AddTile(new Vector2Int(0, terrainSize.y - 2), new TileData(tunnelEnding, 3));

        int iterations = 0;
		while (tilesToGenerate.Count > 0 && iterations < (terrainSize.x * terrainSize.y) * 2)
		{
            int lowestEntropyID = 0;
            int lowestEntropyCount = int.MaxValue;
            for(int i = 0; i < tilesToGenerate.Count; i++)
			{
				if (tilesToGenerate.ElementAt(i).Value.Count < lowestEntropyCount)
				{
                    lowestEntropyCount = tilesToGenerate.ElementAt(i).Value.Count;
                    lowestEntropyID = i;
                }
			}
            //int id = Random.Range(0, tilesToGenerate.Count);

			Vector2Int coords = tilesToGenerate.ElementAt(lowestEntropyID).Key;
            List<TileData> possibleTiles = tilesToGenerate[coords];
            if(possibleTiles.Count == 0)
			{
                print(coords+ "error");
                break;
			}
            TileData randomPossibility = possibleTiles[Random.Range(0,possibleTiles.Count)];
            //print(coords + " poss=" + possibleTiles.Count+" choice="+ randomPossibility);
            AddTile(coords, randomPossibility);
            //break;
            iterations++;
		}
        print(iterations);
		//List<Tile> neighTiles = GetNeighboringTiles(new Vector2Int(1,0));
  //      //print(neighTiles + " "+neighTiles.Count);
  //      foreach(Tile tile in neighTiles)
		//{
  //          //print(tile.name);
  //          //tile.transform.Translate(Vector3.up);
  //          tile.GetComponent<MeshRenderer>().material = redMat;
		//}



        void AddTile(Vector2Int coords, TileData data)
        {
            GameObject GO = Instantiate(tilePrefab, terrainInner);
            GO.transform.localPosition = new Vector3(coords.x * tileSize * 2 * SIN60, -5, (coords.y + (coords.x % 2 == 0 ? 0 : .5f)) * tileSize * 2) * SIN60;
            GO.name = "Tile_" + coords.x + ":" + coords.y;
            Tile tile = GO.GetComponent<Tile>();
            tile.SetData(data);
            tiles.Add(coords, tile);
			
            List<Vector2Int> neighCoords = GetNeighboringCoords(coords);
            foreach (Vector2Int neigh in neighCoords)
            {
                if (!tiles.ContainsKey(neigh))
                {
                    List<TileData> possibleTypesList = GeneratePossibleTypes(neigh);
                    if (!tilesToGenerate.ContainsKey(neigh))
                    {
                        tilesToGenerate.Add(neigh, possibleTypesList);
                    }
                    else
                    {
                        tilesToGenerate[neigh] = possibleTypesList;
                    }                    
                }
            }
			foreach(Vector3 prop in data.scriptableTile.propCircles)
			{
                for(int i = 0; i < 2; i++)
                {
                    Vector2 random = Random.insideUnitCircle * 4;
                    Vector3 randomOnPropCircle = prop + new Vector3(random.x, 0, random.y);
                    float propScale = Random.value * 3 + Mathf.Abs(randomOnPropCircle.x / 4);
                    GameObject propGO = Instantiate(propPrefab, GO.transform);
                    propGO.GetComponent<MeshFilter>().mesh = propMeshes[Random.Range(0, propMeshes.Length)];
                    propGO.transform.localScale = Vector3.one * propScale;
                    propGO.transform.localPosition = randomOnPropCircle + Vector3.up * propScale;
                }
			}
            List<int> dirs = new List<int> { 0, 1, 2 };
            dirs = dirs.OrderBy(a => Random.Range(0,999)).ToList();
			
			if (Random.value > .25f)
			{
                for (int i = 0; i < 3; i++)
                {
                    GameObject powerUpGO = Instantiate(powerUpPrefab, GO.transform);
                    powerUpGO.transform.rotation = Quaternion.identity;
                    powerUpGO.transform.localPosition = new Vector3((i-1) * 3.5f, 0, 15);
                    Vector3 localScale = new Vector3(.85f, .85f, .85f);
                    switch (dirs[i])
                    {
                        case 0:
                            localScale.x = 1.35f;
                            powerUpGO.tag = "X";
                            break;
                        case 1:
                            localScale.y = 1.35f;
                            powerUpGO.tag = "Y";
                            break;
                        case 2:
                            localScale.z = 1.35f;
                            powerUpGO.tag = "Z";
                            break;
                    }
                    PowerUp powerUp = new PowerUp(powerUpGO.transform, localScale);
                    tile.powerups.Add(powerUp);
                }
            }
			if (data.scriptableTile.ending)
			{
                GameObject endingGO = Instantiate(endingPrefab, GO.transform);
            }
            
            tilesToGenerate.Remove(coords);
        }

        List<TileData> GeneratePossibleTypes(Vector2Int coords)
		{
            Dictionary<int, Tile> neighTiles = GetNeighboringTiles(coords);
            List<TileData> possibilities = new List<TileData>();

            possibilities.AddRange(TestTileTypes(tileTypes, neighTiles));

            //try special tiles
			if (possibilities.Count == 0)
            {
                possibilities.AddRange(TestTileTypes(specialTileTypes, neighTiles));
            }
			return possibilities;
		}

        List<TileData> TestTileTypes(ScriptableTile[] tileList, Dictionary<int, Tile> neighTiles)
        {
            List<TileData> possibilities = new List<TileData>();
            foreach (ScriptableTile tileType in tileList)
            {
                for (int orientation = 0; orientation < 6; orientation++)
                {
                    TileData possibleTile = new TileData(tileType, orientation);
                    bool possible = true;
                    for (int o = 0; o < 6; o++)
                    {
                        if (neighTiles.ContainsKey(o))
                        {
                            if (possibleTile.connections[o] != neighTiles[o].data.connections[(o + 3) % 6])
                            {
                                possible = false;
                                break;
                            }
                        }
                    }
                    if (possible)
                    {
                        //print(coords + " "+possibleTile);
                        possibilities.Add(possibleTile);
                    }
                }
            }
            return possibilities;
        }
    }

    

    List<Vector2Int> GetNeighboringCoords(Vector2Int coords)
	{
        List<Vector2Int> answer = new List<Vector2Int>();
        Vector2Int[] coordsOffset = coords.x % 2 == 0 ? coordsOffsetEven : coordsOffsetOdd;
        foreach (Vector2Int offset in coordsOffset)
        {
            Vector2Int tested = coords + offset;
            if (bounds.Contains(tested))
			{
                answer.Add(tested);
			}
        }
        return answer;
    }

	Dictionary<int, Tile> GetNeighboringTiles(Vector2Int coords)
	{
        Dictionary<int, Tile> answer = new Dictionary<int, Tile>();
        Vector2Int[] coordsOffset = coords.x%2==0? coordsOffsetEven : coordsOffsetOdd;
        for(int i = 0; i<6;i++)
		{
            if (tiles.ContainsKey(coords + coordsOffset[i]))
			{
                answer.Add(i, tiles[coords + coordsOffset[i]]);
			}
		}
        return answer;
	}

	// Update is called once per frame
	void FixedUpdate()
    {
		//currentScale = Vector3.one;// + (1-driftValue) * .5f * Vector3.Lerp(previousScaleVector, scaleVector, scaleTime/ (rescaleInterval*.5f));
		scaleVector = new Vector3(
            Mathf.Clamp(scaleVector.x + Time.deltaTime * scaleIncrease*1, .75f, 2.0f),
            Mathf.Clamp(scaleVector.y + Time.deltaTime * scaleIncrease*1, .75f, 2.0f),
            Mathf.Clamp(scaleVector.z + Time.deltaTime * scaleIncrease*1, .75f, 2.0f)
            );
		
		//previousScaleVector = scaleVector;
		//float hFactor = 1 - Mathf.Clamp((Mathf.Clamp(rb.velocity.magnitude - 15, 0, 1000) / 20f), 0, .3f);
		//scaleVector = new Vector3(hFactor, Mathf.Clamp((Mathf.Clamp(rb.velocity.magnitude - 10, 0, 1000) / 8f), 1, 2), hFactor);
		currentScale = Vector3.Lerp(currentScale, scaleVector, .02f);// scaleTime/ (rescaleInterval*.5f));
        //Vector3.ClampMagnitude(currentScale, 2.0f);
        X_Scale.text = "X = " + Mathf.RoundToInt(currentScale.x*100) + "%";
        Y_Scale.text = "Y = " + Mathf.RoundToInt(currentScale.y*100) + "%";
        Z_Scale.text = "Z = " + Mathf.RoundToInt(currentScale.z*100) + "%";
        terrain.localScale = currentScale;
        scaleCube.localScale = currentScale * initialCubeScale.z;
        scaleTime += Time.fixedDeltaTime;

        movementDelta = new Vector3(rb.position.x / currentScale.x, rb.position.y / currentScale.y, rb.position.z / currentScale.z);
        carPosition -= movementDelta;


        if (respawnCar)
		{
            carPosition = -TileToWorldCoords(currentTileCoords);
            print(currentTileCoords + " " + TileToWorldCoords(currentTileCoords));
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            respawnCar = false;
        }
        terrainInner.localPosition = carPosition;        
        rb.position = Vector3.zero;
		
        //      foreach(Tile tile in tiles.Values)
        //{
        //          tile.GetComponent<MeshRenderer>().material = whiteMat;
        //}

        //if (tiles.ContainsKey(tileCoords))
        //      {
        //          tiles[tileCoords].GetComponent<MeshRenderer>().material = redMat;
        //      }

        Vector2Int newTileCoords = WorldToTileCoords(Vector3.zero);
        if(currentTileCoords != newTileCoords) //Tile changed, update tiles displayed
		{
            currentTileCoords = newTileCoords;
            if (occlusion) UpdateTilesDisplayed();
		}
  //      driftValue = Mathf.Clamp01(driftValue);
  //      driftCounter.value = driftValue;
		//if (timeWithoutDrift > driftGraceTime)
		//{
  //          driftValue -= driftDecay;
		//}
  //      timeWithoutDrift += Time.fixedDeltaTime;

    }

    void UpdateTilesDisplayed()
	{
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Vector3 currentWorldCoords = TileToWorldCoords(currentTileCoords);

        //desactivate tiles depending on distance
        foreach (Tile tile in tiles.Values)
		{
            tile.gameObject.SetActive(Vector3.SqrMagnitude(tile.transform.localPosition - currentWorldCoords + occlusionOffset) <= occlusionDistance);
        }
        ////get neighboring tiles
        //var neigh = GetNeighboringCoords(currentTileCoords);

        ////activate the 7 tiles around
        //tiles[currentTileCoords].gameObject.SetActive(true);
        //foreach (Vector2Int neighCoords in neigh)
        //{
        //    var neigh2 = GetNeighboringTiles(neighCoords);
        //    foreach (Tile tile in neigh2.Values)
        //    {
        //        tile.gameObject.SetActive(true);
        //    }
        //}
        print(sw.ElapsedMilliseconds);
        sw.Stop();
    }

 //   public void CenterToCurrentTile()
	//{
 //       //print(TileToWorldCoords(currentTileCoords) + Vector3.down);
 //       terrainInner.localPosition = TileToWorldCoords(currentTileCoords)+Vector3.down;
 //       updatePosition = false;
 //   }

    public Vector2Int WorldToTileCoords(Vector3 worldCoords)
	{
        Vector3 localCoords = terrainInner.InverseTransformPoint(worldCoords);
        int x = Mathf.RoundToInt(localCoords.x / (tileSize * 1.5f));
        int y = Mathf.RoundToInt((localCoords.z / (tileSize * 2f*SIN60)) + (x % 2 == 0 ? 0.0f : -0.5f));
        return new Vector2Int(x,y);
	}

    public Vector3 TileToWorldCoords(Vector2Int tileCoords)
	{
        float localX = tileCoords.x * (tileSize * 1.5f);
        float localZ = (tileCoords.y + (tileCoords.x % 2 == 0 ? 0.0f : 0.5f)) * (tileSize * 2f * SIN60);
        return new Vector3(localX, 0, localZ);
    }
}