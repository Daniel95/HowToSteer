using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TerrainTrail : MonoBehaviour {

    [SerializeField]
    private int wayPointCooldown = 25;

    [SerializeField]
    public int lineHeight = 1;

    [SerializeField]
    private int lineWidth = 2;

    [SerializeField]
    private float lineSmoothness = 0.5f;

    [SerializeField]
    private int skipMeshBuildingAmount = 2;
    private int amountSkippedCounter = 0;

    private int mapChunkSize;
    private int halfMapChunkSize;

    private Vector2 oldChunkPosition;
    private Vector2 oldLocalPosition;

    public Func<Vector3> GetPostion;

    private Vector3 trailPosition;

    private MeshSpawner meshSpawner;

    private List<Vector2> chunksPassedThrough = new List<Vector2>();

    void Awake()
    {
        meshSpawner = FindObjectOfType<MeshSpawner>();
        mapChunkSize = FindObjectOfType<MapGenerator>().MapChunkSize - 1;

        halfMapChunkSize = mapChunkSize / 2;

        oldChunkPosition = new Vector2(0,0);
        oldLocalPosition = new Vector2(halfMapChunkSize - 10, halfMapChunkSize);
    }

    public void StartTerrainTrails()
    {
        StartCoroutine(TerrainTrails());
    }

    public void StopTerrainTrails() {
        StopAllCoroutines();
    }

    //edits the mapdata.noisemap with paths, and updates the meshes of all chunksPassedThrough
    IEnumerator TerrainTrails() {
        while (true)
        {
            int counter = 0;

            if (GetPostion != null)
                trailPosition = GetPostion();

            Vector2 newChunkPosition = new Vector2(Mathf.Floor((trailPosition.x + halfMapChunkSize) / mapChunkSize), Mathf.Floor((trailPosition.z + halfMapChunkSize) / mapChunkSize));
            Vector2 newLocalPosition = new Vector2(trailPosition.x - (newChunkPosition.x * mapChunkSize - halfMapChunkSize), Mathf.Abs(trailPosition.z - (newChunkPosition.y * mapChunkSize + halfMapChunkSize)));

            while (counter < wayPointCooldown)
            {
                counter++;
                yield return new WaitForFixedUpdate();
            }

            Paths.CreatePathChunksOverflow(oldChunkPosition, newChunkPosition, oldLocalPosition, newLocalPosition, lineHeight, lineWidth, lineSmoothness, true, mapChunkSize);

            int chunkCounter = 0;

            if (amountSkippedCounter >= skipMeshBuildingAmount)
            {
                amountSkippedCounter = 0;
                while (chunkCounter < chunksPassedThrough.Count)
                {
                    meshSpawner.MeshTerrainDictonary[chunksPassedThrough[chunkCounter]].UpdateNoiseMesh(MapGenerator.mapDataContainer[chunksPassedThrough[chunkCounter]].noiseMap);
                    chunkCounter++;
                    yield return new WaitForFixedUpdate();
                }
                chunksPassedThrough.Clear();
            }
            amountSkippedCounter++;

            oldChunkPosition = newChunkPosition;
            oldLocalPosition = newLocalPosition;
        }
    }

    //updates all chunks the traillocations passed through, and saves them in chunksPassedThrough
    void Update() {
        if (GetPostion != null)
            trailPosition = GetPostion();

        Vector2 currentChunkLocation = new Vector2(Mathf.Floor((trailPosition.x + halfMapChunkSize) / mapChunkSize), Mathf.Floor((trailPosition.z + halfMapChunkSize) / mapChunkSize));
        bool alreadySavedChunk = false;
        for (int i = 0; i < chunksPassedThrough.Count; i++)
        {
            if (currentChunkLocation == chunksPassedThrough[i])
            {
                alreadySavedChunk = true;
            }
        }

        if (!alreadySavedChunk)
        {
            chunksPassedThrough.Add(currentChunkLocation);
        }
    }
}
